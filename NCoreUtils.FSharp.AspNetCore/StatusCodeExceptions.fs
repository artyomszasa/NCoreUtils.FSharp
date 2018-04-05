namespace NCoreUtils.AspNetCore

open System.Runtime.Serialization

[<RequireQualifiedAccess>]
module private Msg =
  [<Literal>]
  let BadRequest = "The request could not be understood by the server due to malformed syntax."

  [<Literal>]
  let Unauthorized = "The request requires user authentication."

  [<Literal>]
  let Forbidden = "Action is forbidden."

  [<Literal>]
  let NotFound = "Not found."

  [<Literal>]
  let MethodNotAllowed = "Method not allowed."

  [<Literal>]
  let NotAcceptable = "Not acceptable."

  [<Literal>]
  let ProxyAuthenticationRequired = "Proxy authentication required."

  [<Literal>]
  let RequestTimeout = "Request timeout."

  [<Literal>]
  let Conflict = "The request could not be completed due to a conflict with the current state of the resource."

  [<Literal>]
  let Gone = "The requested resource is no longer available at the server and no forwarding address is known."

  [<Literal>]
  let LengthRequired = "The server refuses to accept the request without a defined Content-Length."

  [<Literal>]
  let PreconditionFailed = "The precondition given in one or more of the request-header fields evaluated to false when it was tested on the server."

  [<Literal>]
  let RequestEntityTooLarge = "Request entity is too large."

  [<Literal>]
  let RequestUriTooLong = "Request URI is too long."

  [<Literal>]
  let UnsupportedMediaType = "Unsupported media type."




type BadRequestException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (400, message, innerException) }
  new (message : string) = BadRequestException (message, null)
  new (innerException : exn) = BadRequestException (Msg.BadRequest, innerException)
  new () = BadRequestException (Msg.BadRequest, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type UnauthorizedException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (401, message, innerException) }
  new (message : string) = UnauthorizedException (message, null)
  new (innerException : exn) = UnauthorizedException (Msg.Unauthorized, innerException)
  new () = UnauthorizedException (Msg.Unauthorized, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type ForbiddenException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (403, message, innerException) }
  new (message : string) = ForbiddenException (message, null)
  new (innerException : exn) = ForbiddenException (Msg.Forbidden, innerException)
  new () = ForbiddenException (Msg.Forbidden, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type NotFoundException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (404, message, innerException) }
  new (message : string) = NotFoundException (message, null)
  new (innerException : exn) = NotFoundException (Msg.NotFound, innerException)
  new () = NotFoundException (Msg.NotFound, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type MethodNotAllowedException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (405, message, innerException) }
  new (message : string) = MethodNotAllowedException (message, null)
  new (innerException : exn) = MethodNotAllowedException (Msg.MethodNotAllowed, innerException)
  new () = MethodNotAllowedException (Msg.MethodNotAllowed, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type NotAcceptableException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (406, message, innerException) }
  new (message : string) = NotAcceptableException (message, null)
  new (innerException : exn) = NotAcceptableException (Msg.NotAcceptable, innerException)
  new () = NotAcceptableException (Msg.NotAcceptable, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type ProxyAuthenticationRequiredException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (407, message, innerException) }
  new (message : string) = ProxyAuthenticationRequiredException (message, null)
  new (innerException : exn) = ProxyAuthenticationRequiredException (Msg.ProxyAuthenticationRequired, innerException)
  new () = ProxyAuthenticationRequiredException (Msg.ProxyAuthenticationRequired, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type RequestTimeoutException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (408, message, innerException) }
  new (message : string) = RequestTimeoutException (message, null)
  new (innerException : exn) = RequestTimeoutException (Msg.RequestTimeout, innerException)
  new () = RequestTimeoutException (Msg.RequestTimeout, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type ConflictException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (409, message, innerException) }
  new (message : string) = ConflictException (message, null)
  new (innerException : exn) = ConflictException (Msg.Conflict, innerException)
  new () = ConflictException (Msg.Conflict, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type GoneException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (410, message, innerException) }
  new (message : string) = GoneException (message, null)
  new (innerException : exn) = GoneException (Msg.Gone, innerException)
  new () = GoneException (Msg.Gone, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type LengthRequiredException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (411, message, innerException) }
  new (message : string) = LengthRequiredException (message, null)
  new (innerException : exn) = LengthRequiredException (Msg.LengthRequired, innerException)
  new () = LengthRequiredException (Msg.LengthRequired, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type PreconditionFailedException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (412, message, innerException) }
  new (message : string) = PreconditionFailedException (message, null)
  new (innerException : exn) = PreconditionFailedException (Msg.PreconditionFailed, innerException)
  new () = PreconditionFailedException (Msg.PreconditionFailed, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type RequestEntityTooLargeException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (413, message, innerException) }
  new (message : string) = RequestEntityTooLargeException (message, null)
  new (innerException : exn) = RequestEntityTooLargeException (Msg.RequestEntityTooLarge, innerException)
  new () = RequestEntityTooLargeException (Msg.RequestEntityTooLarge, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type RequestUriTooLongException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (414, message, innerException) }
  new (message : string) = RequestUriTooLongException (message, null)
  new (innerException : exn) = RequestUriTooLongException (Msg.RequestUriTooLong, innerException)
  new () = RequestUriTooLongException (Msg.RequestUriTooLong, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }

type UnsupportedMediaTypeException =
  inherit InvalidRequestException
  new (message: string, innerException : exn) =
    { inherit InvalidRequestException (414, message, innerException) }
  new (message : string) = UnsupportedMediaTypeException (message, null)
  new (innerException : exn) = UnsupportedMediaTypeException (Msg.UnsupportedMediaType, innerException)
  new () = UnsupportedMediaTypeException (Msg.UnsupportedMediaType, null);
  new (info : SerializationInfo, context) =
    { inherit InvalidRequestException (info, context) }