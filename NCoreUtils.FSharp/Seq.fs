namespace NCoreUtils

open System.Collections.Generic
[<RequireQualifiedAccess>]
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
