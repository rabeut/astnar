# Example Inputs for ASTNAR

We provide the tool input for each of the examples presented in the evaluation (Section F.2, [1]). We also give the expected output of the tool.
For terms of interest we also give the bounds on the parameter p (see the [1] for the original terms) needed to ensure that the program is AST.

## geo(1/2):

"if sample - 0.5 then x else f (x+1)"


> The computed distribution P_{approx} is the following:
>
> 0: 0.500000
>
> 1: 0.500000
>
> The entire computation took 221 milliseconds
>
> Of those 43 milliseconds were used by the external VINCI tool for volume computation
>
> 
>
> This term is AST!


## Ex 1.1, p = 1/2

This term is AST whenever p >= 0.5

"if sample - 0.5 then x else f (f (x+1))"


> The computed distribution P_{approx} is the following:
>
> 0: 0.500000
>
> 1: 0.000000
>
> 2: 0.500000
>
> The entire computation took 225 milliseconds
>
> Of those 47 milliseconds were used by the external VINCI tool for volume computation

> 

> This term is AST!


## 3print(2/3)

This term is AST whenever p >= 2/3

"if sample - 0.667 then x else f(f (f x))"


> The computed distribution P_{approx} is the following:
>
> 0: 0.667000
>
> 2: 0.000000
>
> 3: 0.333000
>
> The entire computation took 251 milliseconds
>
> Of those 51 milliseconds were used by the external VINCI tool for volume computation
> 
> This term is AST!


## Ex 5.1, p = 0.6

This term is AST whenever p >= 0.6


"if sample - 0.6 then x else ( if sample - sig(x) then (if sample - 0.5 then f(f(f (x+1))) else  f(f(x+1)) ) else f(f(x+1)) )"


> The computed distribution P_{approx} is the following:
>
> 0: 0.600000
>
> 1: 0.000000
>
> 2: 0.200000
>
> 3: 0.200000
>
> The entire computation took 239 milliseconds
>
> Of those 51 milliseconds were used by the external VINCI tool for volume computation
> 
> This term is AST!


## Ex E 2:


"if sample - sig(x) then (if sample - 0.5 then score(0.3) else f x) else ((^p. if sample - p then 0 else (if sample - (x + p) then f x else f(f x)) ) sample)"



> The computed distribution P_{approx} is the following:
>
> 0: 0.500000
>
> 1: 0.000000
>
> 2: 0.500000
>
> The entire computation took 244 milliseconds
>
> Of those 53 milliseconds were used by the external VINCI tool for volume computation
> 
> This term is AST!


## Ex. 5.15, p = 0.65

This term is AST whenever p >= sqrt(7) - 2


"(^e. if e - 0.65 then x else (if sample - sig(x) then (if sample - e then f(f(f(x+1))) else f(f(x+1)) )   else f(f (x+1)) )) sample"



> The computed distribution P_{approx} is the following:
>
> 0: 0.650000
>
> 1: 0.000000
>
> 2: 0.061250
>
> 3: 0.288750
>
> The entire computation took 241 milliseconds
>
> Of those 50 milliseconds were used by the external VINCI tool for volume computation
> 
> This term is AST!

## References

[1] Raven Beutner and Luke Ong. 2021. On Probabilistic Termination of Functional Programs with Continuous Distributions. 
In Proceedings of the 42nd ACM SIGPLAN International Conference on Programming Language Design and Implementation (PLDI ’21), June 20–25, 2021