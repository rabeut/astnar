(****************************************************************************************)
(*                                                                                      *)
(*                                      Semantics.fs                                    *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Semantics of a SPCF term by computation of symbolic execution trees                  *)
(*                                                                                      *)
(****************************************************************************************)

module Semantics
    open Expression

    // AndOrTree used for the symbolic evaluation
    type AndOrTree =
        | LeafNode of Expr
        | FixNode of AndOrTree
        | ScoreNode of Expr*AndOrTree
        | AndNode of Expr*AndOrTree*AndOrTree
        | OrNode of Expr*AndOrTree*AndOrTree


    /// Checks if a Tree is sufficently independent
    let checkSufficentIndepndence(t:AndOrTree) =
        let rec isTreeValid (a:Set<int>) (t:AndOrTree) = 
            match t with
                | LeafNode V -> true
                | FixNode t' -> isTreeValid a t'
                | ScoreNode (V, t') -> 
                    let usedVars = usedSampleVariables V
                    Set.isEmpty (Set.intersect a usedVars) &&  isTreeValid a t'
                | AndNode(V, t1, t2) -> 
                    let usedVars = usedSampleVariables V 
                    let a' = Set.union a usedVars
                    isTreeValid a' t1 && isTreeValid a' t2
                | OrNode(V, t1, t2) -> 
                    let usedVars = usedSampleVariables V
                    Set.isEmpty (Set.intersect a usedVars) && isTreeValid a t1 && isTreeValid a t2
        isTreeValid Set.empty t

    // Computes the max number of fixpoints along each path
    let rec maximalNumberOfFixpoint = function
        | LeafNode _ -> 0
        | FixNode s' -> 1 + maximalNumberOfFixpoint s'
        | ScoreNode(_, s') -> maximalNumberOfFixpoint s'
        | AndNode (_, s1, s2) -> max (maximalNumberOfFixpoint s1) (maximalNumberOfFixpoint s2)
        | OrNode (_, s1, s2) -> max (maximalNumberOfFixpoint s1) (maximalNumberOfFixpoint s2)

    // Folds a function f over a tree by replacing each node n with f(n)
    let rec treeFold (t:AndOrTree) (f:Expr->AndOrTree) = 
        match t with
        | LeafNode V -> f V
        | FixNode (t1) -> FixNode (treeFold t1 f)
        | ScoreNode (V, t1) -> ScoreNode(V, treeFold t1 f)
        | AndNode (V, t1, t2) -> AndNode(V, treeFold t1 f, treeFold t2 f)
        | OrNode (V, t1, t2) -> OrNode(V, treeFold t1 f, treeFold t2 f)

    let mutable sampleVarCount = 0;

    // Compute the symbolic execution tree for a term
    let rec computeAndOrTree = function 
        | Numeral v  -> LeafNode(Numeral v)
        | Variable x -> LeafNode(Variable x)
        | FixDummy -> LeafNode(FixDummy)
        | SampleVariable n -> LeafNode(SampleVariable n)
        | Star -> LeafNode(Star)
        | Abstraction(y, M') -> LeafNode(Abstraction(y, M'))
        | Fixpoint(f, y, M') -> LeafNode(Fixpoint(f, y, M'))
        | Application (M1, M2) -> 
            let aot1 = computeAndOrTree M1
            let aot2 = computeAndOrTree M2
            treeFold aot1 (fun x -> treeFold aot2 (fun y -> 
                match x with 
                | FixDummy -> FixNode (computeAndOrTree (makeBetaStep x y))
                | _ -> computeAndOrTree (makeBetaStep x y)
            ))
        | Score(M1) -> 
            let aot1 = computeAndOrTree M1
            treeFold aot1 (fun x -> ScoreNode(x, LeafNode(x)))
        | Sample -> 
            sampleVarCount <- sampleVarCount + 1; 
            LeafNode(SampleVariable sampleVarCount)
        | PrimF(name, args) ->
            // compute nested TreeFold of a List
            let rec recursiveTreeFold treeList vList =
                match treeList with
                    | [] -> SymPrimF(name, vList |> List.rev |> List.toArray) |> LeafNode
                    | t::tl -> treeFold t (fun x -> recursiveTreeFold tl (x::vList))
           
            let aotTrees = Array.map computeAndOrTree args |> Array.toList
            recursiveTreeFold aotTrees []
        | SymPrimF(name, args) -> LeafNode (SymPrimF(name, args))
        | Conditional(M1, M2, M3) -> 
            let aot1 = computeAndOrTree M1
            let aot2 = computeAndOrTree M2
            let aot3 = computeAndOrTree M3
            treeFold aot1 (fun x -> if containsStar x then AndNode(x, aot2, aot3) else OrNode(x, aot2, aot3))
        
    type EnvStrategy =
        | LeafStrat of Expr
        | FixStrat of EnvStrategy
        | ScoreStrat of Expr*EnvStrategy
        | OrStrat of Expr*EnvStrategy*EnvStrategy
        | AndLeftStrat of EnvStrategy
        | AndRightStrat of EnvStrategy

    
    
    // Compute all strategies in an AndOrTree
    let rec computeAllStrategies (tree: AndOrTree) =
        match tree with
        | LeafNode(M) -> [LeafStrat M]
        | FixNode (t) -> List.map (fun x -> FixStrat(x)) (computeAllStrategies t)
        | ScoreNode (M, t) -> List.map (fun x -> ScoreStrat(M, x)) (computeAllStrategies t)
        | OrNode(M, t1, t2) -> 
            let left = computeAllStrategies t1
            let right = computeAllStrategies t2
            List.collect (fun x -> List.map (fun y -> OrStrat(M, x, y)) right) left
        | AndNode(_, t1, t2) -> 
            let left = computeAllStrategies t1
            let right = computeAllStrategies t2
            List.map (fun x -> AndLeftStrat(x)) left @ List.map (fun x -> AndRightStrat(x)) right

    type Path =
        | LeafPath of Expr
        | FixPath of Path
        | ScorePath of Expr*Path
        | OrLeftPath of Expr*Path
        | OrRightPath of Expr*Path
        | AndLeftPath of Path
        | AndRightPath of Path

    // Compute all paths in a strategy
    let rec computeAllPaths (s:EnvStrategy) : Path list = 
        match s with 
        | LeafStrat M -> [LeafPath M]
        | FixStrat s -> List.map (fun x -> FixPath(x) ) (computeAllPaths s)
        | ScoreStrat (M, s) -> List.map (fun x -> ScorePath(M, x) ) (computeAllPaths s)
        | OrStrat(M, s1, s2) -> 
            List.map (fun x -> OrLeftPath(M, x)) (computeAllPaths s1) @ List.map (fun x -> OrRightPath(M, x)) (computeAllPaths s2)
        | AndLeftStrat(s) -> List.map (fun x-> AndLeftPath(x)) (computeAllPaths s)
        | AndRightStrat(s) -> List.map (fun x-> AndRightPath(x)) (computeAllPaths s)

    // Count the number of fixpoints along a path
    let rec countFixpoints (p:Path) : int =
        match p with
        | LeafPath _ -> 0
        | FixPath p -> 1 + countFixpoints p
        | ScorePath (_, p) -> countFixpoints p
        | OrLeftPath(_, p) -> countFixpoints p
        | OrRightPath(_, p) -> countFixpoints p
        | AndLeftPath p -> countFixpoints p
        | AndRightPath p -> countFixpoints p

    // Checks if the start symbol is used inside a score construct
    let rec isStarInScore (p:Path) =
        match p with
        | LeafPath _ -> false
        | FixPath p -> isStarInScore p
        | ScorePath (M, p) -> containsStar M || isStarInScore p
        | OrLeftPath(_, p) -> isStarInScore p
        | OrRightPath(_, p) -> isStarInScore p
        | AndLeftPath p -> isStarInScore p
        | AndRightPath p -> isStarInScore p