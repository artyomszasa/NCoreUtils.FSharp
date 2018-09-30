module Tests.Seq

open System
open Xunit
open NCoreUtils

let private rand = new System.Random()

let private swap (a: _[]) x y =
  let tmp = a.[x]
  a.[x] <- a.[y]
  a.[y] <- tmp

// shuffle an array (in-place)
let private shuffle a =
  let a' = Array.copy a
  Array.iteri (fun i _ -> swap a i (rand.Next(i, Array.length a'))) a'
  a'

[<Fact>]
let ``min/max`` () =
  let input = [| 1 .. 30 |]
  let rand = shuffle input
  Assert.Equal (None, Seq.tryMin [])
  Assert.Equal (None, Seq.tryMax [])
  Assert.Equal (Some 1, Seq.tryMin input)
  Assert.Equal (Some 1, Seq.tryMin rand)
  Assert.Equal (Some 30, Seq.tryMax input)
  Assert.Equal (Some 30, Seq.tryMax rand)
  Assert.Equal (ValueNone, Seq.tryMinValue [])
  Assert.Equal (ValueNone, Seq.tryMaxValue [])
  Assert.Equal (ValueSome 1, Seq.tryMinValue input)
  Assert.Equal (ValueSome 1, Seq.tryMinValue rand)
  Assert.Equal (ValueSome 30, Seq.tryMaxValue input)
  Assert.Equal (ValueSome 30, Seq.tryMaxValue rand)

[<Fact>]
let ``min/max with selector`` () =
  let input = [| 1 .. 30 |] |> Array.map ref
  let rand = shuffle input
  let selector (x : _ ref) = x.Value
  Assert.Equal (None, Seq.tryMinBy selector [])
  Assert.Equal (None, Seq.tryMaxBy selector [])
  Assert.Equal (Some (ref 1), Seq.tryMinBy selector input)
  Assert.Equal (Some (ref 1), Seq.tryMinBy selector rand)
  Assert.Equal (Some (ref 30), Seq.tryMaxBy selector input)
  Assert.Equal (Some (ref 30), Seq.tryMaxBy selector rand)
  Assert.Equal (ValueNone, Seq.tryMinValueBy selector [])
  Assert.Equal (ValueNone, Seq.tryMaxValueBy selector [])
  Assert.Equal (ValueSome (ref 1), Seq.tryMinValueBy selector input)
  Assert.Equal (ValueSome (ref 1), Seq.tryMinValueBy selector rand)
  Assert.Equal (ValueSome (ref 30), Seq.tryMaxValueBy selector input)
  Assert.Equal (ValueSome (ref 30), Seq.tryMaxValueBy selector rand)
