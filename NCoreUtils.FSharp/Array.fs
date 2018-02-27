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