(****************************************************************************************)
(*                                                                                      *)
(*                                      HMType.fs                                       *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Simple Hindley-Milner Type system for SPCF                                           *)
(*                                                                                      *)
(****************************************************************************************)

module HMType

open System
open Expression

let mutable varCount = ""

type private TVar = String

type private Type = 
    | Var of TVar
    | Real 
    | Arrow of Type*Type

type private TypeEnv = Map<TVar, Type>

 
let private extend (gamma:TypeEnv) (x:TVar, tau:Type) =
    Map.add x tau gamma

type private Subst = Map<TVar, Type>

let private nullSubst : Subst = Map.empty

let rec private applyT (s:Subst) = function
    | Var i as t -> match Map.tryFind i s with Some tau -> tau | None -> t
    | Real -> Real
    | Arrow(t1, t2) -> Arrow(applyT s t1, applyT s t2)


let private applyE (s:Subst) (gamma:TypeEnv) : TypeEnv =
    Map.map (fun _ t -> applyT s t) gamma

let rec private fvT = function
    | Var i -> Set.singleton i
    | Real -> Set.empty
    | Arrow(t1, t2) -> Set.union (fvT t1) (fvT t2)

let private fvE (gamma:TypeEnv) =
    Map.fold (fun state _ t -> Set.union state (fvT t)) Set.empty gamma


// right-biased union
let private join (p:Map<'a,'b>) (q:Map<'a,'b>) = 
    Map.fold (fun state k v -> Map.add k v state) p q

let private compose (s1:Subst) (s2:Subst) : Subst = 
    join s1 (Map.map (fun _ t -> applyT s1 t) s2)

let private fresh() =
    varCount <- varCount + "a"
    Var varCount

// Gives most general unifer of two types. Fails if none exists
let rec private unify (t1:Type) (t2:Type) : Subst =
    match (t1, t2) with
        | (Real, Real) -> nullSubst
        | (Var i, t) | (t, Var i) ->
            if Var i = t then
                // They are identical
                nullSubst
            else
                let freeVars = fvT t
                if Set.contains i (fvT t) then failwith "Infinite Type" else Map.empty.Add(i, t)
        | (Arrow(t1, t2), Arrow(l1, l2)) ->
            let s1 = unify t1 l1
            let s2 = unify (applyT s1 t2) (applyT s1 l2)
            compose s2 s1
        | _ -> failwith "Failed to unify"


let rec private infer (gamma:TypeEnv) (M: Expr) : (Subst * Type) =
    match M with
        | Variable x -> match Map.tryFind x gamma with Some t -> (nullSubst, t) | None -> failwith "Term is not closed"
        | Abstraction(var, N) ->  
            let tv = fresh()
            let gamma' = Map.add var tv gamma
            let (s, t) = infer gamma' N
            (s, Arrow (applyT s tv, t))
        | Application(N1, N2) -> 
            let tv = fresh()
            let (s1, t1) = infer gamma N1
            let (s2, t2) = infer (applyE s1 gamma) N2
            let s3 = unify (applyT s2 t1) (Arrow(t2, tv))
            (compose s3 (compose s2 s1), applyT s3 tv)
        | Fixpoint(fixvar, var, N) ->
            let tv = fresh()
            let tvret = fresh()
            let gamma' = Map.add fixvar (Arrow(tv, tvret)) (Map.add var tv gamma)
            let (s, t) = infer gamma' N
            let s' = unify (applyT s tvret) t
            let s'' = compose s' s
            (s'', applyT s'' (Arrow(tv, tvret)))
        | Numeral _ -> (nullSubst, Real)
        | Sample -> (nullSubst, Real)
        | Conditional(N1, N2, N3) ->
            let (s1, t1) = infer gamma N1
            let (s2, t2) = infer (applyE s1 gamma) N2
            let s = compose s2 s1
            let (s3, t3) = infer (applyE s gamma) N3
            let s' = compose s s3
            let s4 = unify (applyT s' t1) Real
            let s'' = compose s' s4
            let s5 = unify (applyT s'' t2) (applyT s'' t3)
            let s''' = compose s'' s5
            (s'', applyT s'' t2)
        | Score(N) ->
            let (s, t) = infer gamma N
            let s' = unify t Real
            let s'' = compose s s'
            (s'', Real)

        | PrimF(_, args) ->
            // Infer types for all arguments while keeping the env up to date
            let (types, typeEnv) = 
                Array.mapFold 
                    (fun state e -> 
                        let (s, t) = infer state e
                        ((s, t), applyE s state)
                    ) 
                    gamma 
                    args
            // Join all subs together
            let joinedS = Array.fold (fun state (s, _) -> compose state s) nullSubst types
            // Unify all types with Real
            let finalS = Array.fold (fun state (_, t) -> compose (unify (applyT state t) Real) state ) joinedS types
            (finalS, Real)

        | _ -> failwith "Unknwon Langauge construct"

/// Checks if a Program can be typed
let isWellTyped (M:Expr) =
    try
        let _ = infer nullSubst M
        true
    with
        | _ -> false