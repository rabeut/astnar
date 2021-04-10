(****************************************************************************************)
(*                                                                                      *)
(*                                      CommandLineParser.fs                            *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Parsing of the command line arguments                                                *)
(*                                                                                      *)
(****************************************************************************************)

module CommandLineParser

open System


type CommandLineArgs = 
    {
        Term : String;
        TargetDepth : Option<int>;
        Timeout : Option<int>;
        Precision: Option<int>;
    }


let parseCommand (s:string []) =
    let mutable po =  
        {
            Term = "";
            TargetDepth = None;
            Timeout = None;
            Precision = None;
        }

    let mutable i = 0
    let mutable readTerm = false

    while i <= s.Length - 1 do
        match s.[i] with
            | "-i" -> 
                if readTerm then
                    printfn "Sepcify a term only once"
                    exit -1
                if i = s.Length - 1 then
                    printfn "Option -i must be followed by a second argument"
                    exit -1
                po <- {po with Term = s.[i+1]}
                readTerm <- true
                i <- i + 2
            | "-t" -> 
                if i = s.Length - 1 then
                    printfn "Option -t must be followed by a second argument"
                    exit -1
                po <- {po with Timeout = s.[i+1] |> int |> (fun x -> x * 1000) |> Some}
                i <- i + 2
            | "-d" -> 
                if i = s.Length - 1 then
                    printfn "Option -d must be followed by a second argument"
                    exit -1
                po <- {po with TargetDepth = s.[i+1] |> int |> Some}
                i <- i + 2
            | "-p" -> 
                if i = s.Length - 1 then
                    printfn "Option -p must be followed by a second argument"
                    exit -1
                po <- {po with Precision = s.[i+1] |> int |> Some}
                i <- i + 2
            | _ -> 
                printfn "Unknown commandline argument: %s" s.[i]
                exit -1

    if not readTerm then
        printfn "Must specify a input term"
        exit -1
    
    // Return parsed object
    po