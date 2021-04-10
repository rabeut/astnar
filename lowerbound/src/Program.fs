(****************************************************************************************)
(*                                                                                      *)
(*                                      Program.fs                                      *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Starting point of lower bound compuation via symbolic exploration                    *)
(*                                                                                      *)
(****************************************************************************************)

open System
open System.IO
open Expression
open Parser
open HMType
open WeightComputation
open System.Collections.Generic
open Rational
open Timing
open CommandLineParser



type Node = 
    {   
        Term : Expr;
        SymbolicConstraints : Constraint;
        Depth : int; 
    }


let targetDepth = 200
let targetError = Rational(bigint(1), bigint(10000))

let computeTermProb (M:Expr) (targetDepth:Option<int>) (targetError: Rational) =
    let q = new Queue<Node>()
    q.Enqueue {Term = M; SymbolicConstraints = []; Depth = 0}

    let mutable vol = Rational 0

    let mutable timingCount = 0
    let mutable exit = false


    let mutable currentDepth = 0
    let mutable count = 0
    let mutable valCount = 0

    // Loop up to target depth
    while not exit && q.Count <> 0 do
        let n = q.Dequeue()
        count <- count + 1

        if(n.Depth > currentDepth) then
            currentDepth <- n.Depth
            

        if (isValue n.Term) then
            let t = optimizedComputeWeight n.SymbolicConstraints targetError
            vol <- vol + t
            valCount <- valCount + 1
        elif targetDepth.IsSome && n.Depth >= targetDepth.Value then
            ()
        else 
            for (N, go) in succ n.Term do
                let n' = 
                    {   Term = N; 
                        SymbolicConstraints =
                            match go with
                                | None -> n.SymbolicConstraints
                                | Some g -> g::n.SymbolicConstraints; 
                        Depth = n.Depth+1; }
                q.Enqueue n'

        if Timing.bound.IsSome then
            if timingCount % 100 = 0 then
                if Timing.globalStopwatch.ElapsedMilliseconds >= Timing.bound.Value then
                    exit <- true    

    printfn "The term was analysed up to evaluation depth: %i" currentDepth
    printfn "Number of explored terms: %i" count
    printfn "Number of explored values: %i" valCount
    vol


let programRun argv = 
    
    // Parse the arguments
    let parsedCmd = parseCommand argv


    // Parse the term in question
    let term = parse parsedCmd.Term


    if isWellTyped term |> not then
        failwith "The provided term can not be typed"

    let stopwatch = System.Diagnostics.Stopwatch()

    // Set global stopwatch to specified time bound
    Timing.bound <- parsedCmd.Timeout |> Option.map (fun x -> int64(x)) 

    // Console Output for user convinience
    if parsedCmd.Timeout.IsNone && parsedCmd.TargetDepth.IsNone then
        printfn "You must specify either a timeout (via - t) or a target depth (via - d) (or both)"
        exit -1
    if parsedCmd.Precision.IsNone then
        printfn "You have not specified a precision. We use the default value of 0.0001"

    // Set target precision
    let targetError = match parsedCmd.Precision with None -> Rational(bigint(1), bigint(10000)) | Some x -> Rational(bigint(1), bigint(x))

    Timing.globalStopwatch.Start()
    stopwatch.Start()

    let res = computeTermProb term parsedCmd.TargetDepth targetError

    stopwatch.Stop()

    printfn "A lower bound on the termination probability is %A / %A" res.Numerator res.Denominator
    printfn "The first 10 digits in decimal are: %s" (res.AsDecimalString 10)
    printfn "The total computation time was %i milliseconds" stopwatch.ElapsedMilliseconds


    0


[<EntryPoint>]
let main argv =
    programRun argv
    
    
