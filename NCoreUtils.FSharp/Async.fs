[<AutoOpen>]
module NCoreUtils.Async

open System
open System.Threading
open System.Threading.Tasks

[<AutoOpen>]
module Helpers =

  let inline flatten (exn : AggregateException) =
    let flatExn = exn.Flatten ()
    match flatExn.InnerExceptions.Count with
    | 1 -> flatExn.InnerExceptions.[0]
    | _ -> flatExn :> _

  [<Struct>]
  type TaskResult<'T> =
    | VCancelled
    | VSuccess of Result:'T
    | VFailed  of Exception:exn

  [<Struct>]
  type TaskResult =
    | Cancelled
    | Success
    | Failed  of Exception:exn

  let inline getTaskState (task : Task<'a>) =
    match task.IsFaulted with
    | true -> VFailed (flatten task.Exception)
    | _ ->
    match task.IsCanceled with
    | true -> VCancelled
    | _    -> VSuccess task.Result

  let inline getUnitTaskState (task : Task) =
    match task.IsFaulted with
    | true -> Failed (flatten task.Exception)
    | _ ->
    match task.IsCanceled with
    | true -> Cancelled
    | _    -> Success

  // let inline (|Cancelled|Failed|Success|) (task : Task) =
  //   match task.IsFaulted with
  //   | true -> Failed (flatten task.Exception)
  //   | _ ->
  //   match task.IsCanceled with
  //   | true -> Cancelled
  //   | _    -> Success

  let inline invokeWithContinuations (invocation : CancellationToken -> Task<_>) cancellationToken (success : _ -> unit, error : exn -> unit, cancel : OperationCanceledException -> unit) =
    let continuation =
      Action<Task<_>>
        (fun task ->
          match getTaskState task with
          | VSuccess result -> success result
          | VFailed  exn    -> error exn
          | VCancelled      -> cancel (TaskCanceledException ()))
    (invocation cancellationToken).ContinueWith (
      continuation,
      CancellationToken.None,
      TaskContinuationOptions.None,
      TaskScheduler.Default) |> ignore

  let inline invokeVoidWithContinuations (invocation : CancellationToken -> Task) cancellationToken (success : _ -> unit, error : exn -> unit, cancel : OperationCanceledException -> unit) =
    let continuation =
      Action<Task>
        (fun task ->
          match getUnitTaskState task with
          | Success    -> success ()
          | Failed exn -> error exn
          | Cancelled  -> cancel (TaskCanceledException ()))
    (invocation cancellationToken).ContinueWith (
      continuation,
      CancellationToken.None,
      TaskContinuationOptions.None,
      TaskScheduler.Default) |> ignore

  let inline vinvokeWithContinuations (invocation : CancellationToken -> ValueTask<_>) cancellationToken (success : _ -> unit, error : exn -> unit, cancel : OperationCanceledException -> unit) =
    match invocation cancellationToken with
    | vt when vt.IsCompleted ->
      try success vt.Result
      with exn -> error exn
    | vt ->
      let continuation =
        Action<Task<_>>
          (fun task ->
            match getTaskState task with
            | VSuccess result -> success result
            | VFailed  exn    -> error exn
            | VCancelled      -> cancel (TaskCanceledException ()))
      vt.AsTask().ContinueWith (
        continuation,
        CancellationToken.None,
        TaskContinuationOptions.None,
        TaskScheduler.Default) |> ignore

  let inline vinvokeVoidWithContinuations (invocation : CancellationToken -> ValueTask) cancellationToken (success : _ -> unit, error : exn -> unit, cancel : OperationCanceledException -> unit) =
    match invocation cancellationToken with
    | vt when vt.IsCompleted ->
      try success (vt.GetAwaiter().GetResult())
      with exn -> error exn
    | vt ->
      let continuation =
        Action<Task>
          (fun task ->
            match getUnitTaskState task with
            | Failed exn -> error exn
            | Success    -> success ()
            | Cancelled  -> cancel (TaskCanceledException ()))
      vt.AsTask().ContinueWith (
        continuation,
        CancellationToken.None,
        TaskContinuationOptions.None,
        TaskScheduler.Default) |> ignore


let private fsharpAsyncSequential computations = Async.Sequential computations

type Microsoft.FSharp.Control.Async with

  /// Adapts TPL based asyncronous invokation.
  static member inline Adapt (invocation : CancellationToken -> Task<_>) =
    let inline start cancellationToken =
      Async.FromContinuations (invokeWithContinuations invocation cancellationToken)
    async.Bind (Async.CancellationToken, start)

  /// Adapts TPL based asyncronous invokation.
  static member inline Adapt (invocation : CancellationToken -> Task) =
    let inline start cancellationToken =
      Async.FromContinuations (invokeVoidWithContinuations invocation cancellationToken)
    async.Bind (Async.CancellationToken, start)

  /// Adapts TPL based asyncronous invokation.
  static member inline VAdapt (invocation : CancellationToken -> ValueTask<_>) =
    let inline start cancellationToken =
      Async.FromContinuations (vinvokeWithContinuations invocation cancellationToken)
    async.Bind (Async.CancellationToken, start)

  /// Adapts TPL based asyncronous invokation.
  static member inline VAdapt (invocation : CancellationToken -> ValueTask) =
    let inline start cancellationToken =
      Async.FromContinuations (vinvokeVoidWithContinuations invocation cancellationToken)
    async.Bind (Async.CancellationToken, start)

  static member inline Raise (e : #exn) = Async.FromContinuations (fun (_, error, _) -> error e)

  [<Obsolete("Use FSharp.Core version instead.")>]
  static member Sequential (computations : Async<_>[]) = fsharpAsyncSequential computations

