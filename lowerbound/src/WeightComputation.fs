(****************************************************************************************)
(*                                                                                      *)
(*                                      WeightComputation.fs                            *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Volume/Weight computation of a set of contraints by iterative splitting              *)
(*                                                                                      *)
(****************************************************************************************)

module WeightComputation

open System
open PrimitiveFunctions
open Expression
open System.Collections.Generic
open UnionFind
open Rational
open Timing

type Constraint = Guard list

/// Computes the sample Varaibles used in a constraint. Passes the argument as a read only ref to avoid copying
let constraintUsedVariables (c: inref<Constraint>) =
    c |> List.map (fun x -> x.Value |> usedVariables) |> Set.unionMany


// Normalize the Sample-Varaibles to the set {0, ..., n}
let normalize (c : inref<Constraint>) =
    let usedVars = constraintUsedVariables &c
    let mutable m = Map.empty
    let mutable count = 0
    for i in usedVars do
        m <- Map.add i count m
        count <- count + 1
    let rec replaceVars = function
        | C r -> C r
        | V i -> V (Map.find i m)
        | F(name, args) -> F(name, Array.map replaceVars args)
    List.map (fun x -> {Value = replaceVars x.Value; Com = x.Com} ) c
    //replaceVars s


// Use refs for that slight increase in performance
type Box = array<ref<Rational>*ref<Rational>>

/// Given a symbolic Real Value s and a box, compute the image of s under the box.
/// Converts boxes to real value, as primtive functions operate on floating point values
let rec computeInterval (s : inref<SymbolicRealValue>) (b : Box) : Interval =
    match s with
        | C r -> (r, r)
        | V i -> 
            let (a, b) = b.[i]
            ((!a).AsDouble(), (!b).AsDouble())
        | F(sym, args) -> 
            let f = lookupFunctionSymbol sym
            Array.map (fun (x : SymbolicRealValue) -> computeInterval &x b) args |> f.ComputeInterval


type ContainmentResult = IN | OUT | UNKNOWN

// Checks if a function is continous. This allows a relaxed handeling of interval borders. Currently all supported functions are continous.
let continous (g:Guard) = true

/// Cehcks if a given guard is satfied on a box
let checkGuard (g : inref<Guard>) b =
    let (a, b) = computeInterval &g.Value b

    if (b-a <> 0.0) && continous g then
        // Use simplied version where [] and () are identfied as the function is continous 
        match g.Com with
        | LEQ -> if b <= 0.0 then ContainmentResult.IN elif a >= 0.0 then ContainmentResult.OUT else ContainmentResult.UNKNOWN
        | GT | GEQ -> if a >= 0.0 then ContainmentResult.IN elif b <= 0.0 then ContainmentResult.OUT else ContainmentResult.UNKNOWN
    else
        // Use standard version
        match g.Com with
        | LEQ -> if b <= 0.0 then ContainmentResult.IN elif a > 0.0 then ContainmentResult.OUT else ContainmentResult.UNKNOWN
        | GT -> if a > 0.0 then ContainmentResult.IN elif b <= 0.0 then ContainmentResult.OUT else ContainmentResult.UNKNOWN
        | GEQ -> if a >= 0.0 then ContainmentResult.IN elif b < 0.0 then ContainmentResult.OUT else ContainmentResult.UNKNOWN

/// Compute the volume of a Box
let volume (b:inref<Box>) =
    Array.fold (fun v (a, b) -> v * (!b - !a)) (Rational(1)) b

/// Splits a box in dimension i
let splitBox (b:inref<Box>) (i:int) : Box*Box =
    let first = Array.mapi (fun j (a, b) -> if i = j then (a, !a + (!b - !a) / Rational(2) |> ref) else (a, b)) b
    let second = Array.mapi (fun j (a, b) -> if i = j then (!a + (!b - !a) / Rational(2) |> ref, b) else (a, b)) b
    (first, second)


/// Computes the probabilty of a contraints by itertively spliting the unit box.
/// Epsilon gives the total error, i.e., the computed probabilty is definity a lower bound but might be smaller than the real on by at most epsilon
let myComputeWeight (c : Constraint) (epsilon: Rational) = 
    let normCon = normalize &c

    // Compute the dimension of the box
    let d = constraintUsedVariables &normCon |> Set.count

    // Create initial unit box
    let b' : Box = Array.create d (Rational(0) |> ref, Rational(1) |> ref)
    let q = new Queue<Box>()
    q.Enqueue b'
    let mutable inVol = Rational 0
    let mutable outVol = Rational 0
    //let rnd = System.Random()

    let bound = Rational(1) - epsilon

    let mutable timingCount = 0
    let mutable exit = false
    // Loop
    while (not exit) && q.Count <> 0 && inVol + outVol < bound do
        let b = q.Dequeue()
        let checkRes = List.map (fun (g : Guard) -> checkGuard &g b) normCon
        
        if List.forall (fun x -> x = ContainmentResult.IN) checkRes then
            inVol <- inVol + volume(&b)
        elif List.exists (fun x -> x = ContainmentResult.OUT) checkRes then
            // Box negates one guards, no further anaylsis needed
            outVol <- outVol + volume(&b)
        else
            // Undecided -> Split Box

            // Determine split direction 
            let mutable max = Rational(-1)
            let mutable index = -1
            for i in 0 .. (d-1) do
                let (a, b) = b.[i]
                if (!b - !a > max) then index <- i; max <- !b - !a
            
            let (b1, b2) = splitBox &b index
            q.Enqueue b1
            q.Enqueue b2

        if Timing.bound.IsSome then
            if timingCount % 100 = 0 then
                if Timing.globalStopwatch.ElapsedMilliseconds >= Timing.bound.Value then
                    exit <- true    
    inVol


let asyncWrapper (c : Constraint) (epsilon: Rational) = 
    async { return (myComputeWeight c epsilon) }


// Look for independent Subproblems
let optimizedComputeWeight (c:Constraint) (epsilon: Rational) =

    // Nomrilze and convert to array to allow index based acess
    let normCon = normalize &c |> List.toArray
    // DO not have to normalize here

    // The used varaibles for each set
    let s = Array.map (fun x -> usedVariables x.Value) normCon

    let numberOfGuards = s.Length

    // Create an empty union find object
    let uf = UnionFind(numberOfGuards)

    for i in 0 .. (numberOfGuards - 1) do
        for j in (i + 1) .. (numberOfGuards - 1) do
            if Set.intersect (s.[i]) (s.[j]) |> Set.isEmpty then
                ()
            else
                uf.Union(i, j);

    let partition = uf.ExtractPartition()

    let mutable vol = Rational 1

    let mutable exit = false


    // Handle each partition individually
    for part in partition do
        if not exit then
            // Add all guards from part
            let mutable subProblem : Constraint = []
            for i in part do
                subProblem <- normCon.[i]::subProblem

            
            vol <- vol * myComputeWeight subProblem epsilon
            if Timing.bound.IsSome && Timing.globalStopwatch.ElapsedMilliseconds >= Timing.bound.Value then
                exit <- true  

    if Timing.bound.IsSome && Timing.globalStopwatch.ElapsedMilliseconds >= Timing.bound.Value then
        exit <- true   

    // If aborted the result is not valid
    if exit then Rational(0) else vol



