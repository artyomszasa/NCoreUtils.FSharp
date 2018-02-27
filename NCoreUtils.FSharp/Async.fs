[<AutoOpen>]
module NCoreUtils.Async

open System
open System.Threading
open System.Threading.Tasks

[<AutoOpen>]
module private Helpers =

  let private flatten (exn : AggregateException) =
    let flatExn = exn.Flatten ()
    match flatExn.InnerExceptions.Count with
    | 1 -> flatExn.InnerExceptions.[0]
    | _ -> flatExn :> _

  let (|VCancelled|VFailed|VSuccess|) (task : Task<'a>) =
    match task.IsFaulted with
    | true -> VFailed (flatten task.Exception)
    | _ ->
    match task.IsCanceled with
    | true -> VCancelled
    | _    -> VSuccess task.Result

  let (|Cancelled|Failed|Success|) (task : Task) =
    match task.IsFaulted with
    | true -> Failed (flatten task.Exception)
    | _ ->
    match task.IsCanceled with
    | true -> Cancelled
    | _    -> Success

type Microsoft.FSharp.Control.Async with

  /// Adapts TPL based asyncronous invokation.
  static member Adapt (invocation : CancellationToken -> Task<_>) =
    let start cancellationToken =
      Async.FromContinuations
        (fun (success, error, cancel) ->
          (invocation cancellationToken).ContinueWith (
            (function
              | VSuccess result -> success result
              | VFailed  exn    -> error exn
              | VCancelled      -> cancel (OperationCanceledException ())),
            TaskContinuationOptions.ExecuteSynchronously)
          |> ignore
        )
    async.Bind (Async.CancellationToken, start)

  /// Adapts TPL based asyncronous invokation.
  static member Adapt (invocation : CancellationToken -> Task) =
    let start cancellationToken =
      Async.FromContinuations
        (fun (success, error, cancel) ->
          (invocation cancellationToken).ContinueWith (
            (function
              | Success        -> success ()
              | Failed  exn    -> error exn
              | Cancelled      -> cancel (OperationCanceledException ())),
            TaskContinuationOptions.ExecuteSynchronously)
          |> ignore
        )
    async.Bind (Async.CancellationToken, start)

  static member Raise (e : #exn) = Async.FromContinuations (fun (_, error, _) -> error e)