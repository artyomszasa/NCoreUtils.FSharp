[<AutoOpen>]
module NCoreUtils.Logging.TopLevelFunctions

open System
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


let log (logger : ILogger) logLevel message  = logger.Log (logLevel, noEventId, message, null, formatter)

let logExn (logger : ILogger) logLevel exn message = logger.Log (logLevel, noEventId, message, exn, exnFormatter)

let logf logger logLevel fmt = Printf.kprintf (log logger logLevel) fmt

let logExnf logger logLevel exn fmt = Printf.kprintf (logExn logger logLevel exn) fmt

[<CompiledName("LogTrace")>]
let trace (logger : ILogger) message = logger.LogTrace message

[<CompiledName("LogTrace")>]
let traceExn (logger : ILogger) (exn : exn) message = logger.LogTrace (exn, message)

[<CompiledName("FormatThenLogTrace")>]
let tracef logger fmt = Printf.kprintf (trace logger) fmt

[<CompiledName("FormatThenLogTrace")>]
let traceExnf logger exn fmt = Printf.kprintf (traceExn logger exn) fmt

[<CompiledName("LogDebug")>]
let debug (logger : ILogger) message = logger.LogDebug message

[<CompiledName("LogDebug")>]
let debugExn (logger : ILogger) (exn : exn) message = logger.LogDebug (exn, message)

[<CompiledName("FormatThenLogDebug")>]
let debugf logger fmt = Printf.kprintf (debug logger) fmt

[<CompiledName("FormatThenLogDebug")>]
let debugExnf logger exn fmt = Printf.kprintf (debugExn logger exn) fmt

[<CompiledName("LogInformation")>]
let info (logger : ILogger) message = logger.LogInformation message

[<CompiledName("LogInformation")>]
let infoExn (logger : ILogger) (exn : exn) message = logger.LogInformation (exn, message)

[<CompiledName("FormatThenLogInformation")>]
let infof logger fmt = Printf.kprintf (info logger) fmt

[<CompiledName("FormatThenLogInformation")>]
let infoExnf logger exn fmt = Printf.kprintf (infoExn logger exn) fmt

[<CompiledName("LogWarning")>]
let warn (logger : ILogger) message = logger.LogWarning message

[<CompiledName("LogWarning")>]
let warnExn (logger : ILogger) (exn : exn) message = logger.LogWarning (exn, message)

[<CompiledName("FormatThenLogWarning")>]
let warnf logger fmt = Printf.kprintf (warn logger) fmt

[<CompiledName("FormatThenLogWarning")>]
let warnExnf logger exn fmt = Printf.kprintf (warnExn logger exn) fmt

[<CompiledName("LogError")>]
let error (logger : ILogger) message = logger.LogError message

[<CompiledName("LogError")>]
let errorExn (logger : ILogger) (exn : exn) message = logger.LogError (exn, message)

[<CompiledName("FormatThenLogError")>]
let errorf logger fmt = Printf.kprintf (error logger) fmt

[<CompiledName("FormatThenLogError")>]
let errorExnf logger exn fmt = Printf.kprintf (errorExn logger exn) fmt

[<CompiledName("LogCritical")>]
let critical (logger : ILogger) message = logger.LogCritical message

[<CompiledName("LogCritical")>]
let criticalExn (logger : ILogger) (exn : exn) message = logger.LogCritical (exn, message)

[<CompiledName("FormatThenLogCritical")>]
let criticalf logger fmt = Printf.kprintf (critical logger) fmt

[<CompiledName("FormatThenLogCritical")>]
let criticalExnf logger exn fmt = Printf.kprintf (criticalExn logger exn) fmt

