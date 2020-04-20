[<AutoOpen>]
module NCoreUtils.Data.DataRepositoryContextExtensions

open NCoreUtils
open System.Runtime.CompilerServices

type IDataRepositoryContext with

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncBeginTransaction isolationLevel =
    Async.VAdapt (fun cancellationToken -> this.BeginTransactionAsync (isolationLevel, cancellationToken))

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncTransacted (isolationLevel, action) = async {
    use! tx = this.AsyncBeginTransaction isolationLevel
    let! result = action ()
    tx.Commit ()
    return result; }