(****************************************************************************************)
(*                                                                                      *)
(*                                      SystemCall.fs                                   *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Function for interaction with external VINCI tool                                    *)
(*                                                                                      *)
(****************************************************************************************)


module SystemCall
    open Timing
    
    type ProcessResult = { ExitCode : int; Stdout : string; Stderr : string }

    let private executeProcess (exe,cmdline) =
        let psi = System.Diagnostics.ProcessStartInfo(exe,cmdline) 
        psi.UseShellExecute <- false
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        psi.CreateNoWindow <- true        
        let p = System.Diagnostics.Process.Start(psi) 
        let output = System.Text.StringBuilder()
        let error = System.Text.StringBuilder()
        p.OutputDataReceived.Add(fun args -> output.Append(args.Data) |> ignore)
        p.ErrorDataReceived.Add(fun args -> error.Append(args.Data) |> ignore)
        p.BeginErrorReadLine()
        p.BeginOutputReadLine()
        p.WaitForExit()
        { ExitCode = p.ExitCode; Stdout = output.ToString(); Stderr = error.ToString() }

    open System.IO

    let exec (cmd:string) (arg:string) : string =
        executeProcess(cmd, arg).Stdout

    let readLines (filePath:string) = seq {
        use sr = new StreamReader (filePath)
        while not sr.EndOfStream do
            yield sr.ReadLine ()
    }


    // Computes the volume by calling the external tool vinci and parsing its output
    let computeVol (s:string) : double = 
        try
            vinciStopwatch.Start()
            File.WriteAllText("vol.ine", s)
            let out = exec "vinci" "vol"
            vinciStopwatch.Stop()
            System.Double.Parse(out)
        with ex -> 
                printfn "An error occured while performing analysis via vinci"
                printfn "%A" ex
                exit 0