namespace NCoreUtils

open System.Collections.Generic

[<RequireQualifiedAccess>]
[<CompiledName("Seq")>]
module Seq =

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
