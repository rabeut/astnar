# AST Verification for Non-Affine Recursive Programs (ASTNAR)


This folder contains the ASTNAR (Almost Sure Termination of Non Affine Recursive programs) tool for verifying almost sure termination (AST) of non-affine recursive programs. 
The algorithm and the relevant theory are presented in Sections 5 and 6 of [1].


## General

The theoretical results presented in [1] (Section 5) give a sound (but incomplete) method for showing a program to be AST. In Sections 6 and 7 in [1] we show how this can be turned into a practical algorithm.
The goal is to verify a fixpoint abstraction to be AST on every possible input.  The only argument passed to our tool is therefore the fixpoint term in question. 
For convenience the term is passed without the leading fixpoint abstraction, with the name of the recursive abstraction fixed to `f` and the variable fixed to `x`.
As a running example we consider the fixpoint `fix f x. if sample - 0.5 then x else f x`. 
The tool analyses the term by counting the number of recursive calls along each branch and computes the probability of individual branches by computing the volume of a convex polytope using the external tool vinci (http://www.multiprecision.org/vinci/home.html). 
A short introduction to the syntax is given below. We also recommend a look at the examples in the `examples.md` file. 


## Structure of this folder

- The source code for the external volume computation vinci is located in the `./vinci` subfolder. The main tool written in F# is located in the `./tool` folder. Instruction on how to build the tool are given below in the "How to Build Section".

- The `examples.md` gives some possible input terms that correspond to the examples given in the experimental evaluation in [1]. 



## How to Build

The tool is comprised of two parts. The Vinci backend written in C and a main tool written in F#. 
In order to build this project you require the .NET 5 SDK (tested with Version 5.0.102), a C/C++ Complier (tested with gcc Version 9.3.0) and make (tested with GNU Make 4.2.1). 
Both tools are build separately and he executables should be moved to the same folder.

### Step 1 - Build Vinci

The first step is to build Vinci. 
The source code and make file is located in `./vinci`. To build the tool run `make` in this subdirectory. 
The build executable `vinci` will be needed later.

### Step 2 - Build the AST Tool

In the second step we build the tool located in `./tool`. 
Run `dotnet build -c "Release"` in the `./tool` subfolder to build the F# project. 
The executable `ASTNAR` can be found in `./tool/bin/Release/net5`. 
In order to run this tool the vinci tool build in the first step must be in the same directory. 
The simplest way is to simply copy the `vinci` file into `./tool/bin/Release/net5`.

On details of how how to run the tool, see "Using the Tool" below.


## Build a Docker Image

We also provide a dockerfile `Dockerfile` which can be used to construct a docker image. To construct a docker image simply run `docker build -t astnar .` in the same directory as the dockerfile.
The docker images can the be used by running `docker run -it astnar` followed by the augment for the tool (described below).


## Using the Tool

The only command line argument required by the tool is the fixpoint term in question.
As discussed before, we omit the leading fixpoint construct and fix the name of the recursive abstraction to `f` and of the argument to `x`.
We assume the astnar executable `ASTNAR` is build (and the vinci executable is located in the same folder). Note that the executable build by `dotnet` refers to multiple files contained in the same directory (`./tool/bin/Release/net5`). To move the tool to some other location you therefore need to copy the entire folder.

If we wish to analyse the AST beWhavior of the term `fix f x. if sample - 0.5 then x else f x` we run `./ASTNAR "if sample - 0.5 then x else f x"`. The expected output should (similar to) be:

> The computed distribution P_{approx} is the following:
> 0: 0.500000
> 1: 0.500000
> The entire computation took 235 milliseconds
> Of those 45 milliseconds were used by the external VINCI tool for volume computation
> This term is AST!

Note that we omitted the initial fixpoint abstraction.
The syntax of SPCF (the language of study) is given below.
We recommend to take a look at the example terms given in `examples.md`.


## SPCF syntax

The syntax of SPCF (the language of study) is comparably simple (see Section 2.2, [1]). The language supports the following features:

- Numerical Constants: `5.3`, `4`,...
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
