(****************************************************************************************)
(*                                                                                      *)
(*                                      Linear.fs                                       *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Conversion of symbolic contraints to linear inequalities                             *)
(*                                                                                      *)
(****************************************************************************************)


module Linear

    open System
    open PrimitiveFunctions
    open Expression
    open Semantics
    
    // right-biased union of two maps
    let join (p:Map<'a,'b>) (q:Map<'a,'b>) = 
        Map.fold (fun state k v -> Map.add k v state) p q

    
    // Combines two by appliying a function f on each shared key
    let combineMapping (A:Map<int, Double>) (B:Map<int, Double>) (f:Double*Double -> Double) =  
        let mutable newMap = Map.empty
        // Iterate over every pair
        for k in A |> Map.toSeq |> Seq.map fst do  
            // Find the coenficnet in A
            let wa = Map.find k A
            // Find the coenficnet in B. If it does not exists use 0.0
            let wb = Map.tryFind k B |> Option.defaultValue 0.0
            // Add combined Value to newMap
            newMap <- Map.add k (f(wa, wb)) newMap
        for k in B |> Map.toSeq |> Seq.map fst do  
            // Find the coenficnet in A
            let wb = Map.find k B
            // Find the coenficnet in B. If it does not exists use 0.0
            let wa = Map.tryFind k A |> Option.defaultValue 0.0
            // Add combined Value to newMap
            newMap <- Map.add k (f(wa, wb)) newMap
        newMap

    type LinearFunction = Map<int, Double> * double

    type Comp = 
        | Leq
        | Gt
        | Geq

    // Always relative to 0
    type LinearInequality = LinearFunction * Comp

    /// Converts a Symbolic Value to a linear Function
    let rec convertToLinearFunction (M:Expr) : LinearFunction = 
        match M with   
        | Numeral r -> (Map.empty, r)
        | SampleVariable n -> (Map.empty.Add(n, 1.0), 0.0)
        | SymPrimF(ADD, args) -> 
            let (coef1, r1) = convertToLinearFunction args.[0]
            let (coef2, r2) = convertToLinearFunction args.[1]
            (combineMapping coef1 coef2 (fun (x,y) -> x+y) , r1 + r2)
        | SymPrimF(SUB, args) -> 
            let (coef1, r1) = convertToLinearFunction args.[0]
            let (coef2, r2) = convertToLinearFunction args.[1]
            (combineMapping coef1 coef2 (fun (x,y) -> x-y) , r1 - r2)
        | SymPrimF(MUL, args) ->
            let (coef1, r1) = convertToLinearFunction args.[0]
            let (coef2, r2) = convertToLinearFunction args.[1]

            if coef1.IsEmpty then
                // The first arument is a constant -> Multiply second polynomial by r1
                (Map.map (fun _ i -> r1 * i) coef2, r1 * r2)
            elif coef2.IsEmpty then
                // The second argument is a constant -> Multiply first polynomial by r2
                (Map.map (fun _ i -> r2 * i) coef1, r1 * r2)
            else
                failwith "Multplication is only supported by constants"
        | _ -> failwith "Matched on a non-value"

    let rec extractLinearInequality (p:Path) : Set<LinearInequality> =
        match p with
        | LeafPath _ -> Set.empty
        | FixPath p -> extractLinearInequality p
        | ScorePath (M, p) -> Set.add (convertToLinearFunction M, Geq) (extractLinearInequality p)
        | OrLeftPath (M, p) -> Set.add (convertToLinearFunction M, Leq) (extractLinearInequality p)
        | OrRightPath (M, p) -> Set.add (convertToLinearFunction M, Gt) (extractLinearInequality p)
        | AndLeftPath p -> extractLinearInequality p
        | AndRightPath p -> extractLinearInequality p



    // All variables used in a linear Function
    let usedVariables (pl:LinearFunction) =
        let (m, _) = pl    
        Map.fold (fun state key _ -> Set.add key state  ) Set.empty m

    let usedVariablesConstraint (lInelsit : Set<LinearInequality>) =
        Set.fold (fun a (psi, _) -> Set.union a (usedVariables psi)) Set.empty lInelsit


    /// Print the stndard block for INE File that ensures that each 0 <= x_i <= 1 
    /// n : Number of variables
    let rec printStandartBlock (n:int) : string =
        let mutable output = ""
        for i in 1 .. n do
            output <- output + " 0"
            for j in 1 .. n do
                output <- output + if i = j then " -1" else " 0"
            output <- output + "\n"
            output <- output + " 1"
            for j in 1 .. n do
                output <- output + if i = j then " 1" else " 0"
            output <- output + "\n"
        output


    // Compute the INE file generted from a list of linear inequalties
    let rec computeIneFile (ineConst : Set<LinearInequality>) : string = 
        // Compute all varaiables used 
        let usedVars = usedVariablesConstraint(ineConst)
        
        // Generate a varaiable mapping that maps all used varaibles to a set {0, ..., n} 
        let mutable varMap = Map.empty
        let mutable count = 0
        for i in usedVars do
            varMap <- Map.add count i varMap
            count <- count + 1

        // Print the standart block
        let mutable output : string = "begin\n " + sprintf "%i" (ineConst.Count + 2 * count) + " " + sprintf "%i" (usedVars.Count + 1) + " real" + "\n"
        output <- output + printStandartBlock count 

        // Print every single equality
        for ((coef, abs), comb) in ineConst do
            match comb with
            | Leq -> 
                output <- output + " " + sprintf "%f" (-abs)
                for i in 0 .. (count - 1) do
                    let orVar = Map.find i varMap
                    output <- output + " " + (match Map.tryFind orVar coef with | Some x -> sprintf "%f" x | None -> "0")
            | Gt -> 
                output <- output + " " + sprintf "%f" abs
                for i in 0 .. (count - 1) do
                    let orVar = Map.find i varMap
                    output <- output + " " + (match Map.tryFind orVar coef with | Some x -> sprintf "%f" -x | None -> "0")
            | Geq -> //Identical to Gt
                output <- output + " " + sprintf "%f" abs
                for i in 0 .. (count - 1) do
                    let orVar = Map.find i varMap
                    output <- output + " " + (match Map.tryFind orVar coef with | Some x -> sprintf "%f" -x | None -> "0")
            output <- output + "\n"
        output <- output + "end"
        output


    // If a linear inequality list does not contain any varaibles we check if it is valid
    let rec checkVaraibleFreeCondList (ineConst : Set<LinearInequality>) = 
        Set.forall 
            (fun ((coef, abs) , comb) -> 
                Map.isEmpty coef && (match comb with Leq -> abs <= 0.0| Gt -> abs > 0.0 | Geq -> abs >= 0.0)
            ) 
            ineConst