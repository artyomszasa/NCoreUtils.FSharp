#nowarn "44"
namespace NCoreUtils.AspNetCore

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open NCoreUtils
open System.Text
open Newtonsoft.Json

[<AutoOpen>]
module Helpers =

  let private utf8 = UTF8Encoding false

  /// <summary>
  /// Sets <c>Content-Type</c> header of the response to <c>application/json</c> a writes provided object using
  /// <c>System.Text.Json</c> based asynchronous serialization to the response stream.
  /// </summary>
  /// <param name="httpContext">Context to use.</param>
  /// <param name="options">Serialization options.</param>
  /// <param name="obj">Object to serialize and write to the response.</param>
  [<CompiledName("AsyncJson")>]
  let asyncJsonWith (httpContext: HttpContext) (options : System.Text.Json.JsonSerializerOptions) obj =
    Async.Adapt
      (fun cancellationToken ->
        let response = httpContext.Response
        response.Headers.["Content-Type"] <- StringValues "application/json; charset=utf-8"
        System.Text.Json.JsonSerializer.SerializeAsync (response.Body, obj, options, cancellationToken)
      )

  /// <summary>
  /// Sets <c>Content-Type</c> header of the response to <c>application/json</c> a writes provided object using
  /// <c>System.Text.Json</c> based asynchronous serialization to the response stream.
  /// <para>
  /// Serialization options are requested from the request services. Defaults are used if none provided.
  /// </para>
  /// </summary>
  /// <param name="httpContext">Context to use.</param>
  /// <param name="obj">Object to serialize and write to the response.</param>
  [<CompiledName("AsyncJson")>]
  let asyncJson (httpContext: HttpContext) obj =
    let options =
      match httpContext.RequestServices.GetService typeof<System.Text.Json.JsonSerializerOptions> with
      | :? System.Text.Json.JsonSerializerOptions as options -> options
      | _                                                    -> null
    asyncJsonWith httpContext options obj


  [<System.Obsolete("Async JSON serialization should be used in new projects.")>]
  let jsonWith (httpContext : HttpContext) (settings : JsonSerializerSettings) obj =
    let data =
      JsonConvert.SerializeObject (obj, settings)
      |> utf8.GetBytes
    let response = httpContext.Response
    response.Headers.["Content-Type"] <- StringValues "application/json; charset=utf-8"
    response.Headers.ContentLength    <- Nullable.mk data.LongLength
    response.Body.AsyncWrite data

  [<System.Obsolete("Async JSON serialization should be used in new projects.")>]
  let json (httpContext : HttpContext) obj =
    let settings =
      tryGetService<JsonSerializerSettings> httpContext.RequestServices
      |> Option.defaultWith (fun () -> JsonSerializerSettings(ReferenceLoopHandling = ReferenceLoopHandling.Ignore))
    jsonWith httpContext settings obj
