namespace NCoreUtils

#nowarn "64"

open System
open System.Collections.Generic
open System.Diagnostics
open System.Globalization
open System.Runtime.CompilerServices

[<AutoOpen>]
module Common =

  [<Sealed>]
  type ValueSupply =
    static member SupplyValue (_ : ValueSupply, v : 'a, o : 'a option) = defaultArg o v
    static member SupplyValue (_ : ValueSupply, v : 'a, o : Nullable<'a>) = if o.HasValue then o.Value else v

  let inline (|?) (optional : ^b) (value : ^a) : ^a
    when ^x :> ValueSupply
    and  (^a or ^x) : (static member SupplyValue : ^x * ^a * ^b -> ^a)
    = ((^a or ^x) : (static member SupplyValue : ^x * ^a * ^b -> ^a) (Unchecked.defaultof<ValueSupply>, value, optional))

  let inline (|??) (nullValue : 'a) (defaultValue : 'a) : 'a when 'a : null = if isNull nullValue then defaultValue else nullValue

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  let (|EQI|_|) (x : string) (y : string) =
    match StringComparer.InvariantCultureIgnoreCase.Equals (x, y) with
    | true -> Some ()
    | _    -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  [<CompiledName("TryParseBool")>]
  let tryBool input =
    match input with
    | null -> None
    | _    ->
      let mutable b = Unchecked.defaultof<_>
      match Boolean.TryParse (input, &b) with
      | true -> Some b
      | _    -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  [<CompiledName("TryParseSByte")>]
  let tryInt8 input =
    match input with
    | null -> None
    | _    ->
      let mutable i = Unchecked.defaultof<_>
      match SByte.TryParse (input, NumberStyles.Integer, CultureInfo.InvariantCulture, &i) with
      | true -> Some i
      | _    -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  [<CompiledName("TryParseInt16")>]
  let tryInt16 input =
    match input with
    | null -> None
    | _    ->
      let mutable i = Unchecked.defaultof<_>
      match Int16.TryParse (input, NumberStyles.Integer, CultureInfo.InvariantCulture, &i) with
      | true -> Some i
      | _    -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  [<CompiledName("TryParseInt32")>]
  let tryInt32 input =
    match input with
    | null -> None
    | _    ->
      let mutable i = Unchecked.defaultof<_>
      match Int32.TryParse (input, NumberStyles.Integer, CultureInfo.InvariantCulture, &i) with
      | true -> Some i
      | _    -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  [<CompiledName("TryParseInt64")>]
  let tryInt64 input =
    match input with
    | null -> None
    | _    ->
      let mutable i = Unchecked.defaultof<_>
      match Int64.TryParse (input, NumberStyles.Integer, CultureInfo.InvariantCulture, &i) with
      | true -> Some i
      | _    -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  [<CompiledName("TryEnum")>]
  [<RequiresExplicitTypeArguments>]
  let tryEnum<'e when 'e : (new : unit -> 'e) and 'e : struct and 'e :> ValueType> input =
    match input with
    | null -> None
    | _    ->
      let mutable v = Unchecked.defaultof<_>
      match Enum.TryParse<'e>(input, true, &v) with
      | true -> Some v
      | _    -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  [<CompiledName("TrySingle")>]
  let trySingle input =
    match input with
    | null -> None
    | _    ->
      let mutable v = Unchecked.defaultof<_>
      match System.Single.TryParse (input, NumberStyles.Float, CultureInfo.InvariantCulture, &v) with
      | true -> Some v
      | _    -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  [<CompiledName("TryDouble")>]
  let tryDouble input =
    match input with
    | null -> None
    | _    ->
      let mutable v = Unchecked.defaultof<_>
      match System.Double.TryParse (input, NumberStyles.Float, CultureInfo.InvariantCulture, &v) with
      | true -> Some v
      | _    -> None

  // aliases

  let inline tryInt input = tryInt32 input

  let inline trySByte input = tryInt8 input

  let inline tryFloat input = tryDouble input

  // extensions

  [<CompiledName("PrintFormatToStringThenThrowInvalidOperationException")>]
  [<DebuggerStepThrough>]
  let invalidOpf fmt = Printf.kprintf invalidOp fmt

  [<CompiledName("PrintFormatToStringThenThrowArgumentException")>]
  [<DebuggerStepThrough>]
  let invalidArgf argumentName fmt = Printf.kprintf (invalidArg argumentName) fmt

  [<CompiledName("ThrowNotImplementedException")>]
  [<DebuggerStepThrough>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let notImplemented message = NotImplementedException message |> raise

  [<CompiledName("PrintFormatToStringThenThrowNotImplementedException")>]
  [<DebuggerStepThrough>]
  let notImplementdf fmt = Printf.kprintf notImplemented fmt

  [<CompiledName("ThrowNotSupportedException")>]
  [<DebuggerStepThrough>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let notSupported message = NotSupportedException message |> raise

  [<CompiledName("PrintFormatToStringThenThrowNotSupportedException")>]
  [<DebuggerStepThrough>]
  let notSupportedf fmt = Printf.kprintf notSupported fmt

  [<CompiledName("ThrowFormatException")>]
  [<DebuggerStepThrough>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let invalidFmt message = FormatException message |> raise

  [<CompiledName("PrintFormatToStringThenThrowFormatException")>]
  [<DebuggerStepThrough>]
  let invalidFmtf fmt = Printf.kprintf invalidFmt fmt

  type ReferenceEqualityComparer<'a> () =
    interface IEqualityComparer<'a> with
      member __.Equals (a, b) = obj.ReferenceEquals (a, b)
      member __.GetHashCode obj = RuntimeHelpers.GetHashCode obj

  [<CompiledName("TryGetException")>]
  [<DebuggerStepThrough>]
  let tryGetExn<'exn when 'exn :> exn> exn =
    let alreadyExpected = HashSet (ReferenceEqualityComparer<exn> ())
    let rec impl (exn : exn) =
      match alreadyExpected.Add exn with
      | false -> None
      | _     ->
      match exn with
      | :? 'exn               as e  -> Some e
      | :? AggregateException as ae -> ae.InnerExceptions |> Seq.tryPick impl
      | _                           -> None
    impl exn