[<AutoOpen>]
module NCoreUtils.StorageExt

open System.IO
open System.Runtime.InteropServices
open System.Threading
open FSharp.Control
open NCoreUtils.Storage
open NCoreUtils.Progress

type IStorageProvider with
  member this.AsyncGetRoots () = this.GetRootsAsync () |> ofAsyncEnumerable
  member this.AsyncTryResolve uri = async {
    let! path = Async.Adapt (fun cancellationToken -> this.ResolveAsync (uri, cancellationToken))
    return Option.wrap path }

type IStorageContainer with
  member this.AsyncGetContents () = this.GetContentsAsync () |> ofAsyncEnumerable
  member this.AsyncCreateRecord (name, contents : Stream, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    Async.Adapt (fun cancellationToken -> this.CreateRecordAsync (name, contents, progress, cancellationToken))
  member this.AsyncCreateRecord (name, contents : byte[], [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    Async.Adapt (fun cancellationToken -> this.CreateRecordAsync (name, contents, progress, cancellationToken))
  member this.AsyncCreateFolder (name, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    Async.Adapt (fun cancellationToken -> this.CreateFolderAsync (name, progress, cancellationToken))

type IStorageItem with
  member this.AsyncGetContainer () =
    let inline f (cancellationToken : CancellationToken) = this.GetContainerAsync cancellationToken
    Async.Adapt f
  member this.AsyncDelete ([<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    let inline f (cancellationToken : CancellationToken) = this.DeleteAsync (progress, cancellationToken)
    Async.Adapt f

type IStorageRecord with
  member this.AsyncCreateReadableStream () =
    let inline f (cancellationToken : CancellationToken) = this.CreateReadableStreamAsync cancellationToken
    Async.Adapt f
  member this.AsyncUpdateContent (contents, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    let inline f (cancellationToken : CancellationToken) = this.UpdateContentAsync (contents, progress, cancellationToken)
    Async.Adapt f
  member this.AsyncRename (name, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    let inline f (cancellationToken : CancellationToken) = this.RenameAsync (name, progress, cancellationToken)
    Async.Adapt f
  member this.AsyncReadAllBytes () =
    let inline f (cancellationToken : CancellationToken) = this.ReadAllBytesAsync cancellationToken
    Async.Adapt f