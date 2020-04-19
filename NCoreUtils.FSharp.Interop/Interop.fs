[<AutoOpen>]
[<System.Runtime.CompilerServices.Extension>]
module NCoreUtils.Interop

open System.Threading.Tasks
open FSharp.Control

type private ToAsyncEnumerator<'T> (enumerator : IAsyncEnumerator<'T>, cancellationToken) =
  let mutable current = Unchecked.defaultof<_>
  let returnValue next (ctx: AsyncActivation<_>) =
    match next with
    | Some value ->
      current <- value
      ctx.OnSuccess true
    | None ->
      current <- Unchecked.defaultof<_>
      ctx.OnSuccess false
  let computation =
    let binding ctx =
      AsyncPrimitives.Bind
        ctx
        (enumerator.MoveNext ())
        (returnValue >> AsyncPrimitives.MakeAsync)
    AsyncPrimitives.MakeAsync binding
  interface System.Collections.Generic.IAsyncEnumerator<'T> with
    member __.Current = current
    member __.MoveNextAsync () =
      ValueTask<bool> (Async.StartAsTask (computation, cancellationToken = cancellationToken))
    member __.DisposeAsync () =
      current <- Unchecked.defaultof<_>
      enumerator.Dispose ()
      Unchecked.defaultof<_>


[<System.Runtime.CompilerServices.Extension>]
[<CompiledName("ToAsyncEnumerable")>]
let toAsyncEnumerable (asyncSeq : AsyncSeq<_>) =
  { new System.Collections.Generic.IAsyncEnumerable<_> with
      member __.GetAsyncEnumerator cancellationToken = new ToAsyncEnumerator<_> (asyncSeq.GetEnumerator (), cancellationToken) :> _
  }

[<System.Runtime.CompilerServices.Extension>]
[<CompiledName("OfAsyncEnumerable")>]
let ofAsyncEnumerable (asyncEnumerable : System.Collections.Generic.IAsyncEnumerable<_>) =
  { new IAsyncEnumerable<_> with
      member __.GetEnumerator () =
        let source = asyncEnumerable.GetAsyncEnumerator ()
        { new IAsyncEnumerator<_> with
            member __.MoveNext () =
              match source.MoveNextAsync () with
              | vtask when vtask.IsCompletedSuccessfully ->
                match vtask.Result with
                | true -> Some source.Current
                | _    -> None
                |> async.Return
              | vtask -> async {
                let! hasNext = vtask.AsTask () |> Async.AwaitTask
                return
                  match hasNext with
                  | true -> Some source.Current
                  | _    -> None }
            member __.Dispose () = source.DisposeAsync().AsTask().Wait ()
        }
  }