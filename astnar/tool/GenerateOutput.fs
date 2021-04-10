(****************************************************************************************)
(*                                                                                      *)
(*                                      GenerateOutput.fs                               *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Anaylsis of a term by compuation of symbolic tree and reduction to VINCI             *)
(*                                                                                      *)
(****************************************************************************************)

module GenerateOutput

    open System
    open SystemCall
    open Semantics
    open Linear
    open Expression
    open RandomWalk


    /// Computes the weight of a path
    let computeWeightOfPath (p : Path) : double = 
        if isStarInScore p then 0.0 else
            let ineConst = extractLinearInequality p
            //let polyComList = List.map (fun (M, comp) -> (convertToPoly M, comp)) c
            let usedVars = usedVariablesConstraint ineConst

            // Sepcial Care if there are no variables
            if Set.isEmpty usedVars then 
                if checkVaraibleFreeCondList ineConst then 1.0 else 0.0
            else
                computeIneFile(ineConst) |> computeVol   


    let computeDist (s : EnvStrategy) (maxFix:int) =
        let mutable arr = Array.zeroCreate (maxFix + 1)
        for p in computeAllPaths s do
            let weight = computeWeightOfPath p
            let fixNum = countFixpoints p
            arr.[fixNum] <- arr.[fixNum] + weight
        arr

    let computeCUM (d : double []): double [] =
        let length = Array.length d
        let mutable arr = Array.zeroCreate length
        let mutable runningSum = 0.0
        for i in 0..(length-1) do
            runningSum <- runningSum + d.[i]
            arr.[i] <- runningSum
        arr

    // Computes the cum dist
    let computeCUMOfStratgie (s: EnvStrategy) (maxFix: int) =
        let dist = computeDist s maxFix
        computeCUM dist

    

    // Checks if a term is AST
    let isAST (t: AndOrTree) =
        // Compute all startgies
        let startegies = computeAllStrategies t

        //Compute the maximal number of recuivse Calls
        let maxFix = maximalNumberOfFixpoint t

        let cumDist = [| for i in 0 .. maxFix -> 1.0 |]

        for s in startegies do
            let c = computeCUMOfStratgie s maxFix
            for i in 0..maxFix do
                cumDist.[i] <- min cumDist.[i] c.[i]

        let finalDist = [| for i in 0 .. maxFix -> 1.0 |]
        finalDist.[0] <- cumDist.[0]
        for i in 1..maxFix do
            finalDist.[i] <- cumDist.[i] - cumDist.[i-1]


        let mutable totalProb = 0.0
        let mutable expectation = 0.0

        for i in 0..maxFix do
            totalProb <- totalProb + finalDist.[i]
            expectation <- expectation + (double(i) * finalDist.[i])

        let isTerminating = (totalProb = 1.0) && (expectation <= 1.0) && (finalDist.[0] > 0.0)

        (isTerminating, finalDist)

