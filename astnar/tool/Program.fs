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
(* Main program stiching together individual operations                                 *)
(*                                                                                      *)
(****************************************************************************************)


open System
open Parser
open Expression
open HMType
open System.IO
open Semantics
open GenerateOutput
open Timing

[<EntryPoint>]
let main argv =

    if Array.length argv = 0 then
        printfn "Please specify an input term"
        exit 0

    let content = argv.[0]
    globalStopwatch.Start()

    try
        // Parse the Term
        let mutable M = parse content

        // Add the fix f x. prefix
        M <- subst M "f" FixDummy
        M <- subst M "x" Star

        try 
            // Check that the term is typeable
            let (a, b) = infer Map.empty M

            let aot = computeAndOrTree M

            //Check sufficent independence
            if not (checkSufficentIndepndence aot) then
                printfn "The computed execution tree is not sufficently independent. The analysis is therefore not possible for this term. "
                exit 0

            try
                // Check if the term is AST
                let (isTerm, finalDist) = isAST aot

                printfn "The computed distribution P_{approx} is the following:"

                for i in 0 .. (finalDist.Length - 1) do
                    printfn "%i: %f" i finalDist.[i]

                globalStopwatch.Stop()
                
                printfn "The entire computation took %i milliseconds" globalStopwatch.ElapsedMilliseconds
                printfn "Of those %i milliseconds were used by the external VINCI tool for volume computation" vinciStopwatch.ElapsedMilliseconds

                if isTerm then
                    printfn "\nThis term is AST!"
                else
                    printfn "We can NOT verify that this term is AST"

                exit 0
            with
                | _ ->
                    printfn "An unpredicted error occured. Please contact the authors."
                    exit -1
        with 
            | _ ->
                // Typing error
                printfn "The provided term is not typeable."
                exit 0
    with
        | _ -> 
            // Error when Parsing
            printfn "The provided term can not be parsed."
            exit 0

    0