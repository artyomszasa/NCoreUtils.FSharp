namespace NCoreUtils
open System
#nowarn "64"

[<AutoOpen>]
module BindTopLevelOperator =

  [<Sealed; AbstractClass>]
  type BindMonad private () =
    static member inline Bind (_ : BindMonad, o : _ option, binder) =
      match o with
      | Some x -> binder x
      | _      -> None
    static member inline Bind (_ : BindMonad, n : Nullable<_>, binder) = Nullable.bind binder n
    static member inline Bind (_ : BindMonad, a : Async<_>, binder) = async.Bind (a, binder)
    static member inline Bind (_ : BindMonad, res : Result<_,_>, binder) =
      match res with
      | Ok    value -> binder value
      | Error error -> Error  error

  [<Sealed; AbstractClass>]
  type AsyncBindMonad private () =
    static member inline AsyncBind (_ : AsyncBindMonad, o : _ option, binder) =
      match o with
      | Some x -> binder x
      | _      -> async.Return None
    static member inline AsyncBind (_ : AsyncBindMonad, res : Result<_,_>, binder) =
      match res with
      | Ok    value -> binder value
      | Error error -> async.Return <| Error error

  [<Sealed; AbstractClass>]
  type FMapMonad private () =
    static member inline FMap (_ : FMapMonad, o : _ option, mapper) =
      match o with
      | Some x -> Some (mapper x)
      | _      -> None
    static member inline FMap (_ : FMapMonad, n : Nullable<_>, mapper) = Nullable.map mapper n
    static member inline FMap (_ : FMapMonad, a : Async<_>, mapper) =
      async.Bind (a, mapper >> async.Return)
    static member inline FMap (_ : FMapMonad, res : Result<_,_>, binder) =
      match res with
      | Ok    value -> Ok (binder value)
      | Error error -> Error error

  [<Sealed; AbstractClass>]
  type ApplyMonad private () =
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
    static member inline Apply (_ : ApplyMonad, res : Result<_,_>, applicant) =
      match res with
      | Ok value -> applicant value
      | _        -> ()
      res

  [<Sealed; AbstractClass>]
  type MkTupleMonad private () =
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

  let inline (=<<) (binder : ^b) (source : ^a) : ^c
    when ^x :> BindMonad
    and  (^a or ^x) : (static member Bind : ^x * ^a * ^b -> ^c)
    = ((^a or ^x) : (static member Bind : ^x * ^a * ^b -> ^c) (Unchecked.defaultof<_>, source, binder))


  let inline (>>!) (source : ^a) (binder : ^b) : ^c
    when ^x :> AsyncBindMonad
    and  (^a or ^x) : (static member AsyncBind : ^x * ^a * ^b -> ^c)
    = ((^a or ^x) : (static member AsyncBind : ^x * ^a * ^b -> ^c) (Unchecked.defaultof<_>, source, binder))


  let inline (>>|) (source : ^a) (mapper : ^b) : ^c
    when ^x :> FMapMonad
    and  (^a or ^x) : (static member FMap : ^x * ^a * ^b -> ^c)
    = ((^a or ^x) : (static member FMap : ^x * ^a * ^b -> ^c) (Unchecked.defaultof<_>, source, mapper))

  let inline (|<<) (mapper : ^b) (source : ^a) : ^c
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


  let inline (>>=!) (source : Async<(^a)>) (binder : ^b) : Async<(^c)> =
    async.Bind (source, fun value -> value >>! binder)

  let inline (>>!=) (source : Async<(^a)>) (binder : ^b) : Async<(^c)> =
    source >>| ((=<<) binder)

  let inline (>>!|) (source : Async<(^a)>) (mapper : ^b) : Async<(^c)> =
    source >>| ((|<<) mapper)
