namespace NCoreUtils
#nowarn "64"

open System
open System.Runtime.CompilerServices

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Nullable =

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let (|Empty|Value|) (x : Nullable<_>) =
    match x.HasValue with
    | true -> Value x.Value
    | _    -> Empty

  [<CompiledName("OfOption")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let ofOption o =
    match o with
    | Some x -> Nullable x
    | _      -> Nullable ()

  [<CompiledName("OfValueOption")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let ofValueOption o =
    match o with
    | ValueSome x -> Nullable x
    | _           -> Nullable ()

  [<GeneralizableValue>]
  [<CompiledName("Empty")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let empty<'a when 'a : (new : unit -> 'a) and 'a : struct and 'a :> ValueType> = Nullable<'a> ()

  [<CompiledName("Create")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let mk x = Nullable x

  [<CompiledName("Map")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let map f (n : Nullable<_>) =
    match n.HasValue with
    | true -> f n.Value |> mk
    | _    -> empty

  [<CompiledName("Filter")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let filter f (n : Nullable<_>) =
    match n.HasValue && f n.Value with
    | true -> n
    | _    -> empty

  [<CompiledName("Bind")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let bind f (n : Nullable<_>) =
    match n.HasValue with
    | true -> f n.Value
    | _    -> empty

  [<CompiledName("Iterate")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let iter f (n : Nullable<_>) =
    if n.HasValue then f n.Value

  [<CompiledName("DefaultWith")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let defaultWith defThunk (n : Nullable<_>) =
    match n.HasValue with
    | true -> n.Value
    | _    -> defThunk ()

  [<CompiledName("OrElse")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let orElse ifNone (n : Nullable<_>) =
    match n.HasValue with
    | true -> n
    | _    -> ifNone

  [<CompiledName("OrElseWith")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let orElseWith ifNoneThunk (n : Nullable<_>) =
    match n.HasValue with
    | true -> n
    | _    -> ifNoneThunk ()

  [<CompiledName("Count")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let count (n : Nullable<_>) = if n.HasValue then 1 else 0

  [<CompiledName("Fold")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let fold folder state (n : Nullable<_>) =
    match n.HasValue with
    | true -> folder state n.Value
    | _    -> state

  [<CompiledName("FoldBack")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let foldBack folder (n : Nullable<_>) state =
    match n.HasValue with
    | false -> state
    | _     -> folder n.Value state

  [<CompiledName("Exists")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let exists predicate (n : Nullable<_>) = n.HasValue && predicate n.Value

  [<CompiledName("ForAll")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let forall predicate (n : Nullable<_>) = not n.HasValue || predicate n.Value

  [<CompiledName("Contains")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let inline contains value (n : Nullable<_>) = n.HasValue && n.Value = value

  [<CompiledName("Map2")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let map2 mapping (n1 : Nullable<_>) (n2 : Nullable<_>) =
    match n1.HasValue, n2.HasValue with
    | true, true -> mapping n1.Value n2.Value |> mk
    | _          -> empty


[<AutoOpen>]
module NullableTopLevelExtra =

  [<Sealed>]
  type ToNullable () =
    static member Convert (_ : ToNullable, o : 'a option) = match o with | Some x -> Nullable x | _ -> Nullable ()

    static member Convert (_ : ToNullable, i : int) = Nullable i

    static member Convert (_ : ToNullable, d : decimal) = Nullable d

  let inline nullable (x : ^a) : Nullable< ^b>
    when ^x :> ToNullable
    and (^a or ^x) : (static member Convert : ^x * ^a -> Nullable< ^b>)
    = ((^a or ^x) : (static member Convert : ^x * ^a -> Nullable< ^b>) (Unchecked.defaultof< ^x>, x))
