namespace NCoreUtils

open System
open System.Runtime.InteropServices

// C# interop helpers

[<Serializable>]
type AsyncResult<'TError> internal (isSuccess : bool, err : 'TError) =
  member val IsSuccess = isSuccess
  member __.TryGetError ([<Out>] error : byref<'TError>) =
    match isSuccess with
    | true -> error <- Unchecked.defaultof<'TError>
    | _    -> error <- err
    not isSuccess
  member __.Match (onSuccess : Action, onError : Action<'TError>) =
    match isSuccess with
    | true -> onSuccess.Invoke ()
    | _    -> onError.Invoke err
  member __.Match<'TResult> (onSuccess : Func<'TResult>, onError : Func<'TError, 'TResult>) =
    match isSuccess with
    | true -> onSuccess.Invoke ()
    | _    -> onError.Invoke err

// FIXME: move to NCoreUtils.FSharp
[<Serializable>]
type AsyncResult<'T, 'TError> internal (isSuccess : bool, value : 'T, err : 'TError) =
  member val IsSuccess = isSuccess
  member __.TryGetError ([<Out>] error : byref<'TError>) =
    error <-
      match isSuccess with
      | true -> Unchecked.defaultof<'TError>
      | _    -> err
    not isSuccess
  member __.TryGetResult ([<Out>] result : byref<'T>) =
    result <-
      match isSuccess with
      | true -> value
      | _    -> Unchecked.defaultof<'T>
    not isSuccess
  member __.Extract ([<Out>] result : byref<'T>, [<Out>] error : byref<'TError>) =
    match isSuccess with
    | true ->
      result <- value
      error  <- Unchecked.defaultof<_>
      true
    | _ ->
      result <- Unchecked.defaultof<_>
      error  <- err
      false
  member __.Match (onSuccess : Action<'T>, onError : Action<'TError>) =
    match isSuccess with
    | true -> onSuccess.Invoke value
    | _    -> onError.Invoke err
  member __.Match<'TResult> (onSuccess : Func<'T, 'TResult>, onError : Func<'TError, 'TResult>) =
    match isSuccess with
    | true -> onSuccess.Invoke value
    | _    -> onError.Invoke err

[<Sealed; AbstractClass>]
type AsyncResult =
  static member Success<'TError> () = AsyncResult<'TError> (true, Unchecked.defaultof<_>)
  static member Error<'TError> error = AsyncResult<'TError> (false, error)
  static member Success<'T, 'TError> result = AsyncResult<'T, 'TError> (true, result, Unchecked.defaultof<_>)
  static member Error<'T, 'TError> error = AsyncResult<'T, 'TError> (false, Unchecked.defaultof<_>, error)

[<AutoOpen>]
module AsyncResult =

  type AsyncResult<'TError> with
    static member FromResult =
      function
      | Ok    ()    -> AsyncResult.Success<_> ()
      | Error error -> AsyncResult.Error<'TError> error
    static member ToResult (ares : AsyncResult<'TError>) =
      let mutable error = Unchecked.defaultof<_>
      match ares.TryGetError (&error) with
      | true -> Error error
      | _    -> Ok ()

  type AsyncResult<'T, 'TError> with
    static member FromResult =
      function
      | Ok    result -> AsyncResult.Success<'T, 'TError> result
      | Error error  -> AsyncResult.Error<_, _> error
    static member ToResult (ares : AsyncResult<'T, 'TError>) =
      let mutable error = Unchecked.defaultof<_>
      let mutable result = Unchecked.defaultof<_>
      match ares.Extract (&result, &error) with
      | true -> Error error
      | _    -> Ok result
