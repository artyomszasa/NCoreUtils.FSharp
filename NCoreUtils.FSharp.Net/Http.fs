[<AutoOpen>]
module NCoreUtils.NetExt

open System
open System.Net.Http
open System.Runtime.CompilerServices
open System.Threading

type HttpClient with
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncSend (requestMessage, httpCompletion) =
    Async.Adapt (fun cancellationToken -> this.SendAsync (requestMessage, httpCompletion, cancellationToken))

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncSend (requestMessage) =
    Async.Adapt (fun cancellationToken -> this.SendAsync (requestMessage, cancellationToken))

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncGetStream (uri : Uri) =
    Async.Adapt (fun _ -> this.GetStreamAsync uri)

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncGetStream (uri : string) =
    Async.Adapt (fun _ -> this.GetStreamAsync uri)

type HttpContent with
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncReadAsString () = Async.Adapt (fun (_ : CancellationToken) -> this.ReadAsStringAsync ())
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncReadAsStream () = Async.Adapt (fun (_ : CancellationToken) -> this.ReadAsStreamAsync ())
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncReadAsByteArray () = Async.Adapt (fun (_ : CancellationToken) -> this.ReadAsByteArrayAsync ())
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  member this.AsyncLoadIntoBuffer () = Async.Adapt (fun (_ : CancellationToken) -> this.LoadIntoBufferAsync ())