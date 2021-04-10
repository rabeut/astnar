# Example Inputs for Lower Bound Computation

In this file we provide the examples presented in the evaluation (Section 7, [1]) by giving the input to the tool as well as the expected output. 

## geo(1/2):

-i "(fix f x. if sample - 0.5 then x else f (x+1) ) 0 " -d 100

Expected output:

> You have not specified a precision. We use the default value of 0.0001
>
> The term was analysed up to evaluation depth: 100
>
> Number of explored terms: 356
>
> Number of explored values: 20
>
> A lower bound on the termination probability is 1048575 / 1048576
>
> The first 10 digits in decimal are: 0.9999990463
>
> The total computation time was 129 milliseconds


## geo(1/5):

-i "(fix f x. if sample - 0.2 then x else f (x+1) ) 0 " -d 200


> You have not specified a precision. We use the default value of 0.0001
>
> The term was analysed up to evaluation depth: 200
>
> Number of explored terms: 1211
>
> Number of explored values: 40
>
> A lower bound on the termination probability is 943077396651710339189234999743967894114250965653705801049970303643782271687794055133445162966105465448610324790771094352454460862575411102612413734627002210763067825825 / 943490606205385338060388645247067222729230305104110107094051575061406040598037213021531681294414691885367093757690961224942646157481198158140358562858174010912348831744
>
> The first 10 digits in decimal are: 0.9995620416
>
> The total computation time was 207 milliseconds


## 1dRW(1/2, 1):

-i "(fix f x. if x then 0 else ( if sample - 0.5 then f(x-1) else f (x+1) ) ) 1" -d 200

> You have not specified a precision. We use the default value of 0.0001
>
> The term was analysed up to evaluation depth: 200
>
> Number of explored terms: 1376252
>
> Number of explored values: 65535
>
> A lower bound on the termination probability is 26333 / 32768
>
> The first 10 digits in decimal are: 0.8036193847
>
> The total computation time was 26475 milliseconds

## 1dRW(7/10, 1):

-i "(fix f x. if x then 0 else ( if sample - 0.7 then f(x-1) else f (x+1) ) ) 1" -d 150

> You have not specified a precision. We use the default value of 0.0001
>
> The term was analysed up to evaluation depth: 150
>
> Number of explored terms: 204796
>
> Number of explored values: 8191
>
> A lower bound on the termination probability is 2709809335420691097428727812250347524454759 / 2787593149816327892691964784081045188247552
>
> The first 10 digits in decimal are: 0.9720964250
>
> The total computation time was 9334 milliseconds


## gr:

-i "(fix f x. if sample - 0.5 then x else f(f (f x))) 0" -d 80

> You have not specified a precision. We use the default value of 0.0001
>
> The term was analysed up to evaluation depth: 80
>
> Number of explored terms: 2046981
>
> Number of explored values: 1773
>
> A lower bound on the termination probability is 80119 / 131072
>
> The first 10 digits in decimal are: 0.6112594604
>
> The total computation time was 4551 milliseconds

## print(1/2):

-i "(fix f x. if sample - 0.5 then x else f(f x) ) 1" -d 90

> You have not specified a precision. We use the default value of 0.0001
>
> The term was analysed up to evaluation depth: 90
>
> Number of explored terms: 5056590
>
> Number of explored values: 23714
>
> A lower bound on the termination probability is 436109 / 524288
>
> The first 10 digits in decimal are: 0.8318119049
>
> The total computation time was 14800 milliseconds

## 3print(3/4):

-i "(fix f x. if sample - 0.75 then x else f(f (f x))) 0" -d 80

> You have not specified a precision. We use the default value of 0.0001
>
> The term was analysed up to evaluation depth: 80
>
> Number of explored terms: 2046981
>
> Number of explored values: 1773
>
> A lower bound on the termination probability is 66016437231 / 68719476736
>
> The first 10 digits in decimal are: 0.9606655982
>
> The total computation time was 4340 milliseconds


## bin(1/2, 2):

-i "(fix f x. if x then 0 else ( if sample - 0.5 then f(x-1) else f x ) ) 2" -d 100

> You have not specified a precision. We use the default value of 0.0001
>
> The term was analysed up to evaluation depth: 100
>
> Number of explored terms: 118907
>
> Number of explored values: 9445
>
> A lower bound on the termination probability is 524209 / 524288
>
> The first 10 digits in decimal are: 0.9998493194
>
> The total computation time was 2418 milliseconds



## pedestrian:
-i "(fix f x. if x then 0 else ( (^s. s + (if sample - 0.7 then f(x-s) else f (x+s)) ) sample ) ) sample" -d 40 -p 200

> The term was analysed up to evaluation depth: 40
>
> Number of explored terms: 197
>
> Number of explored values: 7
>
> A lower bound on the termination probability is 5413201134907631 / 9007199254740992
>
> The first 10 digits in decimal are: 0.6009860536
>
> The total computation time was 4802 milliseconds


## References

[1] Raven Beutner and Luke Ong. 2021. On Probabilistic Termination of Functional Programs with Continuous Distributions. 
In Proceedings of the 42nd ACM SIGPLAN International Conference on Programming Language Design and Implementation (PLDI ’21), June 20–25, 2021