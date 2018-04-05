namespace NCoreUtils.AspNetCore

open Microsoft.AspNetCore.Builder
open System.Runtime.CompilerServices

[<Sealed; AbstractClass>]
[<Extension>]
type RequestErrorApplicationBuilderExtensions =

  [<Extension>]
  static member UseRequestErrorHandler (this : IApplicationBuilder) =
    this.Use RequestErrorHandlerMiddleware.run