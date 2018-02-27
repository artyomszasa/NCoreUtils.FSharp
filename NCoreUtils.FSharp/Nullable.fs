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