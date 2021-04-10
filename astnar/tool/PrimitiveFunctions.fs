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
(* Collection of all supported primtive functions                                       *)
(*                                                                                      *)
(****************************************************************************************)


module PrimitiveFunctions

    open System

    type Interval = Double*Double

    type FunctionSymbol = ADD | SUB | MUL | EXP | LOG | SIG | ABS | MAX | MIN

    type PrimtiveFunction = 
        {   
            Name : String;
            Symbol: FunctionSymbol;
            Arity : int;
        }

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
        }


    let Sub = 
        {
            Name = "sub";
            Symbol = SUB;
            Arity = 2;
        }


    let Mul = 
        {
            Name = "mul";
            Symbol = MUL;
            Arity = 2;
        }

    let Exp = 
        {
            Name = "exp";
            Symbol = EXP;
            Arity = 1;
        }

    let Log = 
        {
            Name = "log";
            Symbol = LOG;
            Arity = 1;
        }

    let Sig = 

        {
            Name = "sig";
            Symbol = SIG;
            Arity = 1;
        }

    let Abs = 
        {
            Name = "abs";
            Symbol = ABS;
            Arity = 1;
        }

    let Max = 
        {
            Name = "max";
            Symbol = MAX;
            Arity = 2;
        }

    let Min = 
        {
            Name = "min";
            Symbol = MIN;
            Arity = 2;
        }


    let primFunction = [Add; Sub; Mul; Exp; Log; Sig; Abs; Max; Min]



    // Get the map dictonary, mapping names to primtiveFunctions
    let nameMap = 
        let mutable m = Map.empty
        for f in primFunction do
            m <- m.Add (f.Name, f)
        m


    // Given a Function symbol, returns the corresponding PrimtiveFunction
    let lookupFunctionSymbol (s:FunctionSymbol) = List.find(fun x -> x.Symbol = s) primFunction


    // Checks if there exists a function symbol for a name 
    let lookupFunctionName (name:String) =  Map.tryFind name nameMap
