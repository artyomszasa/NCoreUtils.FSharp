namespace NCoreUtils.Synchronization

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module AsyncLock =

  [<CompiledName("AsyncLocked")>]
  let asyncLock (lock : IAsyncLock) f = async {
    do! lock.AsyncLock ()
    try return! f ()
    finally lock.Release () }
