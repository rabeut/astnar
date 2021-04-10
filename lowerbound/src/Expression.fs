(****************************************************************************************)
(*                                                                                      *)
(*                                      Expression.fs                                   *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Type of Terms in SPCF                                                                *)
(*                                                                                      *)
(****************************************************************************************)

module Expression

open System
open PrimitiveFunctions

/// Symbolic Expr
type Expr = 
    | Variable of String 
    | Numeral of Double
    | Application of Expr*Expr
    | Abstraction of String*Expr
    | Fixpoint of String*String*Expr
    | Conditional of Expr*Expr*Expr
    | Sample 
    | Score of Expr
    | PrimF of FunctionSymbol* (Expr array)
    | SymPrimF of FunctionSymbol* (Expr array)
    | SampleVariable of int

let isValue = function 
    | Variable _ -> true
    | Numeral _ -> true
    | Abstraction _ -> true
    | Fixpoint _ -> true
    | SymPrimF _ -> true
    | SampleVariable _ -> true
    | _ -> false


/// Converts a symbolic Expr to a symbolic Value. Fails if the argument is no well typed value of type real
let rec convertToSymbolicRealValue = function
    | Numeral r -> C r
    | SampleVariable n -> V n
    | SymPrimF(name, args) -> F(name, Array.map convertToSymbolicRealValue args)
    | _ -> failwith "Error while converting to symbolic value"

/// Prints a term
let rec toString(M: Expr) = 
    match M with
        | Variable s -> s
        | Numeral r -> string(r)
        | Application(N, P) -> toString N + " " + toString P 
        | Abstraction (var, N) -> "fun " + var + ". " + toString N
        | Fixpoint (recVar, var, N) -> "fix " + recVar + " " + var + ". " + toString N 
        | Conditional (N, P, Q) -> "if " + toString N + " then " + toString P + " else " + toString Q 
        | Sample -> "sample" 
        | Score N -> "score " + toString N 
        | PrimF(sym, args) -> "F(" + lookupFunctionSymbol(sym).Name + ":" + (Array.fold (fun s a -> s + ", " + toString a) "" args) + ")"
        | SampleVariable n -> "alpha " + string(n)
        | SymPrimF(sym, args) -> "SymF(" + lookupFunctionSymbol(sym).Name + ":" + (Array.fold (fun s a -> s + ", " + toString a) "" args) + ")"

/// Computes all Sample Varaibles that occur in an Expr
let rec usedSampleVariables (M:Expr) = 
    match M with
        | SampleVariable n -> Set.singleton n
        | Application(N1, N2) -> Set.union (usedSampleVariables N1) (usedSampleVariables N2)
        | Conditional(N1, N2, N3) -> Set.union (usedSampleVariables N1) (Set.union (usedSampleVariables N2) (usedSampleVariables N3))
        | Abstraction(_, N) | Fixpoint(_, _, N)| Score N -> usedSampleVariables N
        | PrimF(_, args) -> Array.fold (fun s a -> Set.union s (usedSampleVariables a)) Set.empty args
        | SymPrimF(_, args) -> Array.fold (fun s a -> Set.union s (usedSampleVariables a)) Set.empty args
        | _ -> Set.empty

/// Perfroms standard, capture-avoiding Substitution
let rec subst (M:Expr) (x:string) (N:Expr) = 
    match M with
        | Numeral v -> Numeral v
        | Variable y -> if x = y then N else Variable y
        | Application (M1, M2) -> Application (subst M1 x N, subst M2 x N)
        | Abstraction (y, M1) -> if x = y then Abstraction(y, M1) else Abstraction(y, subst M1 x N)
        | Fixpoint (f, y, M1) -> if x=y || x=f then Fixpoint(f, y, M1) else Fixpoint(f, y, subst M1 x N)
        | Sample -> Sample
        | Conditional (M1, M2, M3) -> Conditional(subst M1 x N, subst M2 x N, subst M3 x N)
        | Score (M1) -> Score(subst M1 x N) 
        | SampleVariable n -> SampleVariable n
        | PrimF(name, args) -> PrimF(name, Array.map (fun a -> subst a x N) args)
        | SymPrimF(name, args) -> SymPrimF(name, Array.map (fun a -> subst a x N) args)

/// Global Counter to get Fresh Sample Varaibles
let mutable sampleVar = 0

type Compare = LEQ | GT | GEQ

/// Respresnts a guard in the program execution, i.e., a constraints of the form V \bowtie 0 for \bowtie \in Compare
type Guard = 
    {
        Value : SymbolicRealValue;
        Com : Compare;
    }

/// Computes the (CbN) sucessors of a Term as a list of pairs sucessor terms and a possible guard
let rec succ (M:Expr) : (Expr*option<Guard>) list = 
    match M with
        | Application(M, N) -> 
            if isValue M then
                match M with
                        | Abstraction(x, M') -> [(subst M' x N, None)]
                        | Fixpoint(f, x, M') -> [(subst (subst M' f (Fixpoint(f, x, M'))) x N, None)]
                        | _ -> failwith "Should not be possible for well typed terms"
            else
                List.map (fun (x, y) -> (Application(x, N), y)) (succ M)
        | Conditional(M, N, P) ->
            if isValue M then
                [(N, {Value = convertToSymbolicRealValue M; Com = LEQ} |> Some); (P, {Value = convertToSymbolicRealValue M; Com = GT} |> Some)]
            else
                List.map (fun (x, y) -> (Conditional(x, N, P), y)) (succ M)
        | Score(M) -> 
            if isValue M then
                [(M, {Value = convertToSymbolicRealValue M; Com = GEQ} |> Some)]
            else 
                List.map (fun (x, y) -> (Score(x), y)) (succ M)
        | Sample -> 
            let temp = SampleVariable sampleVar
            sampleVar <- sampleVar + 1
            [(temp, None)]
        | PrimF(name, args) -> 
            if Array.forall isValue args then
                // Can reduce further
                [(SymPrimF(name, args), None)]
            else
                // Reduce contex
                let index = Array.findIndex (isValue >> not) args
                let temp = args.[index] |> succ
                List.map (fun (x, y) -> (PrimF(name, Array.mapi (fun i e -> if i = index then x else e) args), y) ) temp
        | _ -> []

                
                