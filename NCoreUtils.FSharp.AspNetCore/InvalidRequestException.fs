namespace NCoreUtils.AspNetCore

open System
open System.Runtime.Serialization

module private InvalidRequestException =

  let StatusCodeKey = "StatusCode"

[<Serializable>]
type InvalidRequestException =
  inherit Exception
  val mutable private statusCode : int
  /// Creates new instance from the specified parameters.
  new (statusCode : int, message : string, innerException : exn) =
    { inherit Exception (message, innerException)
      statusCode = statusCode }
  /// Creates new instance from the specified parameters.
  new (statusCode, message) = InvalidRequestException (statusCode, message, null)
  /// Creates new instance during serialization.
  new (info : SerializationInfo, context) =
    { inherit Exception (info, context)
      statusCode = info.GetInt32 InvalidRequestException.StatusCodeKey }
  /// Gets suggested HTTP status code.
  member this.StatusCode = this.statusCode
  /// Overrides base method adding status code serialization.
  override this.GetObjectData (info, context) =
    base.GetObjectData (info, context)
    info.AddValue (InvalidRequestException.StatusCodeKey, this.statusCode)