namespace NCoreUtils

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Option =

  let inline wrap (x : 'a when 'a : not struct) =
    match box x with
    | null -> None
    | _    -> Some x

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

[<AutoOpen>]
module OptionTopLevelOperators =

  let (|?=) option supply = Option.trySupply supply option

[<AutoOpen>]
module ValueOptionTopLevelOperators =

  let (|?=) option supply =
    match option with
    | ValueSome _ -> option
    | _           -> supply ()
