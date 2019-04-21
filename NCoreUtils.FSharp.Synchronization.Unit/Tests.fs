namespace NCoreUtils.Synchronization

open System
open Xunit
open System.Threading
open System.Runtime.CompilerServices

type MutexTests () =

  static let mutexName = "NCoreUtils.Synchronization.TestMutex"

  static let start computation = Async.StartAsTask computation

  static let startWithTimeout (timeout : int) computation =
    let cancellationSource = new CancellationTokenSource (timeout)
    Async.StartAsTask (computation, cancellationToken = cancellationSource.Token)

  [<Fact>]
  member __.``Basic tests`` () = startWithTimeout 1000 <| async {

    use mutex0 = new MutexLock (mutexName) :> IAsyncLock
    use mutex1 = new MutexLock (mutexName) :> IAsyncLock
    do! mutex0.AsyncLock ()
    let! owned = mutex1.AsyncTryLock ()
    Assert.False owned
    mutex0.Release ()
    let! owned = mutex1.AsyncTryLock ()
    Assert.True owned
    mutex1.Release () }

  [<Fact>]
  member __.``Proper disposal`` () = startWithTimeout 2000 <| async {
    let mutex = new MutexLock (mutexName)
    do! async {
      use m = mutex :> IAsyncLock
      do! m.AsyncLock ()
      m.Release () }
    do! Async.Sleep 100
    Assert.True mutex.IsDisposed
    Assert.Equal (1, !mutex.SynchronizationContext.disposed)
    Assert.False mutex.SynchronizationContext.thread.IsAlive }

  [<Fact>]
  member __.``Cancelled mutex lock`` () = startWithTimeout 2000 <| async {
    use mutex0 = new MutexLock (mutexName) :> IAsyncLock
    use mutex1 = new MutexLock (mutexName)
    do! mutex0.AsyncLock ()
    use cancellation = new CancellationTokenSource ()
    let lockTask = mutex1.LockAsync cancellation.Token
    do! Async.Sleep 20
    cancellation.Cancel ()
    do! Async.Sleep 20
    Assert.True lockTask.IsCanceled
    mutex0.Release () }

  [<Fact>]
  member __.``Dispose consistency`` () = startWithTimeout 2000 <| async {
    use mutex0 = new MutexLock (mutexName) :> IAsyncLock
    do! mutex0.AsyncLock ()
    mutex0.Release ()
    mutex0.Dispose ()
    try do! mutex0.AsyncLock ()
    with e -> Assert.IsAssignableFrom<ObjectDisposedException> e |> ignore }

  [<Fact>]
  member __.``Disposed lock is released`` () = startWithTimeout 2000 <| async {
    let mutex0 = new MutexLock (mutexName) :> IAsyncLock
    do! mutex0.AsyncLock ()
    mutex0.Dispose ()
    use mutex1 = new MutexLock (mutexName) :> IAsyncLock
    let! owned = mutex1.AsyncTryLock ()
    Assert.True owned }

  member __.LockWeak () =
    let m = new MutexLock(mutexName)
    m.LockAsync(CancellationToken.None).Wait ()

  // [<Fact>]
  member this.``Finalization consistency`` () =
    this.LockWeak ()
    GC.Collect ()
    GC.WaitForPendingFinalizers ()
    GC.Collect ()
    GC.WaitForPendingFinalizers ()
    async {
      use mutex1 = new MutexLock (mutexName) :> IAsyncLock
      let! owned = mutex1.AsyncTryLock ()
      Assert.True owned }
    |> Async.RunSynchronously
