(****************************************************************************************)
(*                                                                                      *)
(*                                      Timing.fs                                       *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* globale variables used to track the time consumption of the computation              *)
(*                                                                                      *)
(****************************************************************************************)


module Timing

    // Used to compute the overall computation time

    let globalStopwatch = System.Diagnostics.Stopwatch()

    let vinciStopwatch = System.Diagnostics.Stopwatch()