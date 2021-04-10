(****************************************************************************************)
(*                                                                                      *)
(*                                      PrimitiveFunctions.fs                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Collection of supported primtive functions in SPCF                                   *)
(*                                                                                      *)
(****************************************************************************************)

module PrimitiveFunctions

open System

type Interval = Double*Double

type FunctionSymbol = ADD | SUB | MUL | EXP | LOG | SIG | ABS | MAX | MIN

/// Type of the representation of a primtive Function
type PrimtiveFunction = 
    {   
        // The name of the Function as it is used in terms
        Name : String;

        // The Function Symbol, used to idetfy the primitiveFunction via an integer
        Symbol: FunctionSymbol;

        Arity : int;

        // Computes the pointwise value of the function
        Compute : array<Double> -> Double;

        // Computes the image under intervals 
        ComputeInterval : array<Interval> -> Interval;
    }

/// A symbolic Value of type Real
type SymbolicRealValue = 
    | V of int // Variable
    | C of Double // Constant
    | F of FunctionSymbol * (array<SymbolicRealValue>) // Symolic Function application

let rec usedVariables = function
    | V n -> Set.singleton n
    | C _ -> Set.empty
    | F(_, args) -> Set.unionMany (Array.map usedVariables args)



let Add = 
    {
        Name = "add";
        Symbol = ADD;
        Arity = 2;
        Compute = fun args -> args.[0] + args.[1];
        ComputeInterval = fun args ->
            let (a, b) = args.[0]
            let (c, d) = args.[1]
            (a+c, b+d)
    }


let Sub = 
    {
        Name = "sub";
        Symbol = SUB;
        Arity = 2;
        Compute = fun args -> args.[0] - args.[1];
        ComputeInterval = fun args ->
            let (a, b) = args.[0]
            let (c, d) = args.[1]
            (a-d, b-c)
    }


let Mul = 
    {
        Name = "mul";
        Symbol = MUL;
        Arity = 2;
        Compute = fun args -> args.[0] * args.[1];
        ComputeInterval = fun args ->
            let (a, b) = args.[0]
            let (c, d) = args.[1]
            let t = [|a*c; a*d; b*c; b*d|]
            (Array.min t, Array.max t)
    }

let Exp = 
    {
        Name = "exp";
        Symbol = EXP;
        Arity = 1;
        Compute = fun args -> Math.Exp(args.[0]);
        ComputeInterval = fun args ->
            let (a, b) = args.[0]
            (Math.Exp(a), Math.Exp(b))
    }

let Log = 
    {
        Name = "log";
        Symbol = LOG;
        Arity = 1;
        Compute = fun args -> Math.Log(args.[0]);
        ComputeInterval = fun args ->
            let (a, b) = args.[0]
            (Math.Log(a), Math.Log(b))
    }



let Sig = 
    let sigmoid x = 1.0 / (1.0 + Math.Exp(x))
    {
        Name = "sig";
        Symbol = SIG;
        Arity = 1;
        Compute = fun args -> sigmoid(args.[0]);
        ComputeInterval = fun args ->
            let (a, b) = args.[0]
            (sigmoid(a), sigmoid(b))
    }

let Abs = 
    {
        Name = "abs";
        Symbol = ABS;
        Arity = 1;
        Compute = fun args -> Math.Abs(args.[0]);
        ComputeInterval = fun args ->
            let (a, b) = args.[0]
            let absA = Math.Abs(a)
            let absB = Math.Abs(b)
            (Math.Min(absA, absB), Math.Max(absA, absB))
    }

let Max = 
    {
        Name = "max";
        Symbol = MAX;
        Arity = 2;
        Compute = fun args -> Math.Max(args.[0], args.[1]);
        ComputeInterval = fun args ->
            let (a, b) = args.[0]
            let (c, d) = args.[1]
            (Math.Max(a, c), Math.Max(b, d))
    }

let Min = 
    {
        Name = "min";
        Symbol = MIN;
        Arity = 2;
        Compute = fun args -> Math.Min(args.[0], args.[1]);
        ComputeInterval = fun args ->
            let (a, b) = args.[0]
            let (c, d) = args.[1]
            (Math.Min(a, c), Math.Min(b, d))
    }

/// All primtive Functions used. To add support for a new function add it to tis list
let private primFunction = [Add; Sub; Mul; Exp; Log; Sig; Abs; Max; Min]

 
let private nameMap = 
    let mutable m = Map.empty
    for f in primFunction do
        m <- m.Add (f.Name, f)
    m


/// For a given Function symbol, look up the Function
let lookupFunctionSymbol (s:FunctionSymbol) = List.find(fun x -> x.Symbol = s) primFunction



/// For a given string, look up the Function. Used by the Parser
let lookupFunctionName (name:String) =  Map.tryFind name nameMap
