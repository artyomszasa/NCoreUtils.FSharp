namespace NCoreUtils

open System
open System.Collections
open System.Collections.Generic

[<RequireQualifiedAccess>]
[<CompiledName("Seq")>]
module Seq =

  [<Sealed>]
  type private ChoosingEnumerator<'a, 'b> (source : IEnumerator<'a>, chooser : 'a -> 'b voption) =
    let mutable current = Unchecked.defaultof<_>
    interface IEnumerator with
      member __.Current = box current
      member __.MoveNext () =
        let mutable found = false
        while not found && source.MoveNext () do
          match chooser source.Current with
          | ValueSome value ->
            current <- value
            found   <- true
          | _ -> ()
        if not found then
          current <- Unchecked.defaultof<_>
        found
      member __.Reset () = source.Reset ()
    interface IEnumerator<'b> with
      member __.Current = current
      member __.Dispose () = source.Dispose ()

  [<Sealed>]
  type private ChoosingEnumerable<'a, 'b> (source : IEnumerable<'a>, chooser) =
    member private __.GetEnumerator () = new ChoosingEnumerator<_,_> (source.GetEnumerator (), chooser)
    interface IEnumerable with
      member this.GetEnumerator () = this.GetEnumerator () :> _
    interface IEnumerable<'b> with
      member this.GetEnumerator () = this.GetEnumerator () :> _

  [<Sealed>]
  type UnfoldingEnumerator<'state, 'a> (initialState : 'state, generator) =
    let mutable state = initialState
    let mutable current = Unchecked.defaultof<_>
    interface IEnumerator with
      member __.Current = box current

      member __.MoveNext () =
        match generator state with
        | ValueSome struct (item, state') ->
          state <- state'
          current <- item
          true
        | _ ->
          state <- Unchecked.defaultof<_>
          current <- Unchecked.defaultof<_>
          false
      member __.Reset () = NotSupportedException () |> raise
    interface IEnumerator<'a> with
      member __.Current = current
      member __.Dispose () = ()

  [<Sealed>]
  type private UnfoldingEnumerable<'state, 'a> (initialState : 'state, generator) =
    member private __.GetEnumerator () = new UnfoldingEnumerator<_,_> (initialState, generator)
    interface IEnumerable with
      member this.GetEnumerator () = this.GetEnumerator () :> _
    interface IEnumerable<'a> with
      member this.GetEnumerator () = this.GetEnumerator () :> _


  [<CompiledName("ChooseAsValue")>]
  let chooseValue chooser (seq : seq<_>) = new ChoosingEnumerable<_, _> (seq, chooser) :> seq<_>

  [<CompiledName("PickAsValue")>]
  let pickValue picker (seq : seq<_>) =
    let rec pick (enumerator : IEnumerator<_>) =
      match enumerator.MoveNext () with
      | true ->
        match picker enumerator.Current with
        | ValueSome _ as result -> result
        | _                     -> pick enumerator
      | _ -> ValueNone
    use enumerator = seq.GetEnumerator ()
    pick enumerator

  [<CompiledName("UnfoldAsValue")>]
  let unfoldValue generator state = new UnfoldingEnumerable<_,_> (state, generator) :> seq<_>


  [<CompiledName("MapToArray")>]
  let mapToArray mapper (seq : seq<_>) =
    match seq with
    | :? (_[])                  as array -> Array.map mapper array
    | :? IReadOnlyList<_>       as list  -> Array.init list.Count (fun i -> mapper list.[i])
    | :? IList<_>               as list  -> Array.init list.Count (fun i -> mapper list.[i])
    | :? IReadOnlyCollection<_> as collection ->
      let result = Array.zeroCreate collection.Count
      Seq.iteri (fun i item -> result.[i] <- mapper item) collection
      result
    | :? ICollection<_> as collection ->
      let result = Array.zeroCreate collection.Count
      Seq.iteri (fun i item -> result.[i] <- mapper item) collection
      result
    | _ -> Seq.map mapper seq |> Seq.toArray

  [<CompiledName("TryMin")>]
  let tryMin (seq : seq<_>) =
    use enumerator = seq.GetEnumerator ()
    let mutable found = false
    let mutable value = Unchecked.defaultof<_>
    while enumerator.MoveNext () do
      let current = enumerator.Current
      match not found || value > current with
      | true ->
        value <- current
        found <- true
      | _ -> ()
    match found with
    | true -> Some value
    | _    -> None

  [<CompiledName("TryMinBy")>]
  let tryMinBy selector (seq : seq<_>) =
    use enumerator = seq.GetEnumerator ()
    let mutable found = false
    let mutable key = Unchecked.defaultof<_>
    let mutable value = Unchecked.defaultof<_>
    while enumerator.MoveNext () do
      let current = enumerator.Current
      let currentKey = selector current
      match not found || key > currentKey with
      | true ->
        value <- current
        key   <- currentKey
        found <- true
      | _ -> ()
    match found with
    | true -> Some value
    | _    -> None

  [<CompiledName("TryMax")>]
  let tryMax (seq : seq<_>) =
    use enumerator = seq.GetEnumerator ()
    let mutable found = false
    let mutable value = Unchecked.defaultof<_>
    while enumerator.MoveNext () do
      let current = enumerator.Current
      match not found || value < current with
      | true ->
        value <- current
        found <- true
      | _ -> ()
    match found with
    | true -> Some value
    | _    -> None

  [<CompiledName("TryMaxBy")>]
  let tryMaxBy selector (seq : seq<_>) =
    use enumerator = seq.GetEnumerator ()
    let mutable found = false
    let mutable key = Unchecked.defaultof<_>
    let mutable value = Unchecked.defaultof<_>
    while enumerator.MoveNext () do
      let current = enumerator.Current
      let currentKey = selector current
      match not found || key < currentKey with
      | true ->
        value <- current
        key   <- currentKey
        found <- true
      | _ -> ()
    match found with
    | true -> Some value
    | _    -> None

  [<CompiledName("TryMinAsValue")>]
  let tryMinValue (seq : seq<_>) =
    use enumerator = seq.GetEnumerator ()
    let mutable found = false
    let mutable value = Unchecked.defaultof<_>
    while enumerator.MoveNext () do
      let current = enumerator.Current
      match not found || value > current with
      | true ->
        value <- current
        found <- true
      | _ -> ()
    match found with
    | true -> ValueSome value
    | _    -> ValueNone

  [<CompiledName("TryMinAsValueBy")>]
  let tryMinValueBy selector (seq : seq<_>) =
    use enumerator = seq.GetEnumerator ()
    let mutable found = false
    let mutable key = Unchecked.defaultof<_>
    let mutable value = Unchecked.defaultof<_>
    while enumerator.MoveNext () do
      let current = enumerator.Current
      let currentKey = selector current
      match not found || key > currentKey with
      | true ->
        value <- current
        key   <- currentKey
        found <- true
      | _ -> ()
    match found with
    | true -> ValueSome value
    | _    -> ValueNone

  [<CompiledName("TryMaxAsValue")>]
  let tryMaxValue (seq : seq<_>) =
    use enumerator = seq.GetEnumerator ()
    let mutable found = false
    let mutable value = Unchecked.defaultof<_>
    while enumerator.MoveNext () do
      let current = enumerator.Current
      match not found || value < current with
      | true ->
        value <- current
        found <- true
      | _ -> ()
    match found with
    | true -> ValueSome value
    | _    -> ValueNone

  [<CompiledName("TryMaxAsValueBy")>]
  let tryMaxValueBy selector (seq : seq<_>) =
    use enumerator = seq.GetEnumerator ()
    let mutable found = false
    let mutable key = Unchecked.defaultof<_>
    let mutable value = Unchecked.defaultof<_>
    while enumerator.MoveNext () do
      let current = enumerator.Current
      let currentKey = selector current
      match not found || key < currentKey with
      | true ->
        value <- current
        key   <- currentKey
        found <- true
      | _ -> ()
    match found with
    | true -> ValueSome value
    | _    -> ValueNone

