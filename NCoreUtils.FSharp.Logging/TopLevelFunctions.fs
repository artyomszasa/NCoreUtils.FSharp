[<AutoOpen>]
module NCoreUtils.Logging.TopLevelFunctions

open System
open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging
open System.Text

let private noEventId = EventId -1

let private formatter =
  Func<string, exn, string>(fun message _ -> message)

let private exnFormatter =
  Func<string, exn, string>
    (fun message exn ->
      let builder = StringBuilder ()
      match exn with
      | null -> builder.Append(message).ToString ()
      | _    -> builder.AppendLine(message).Append(sprintf "%A" exn).ToString ())


[<CompiledName("Log")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let log (logger : ILogger) logLevel message  = logger.Log (logLevel, noEventId, message, null, formatter)

[<CompiledName("Log")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let logExn (logger : ILogger) logLevel exn message = logger.Log (logLevel, noEventId, message, exn, exnFormatter)

[<CompiledName("FormatThenLog")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let logf logger logLevel fmt = Printf.kprintf (log logger logLevel) fmt

[<CompiledName("FormatThenLog")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let logExnf logger logLevel exn fmt = Printf.kprintf (logExn logger logLevel exn) fmt

[<CompiledName("LogTrace")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let trace (logger : ILogger) message = logger.LogTrace message

[<CompiledName("LogTrace")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let traceExn (logger : ILogger) (exn : exn) message = logger.LogTrace (exn, message)

[<CompiledName("FormatThenLogTrace")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let tracef logger fmt = Printf.kprintf (trace logger) fmt

[<CompiledName("FormatThenLogTrace")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let traceExnf logger exn fmt = Printf.kprintf (traceExn logger exn) fmt

[<CompiledName("LogDebug")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let debug (logger : ILogger) message = logger.LogDebug message

[<CompiledName("LogDebug")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let debugExn (logger : ILogger) (exn : exn) message = logger.LogDebug (exn, message)

[<CompiledName("FormatThenLogDebug")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let debugf logger fmt = Printf.kprintf (debug logger) fmt

[<CompiledName("FormatThenLogDebug")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let debugExnf logger exn fmt = Printf.kprintf (debugExn logger exn) fmt

[<CompiledName("LogInformation")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let info (logger : ILogger) message = logger.LogInformation message

[<CompiledName("LogInformation")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let infoExn (logger : ILogger) (exn : exn) message = logger.LogInformation (exn, message)

[<CompiledName("FormatThenLogInformation")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let infof logger fmt = Printf.kprintf (info logger) fmt

[<CompiledName("FormatThenLogInformation")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let infoExnf logger exn fmt = Printf.kprintf (infoExn logger exn) fmt

[<CompiledName("LogWarning")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let warn (logger : ILogger) message = logger.LogWarning message

[<CompiledName("LogWarning")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let warnExn (logger : ILogger) (exn : exn) message = logger.LogWarning (exn, message)

[<CompiledName("FormatThenLogWarning")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let warnf logger fmt = Printf.kprintf (warn logger) fmt

[<CompiledName("FormatThenLogWarning")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let warnExnf logger exn fmt = Printf.kprintf (warnExn logger exn) fmt

[<CompiledName("LogError")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let error (logger : ILogger) message = logger.LogError message

[<CompiledName("LogError")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let errorExn (logger : ILogger) (exn : exn) message = logger.LogError (exn, message)

[<CompiledName("FormatThenLogError")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let errorf logger fmt = Printf.kprintf (error logger) fmt

[<CompiledName("FormatThenLogError")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let errorExnf logger exn fmt = Printf.kprintf (errorExn logger exn) fmt

[<CompiledName("LogCritical")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let critical (logger : ILogger) message = logger.LogCritical message

[<CompiledName("LogCritical")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let criticalExn (logger : ILogger) (exn : exn) message = logger.LogCritical (exn, message)

[<CompiledName("FormatThenLogCritical")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let criticalf logger fmt = Printf.kprintf (critical logger) fmt

[<CompiledName("FormatThenLogCritical")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let criticalExnf logger exn fmt = Printf.kprintf (criticalExn logger exn) fmt

[<CompiledName("CreateLogger")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let createLoggerByCategoryName (categoryName : string) (loggerFactory : ILoggerFactory) =
  loggerFactory.CreateLogger categoryName

[<CompiledName("CreateLogger")>]
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let createLoggerByType (``type`` : Type) (loggerFactory : ILoggerFactory) =
  loggerFactory.CreateLogger ``type``
