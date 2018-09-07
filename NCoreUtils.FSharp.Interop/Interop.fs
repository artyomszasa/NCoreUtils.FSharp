[<AutoOpen>]
[<System.Runtime.CompilerServices.Extension>]
module NCoreUtils.Interop

open FSharp.Control

type private ToAsyncEnumerator<'T> (enumerator : IAsyncEnumerator<'T>) =
  let mutable current = Unchecked.defaultof<_>
  interface System.Collections.Generic.IAsyncEnumerator<'T> with
    member __.Current = current
    member __.MoveNext cancellationToken =
      let computation = async {
        let! next = enumerator.MoveNext ()
        return
          match next with
          | Some value ->
            current <- value
            true
          | _ ->
            current <- Unchecked.defaultof<_>
            false }
      Async.StartAsTask (computation, cancellationToken = cancellationToken)
    member __.Dispose () =
      current <- Unchecked.defaultof<_>
      enumerator.Dispose ()


[<System.Runtime.CompilerServices.Extension>]
[<CompiledName("ToAsyncEnumerable")>]
let toAsyncEnumerable (asyncSeq : AsyncSeq<_>) =
  { new System.Collections.Generic.IAsyncEnumerable<_> with
      member __.GetEnumerator () = new ToAsyncEnumerator<_> (asyncSeq.GetEnumerator ()) :> _
  }

[<System.Runtime.CompilerServices.Extension>]
[<CompiledName("OfAsyncEnumerable")>]
let ofAsyncEnumerable (asyncEnumerable : System.Collections.Generic.IAsyncEnumerable<_>) =
  { new IAsyncEnumerable<_> with
      member __.GetEnumerator () =
        let source = asyncEnumerable.GetEnumerator ()
        let moveNext = source.MoveNext
        { new IAsyncEnumerator<_> with
            member __.MoveNext () = async {
              let! hasNext = Async.Adapt moveNext
              return
                match hasNext with
                | true -> Some source.Current
                | _    -> None }
            member __.Dispose () = source.Dispose ()
        }
  }