[<AutoOpen>]
module NCoreUtils.Data.DataRepositoryContextExtensions

open NCoreUtils
open System.Runtime.CompilerServices

type IDataRepositoryContext with

  member inline this.AsyncBeginTransaction isolationLevel =
    Async.Adapt (fun cancellationToken -> this.BeginTransactionAsync (isolationLevel, cancellationToken))

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncTransacted (isolationLevel, action) = async {
    use! tx = this.AsyncBeginTransaction isolationLevel
    let! result = action ()
    tx.Commit ()
    return result; }