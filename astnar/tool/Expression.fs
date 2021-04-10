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
(* Type of Expression together with basic term operations                               *)
(*                                                                                      *)
(****************************************************************************************)


module Expression

    open System
    open PrimitiveFunctions


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
        | FixDummy
        | Star


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
            | FixDummy -> "fix!"
            | Star -> "star"


    let rec usedSampleVariables (M:Expr) = 
        match M with
            | SampleVariable n -> Set.singleton n
            | Application(N1, N2) -> Set.union (usedSampleVariables N1) (usedSampleVariables N2)
            | Conditional(N1, N2, N3) -> Set.union (usedSampleVariables N1) (Set.union (usedSampleVariables N2) (usedSampleVariables N3))
            | Abstraction(_, N) | Fixpoint(_, _, N)| Score N -> usedSampleVariables N
            | PrimF(_, args) -> Array.fold (fun s a -> Set.union s (usedSampleVariables a)) Set.empty args
            | SymPrimF(_, args) -> Array.fold (fun s a -> Set.union s (usedSampleVariables a)) Set.empty args
            | _ -> Set.empty


    let rec subst (M:Expr) (x:string) (N:Expr) = 
        match M with
            | Numeral v -> Numeral v
            | Variable y -> if x = y then N else Variable y
            | Application (M1, M2) -> Application (subst M1 x N, subst M2 x N)
            | Abstraction (y, M1) -> if x = y then Abstraction(y, M1) else Abstraction(y, subst M1 x N)
            | Fixpoint (f, y, M1) -> if x=y || x=f then Fixpoint(y, f, M1) else Fixpoint(y, f, subst M1 x N)
            | Sample -> Sample
            | Conditional (M1, M2, M3) -> Conditional(subst M1 x N, subst M2 x N, subst M3 x N)
            | Score (M1) -> Score(subst M1 x N) 
            | FixDummy -> FixDummy
            | SampleVariable n -> SampleVariable n
            | PrimF(name, args) -> PrimF(name, Array.map (fun a -> subst a x N) args)
            | SymPrimF(name, args) -> SymPrimF(name, Array.map (fun a -> subst a x N) args)
            | Star -> Star

    // Checks if Star is contained
    let rec containsStar (M:Expr) = 
        match M with
            | Numeral _ -> false
            | Variable _ -> false
            | Application (M1, M2) -> containsStar M1 || containsStar M2
            | Abstraction (_, M1) -> containsStar M1
            | Fixpoint (_, _, M1) -> containsStar M1
            | Sample -> false
            | Conditional (M1, M2, M3) -> containsStar M1 || containsStar M2 || containsStar M3
            | Score (M1) -> containsStar M1
            | FixDummy -> false
            | SampleVariable n -> false
            | PrimF(_, args) -> Array.exists containsStar args
            | SymPrimF(_, args) -> Array.exists containsStar args
            | Star -> true

    let makeBetaStep (M:Expr) (N:Expr) = 
        match M with
            | Abstraction(y, M') -> subst M' y N
            | FixDummy -> Star
            | _ -> failwith "Error"
