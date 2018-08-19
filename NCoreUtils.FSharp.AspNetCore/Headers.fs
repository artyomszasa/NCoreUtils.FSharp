[<RequireQualifiedAccess>]
module NCoreUtils.AspNetCore.Headers

open System.Collections.Generic
open Microsoft.AspNetCore.Http

[<Literal>]
let Authorization = "Authorization"

[<Literal>]
let LastModified = "LastModified"

[<Literal>]
let IfModifiedSince = "IfModifiedSince"

[<Literal>]
let ContentType = "Content-Type"

[<Literal>]
let Location = "Location"


[<CompiledName("TryGetFirstHeader")>]
let tryGetFirst headerName (headers : IHeaderDictionary) =
  let mutable values = Unchecked.defaultof<_>
  match headers.TryGetValue (headerName, &values) with
  | true when values.Count > 0 -> Some values.[0]
  | _                          -> None

[<CompiledName("TryGetFirstHeaderAsValue")>]
let tryGetFirstValue headerName (headers : IHeaderDictionary) =
  let mutable values = Unchecked.defaultof<_>
  match headers.TryGetValue (headerName, &values) with
  | true when values.Count > 0 -> ValueSome values.[0]
  | _                          -> ValueNone

[<CompiledName("TryGetHeaders")>]
let tryGet headerName (headers : IHeaderDictionary) =
  let mutable values = Unchecked.defaultof<_>
  match headers.TryGetValue (headerName, &values) with
  | true -> values |> Seq.toList
  | _    -> []

let resGetFirst headerName (headers : IHeaderDictionary) =
  let mutable values = Unchecked.defaultof<_>
  match headers.TryGetValue (headerName, &values) with
  | true when values.Count > 0 -> Ok values.[0]
  | _                          -> Error <| sprintf "No header with name \"%s\" was found in header dictionary." headerName

[<CompiledName("TryGetContentType")>]
let tryGetContentType headers = tryGetFirst ContentType headers

[<CompiledName("TryGetContentTypeAsValue")>]
let tryGetContentTypeValue headers = tryGetFirstValue ContentType headers

[<CompiledName("ToSequence")>]
let toSeq (headers : IHeaderDictionary) = seq {
  for kv in headers do
    for v in kv.Value do
      yield KeyValuePair (kv.Key, v) }



