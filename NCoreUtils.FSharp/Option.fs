namespace NCoreUtils

open System.Runtime.CompilerServices

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Option =

  [<CompiledName("Wrap")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let wrap (x : 'a when 'a : not struct) =
    match box x with
    | null -> None
    | _    -> Some x

  [<CompiledName("Unwrap")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let unwrap (x : 'a option when 'a : null) =
    match x with
    | Some x -> x
    | _      -> null

  [<CompiledName("GetOrDefault")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let getOrDef defaultValue o =
    match o with
    | Some x -> x
    | _      -> defaultValue

  [<CompiledName("GetOrDefault")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let getOrUndef o =
    match o with
    | Some x -> x
    | _      -> Unchecked.defaultof<_>

  [<CompiledName("OfNullable")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let ofNullable (n : System.Nullable<_>) =
    match n.HasValue with
    | true -> Some n.Value
    | _    -> None

[<AutoOpen>]
module OptionTopLevelOperators =

  let (!?) = Option.getOrUndef
