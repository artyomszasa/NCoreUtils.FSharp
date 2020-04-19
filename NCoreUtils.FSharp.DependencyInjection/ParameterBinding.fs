namespace NCoreUtils

open System
open System.Reflection
open Microsoft.FSharp.Reflection
open System.ComponentModel
open System.Runtime.CompilerServices
open System.Collections.Generic
open Microsoft.Extensions.DependencyInjection

[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Struct ||| AttributeTargets.Property, AllowMultiple = false)>]
type ParameterNameAttribute (name : string) =
  inherit Attribute ()
  static member inline internal GetName (attr : ParameterNameAttribute) = attr.Name
  member val Name = name

[<AttributeUsage(AttributeTargets.Property, AllowMultiple = false)>]
type ParameterBinderAttribute (binderType : Type) =
  inherit Attribute ()
  member val BinderType = binderType

type ParameterDescriptor = {
  Path       : string
  Attributes : ICustomAttributeProvider
  Type       : Type }

type IValueBinder =
  abstract AsyncBind : descriptor:ParameterDescriptor * tryGetParameters:(string -> string list option) -> Async<obj>

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ParameterDescriptor =

  let ofProperty path (prop : PropertyInfo) =
    { Path       = path
      Attributes = prop
      Type       = prop.PropertyType }

exception MissingParameterException of ParameterName:string

