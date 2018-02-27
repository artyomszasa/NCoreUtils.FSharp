namespace NCoreUtils
#nowarn "64"

open System

[<AutoOpen>]
module BindTopLevelOperator =

  [<Sealed>]
  type BindMonad () =
    static member Bind (_ : BindMonad, o : _ option, binder) = Option.bind binder o
    static member Bind (_ : BindMonad, n : Nullable<_>, binder) = Nullable.bind binder n
    static member Bind (_ : BindMonad, a : Async<_>, binder) = async.Bind (a, binder)

  let inline (>>=) (source : ^a) (binder : ^b) : ^c
    when ^x :> BindMonad
    and  (^a or ^x) : (static member Bind : ^x * ^a * ^b -> ^c)
    = ((^a or ^x) : (static member Bind : ^x * ^a * ^b -> ^c) (Unchecked.defaultof<_>, source, binder))