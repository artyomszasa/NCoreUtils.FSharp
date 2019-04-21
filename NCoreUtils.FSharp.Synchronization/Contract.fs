namespace NCoreUtils.Synchronization

open System
open System.Threading
open System.Threading.Tasks
open NCoreUtils
open System.Runtime.InteropServices

type IAsyncLock =
  inherit IDisposable
  abstract AsyncTryLock : unit -> Async<bool>
  abstract AsyncLock : unit -> Async<unit>
  abstract Release : unit -> unit

type IAsyncLockFactory =
  abstract CreateLock : [<Optional; DefaultParameterValue(null : string)>] name:string -> IAsyncLock

[<AbstractClass>]
type AsyncLock () =
  abstract TryLockAsync : cancellationToken:CancellationToken -> Task<bool>
  abstract LockAsync : cancellationToken:CancellationToken -> Task
  abstract Release : unit -> unit
  abstract Dispose : unit -> unit
  interface IAsyncLock with
    member this.AsyncLock () =
      Async.Adapt this.LockAsync
    member this.AsyncTryLock () =
      Async.Adapt this.TryLockAsync
    member this.Dispose () =
      this.Dispose ()
    member this.Release () =
      this.Release ()
