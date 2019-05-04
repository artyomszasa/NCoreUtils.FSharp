[<AutoOpen>]
module NCoreUtils.Data.DataRepositoryExtensions

open System.Runtime.CompilerServices
open NCoreUtils

type IDataRepository<'data> with

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncPersist item =
    Async.Adapt (fun cancellationToken -> this.PersistAsync (item, cancellationToken))

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncRemove (item, ?force) =
    Async.Adapt (fun cancellationToken -> this.RemoveAsync (item, (defaultArg force false), cancellationToken))

type IDataRepository<'data, 'id when 'data :> IHasId<'id>> with

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncLookup id =
    Async.Adapt (fun cancellationToken -> this.LookupAsync (id, cancellationToken))