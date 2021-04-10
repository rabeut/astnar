(****************************************************************************************)
(*                                                                                      *)
(*                                      Timing.fs                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Global variable to keep track of timing constrints for specified timeouts            *)
(*                                                                                      *)
(****************************************************************************************)

module Timing

let globalStopwatch = System.Diagnostics.Stopwatch()

let mutable bound : Option<int64> = None