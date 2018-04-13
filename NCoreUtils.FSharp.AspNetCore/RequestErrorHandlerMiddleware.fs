[<RequireQualifiedAccess>]
module NCoreUtils.AspNetCore.RequestErrorHandlerMiddleware

open System.Runtime.ExceptionServices
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Primitives
open System.Threading.Tasks

let run (httpContext : HttpContext) asyncNext = async {
  try do! asyncNext
  with
    | :? InvalidRequestException as exn ->
      let logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger "RequestErrorHandlerMiddleware"
      httpContext.Response.Headers.Add ("X-Message", (StringValues : string -> _) exn.Message)
      HttpContext.setResponseStatusCode exn.StatusCode httpContext
      logger.LogDebug (exn, "Request cound not be processed.")
    | :? TaskCanceledException as exn ->
      match httpContext.RequestAborted.IsCancellationRequested with
      | true ->
        let logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger "RequestErrorHandlerMiddleware"
        logger.LogDebug (exn, "Request has been cancelled.")
      | _ ->
        let logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger "RequestErrorHandlerMiddleware"
        httpContext.Response.Headers.Add ("X-Message", (StringValues : string -> _) "Request timed out.")
        HttpContext.setResponseStatusCode 408 httpContext
        logger.LogDebug (exn, "Request timed out.")
    | exn ->
      ExceptionDispatchInfo.Capture(exn).Throw () }