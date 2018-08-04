[<RequireQualifiedAccess>]
module NCoreUtils.AsyncSeq

open FSharp.Control

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