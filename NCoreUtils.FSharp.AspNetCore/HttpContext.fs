namespace NCoreUtils.AspNetCore

open System.Collections.Generic
open Microsoft.AspNetCore.Http
open NCoreUtils

type HttpMethod =
  | HttpGet
  | HttpPost
  | HttpOptions
  | HttpPut
  | HttpDelete
  | HttpHead
  | HttpCustom of Method:CaseInsensitive

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module HttpMethod =

  let private wellKnownMethods =
    Map.ofList
      [ CaseInsensitive "GET",     HttpGet
        CaseInsensitive "POST",    HttpPost
        CaseInsensitive "OPTIONS", HttpOptions
        CaseInsensitive "PUT",     HttpPut
        CaseInsensitive "DELETE",  HttpDelete
        CaseInsensitive "HEAD",    HttpHead ]

  let parse (input : string) =
    let ci = CaseInsensitive input
    match Map.tryFind ci wellKnownMethods with
    | Some m -> m
    | _      -> HttpCustom ci

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module HttpRequest =

  [<CompiledName("GetPath")>]
  let path (request : HttpRequest) =
    match request.Path with
    | path when path.HasValue -> path.Value.Trim('/').Split '/' |> Seq.map CaseInsensitive |> List.ofSeq
    | _                       -> []

  [<CompiledName("GetHttpMethod")>]
  let httpMethod (request : HttpRequest) =
    request.Method |> HttpMethod.parse

  [<CompiledName("TryGetQueryParameters")>]
  let tryQueryParameters (request : HttpRequest) name =
    let mutable values = Unchecked.defaultof<_>
    match request.Query.TryGetValue (name, &values) with
    | true when values.Count > 0 -> values.ToArray () |> List.ofArray |> Some
    | _                          -> None

  [<CompiledName("TryGetQueryParameter")>]
  let tryQueryParameter (request : HttpRequest) name =
    let mutable values = Unchecked.defaultof<_>
    match request.Query.TryGetValue (name, &values) with
    | true when values.Count > 0 -> Some <| values.[0]
    | _                          -> None

  [<CompiledName("GetQueryParameters")>]
  let getQueryParameters (request : HttpRequest) = seq {
    for kv in request.Query do
      let key = kv.Key
      for value in kv.Value do
        yield KeyValuePair (key, value) }

  [<CompiledName("TryGetFormParameters")>]
  let tryFormParameters (request : HttpRequest) name =
    let mutable values = Unchecked.defaultof<_>
    match request.Form.TryGetValue (name, &values) with
    | true when values.Count > 0 -> values.ToArray () |> List.ofArray |> Some
    | _                          -> None

  [<CompiledName("TryGetFormParameter")>]
  let tryFormParameter (request : HttpRequest) name =
    let mutable values = Unchecked.defaultof<_>
    match request.Form.TryGetValue (name, &values) with
    | true when values.Count > 0 -> Some <| values.[0]
    | _                          -> None

  [<CompiledName("GetFormParameters")>]
  let getFormParameters (request : HttpRequest) = seq {
    for kv in request.Form do
      let key = kv.Key
      for value in kv.Value do
        yield KeyValuePair (key, value) }


[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module HttpContext =

  let inline request (httpContext : HttpContext) = httpContext.Request

  let inline path httpContext = request httpContext |> HttpRequest.path

  let inline httpMethod httpContext = request httpContext |> HttpRequest.httpMethod

  let inline tryQueryParameters httpContext = HttpRequest.tryQueryParameters (request httpContext)

  let inline tryQueryParameter httpContext = HttpRequest.tryQueryParameter (request httpContext)

  let getQueryParameters httpContext = request httpContext |> HttpRequest.getQueryParameters

  let inline tryFormParameters httpContext = HttpRequest.tryFormParameters (request httpContext)

  let inline tryFormParameter httpContext = HttpRequest.tryFormParameter (request httpContext)

  let inline getFormParameters httpContext = request httpContext |> HttpRequest.getFormParameters

  let requestServices (httpContext : HttpContext) = httpContext.RequestServices

  let asyncBindAndExecute (httpContext : HttpContext) tryGetParameters action =
    DependencyInjection.asyncBindAndExecuteWith httpContext requestServices tryGetParameters action
