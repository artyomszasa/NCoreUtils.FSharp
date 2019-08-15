namespace NCoreUtils

open System.Runtime.CompilerServices

[<AutoOpen>]
module ValueOptionTopLevelOperators =

  [<CompiledName("ValueFst")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let vfst struct (x, _) = x

  [<CompiledName("ValueSnd")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let vsnd struct (_, x) = x