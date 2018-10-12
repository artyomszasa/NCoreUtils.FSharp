namespace NCoreUtils

open System
open System.Runtime.CompilerServices

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Option =

  [<Obsolete("Use Option.ofObj instead")>]
  let inline wrap (x : 'a when 'a : not struct) =
    match box x with
    | null -> None
    | _    -> Some x

  [<Obsolete("Use Option.toObj instead")>]
  let inline unwrap (x : 'a option when 'a : null) =
    match x with
    | Some x -> x
    | _      -> null

  let inline getOrDef defaultValue o =
    match o with
    | Some x -> x
    | _      -> defaultValue

  let inline getOrUndef o =
    match o with
    | Some x -> x
    | _      -> Unchecked.defaultof<_>

  let inline ofNullable (n : System.Nullable<_>) =
    match n.HasValue with
    | true -> Some n.Value
    | _    -> None

  let inline trySupply supply o =
    match o with
    | None -> supply ()
    | _    -> o

[<Obsolete("Use generalized version instead")>]
module OptionTopLevelOperators =

  let (|?=) option supply = Option.trySupply supply option
