namespace NCoreUtils

open FSharp.Control
open System.Data.Common
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks

[<AutoOpen>]
module DbCommandExtensions =

  type DbDataReader with
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncRead () = Async.Adapt this.ReadAsync

  [<RequireQualifiedAccess>]
  [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
  module AsyncSeq =

    [<CompiledName("ReadDbDataReader")>]
    let readDbDataReader selector reader =
      AsyncSeq.unfoldAsync
        (fun (reader : DbDataReader) -> async {
          let! next = reader.AsyncRead ()
          return
            match next with
            | true ->
              let item = selector reader
              Some (item, reader)
            | _ -> None
        })
        reader

    [<CompiledName("AsyncReadDbDataReader")>]
    let asyncReadDbDataReader selector reader =
      AsyncSeq.unfoldAsync
        (fun (reader : DbDataReader) -> async {
          let! next = reader.AsyncRead ()
          match next with
          | true ->
            let! item = selector reader
            return Some (item, reader)
          | _ -> return None
        })
        reader

  type DbCommand with
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncExecuteScalar () = Async.Adapt this.ExecuteScalarAsync

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncExecuteReader () =
      Async.Adapt (this.ExecuteReaderAsync : CancellationToken -> Task<_>)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncExecuteReader commandBehavior =
      Async.Adapt (fun cancellationToken -> this.ExecuteReaderAsync(commandBehavior, cancellationToken))

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncExecuteNonQuery () = Async.Adapt this.ExecuteNonQueryAsync

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncExecuteAsyncSeq (selector : _ -> 'a) : AsyncSeq<'a> = asyncSeq {
      let! reader = this.AsyncExecuteReader ()
      return AsyncSeq.readDbDataReader selector reader }

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncExecuteAsyncSeq (commandBehavior, selector : _ -> 'a) : AsyncSeq<'a> = asyncSeq {
      let! reader = this.AsyncExecuteReader commandBehavior
      return AsyncSeq.readDbDataReader selector reader }

  type DbConnection with
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncOpen () = Async.Adapt this.OpenAsync
