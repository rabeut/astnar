(****************************************************************************************)
(*                                                                                      *)
(*                                      Rational.fs                                     *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Author: Raven Beutner                                                                *)
(*                                                                                      *)
(* Last Changes: 5 April 2021                                                           *)
(*                                                                                      *)
(****************************************************************************************)
(*                                                                                      *)
(* Type of rational numbers and basic opertions on them                                  *)
(*                                                                                      *)
(****************************************************************************************)

module Rational

open System
open System.Numerics


// A type that specifies a Rational Number
type Rational(p: BigInteger, q: BigInteger) =
    let rec gcd a (b: BigInteger) =
        if b.IsZero then a else
            gcd b (a % b)

    let fixSign(p: BigInteger, q: BigInteger) =
        if q.Sign > 0 then p, q else -p, -q

    /// Prints the first count digints of the decimal representation of num/denom without rounding
    /// We assume that 0 <= num/denom <= 1
    let rec prettyPrint (num:BigInteger) (denom: BigInteger) (count: int) =
        // Assume num/denum < 1
        // Compute first digit
        if num.IsZero then
            "0"
        elif count = 0 then
            ""
        else
            let temp = bigint(10) * num
            let r = temp / denom
            // Due to the assumption r \in {0, 1, ..., 9}
            let s = string(r)
            s + prettyPrint(temp % denom) denom (count - 1)

    let p, q =
        if q.IsZero then raise(System.DivideByZeroException())
        let g = gcd q p
        fixSign(p/g, q/g)

    new (i:int) =
        Rational(bigint(i), bigint(1))


    /// Prints the first digits digints of the decimal representation of num/denom without rounding
    /// Assumes that 0 <= num/denom <= 1
    member __.AsDecimalString (digits:int) = 
        if p > q || p.Sign * q.Sign = -1 then
            failwith "Only for arguments between 0 and 1"
        elif p = q then
            "1"
        else
            "0." + prettyPrint p q digits
    

    member __.Numerator = p
    member __.Denominator = q

    member __.AsDouble() : Double = double(p) / double(q)

    override __.ToString() =
        if q.IsOne then p.ToString() else sprintf "%A/%A" p q

    static member (+) (m: Rational, n: Rational) =
        Rational(m.Numerator*n.Denominator + n.Numerator*m.Denominator, m.Denominator*n.Denominator)

    static member (-) (m: Rational, n: Rational) =
        Rational(m.Numerator*n.Denominator - n.Numerator*m.Denominator, m.Denominator*n.Denominator)

    static member (*) (m: Rational, n: Rational) =
        Rational(m.Numerator*n.Numerator, m.Denominator*n.Denominator)

    static member (/) (m: Rational, n: Rational) =
        Rational(m.Numerator*n.Denominator, m.Denominator*n.Numerator)


    interface IComparable<Rational> with
        member this.CompareTo other =
            // Compute the nomilator if brought to the same denominator
            let a = this.Numerator * other.Denominator
            let b = other.Numerator * this.Denominator
            if a = b then 0 elif a < b then -1 else 1

    interface IComparable with
        member this.CompareTo obj =
            match obj with
              | null                 -> 1
              | :? Rational as other -> (this :> IComparable<_>).CompareTo other
              | _                    -> invalidArg "obj" "not a Category"

    interface IEquatable<Rational> with
        member this.Equals other =
            this.Numerator = other.Numerator && this.Denominator = other.Denominator

    override this.Equals obj =
        match obj with
          | :? Rational as other -> (this :> IEquatable<_>).Equals other
          | _                    -> false

    override this.GetHashCode () =
        int(this.Numerator + this.Denominator)