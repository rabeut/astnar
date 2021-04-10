(****************************************************************************************)
(*                                                                                      *)
(*                                      Parser.fs                                       *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Parser for SPCF                                                                      *)
(*                                                                                      *)
(****************************************************************************************)


module Parser 

    open System
    open PrimitiveFunctions
    open Expression


    type private  Token =   IF | THEN | ELSE | LAMBDA | FIX | DOT | LPAR | RPAR | SAMPLE | SCORE | NUM of Double | ID of String | ADD | SUB | MULT | COLON

    // Lexes a given string into tokens
    let rec private  lexer charSeq = 
        match charSeq with
            | [] -> []
            | 'i'::'f'::lr -> IF::lexer lr
            | 't'::'h'::'e'::'n'::lr -> THEN::lexer lr
            | 'e'::'l'::'s'::'e'::lr -> ELSE::lexer lr 
            | '^'::lr -> LAMBDA::lexer lr
            | 'f'::'i'::'x'::lr -> FIX::lexer lr
            | '('::lr -> LPAR::lexer lr 
            | ')'::lr -> RPAR::lexer lr
            | 's'::'a'::'m'::'p'::'l'::'e'::lr -> SAMPLE::lexer lr 
            | 's'::'c'::'o'::'r'::'e'::lr -> SCORE::lexer lr 
            | '.'::lr -> DOT::lexer lr 
            | ','::lr -> COLON::lexer lr
            | '-'::'-'::lr -> 
                // This is a unary minus
                let (v, lr') = lexerNum [] lr
                NUM (-v) :: lexer lr'
            | c::lr when Char.IsDigit c -> 
                let (v, lr') = lexerNum [] (c::lr)
                NUM (v) :: lexer lr'
            | '+'::lr -> ADD::lexer lr
            | '-'::lr -> SUB::lexer lr // - is only a opertor if followed by a space
            | '*'::lr -> MULT::lexer lr
            | c::lr when Char.IsLetter c -> 
                let (n, lr') = lexerID [] (c::lr)
                ID (n) :: lexer lr'
            | ' '::lr -> lexer lr
            | _ -> failwith "Lexer Error"
    and private  lexerID (a:char list) lr = 
        if List.isEmpty lr || not(Char.IsLetter(List.head lr)) 
            then (a |> List.rev |> String.Concat, lr)
            else lexerID (List.head lr :: a) (List.tail lr)
    and private  lexerNum (a:char list) lr  = 
        match lr with
            | [] -> (a |> List.rev |> String.Concat |> double, [])
            | c::lr' -> if c = '.' || Char.IsNumber c then lexerNum (c::a) lr' else (a |> List.rev |> String.Concat |> double, c::lr')


    let rec private  expr = function
        | IF::tr1 -> 
            match expr tr1 with
                | (e1, THEN::tr2) -> 
                    match expr tr2 with
                        | (e2, ELSE::tr3) -> 
                            let (e3, tr4) = expr tr3
                            (Conditional(e1, e2, e3), tr4)
                        | _ -> failwith "Parsing conditional"
                | _ -> failwith "Parsing conditional"
        | LAMBDA::(ID x)::DOT::tr -> 
            let (e, tr') = expr tr
            (Abstraction(x, e), tr')
        | FIX::(ID x)::(ID y)::DOT::tr -> 
            let (e, tr') = expr tr
            (Fixpoint(x, y, e), tr')
        | s -> aexpr s
    //
    and private  aexpr x = aexpr' (mexpr x)
    //
    and private  aexpr' = function
        | (e, ADD::tr) -> 
            let (e',tr') = mexpr tr 
            aexpr' (PrimF(FunctionSymbol.ADD, [|e; e'|]), tr') // Shorthand notation to allow infix opertor for +, -, *
        | (e, SUB::tr) -> 
            let (e',tr') = mexpr tr 
            aexpr' (PrimF(FunctionSymbol.SUB, [|e; e'|]), tr') // Shorthand notation to allow infix opertor for +, -, *
        | s -> s
    //
    and private  mexpr x = mexpr' (sexpr x)
    //
    and private  mexpr' = function 
        | (e, MULT::tr) ->
            let (e', tr') = sexpr tr
            mexpr'(PrimF(FunctionSymbol.MUL, [|e; e'|]), tr') // Shorthand notation to allow infix opertor for +, -, *
        | s -> s
    //
    and private  sexpr x = sexpr' (pexpr x)
    //
    and private  sexpr' = function
        | (e, (ID x)::tr) -> 
            let (e', tr') = pexpr ((ID x)::tr)
            sexpr' (Application(e, e'), tr')
        | (e, (NUM x)::tr) -> 
            let (e', tr') = pexpr ((NUM x)::tr)
            sexpr' (Application(e, e'), tr')
        | (e, SAMPLE::tr) -> 
            let (e', tr') = pexpr (SAMPLE::tr)
            sexpr' (Application(e, e'), tr')
        | (e, SCORE::tr) -> 
            let (e', tr') = pexpr (SCORE::tr)
            sexpr' (Application(e, e'), tr')
        | (e, LPAR::tr) -> 
            let (e', tr') = pexpr (LPAR::tr)
            sexpr' (Application(e, e'), tr')
        | s -> s 
    //
    and private  pexpr = function
        | (ID x)::tr -> 
            let res = lookupFunctionName x
            match res with
                | None -> (Variable x, tr) // is not a primtive function, hence a varaiable
                | Some f -> 
                    // Is a primtive function, pars all arguments
                    let mutable args = []
                    let mutable mytr = []
                    match tr with
                        | LPAR::tr' -> 
                            mytr <- tr'
                            // Parse all but the last argument as for the last one no colon is needed
                            for _ in [1 .. (f.Arity - 1) ] do
                                match expr mytr with
                                    | (e, COLON::tr'') ->  
                                        args <- e::args
                                        mytr <- tr''
                                    | _ -> failwith "Parser Error"
                            // Parse the last argument and expect the closing RPAR
                            match expr mytr with
                                | (e, RPAR::tr'') -> 
                                    args <- e::args
                                    mytr <- tr''
                                | _ -> failwith "Parser Error"
                        | _ -> failwith "Parser Error"
                    (PrimF(f.Symbol, args |> List.toArray |> Array.rev), mytr)
        | (NUM r)::tr -> (Numeral r, tr)
        | SAMPLE::tr -> (Sample, tr)
        | SCORE::tr ->
            let (e, tr') = expr tr
            (Score(e), tr')
        | LPAR::tr ->
            match expr tr with
                | (e, RPAR::tr') -> (e, tr')
                | _ -> failwith "Parser"
        | _ -> failwith "Parser Error"

    // parses a string into an expression. Throws an exception if parsing fails
    let parse (s:String) = 
        let tokenList = lexer(Seq.toList s)
        let (e, tr) = expr tokenList
        if List.isEmpty tr then e else failwith "Error when parsing"