[<AutoOpen>]
module ParameterBinding =

  type ICustomAttributeProvider with
    member internal this.TryGetAttribute<'attribute when 'attribute :> Attribute> (?``inherit``) =
      let attrs = this.GetCustomAttributes (typeof<'attribute>, defaultArg ``inherit`` true)
      match attrs.Length with
      | 0 -> ValueNone
      | _ -> ValueSome (attrs.[0] :?> 'attribute)


  let internal noCustomAttributes =
    let emptyArray = Array.empty<obj>
    { new ICustomAttributeProvider with
        member __.IsDefined (_, _)           = false
        member __.GetCustomAttributes _      = emptyArray
        member __.GetCustomAttributes (_, _) = emptyArray
    }

  let internal binderTypeAttributes binderType =
    let attrs = [| ParameterBinderAttribute binderType :> obj |]
    let emptyArray = Array.empty<obj>
    { new ICustomAttributeProvider with
        member __.IsDefined (ty, _)           = ty = typeof<ParameterBinderAttribute>
        member __.GetCustomAttributes  _      = attrs
        member __.GetCustomAttributes (ty, _) = if ty = typeof<ParameterBinderAttribute> then attrs else emptyArray
    }

  let private concatPath a b =
    match String.IsNullOrEmpty a with
    | true -> b
    | _    ->
    match String.IsNullOrEmpty b with
    | true -> a
    | _    -> sprintf "%s.%s" a b

  let inline private mkRecord recordType values = FSharpValue.MakeRecord (recordType, values, true)

  // ************************************************************************
  // DEFAULT BINDERS

  let private missing parameterName = MissingParameterException parameterName |> raise

  type DefaultParameterBinder<'a> () =
    abstract Convert : obj -> 'a
    default __.Convert raw =
      let converter = TypeDescriptor.GetConverter typeof<'a>
      converter.ConvertFrom raw :?> 'a
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.AsyncBind (descriptor : ParameterDescriptor, tryGetParameters) =
      let value =
        match tryGetParameters descriptor.Path with
        | Some (raw :: _) -> this.Convert raw
        | _               ->
          match descriptor.Attributes.TryGetAttribute<DefaultValueAttribute> () with
          | ValueSome attr ->
            match attr.Value with
            | :? 'a as defaultValue -> defaultValue
            | defaultValue          -> this.Convert defaultValue
          | _ -> missing descriptor.Path
      async.Return <| box value
    interface IValueBinder with
      member this.AsyncBind (descriptor, tryGetParameters) = this.AsyncBind (descriptor, tryGetParameters)

  type DefaultStringParameterBinder () =
    inherit DefaultParameterBinder<string> ()
    override __.Convert raw = if isNull raw then null else raw.ToString ()

  type DefaultGuidParameterBinder () =
    inherit DefaultParameterBinder<Guid> ()
    override __.Convert raw =
      match raw with
      | null -> Guid.Empty
      | :? string   as string -> Guid.Parse string
      | :? (byte[]) as bytes  -> Guid bytes
      | _ -> invalidOpf "Unable to convert object of type %s to Guid" <| raw.GetType().FullName

  type EnumParameterBinder<'e when 'e : (new : unit -> 'e) and 'e :> ValueType and 'e : struct> () =
    inherit DefaultParameterBinder<'e> ()
    static let underlyingType = Enum.GetUnderlyingType typeof<'e>
    override __.Convert raw =
      match raw with
      | null -> invalidOpf "null cannot be converted to %s" typeof<'e>.FullName
      | :? string as str ->
        let mutable result = Unchecked.defaultof<_>
        match Enum.TryParse<'e> (str, true, &result) with
        | true -> result
        | _ ->
          try
            let i = Convert.ChangeType (str, underlyingType)
            Enum.ToObject (typeof<'e>, i) :?> 'e
          with _ ->
            invalidOpf "\"%s\" cannot be converted to %s" str typeof<'e>.FullName
      | obj when obj.GetType () = underlyingType ->
        Enum.ToObject (typeof<'e>, obj) :?> 'e
      | _ -> invalidOpf "Object of type %s cannot be converted to %s" (raw.GetType().FullName) typeof<'e>.FullName

  [<Struct>]
  [<NoEquality; NoComparison>]
  type TypeOrInstance =
    | BinderType     of BinderType:Type
    | BinderInstance of BinderInstance:IValueBinder

  // ************************************************************************

  let tryGetDefaultBinder =
    let dict = Dictionary ()
    [ typeof<string>,   DefaultStringParameterBinder     () :> IValueBinder |> BinderInstance
      typeof<byte>,     DefaultParameterBinder<byte>     () :> IValueBinder |> BinderInstance
      typeof<uint16>,   DefaultParameterBinder<uint16>   () :> IValueBinder |> BinderInstance
      typeof<uint32>,   DefaultParameterBinder<uint32>   () :> IValueBinder |> BinderInstance
      typeof<uint64>,   DefaultParameterBinder<uint64>   () :> IValueBinder |> BinderInstance
      typeof<sbyte>,    DefaultParameterBinder<sbyte >   () :> IValueBinder |> BinderInstance
      typeof<int16>,    DefaultParameterBinder<int16>    () :> IValueBinder |> BinderInstance
      typeof<int32>,    DefaultParameterBinder<int32>    () :> IValueBinder |> BinderInstance
      typeof<int64>,    DefaultParameterBinder<int64>    () :> IValueBinder |> BinderInstance
      typeof<single>,   DefaultParameterBinder<single>   () :> IValueBinder |> BinderInstance
      typeof<float>,    DefaultParameterBinder<float>    () :> IValueBinder |> BinderInstance
      typeof<char>,     DefaultParameterBinder<char>     () :> IValueBinder |> BinderInstance
      typeof<DateTime>, DefaultParameterBinder<DateTime> () :> IValueBinder |> BinderInstance
      typeof<TimeSpan>, DefaultParameterBinder<TimeSpan> () :> IValueBinder |> BinderInstance
      typeof<Guid>,     DefaultGuidParameterBinder       () :> IValueBinder |> BinderInstance ]
    |> List.iter (fun (k, v) -> dict.Add (k, v))
    fun ``type`` ->
      let mutable binder = Unchecked.defaultof<_>
      match dict.TryGetValue (``type``, &binder) with
      | true -> Some binder
      | _    ->
      match ``type``.IsEnum with
      | true -> typedefof<EnumParameterBinder<_>>.MakeGenericType ``type`` |> BinderType |> Some
      | _    -> None

  let rec asyncBindParameter (serviceProvider : IServiceProvider) tryGetParameters (descriptor : ParameterDescriptor) =
    match descriptor.Type with
    | t when t = typeof<unit> -> async.Return (box ())
    | _ ->
      let path =
        let usePropertyName () =
          match descriptor.Attributes with
          | :? PropertyInfo as prop -> ValueSome prop.Name
          | _ -> ValueNone
        descriptor.Attributes.TryGetAttribute<ParameterNameAttribute> ()
        >>| ParameterNameAttribute.GetName
        |?= usePropertyName
        |?  String.Empty
        |>  concatPath descriptor.Path
      match FSharpType.IsRecord (descriptor.Type, true) with
      | true ->
        FSharpType.GetRecordFields (descriptor.Type, true)
        |>  Seq.map (ParameterDescriptor.ofProperty path >> asyncBindParameter serviceProvider tryGetParameters)
        |>  Async.Sequential
        >>| mkRecord descriptor.Type
      | _ ->
        let binder =
          match descriptor.Attributes.TryGetAttribute<ParameterBinderAttribute> () with
          | ValueSome attr -> ActivatorUtilities.CreateInstance (serviceProvider, attr.BinderType) :?> IValueBinder
          | _         ->
          match tryGetDefaultBinder descriptor.Type with
          | Some (BinderType binderType) -> ActivatorUtilities.CreateInstance (serviceProvider, binderType) :?> IValueBinder
          | Some (BinderInstance binder) -> binder
          | _ -> invalidOpf "no default binder for type %s" descriptor.Type.FullName
        binder.AsyncBind ({ descriptor with Path = path }, tryGetParameters)

  let asyncBindObject serviceProvider tryGetValues path ``type`` attributes =
    asyncBindParameter serviceProvider tryGetValues { Path = path; Type = ``type``; Attributes = attributes }

  let asyncBindObjectNoAttrs serviceProvider tryGetValues path ``type`` =
    asyncBindObject serviceProvider tryGetValues path ``type`` noCustomAttributes

  let asyncBindObjectBy serviceProvider tryGetValues path ``type`` binderType =
    binderTypeAttributes binderType |> asyncBindObject serviceProvider tryGetValues path ``type``

  [<RequiresExplicitTypeArguments>]
  let asyncBind<'a> serviceProvider tryGetValues path attributes = async {
    let! boxed = asyncBindObject serviceProvider tryGetValues path typeof<'a> attributes
    return boxed :?> 'a }

  [<RequiresExplicitTypeArguments>]
  let asyncBindNoAttrs<'a> serviceProvider tryGetValues path = async {
    let! boxed = asyncBindObject serviceProvider tryGetValues path typeof<'a> noCustomAttributes
    return boxed :?> 'a }