[<RequireQualifiedAccess>]
module NCoreUtils.AspNetCore.RequestErrorHandlerMiddleware

open System.Runtime.ExceptionServices
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Primitives

let run (httpContext : HttpContext) asyncNext = async {
  try do! asyncNext
  with
    | :? InvalidRequestException as exn ->
      let logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger "RequestErrorHandlerMiddleware"
      httpContext.Response.Headers.Add ("X-Message", (StringValues : string -> _) exn.Message)
      HttpContext.setResponseStatusCode exn.StatusCode httpContext
      logger.LogDebug (exn, "Request cound not be processed.")
    | exn ->
      ExceptionDispatchInfo.Capture(exn).Throw () }