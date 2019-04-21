// Learn more about F# at http://fsharp.org

open System.Threading
open NCoreUtils
open NCoreUtils.Synchronization


[<EntryPoint>]
let main argv =

    let mutexName = "NCoreUtils.Synchronization.TestMutex"
    use mutex0 = new MutexLock (mutexName) :> IAsyncLock
    use mutex1 = new MutexLock (mutexName) :> IAsyncLock
    eprintfn "Attempt to lock mutex0"
    async {
        do! mutex0.AsyncLock ()
        let! owned = mutex1.AsyncTryLock ()
        eprintfn "[%A] mutex1: %A" Thread.CurrentThread.ManagedThreadId owned
        mutex0.Release ()
        let! owned = mutex1.AsyncTryLock ()
        eprintfn "[%A] mutex1: %A" Thread.CurrentThread.ManagedThreadId owned
        // let! owned = mutex0.AsyncTryLock ()
        // eprintfn "mutex0: %A" owned
        mutex1.Release ()
    } |> Async.RunSynchronously


    0 // return an integer exit code
