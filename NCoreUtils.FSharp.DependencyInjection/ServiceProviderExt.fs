[<AutoOpen>]
module NCoreUtils.ServiceProviderExt

open System
open Microsoft.Extensions.DependencyInjection

[<RequiresExplicitTypeArguments>]
let inline getRequiredService<'a> (serviceProvider : IServiceProvider) : 'a =
  serviceProvider.GetRequiredService<'a> ()

[<RequiresExplicitTypeArguments>]
let inline tryGetService<'a> (serviceProvider : IServiceProvider) : 'a option =
  match serviceProvider.GetService typeof<'a> with
  | null  -> None
  | boxed -> Some <| unbox boxed

[<RequiresExplicitTypeArguments>]
let inline tryGetServiceAs<'a> implementationType (serviceProvider : IServiceProvider) : 'a option =
  match serviceProvider.GetService implementationType with
  | null  -> None
  | boxed -> Some <| unbox boxed


type IServiceProvider with
  [<RequiresExplicitTypeArguments>]
  member this.TryGetService<'a> () = tryGetService<'a> this