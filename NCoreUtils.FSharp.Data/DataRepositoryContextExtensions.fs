[<AutoOpen>]
module NCoreUtils.Data.DataRepositoryContextExtensions

open NCoreUtils

type IDataRepositoryContext with

  member inline this.AsyncBeginTrasaction isolationLevel =
    Async.Adapt (fun cancellationToken -> this.BeginTransactionAsync (isolationLevel, cancellationToken))

  member this.AsyncTransacted (isolationLevel, action) = async {
    use! tx = this.AsyncBeginTrasaction isolationLevel
    let! result = action ()
    tx.Commit ()
    return result; }