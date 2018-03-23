[<AutoOpen>]
module NCoreUtils.Data.DataRepositoryExtensions

open NCoreUtils

type IDataRepository<'data> with

  member inline this.AsyncPersist item =
    Async.Adapt (fun cancellationToken -> this.PersistAsync (item, cancellationToken))

  member inline this.AsyncRemove (item, ?force) =
    Async.Adapt (fun cancellationToken -> this.RemoveAsync (item, (defaultArg force false), cancellationToken))

type IDataRepository<'data, 'id when 'data :> IHasId<'id>> with

  member inline this.AsyncLookup id =
    Async.Adapt (fun cancellationToken -> this.LookupAsync (id, cancellationToken))