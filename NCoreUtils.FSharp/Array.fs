namespace NCoreUtils

open System.Diagnostics
open System.Runtime.CompilerServices

/// Contains array related function.
[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Array =

  /// Swaps two elements of the array specified by their indices.
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  let swap n m array =
    let tmp = Array.get array n
    Array.set array n (Array.get array m)
    Array.set array m tmp

  /// Performs reverse operation in place.
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  [<DebuggerStepThrough>]
  let revInPlace array =
    let l = Array.length array / 2
    for i = 0 to l - 1 do
      swap i (l - i) array

  [<CompiledName("TryMin")>]
  let tryMin (array : _[]) =
    match array.Length with
    | 0      -> None
    | length ->
      let mutable result = array.[0]
      let mutable i = 1
      while i < length do
        if array.[i] > result then
          result <- array.[i]
      Some result

  [<CompiledName("TryMinBy")>]
  let tryMinBy keySelector (array : _[]) =
    match array.Length with
    | 0      -> None
    | length ->
      let mutable result = array.[0]
      let mutable key = keySelector result
      let mutable i = 1
      while i < length do
        let current = array.[i]
        let currentKey = keySelector current
        if currentKey > key then
          result <- current
          key    <- currentKey
      Some result

  [<CompiledName("TryMax")>]
  let tryMax (array : _[]) =
    match array.Length with
    | 0      -> None
    | length ->
      let mutable result = array.[0]
      let mutable i = 1
      while i < length do
        if array.[i] < result then
          result <- array.[i]
      Some result

  [<CompiledName("TryMaxBy")>]
  let tryMaxBy keySelector (array : _[]) =
    match array.Length with
    | 0      -> None
    | length ->
      let mutable result = array.[0]
      let mutable key = keySelector result
      let mutable i = 1
      while i < length do
        let current = array.[i]
        let currentKey = keySelector current
        if currentKey < key then
          result <- current
          key    <- currentKey
      Some result

  [<CompiledName("TryMinAsValue")>]
  let tryMinValue (array : _[]) =
    match array.Length with
    | 0      -> ValueNone
    | length ->
      let mutable result = array.[0]
      let mutable i = 1
      while i < length do
        if array.[i] > result then
          result <- array.[i]
      ValueSome result

  [<CompiledName("TryMinAsValueBy")>]
  let tryMinValueBy keySelector (array : _[]) =
    match array.Length with
    | 0      -> ValueNone
    | length ->
      let mutable result = array.[0]
      let mutable key = keySelector result
      let mutable i = 1
      while i < length do
        let current = array.[i]
        let currentKey = keySelector current
        if currentKey > key then
          result <- current
          key    <- currentKey
      ValueSome result

  [<CompiledName("TryMaxAsValue")>]
  let tryMaxValue (array : _[]) =
    match array.Length with
    | 0      -> ValueNone
    | length ->
      let mutable result = array.[0]
      let mutable i = 1
      while i < length do
        if array.[i] < result then
          result <- array.[i]
      ValueSome result

  [<CompiledName("TryMaxAsValueBy")>]
  let tryMaxValueBy keySelector (array : _[]) =
    match array.Length with
    | 0      -> ValueNone
    | length ->
      let mutable result = array.[0]
      let mutable key = keySelector result
      let mutable i = 1
      while i < length do
        let current = array.[i]
        let currentKey = keySelector current
        if currentKey < key then
          result <- current
          key    <- currentKey
      ValueSome result