namespace NCoreUtils
open System
#nowarn "64"

open System

[<AutoOpen>]
module BindTopLevelOperator =

  [<Sealed>]
  type BindMonad () =
    static member inline Bind (_ : BindMonad, o : _ option, binder) =
      match o with
      | Some x -> binder x
      | _      -> None
    static member inline Bind (_ : BindMonad, n : Nullable<_>, binder) = Nullable.bind binder n
    static member inline Bind (_ : BindMonad, a : Async<_>, binder) = async.Bind (a, binder)

  [<Sealed>]
  type FMapMonad () =
    static member inline FMap (_ : FMapMonad, o : _ option, mapper) =
      match o with
      | Some x -> Some (mapper x)
      | _      -> None
    static member inline FMap (_ : FMapMonad, n : Nullable<_>, mapper) = Nullable.map mapper n
    static member inline FMap (_ : FMapMonad, a : Async<_>, mapper) = async {
      let! x = a
      return mapper x }

  [<Sealed>]
  type ApplyMonad () =
    static member inline Apply (_ : ApplyMonad, o : _ option, applicant) =
      match o with
      | Some x ->
        applicant x
      | _      -> ()
      o
    static member inline Apply (_ : ApplyMonad, n : Nullable<_>, applicant) =
      match n.HasValue with
      | true ->
        applicant n.Value
      | _ -> ()
      n
    static member inline Apply (_ : ApplyMonad, a : Async<_>, applicant) = async {
      let! x = a
      do! applicant x
      return x }

  [<Sealed>]
  type MkTupleMonad () =
    static member inline MkTuple (_ : MkTupleMonad, o : _ option, supply) =
      match o with
      | Some x -> Some (x, supply x)
      | _      -> None
    static member inline MkTuple (_ : MkTupleMonad, a : Async<_>, supply) = async {
      let! x = a
      let! y = supply x
      return (x, y) }


  let inline (>>=) (source : ^a) (binder : ^b) : ^c
    when ^x :> BindMonad
    and  (^a or ^x) : (static member Bind : ^x * ^a * ^b -> ^c)
    = ((^a or ^x) : (static member Bind : ^x * ^a * ^b -> ^c) (Unchecked.defaultof<_>, source, binder))

  let inline (>>|) (source : ^a) (mapper : ^b) : ^c
    when ^x :> FMapMonad
    and  (^a or ^x) : (static member FMap : ^x * ^a * ^b -> ^c)
    = ((^a or ^x) : (static member FMap : ^x * ^a * ^b -> ^c) (Unchecked.defaultof<_>, source, mapper))

  let inline (>>*) (source : ^a) (applicant : ^b) : ^c
    when ^x :> ApplyMonad
    and  (^a or ^x) : (static member Apply : ^x * ^a * ^b -> ^c)
    = ((^a or ^x) : (static member Apply : ^x * ^a * ^b -> ^c) (Unchecked.defaultof<_>, source, applicant))

  let inline (>>+) (source : ^a) (supply : ^b) : ^c
    when ^x :> MkTupleMonad
    and  (^a or ^x) : (static member MkTuple : ^x * ^a * ^b -> ^c)
    = ((^a or ^x) : (static member MkTuple : ^x * ^a * ^b -> ^c) (Unchecked.defaultof<_>, source, supply))