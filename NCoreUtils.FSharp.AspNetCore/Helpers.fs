namespace NCoreUtils.AspNetCore

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open NCoreUtils
open System.Text

[<AutoOpen>]
module Helpers =

  let private utf8 = UTF8Encoding false

  let json (httpContext : HttpContext) obj =
    let data =
      Newtonsoft.Json.JsonConvert.SerializeObject obj
      |> utf8.GetBytes
    let response = httpContext.Response
    response.Headers.["Content-Type"] <- StringValues "application/json; charset=utf-8"
    response.Headers.ContentLength    <- Nullable.mk data.LongLength
    response.Body.AsyncWrite data
