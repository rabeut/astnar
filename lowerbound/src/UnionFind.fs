(****************************************************************************************)
(*                                                                                      *)
(*                                      UnionFind.fs                                    *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Minimal Union-find implmentation without path compression                            *)
(*                                                                                      *)
(****************************************************************************************)

module UnionFind

/// A simple Union Find strcture for n elements without path compression
type UnionFind(n:int) =
    let mutable parents = [| for i in 0 .. (n-1) do i |]

    /// Gives the root of the tree containing i
    let rec find (i:int) = 
        if (parents.[i] = i) then 
            i
        else
            find(parents.[i])

    /// Joins i and j
    member this.Union (i, j) =
        let fi = find i
        let fj = find j
        if (fi = fj) then
            ()
        else
            parents.[fi] <- fj

    /// Outputs the current partition as an array of sets
    member this.ExtractPartition () =
        // First, map all roots to a fresh number
        let mutable m = Map.empty
        let mutable count = 0
        for i in 0 .. (n-1) do
            if i = parents.[i] then
                //i is the root of a tree, add to m
                m <- m.Add(i, count)
                count <- count + 1
            else
                ()
        let mutable out = Array.create m.Count Set.empty
        for i in 0 .. (n-1) do
            // Find the root of i
            let pos = Map.find (find i) m
            out.[pos] <- out.[pos].Add(i)
        out