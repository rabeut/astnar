# Lower Bound Computation and AST Verification of Probabilistic Programs with Continuous Distributions

This folder contains prototype implementations for computing lower bounds on the probability of termination and for verification of almost-sure termination (AST) of probabilistic functional programs with continuous distributions as presented in [1].

This folder contains two tools:
- LowerBound, which computes lower bounds on the termination probability of probabilistic programs (see Sections 3 and 4, [1]), located in the `./lowerbound` subfolder.
- ASTNAR, which verifies Almost Sure Termination of Non-Affine Recursive probabilistic programs (see Sections 5 and 6, [1]), located in the `./astnar` subfolder.

In each subfolder `./lowerbound` and `./astnar` a separate `README.md` is located with detailed descriptions of how to build and run the tool.
Each folder also contains an `examples.md` file, containing example input terms used in the experimental evaluation (Section 7, [1]). 



## References

[1] Raven Beutner and Luke Ong. 2021. On Probabilistic Termination of Functional Programs with Continuous Distributions. 
In Proceedings of the 42nd ACM SIGPLAN International Conference on Programming Language Design and Implementation (PLDI ’21), June 20–25, 2021


