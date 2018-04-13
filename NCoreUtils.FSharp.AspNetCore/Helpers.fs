namespace NCoreUtils.AspNetCore

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open NCoreUtils
open System.Text
open Newtonsoft.Json

[<AutoOpen>]
module Helpers =

  let private utf8 = UTF8Encoding false


  let jsonWith (httpContext : HttpContext) (settings : JsonSerializerSettings) obj =
    let data =
      JsonConvert.SerializeObject (obj, settings)
      |> utf8.GetBytes
    let response = httpContext.Response
    response.Headers.["Content-Type"] <- StringValues "application/json; charset=utf-8"
    response.Headers.ContentLength    <- Nullable.mk data.LongLength
    response.Body.AsyncWrite data

  let json (httpContext : HttpContext) obj =
    let settings =
      tryGetService<JsonSerializerSettings> httpContext.RequestServices
      |> Option.defaultWith (fun () -> JsonSerializerSettings(ReferenceLoopHandling = ReferenceLoopHandling.Ignore))
    jsonWith httpContext settings obj
