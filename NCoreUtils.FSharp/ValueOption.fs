namespace NCoreUtils

open System
open System.Runtime.CompilerServices

[<RequireQualifiedAccess>]
[<CompiledName("NCoreUtilsValueOptionModule")>]
[<Obsolete("Since 4.6.x ValueOption functions are implemented in FSharp.Core.")>]
module ValueOption =

  [<CompiledName("Bind")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let bind binder voption =
    Microsoft.FSharp.Core.ValueOption.bind binder voption

  [<CompiledName("Contains")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let contains value voption =
    Microsoft.FSharp.Core.ValueOption.contains value voption

  [<CompiledName("Count")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let count voption =
    Microsoft.FSharp.Core.ValueOption.count voption

  [<CompiledName("DefaultValue")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let defaultValue ``default`` voption =
    Microsoft.FSharp.Core.ValueOption.defaultValue ``default`` voption

  [<CompiledName("DefaultWith")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let defaultWith defThunk voption =
    Microsoft.FSharp.Core.ValueOption.defaultWith defThunk voption

  [<CompiledName("Exists")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let exists predicate voption =
    Microsoft.FSharp.Core.ValueOption.exists predicate voption

  [<CompiledName("Filter")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let filter predicate voption =
    Microsoft.FSharp.Core.ValueOption.filter predicate voption

  [<CompiledName("Flatten")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let flatten voption =
    Microsoft.FSharp.Core.ValueOption.flatten voption

  [<CompiledName("Fold")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let fold folder state voption =
    Microsoft.FSharp.Core.ValueOption.fold folder state voption

  [<CompiledName("FoldBack")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let foldBack folder voption state =
    Microsoft.FSharp.Core.ValueOption.foldBack folder voption state

  [<CompiledName("ForAll")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let forall predicate voption =
    Microsoft.FSharp.Core.ValueOption.forall predicate voption

  [<CompiledName("GetValue")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let get voption =
    Microsoft.FSharp.Core.ValueOption.get voption

  [<CompiledName("IsNone")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let isNone voption =
    Microsoft.FSharp.Core.ValueOption.isNone voption

  [<CompiledName("IsSome")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let isSome voption =
    Microsoft.FSharp.Core.ValueOption.isSome voption

  [<CompiledName("Iterate")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let iter action voption =
    Microsoft.FSharp.Core.ValueOption.iter action voption

  [<CompiledName("Map")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let map mapping voption =
    Microsoft.FSharp.Core.ValueOption.map mapping voption

  [<CompiledName("Map2")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let map2 mapping voption1 voption2 =
    Microsoft.FSharp.Core.ValueOption.map2 mapping voption1 voption2

  [<CompiledName("OfNullable")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let ofNullable (nullable : Nullable<_>) =
    Microsoft.FSharp.Core.ValueOption.ofNullable nullable

  [<CompiledName("OfObj")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let ofObj obj =
    Microsoft.FSharp.Core.ValueOption.ofObj obj

  [<CompiledName("OrElse")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let orElse ifNone voption =
    Microsoft.FSharp.Core.ValueOption.orElse ifNone voption

  [<CompiledName("OrElseWith")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let orElseWith ifNoneThunk voption =
    Microsoft.FSharp.Core.ValueOption.orElseWith ifNoneThunk voption

  [<CompiledName("ToArray")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let toArray voption =
    Microsoft.FSharp.Core.ValueOption.toArray voption

  [<CompiledName("ToList")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let toList voption =
    Microsoft.FSharp.Core.ValueOption.toList voption

  [<CompiledName("ToNullable")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let toNullable voption =
    Microsoft.FSharp.Core.ValueOption.toNullable voption

  [<CompiledName("ToObj")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let toObj voption =
    Microsoft.FSharp.Core.ValueOption.toObj voption

[<AutoOpen>]
module ValueOptionTopLevelOperators =

  [<CompiledName("ValueFst")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let vfst struct (x, _) = x

  [<CompiledName("ValueSnd")>]
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  let vsnd struct (_, x) = x