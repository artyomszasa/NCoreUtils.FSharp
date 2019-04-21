namespace NCoreUtils

open System

[<RequireQualifiedAccess>]
module Result =

  [<CompiledName("Wrap")>]
  let wrap f = try Ok f with e -> Error e

type ResultBuilder () =
  member inline __.Bind (res, binder) = Result.bind binder res
  member inline __.Delay (init : _ -> Result<_, _>) = init ()
  member inline __.Return value = Ok value
  member inline __.ReturnFrom (res : Result<_, _>) = res
  member inline __.Run (res : Result<_, _>) = res
  member inline __.Zero () = Ok ()
  member inline __.Using (disposable : 'a when 'a :> IDisposable, init) =
    try     init disposable
    finally disposable.Dispose ()