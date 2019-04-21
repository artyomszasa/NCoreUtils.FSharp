namespace NCoreUtils.Synchronization

open System.Threading

[<Sealed>]
type SemaphoreLock () =
  inherit AsyncLock ()
  let semaphore = new SemaphoreSlim (1)
  override __.LockAsync cancelllationToken =
    semaphore.WaitAsync cancelllationToken
  override __.TryLockAsync cancellationToken =
    semaphore.WaitAsync (0, cancellationToken)
  override __.Release () = semaphore.Release () |> ignore
  override __.Dispose () = semaphore.Dispose ()

[<Sealed>]
type SemaphoreLockFactory private () =
  static member val Instance = SemaphoreLockFactory ()
  member __.CreateLock () = new SemaphoreLock ()
  interface IAsyncLockFactory with
    member this.CreateLock _ = this.CreateLock () :> _
