(****************************************************************************************)
(*                                                                                      *)
(*                                      RandomWalk.fs                                   *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* RandomWalks and termination analysis                                                 *)
(*                                                                                      *)
(****************************************************************************************)


module RandomWalk
    
    type RandomWalk = Map<int, double>

    // Check if a RW is terminting by computing the expectation
    let isRWterminating (walk : RandomWalk) =
        let totalProb = Map.fold (fun p _ v -> p + v) 0.0 walk
        let expectation = Map.fold (fun p i v -> p + (double(i) * v)) 0.0 walk
        (totalProb = 1.0) && (expectation <= 1.0) && (match walk.TryFind 0 with Some x -> x > 0.0 | None -> false)

    let printRW (w : RandomWalk) = 
        for i in w |> Map.toSeq |> Seq.map fst do
            let p = match w.TryFind i with Some x -> x | None -> 0.0
            if p > 0.0 then printfn "%i: %f " i p else ()



