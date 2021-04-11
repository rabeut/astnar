# Lower Bound Computation Via Interval-based Semantics

This folder contains the tool for computing lower bounds on the termination probability of probabilistic programs with continuous distributions.
The algorithm and the relevant theory are presented in Sections 3 and 4 of [1]. The experimental results are presented in the Section 7 [1].


## General

[1] shows that intervals give rise to a sound and complete method to abstract from concrete program traces. This yields a recursion-theoretically optimal procedure to compute termination probabilities. At the same time incremental computations can give arbitrarily tight lower bounds. 
This tool uses an interval-based approach to incrementally analyse the termination probability of arbitrary terms via a combination of stochastic symbolic execution and interval splitting.
As this task is undecidable (even \Pi_0^2-complete) we cannot compute the exact probability of termination. Instead the tool gives lower bounds by stopping the computation at a user defined point. This can be specified by a timeout or a recursion depth. 
Each time the symbolic execution reaches a value we estimate the probability of this path via interval splitting. The tool therefore takes an additional parameter that specifies how exhaustive this path should be analysed. For example, analyzing the path up to a precision of `0.01` means that each terminating path gives a lower bound that is tight up to a factor of `0.01`.
Note that this tool assumes call-by-name evaluation; whereas the other tool, ASTNAR, assumes call-by-value evaluation. 


## Structure of this folder

The structure of the folder is as follows. 
- The `\src` folder contains the source code written in F#, instruction on how to build are given in the "How to Build" section below. 
- The `examples.md` file contains the examples presented in [1] as well as the expected output. 


## How to Build

In order to build this project you require the .NET 5 SDK (tested with Version 5.0.102).
To build the project run `dotnet build -c "Release"` in the same folder the `LowerBound.fsproj` file is located.
The executable `LowerBound` can be found in `./tool/bin/Release/net5`.

The command line arguments for the tool are given in the "Command Line Arguments" section below. 
As an initial example, if we wish to analyse the AST behavior of the term `(fix f x. if sample - 0.5 then x else f x) 0` up to depths 50 with the predefined precision, run `./LowerBound -i "(fix f x. if sample - 0.5 then x else f x) 0" -d 50`. The expected output should be (similar to):

> You have not specified a precision. We use the default value of 0.0001
> The term was analysed up to evaluation depth: 50
> Number of explored terms: 63
> Number of explored values: 12
> A lower bound on the termination probability is 4095 / 4096
> The first 10 digits in decimal are: 0.9997558593
> The total computation time was 131 milliseconds


## Build a Docker Image

We provide a `Dockerfile` used to construct a docker image. To create the docker image first run `dotnet publish -c release` in the `./src` subfolder. Afterwards you can run `docker build -t lowerbound .`.
To use the docker image (i.e., create a docker container running the tool) run `docker run -it lowerbound` directly followed by the arguments for the tool (see below).


## Command Line Arguments

The following command line arguments are available:

- `-i ` specifies the term to be analysed, required. 
- `-t ` specifies a timeout in seconds. After the timeout the currently computed value is returned. 
- `-d ` specifies a target depth. After reaching the target depth the process is aborted and the currently computed lower bound is returned. 
- `-p ` specifies a precision, i.e., `-p x` specifies a precision of `1/x`.


The tool analyses the input term by symbolically executing and using the intervals to compute approximations on the termination probability (see Sections 3 and 4, [1]). 
Each execution path leading to a value is analysed individually. The precision (`-p `) gives the error permitted in that analysis. E.g. `-p 100` would bound the error to `1/100 = 0.01`, i.e., the tool iteratively computes intervals until it obtains an underapproximation on the termination probability (on that particular branch) that is at most `0.01` lower than the actual probability. 
If no precision is specified, we use a default value of `1/10000`, i.e., the same as setting `-p 10000`.
As the lower bound termination is intrinsically non-termination one of the two termination criteria (timeout or target depth) must be set. If both are set the tool stops whenever one of them is reached. 

## SPCF syntax

The syntax of SPCF (the language of study) is comparably simple (see Section 2.2, [1]). The language supports the following features:

- Numerical Constants: `5.3`, `4`,..
- Variables: `x`, `y`,...
- Lambda-Abstraction: `^x. x`,...
- Application: `(^x. x) 4`, ...
- Conditional `if x then y else z`, ...
- Samples: `sample`
- Scores: `score(4)`
- Primitive Function: `add(4, 5)`. The supported functions are `add(x, y)`, `sub(x, y)`, `mul(x, y`, `exp(x)`, `log(x)`, `sig(x)`, `abs(x)`, `max(x, y)`, `min(x, y)`. As syntactic sugar it is also possible to write addition, subtraction and multiplication via `+, -, *` in infix notation. 

The language uses a simple type system as discussed in Section 2.2, [1]. 


## References

[1] Raven Beutner and Luke Ong. 2021. On Probabilistic Termination of Functional Programs with Continuous Distributions. 
In Proceedings of the 42nd ACM SIGPLAN International Conference on Programming Language Design and Implementation (PLDI ’21), June 20–25, 2021