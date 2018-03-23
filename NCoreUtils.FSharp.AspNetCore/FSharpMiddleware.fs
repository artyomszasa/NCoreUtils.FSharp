namespace NCoreUtils.AspNetCore

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open NCoreUtils

[<AbstractClass>]
type FSharpMiddleware (next : RequestDelegate) =

  member this.InvokeAsync httpContext =
    let asyncNext = async { do! Async.Adapt (fun _ -> next.Invoke httpContext) }
    Async.StartAsTask (this.AsyncInvoke (httpContext, asyncNext), cancellationToken = httpContext.RequestAborted) :> Task

  abstract AsyncInvoke : httpContext:HttpContext * asyncNext:Async<unit> -> Async<unit>

[<AutoOpen>]
module FSharpMiddleware =

  type IApplicationBuilder with
    member this.Use (action : HttpContext -> Async<unit> -> Async<unit>) =
      let func =
        Func<HttpContext, Func<Task>, Task>
          (fun httpContext next ->
            let asyncNext = async { do! Async.Adapt (ignore >> next.Invoke) }
            Async.StartAsTask (action httpContext asyncNext, cancellationToken = httpContext.RequestAborted) :> _)
      this.Use func
