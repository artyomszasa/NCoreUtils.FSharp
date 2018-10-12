namespace NCoreUtils

open System
open System.Runtime.CompilerServices

[<RequireQualifiedAccess>]
[<CompiledName("NCoreUtilsValueOptionModule")>]
module ValueOption =

  [<CompiledName("Bind")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let bind binder voption =
    match voption with
    | ValueSome v -> binder v
    | _           -> ValueNone

  [<CompiledName("Contains")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let contains value voption =
    match voption with
    | ValueSome v -> v = value
    | _           -> false

  [<CompiledName("Count")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let count voption =
    match voption with
    | ValueSome _ -> 1
    | _           -> 0

  [<CompiledName("DefaultValue")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let defaultValue ``default`` voption =
    match voption with
    | ValueSome v -> v
    | _           -> ``default``

  [<CompiledName("DefaultWith")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let defaultWith defThunk voption =
    match voption with
    | ValueSome v -> v
    | _           -> defThunk ()

  [<CompiledName("Exists")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let exists predicate voption =
    match voption with
    | ValueSome v -> predicate v
    | _           -> false

  [<CompiledName("Filter")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let filter predicate voption =
    match voption with
    | ValueSome v as result when predicate v -> result
    | _                                      -> ValueNone

  [<CompiledName("Flatten")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let flatten voption =
    match voption with
    | ValueSome x -> x
    | _           -> ValueNone

  [<CompiledName("Fold")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let fold folder state voption =
    match voption with
    | ValueSome v -> folder state v
    | _           -> state

  [<CompiledName("FoldBack")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let foldBack folder voption state =
    match voption with
    | ValueSome v -> folder v state
    | _           -> state

  [<CompiledName("ForAll")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let forall predicate voption =
    match voption with
    | ValueSome v -> predicate v
    | _           -> true

  [<CompiledName("GetValue")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let get voption =
    match voption with
    | ValueSome v -> v
    | _           -> ArgumentException ("Empty value", "voption") |> raise

  [<CompiledName("IsNone")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let isNone voption =
    match voption with
    | ValueSome _ -> false
    | _           -> true

  [<CompiledName("IsSome")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let isSome voption =
    match voption with
    | ValueSome _ -> true
    | _           -> false

  [<CompiledName("Iterate")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let iter action voption =
    match voption with
    | ValueSome v -> action v
    | _           -> ()

  [<CompiledName("Map")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let map mapping voption =
    match voption with
    | ValueSome v -> ValueSome <| mapping v
    | _           -> ValueNone

  [<CompiledName("Map2")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let map2 mapping voption1 voption2 =
    match voption1, voption2 with
    | ValueSome v1, ValueSome v2 -> ValueSome <| mapping v1 v2
    | _                          -> ValueNone

  [<CompiledName("OfNullable")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let ofNullable (nullable : Nullable<_>) =
    match nullable.HasValue with
    | true -> ValueSome nullable.Value
    | _    -> ValueNone

  [<CompiledName("OfObj")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let ofObj obj =
    match obj with
    | null -> ValueNone
    | _    -> ValueSome obj

  [<CompiledName("OrElse")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let orElse ifNone voption =
    match voption with
    | ValueSome _ as result -> result
    | _                     -> ifNone

  [<CompiledName("OrElseWith")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let orElseWith ifNoneThunk voption =
    match voption with
    | ValueSome _ as result -> result
    | _                     -> ifNoneThunk ()

  [<CompiledName("ToArray")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let toArray voption =
    match voption with
    | ValueSome v -> [| v |]
    | _           -> [|   |]

  [<CompiledName("ToList")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let toList voption =
    match voption with
    | ValueSome v -> [ v ]
    | _           -> [   ]

  [<CompiledName("ToNullable")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let toNullable voption =
    match voption with
    | ValueSome v -> Nullable v
    | _           -> Nullable ()

  [<CompiledName("ToObj")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let toObj voption =
    match voption with
    | ValueSome v -> v
    | _           -> null

[<AutoOpen>]
module ValueOptionTopLevelOperators =

  [<CompiledName("ValueFst")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let vfst struct (x, _) = x

  [<CompiledName("ValueSnd")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let vsnd struct (_, x) = x