[<AutoOpen>]
module NCoreUtils.StorageExt

open System.IO
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open System.Threading
open FSharp.Control
open NCoreUtils.Storage
open NCoreUtils.Progress

type IStorageProvider with
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncGetRoots () = this.GetRootsAsync () |> ofAsyncEnumerable
  member this.AsyncTryResolve uri = async {
    let! path = Async.Adapt (fun cancellationToken -> this.ResolveAsync (uri, cancellationToken))
    return Option.ofObj path }
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncCreateRecord (path, contents : Stream, [<Optional; DefaultParameterValue(null : string)>] contentType, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    Async.Adapt (fun cancellationToken -> this.CreateRecordAsync (path, contents, contentType, progress, cancellationToken))
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncCreateRecord (path, contents : byte[], [<Optional; DefaultParameterValue(null : string)>] contentType, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    Async.Adapt (fun cancellationToken -> this.CreateRecordAsync (path, contents, contentType, progress, cancellationToken))
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncCreateFolder (path, [<Optional; DefaultParameterValue(false : bool)>] recursive, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    Async.Adapt (fun cancellationToken -> this.CreateFolderAsync (path, recursive, progress, cancellationToken))

type IStoragePath with
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncGetParent () = Async.Adapt (fun cancellationToken -> this.GetParentAsync cancellationToken)

type IStorageContainer with
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncGetContents () = this.GetContentsAsync () |> ofAsyncEnumerable
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncCreateRecord (name, contents : Stream, [<Optional; DefaultParameterValue(null : string)>] contentType, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    Async.Adapt (fun cancellationToken -> this.CreateRecordAsync (name, contents, contentType, progress, cancellationToken))
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncCreateRecord (name, contents : byte[], [<Optional; DefaultParameterValue(null : string)>] contentType, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    Async.Adapt (fun cancellationToken -> this.CreateRecordAsync (name, contents, contentType, progress, cancellationToken))
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncCreateFolder (name, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    Async.Adapt (fun cancellationToken -> this.CreateFolderAsync (name, progress, cancellationToken))

type IStorageItem with
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncGetContainer () =
    let inline f (cancellationToken : CancellationToken) = this.GetContainerAsync cancellationToken
    Async.Adapt f
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncDelete ([<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    let inline f (cancellationToken : CancellationToken) = this.DeleteAsync (progress, cancellationToken)
    Async.Adapt f

type IStorageRecord with
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncCreateReadableStream () =
    let inline f (cancellationToken : CancellationToken) = this.CreateReadableStreamAsync cancellationToken
    Async.Adapt f
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncUpdateContent (contents, [<Optional; DefaultParameterValue(null : string)>] contentType, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    let inline f (cancellationToken : CancellationToken) = this.UpdateContentAsync (contents, contentType, progress, cancellationToken)
    Async.Adapt f
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncRename (name, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    let inline f (cancellationToken : CancellationToken) = this.RenameAsync (name, progress, cancellationToken)
    Async.Adapt f
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncReadAllBytes () =
    let inline f (cancellationToken : CancellationToken) = this.ReadAllBytesAsync cancellationToken
    Async.Adapt f
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncCopyTo (destination, ?bufferSize, ?progress) =
    Async.Adapt (fun cancellationToken -> this.CopyToAsync (destination, Option.toNullable bufferSize, defaultArg progress null, cancellationToken))
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncUpdateSecurity (security, [<Optional; DefaultParameterValue(null : IProgress)>] progress) =
    Async.Adapt (fun cancellationToken -> this.UpdateSecurityAsync (security, progress, cancellationToken))