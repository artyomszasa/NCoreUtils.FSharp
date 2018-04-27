namespace NCoreUtils

open Microsoft.FSharp.Reflection
open System.Runtime.CompilerServices

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ReflectionTopLevelOperators =

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let (|TupleType|_|) ty =
    match FSharpType.IsTuple ty with
    | true -> FSharpType.GetTupleElements ty |> Some
    | _ -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let (|RecordType|_|) ty =
    match FSharpType.IsRecord (ty, true) with
    | true -> FSharpType.GetRecordFields (ty, true) |> Some
    | _ -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let (|UnionType|_|) ty =
    match FSharpType.IsUnion (ty, true) with
    | true -> FSharpType.GetUnionCases (ty, true) |> Some
    | _    -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let (|ExceptionType|_|) ty =
    match FSharpType.IsExceptionRepresentation (ty, true) with
    | true -> FSharpType.GetExceptionFields (ty, true) |> Some
    | _    -> None