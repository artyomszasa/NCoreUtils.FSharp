namespace NCoreUtils

open System.IO
open System.Runtime.CompilerServices

[<AutoOpen>]
module IOExtensions =

  type Stream with
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncCopyTo (destination : Stream, ?bufferSize : int) =
      Async.Adapt (fun cancellationToken -> this.CopyToAsync (destination, defaultArg bufferSize 8192, cancellationToken))

    member this.AsyncReadComplete (buffer, offset, count) =
      let rec readSome (read) = async {
        if read < count then
          let! readOnce = this.AsyncRead (buffer, offset + read, count - read)
          match readOnce with
          | 0 -> EndOfStreamException () |> raise
          | _ -> do! readSome (read + readOnce) }
      readSome 0

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncReadComplete buffer = this.AsyncReadComplete (buffer, 0, buffer.Length)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncFlush () = Async.Adapt this.FlushAsync
