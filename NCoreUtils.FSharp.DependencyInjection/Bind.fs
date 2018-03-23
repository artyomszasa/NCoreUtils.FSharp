module NCoreUtils.DependencyInjection

open System
open Microsoft.FSharp.Reflection

[<RequiresExplicitTypeArguments>]
let inline tryService<'a> (serviceProvider : IServiceProvider) =
  match serviceProvider.GetService typeof<'a> with
  | null  -> None
  | boxed -> Some (boxed :?> 'a)

[<RequiresExplicitTypeArguments>]
let inline getService<'a> (serviceProvider : IServiceProvider) =
  match serviceProvider.GetService typeof<'a> with
  | null  -> invalidOpf "Unable to get service for %s" typeof<'a>.FullName
  | boxed -> boxed :?> 'a

let inline private getSvc (serviceProvider : IServiceProvider) ``type`` =
  match ``type`` with
  | null  -> nullArg "type"
  | _     ->
  match serviceProvider.GetService ``type`` with
  | null  -> invalidOpf "Unable to get service for %s" ``type``.FullName
  | boxed -> boxed :?> 'a

[<CompiledName("BindServices")>]
[<RequiresExplicitTypeArguments>]
let bindServices<'a> (serviceProvider : IServiceProvider) =
  match FSharpType.IsRecord (typeof<'a>, true) with
  | true ->
    let props  = FSharpType.GetRecordFields (typeof<'a>, true)
    let count  = props.Length
    let values = Array.zeroCreate count
    for i = 0 to count - 1 do
      values.[i] <- getSvc serviceProvider props.[i].PropertyType
    FSharpValue.MakeRecord (typeof<'a>, values, true) :?> 'a
  | _ -> getService<'a> serviceProvider

let asyncBindAndExecute (serviceProvider : #IServiceProvider) tryGetParameters (action : _ -> 'services -> 'parameters -> Async<_>) =
  let services = bindServices<'services> serviceProvider
  ParameterBinding.asyncBindNoAttrs<'parameters> serviceProvider tryGetParameters null
  >>= action serviceProvider services

let asyncBindAndExecuteWith (context : 'context) (getServiceProvider : 'context -> IServiceProvider) tryGetParameters (action : 'context -> 'services -> 'parameters -> Async<_>) =
  let serviceProvider = getServiceProvider context
  let services = bindServices<'services> serviceProvider
  ParameterBinding.asyncBindNoAttrs<'parameters> serviceProvider tryGetParameters null
  >>= action context services
