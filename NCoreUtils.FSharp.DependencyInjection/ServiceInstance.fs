namespace NCoreUtils

open System
open System.Runtime.CompilerServices
open System.Collections.Concurrent
open Microsoft.Extensions.DependencyInjection

type IServiceInstance =
  inherit IDisposable
  abstract member BoxedService : obj


type IServiceInstance<'TService when 'TService : not struct> =
  inherit IServiceInstance
  abstract member Service : 'TService

[<AbstractClass>]
type private Factory () =
  static let cache = ConcurrentDictionary<Type, Factory> ()
  static let create = Func<_, _>(fun (ty : Type) -> Activator.CreateInstance(typedefof<Factory<_>>.MakeGenericType ty, true) :?> Factory)
  abstract GetDisposableInstance : instance:obj -> IServiceInstance
  abstract GetNonDisposableInstance : instance:obj -> IServiceInstance
  static member GetDisposableInstance (serviceType, instance) = cache.GetOrAdd(serviceType, create).GetDisposableInstance instance
  static member GetNonDisposableInstance (serviceType, instance) = cache.GetOrAdd(serviceType, create).GetNonDisposableInstance instance


and [<Sealed>] private Factory<'service when 'service : not struct> () =
  inherit Factory ()
  override __.GetDisposableInstance instance =
    let disposable = instance :?> IDisposable
    let service = instance :?> 'service
    { new IServiceInstance<'service> with
        member __.BoxedService = box service
        member __.Service      = service
        member __.Dispose ()   = disposable.Dispose ()
    } :> _
  override __.GetNonDisposableInstance instance =
    let service = instance :?> 'service
    { new IServiceInstance<'service> with
        member __.BoxedService = box service
        member __.Service      = service
        member __.Dispose ()   = ()
    } :> _

[<Sealed; AbstractClass>]
[<Extension>]
type ServiceProviderServiceInstanceExtensions =

  [<Extension>]
  static member GetOrActivateService (this : IServiceProvider, serviceType : Type) =
    match this.GetService serviceType with
    | null ->
      let instance = ActivatorUtilities.CreateInstance (this, serviceType)
      match typeof<IDisposable>.IsAssignableFrom serviceType with
      | true -> Factory.GetDisposableInstance (serviceType, instance)
      | _    -> Factory.GetNonDisposableInstance (serviceType, instance)
    | instance -> Factory.GetNonDisposableInstance (serviceType, instance)

  [<Extension>]
  static member GetOrActivateService<'TService when 'TService : not struct> (this : IServiceProvider) : IServiceInstance<'TService> =
    match this.GetService typeof<'TService> with
    | null ->
      let instance = ActivatorUtilities.CreateInstance (this, typeof<'TService>) :?> 'TService
      match box instance with
      | :? IDisposable as disposable ->
        { new IServiceInstance<'TService> with
            member __.BoxedService = box instance
            member __.Service      = instance
            member __.Dispose ()   = disposable.Dispose ()
        }
      | _ ->
        { new IServiceInstance<'TService> with
            member __.BoxedService = box instance
            member __.Service      = instance
            member __.Dispose ()   = ()
        }
    | instance ->
        { new IServiceInstance<'TService> with
            member __.BoxedService = instance
            member __.Service      = instance :?> 'TService
            member __.Dispose ()   = ()
        }
