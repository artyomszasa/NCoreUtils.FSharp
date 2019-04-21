namespace NCoreUtils.Synchronization

open System.Threading
open System
open System.Collections.Concurrent
open System.Runtime.ExceptionServices
open System.Threading.Tasks
open System.Runtime.InteropServices
open NCoreUtils
open System.Diagnostics.CodeAnalysis

type private WorkItem = {
  Callback  : SendOrPostCallback
  State     : obj
  Processed : (ExceptionDispatchInfo -> unit) voption }

[<Sealed>]
type MutexSynchronizationContext =
  inherit SynchronizationContext

  static member private RunItem (disposed : int ref, item : WorkItem) =
    let exn = ref null
    try
      match !disposed with
      | 0 -> item.Callback.Invoke item
      | _ -> ObjectDisposedException "MutexSynchronizationContext" |> raise
    with e -> exn := ExceptionDispatchInfo.Capture e
    match item.Processed with
    | ValueNone          -> ()
    | ValueSome callback -> callback !exn

  static member private Run (messageQueue : BlockingCollection<_>, disposed) =
    try
      messageQueue.GetConsumingEnumerable ()
      |> Seq.iter (fun item -> MutexSynchronizationContext.RunItem (disposed, item))
    with
      | :? OperationCanceledException -> ()

  static member private FinalizeDispose (obj : obj) =
    match obj with
    | :? MutexSynchronizationContext as this ->
      this.thread.Join ()
      this.messageQueue.Dispose ()
    | _ -> ()

  val private messageQueue : BlockingCollection<WorkItem>

  val internal thread       : Thread

  val internal disposed     : int ref

  new () =
    let disposed     = ref 0
    let messageQueue = new BlockingCollection<WorkItem> ()
    let thread       = Thread (ThreadStart (fun  () -> MutexSynchronizationContext.Run (messageQueue, disposed)))
    thread.Start ()
    { messageQueue = messageQueue
      thread       = thread
      disposed     = ref 0 }

  override this.Finalize () =
    this.Dispose false

  member inline private this.ThrowIfDisposed () =
    if 0 <> !this.disposed then
      ObjectDisposedException "MutexSynchronizationContext" |> raise

  override __.CreateCopy () = NotSupportedException ("mutex synchronization context cannot be cloned.") |> raise

  override this.Post (cb, state) =
    this.ThrowIfDisposed ()
    this.messageQueue.Add { Callback = cb; State = state; Processed = ValueNone }

  override this.Send (cb, state) =
    this.ThrowIfDisposed ()
    use waiter = new ManualResetEventSlim (false)
    let exn0 = ref null
    let callback (exn : ExceptionDispatchInfo) =
      exn0 := exn
      Interlocked.MemoryBarrier ()
      waiter.Set ()
    this.messageQueue.Add { Callback = cb; State = state; Processed = ValueSome callback }
    waiter.Wait ()
    match !exn0 with
    | null -> ()
    | exn  -> exn.Throw ()

  member this.Dispose disposing =
    if 0 = Interlocked.CompareExchange (this.disposed, 1, 0) then
      match disposing with
      | true ->
        this.messageQueue.CompleteAdding ()
        let callback = WaitCallback MutexSynchronizationContext.FinalizeDispose
        ThreadPool.QueueUserWorkItem (callback, this :> obj) |> ignore
        // thread should exit normally...
      | false ->
        // force thread to exit on finalization
        try this.thread.Abort () with e -> eprintfn "%A" e

  member this.Dispose () =
    GC.SuppressFinalize this
    this.Dispose true

  interface IDisposable with
    member this.Dispose () = this.Dispose ()

[<Sealed>]
type MutexLock ([<Optional; DefaultParameterValue(null : string)>] name) =
  inherit AsyncLock ()
  let context = new MutexSynchronizationContext ()
  let mutex = new Mutex (false, name)
  let mutable disposed = 0
  static let tryLockWithCancellation (mutex: Mutex) instant (cancellationToken : CancellationToken) =
    let rec impl (_ : int) =
      match cancellationToken.IsCancellationRequested with
      | true -> false
      | _    ->
      match mutex.WaitOne (if instant then 0 else 10) with
      | true -> true
      | _ ->
      match instant with
      | true -> false
      | _    -> impl Unchecked.defaultof<_>
    impl Unchecked.defaultof<_>
  /// Internal synchronization context access.
  member internal __.SynchronizationContext with [<ExcludeFromCodeCoverage>] get () = context
  /// Internal mutex access.
  member internal __.Mutex with [<ExcludeFromCodeCoverage>] get () = mutex
  /// Internal disposed access.
  member internal __.IsDisposed with [<ExcludeFromCodeCoverage>] get () = 0 <> disposed
  override __.LockAsync cancellationToken =
    let completion = TaskCompletionSource<bool> ()
    // local copy
    let m = mutex
    context.Post (
      (fun (_ : obj) ->
        try
          match tryLockWithCancellation m false cancellationToken with
          | true -> completion.SetResult true
          | _    -> completion.SetCanceled ()
        with exn -> completion.SetException exn
      ),
      null)
    completion.Task :> Task
  override __.TryLockAsync cancellationToken =
    let completion = TaskCompletionSource<bool> ()
    // local copy
    let m = mutex
    context.Post (
      (fun (_ : obj) ->
        match cancellationToken.IsCancellationRequested with
        | true -> completion.SetCanceled ()
        | _ ->
          try tryLockWithCancellation m true cancellationToken |> completion.SetResult
          with exn -> completion.SetException exn
      ),
      null)
    completion.Task
  override __.Release () = context.Send ((fun _ -> mutex.ReleaseMutex ()), null)
  override this.Dispose () =
    GC.SuppressFinalize this
    this.Dispose true
  member __.Dispose disposing =
    if 0 = Interlocked.CompareExchange (&disposed, 1, 0) then
      match disposing with
      | true ->
        context.Send ((fun _ -> mutex.Dispose ()), null)
        context.Dispose ()
      | _ ->
        // For some reason mutex is not beeing freed on finalization...
        mutex.Dispose ()

  override this.Finalize () = this.Dispose false

[<Sealed>]
type MutexLockFactory private () =
  static member val Instance = MutexLockFactory ()
  member __.CreateLock name = new MutexLock (name)
  interface IAsyncLockFactory with
    member this.CreateLock name = this.CreateLock name :> _

