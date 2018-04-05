namespace NCoreUtils.Linq

open FSharp.Control
open NCoreUtils
open System.Linq.Expressions
open System.Reflection
open System.Runtime.CompilerServices
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter
open System
open System.Diagnostics.CodeAnalysis
open System.Collections.Immutable
open System.Collections.Generic
open System.Linq

[<AbstractClass; Sealed>]
[<SuppressMessage("NameConventions", "*")>]
type Q private () =

  [<Literal>]
  static let PublicInstanceBinding =
    BindingFlags.Instance
      ||| BindingFlags.Public

  [<Literal>]
  static let AnyCtorBinding =
    BindingFlags.NonPublic
      ||| BindingFlags.Public
      ||| BindingFlags.Instance

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static let (|FSharpFunc|_|) (e : Expression) =
    match e with
    | :? MethodCallExpression as me when me.Method.Name = "ToFSharpFunc" && me.Arguments.Count = 1 ->
      match me.Arguments.[0] with
      | :? LambdaExpression as le -> Some le
      | _ -> None
    | _ -> None

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static let (|LambdaFunc|_|) (le : LambdaExpression) =
    match le.Parameters with
    | args when args.Count = 1 -> Some (args.[0], le.Body)
    | _ -> None

  /// <summary>
  /// Fixes new expression when creating F# types (only Tuples and Records supported at the moment)
  /// </summary>
  static let fixNewExpression (e : NewExpression) : NewExpression =
    let ty = e.Type
    match ty with
    | TupleType memberTypes ->
      match e.Members with
      | null ->
        let items =
          Array.mapi
            (fun i _ ->
              match ty.GetProperty (sprintf "Item%d" (i + 1), PublicInstanceBinding) with
              | null         -> failwithf "Could not get field item%d for tuple" (i + 1)
              | propertyInfo -> propertyInfo :> MemberInfo
            )
            memberTypes
        let ctor = ty.GetConstructor (AnyCtorBinding, null, memberTypes, null)
        Expression.New (ctor, e.Arguments, items)
      | _ -> e
    | RecordType members ->
      match e.Members with
      | null ->
        let items       = members |> Array.map (fun p -> p :> MemberInfo)
        let memberTypes = members |> Array.map (fun m -> m.PropertyType)
        let ctor        = ty.GetConstructor (AnyCtorBinding, null, memberTypes, null)
        Expression.New (ctor, e.Arguments, items)
      | _ -> e
    | _ -> e

  /// <summary>
  /// Fixes new expression wrapped in lambda expression
  /// </summary>
  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static let fixLambdaExpression (l : 'a when 'a :> LambdaExpression) =
    match l.Body with
    | :? NewExpression as e -> Expression.Lambda (fixNewExpression e, l.Parameters) :?> 'a
    | _ -> l

  static let fixNewExpressions =
    let visitor =
      { new ExpressionVisitor () with
          override this.VisitNew node =
            let ty = node.Type
            match ty with
            | TupleType memberTypes ->
              match node.Members with
              | null ->
                let items =
                  Array.mapi
                    (fun i _ ->
                      match ty.GetProperty (sprintf "Item%d" (i + 1), PublicInstanceBinding) with
                      | null         -> failwithf "Could not get field item%d for tuple" (i + 1)
                      | propertyInfo -> propertyInfo :> MemberInfo
                    )
                    memberTypes
                let ctor = ty.GetConstructor (AnyCtorBinding, null, memberTypes, null)
                Expression.New (ctor, this.Visit node.Arguments, items) :> _
              | _ -> base.VisitNew node
            | RecordType members ->
              match node.Members with
              | null ->
                let items       = members |> Array.map (fun p -> p :> MemberInfo)
                let memberTypes = members |> Array.map (fun m -> m.PropertyType)
                let ctor        = ty.GetConstructor (AnyCtorBinding, null, memberTypes, null)
                Expression.New (ctor, this.Visit node.Arguments, items) :> _
              | _ -> base.VisitNew node
            | _ -> base.VisitNew node
      }
    visitor.Visit : Expression -> Expression

  static let rec funcToLambda acc expression =
    match expression with
    | FSharpFunc (LambdaFunc (arg, body)) -> funcToLambda (arg :: acc) body
    | _ ->
      let args = List.toArray acc
      Array.revInPlace args
      Expression.Lambda (expression, args)

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static let fsharpFuncToLambda (node : Expression) =
    match node with
    | FSharpFunc (LambdaFunc (arg, body)) -> funcToLambda [arg] body
    | _ -> invalidOpf "Not an F# Function: %A" node

  static let replaceSequenceMethods =
    let getMethodDefinition (expr : Expr) =
      match expr with
      | Microsoft.FSharp.Quotations.DerivedPatterns.Lambdas (_, Call (_, m, _)) -> m.GetGenericMethodDefinition ()
      | _ -> invalidOp "should never happen"

    let getEnumerableElementType (ty : Type) =
      match ty.IsConstructedGenericType && ty.GetGenericTypeDefinition () = typedefof<IEnumerable<_>> with
      | true -> ty.GenericTypeArguments.[0]
      | _ ->
        ty.GetInterfaces ()
        |> Array.find (fun ty -> ty.IsConstructedGenericType && ty.GetGenericTypeDefinition () = typedefof<IEnumerable<_>>)
        |> (fun ty -> ty.GenericTypeArguments.[0])

    let mEnumerableWhere = getMethodDefinition <@ System.Linq.Enumerable.Where : seq<int> * Func<int, bool> -> seq<int> @>

    let transformSeqFilter (visitor : ExpressionVisitor) (expression : MethodCallExpression) =
      let source = expression.Arguments.[1] |> visitor.Visit
      let predicate =
        match expression.Arguments.[0] with
        | e when e.Type.IsConstructedGenericType && e.Type.GetGenericTypeDefinition () = typedefof<FSharpFunc<_, _>> ->
          fsharpFuncToLambda e :> Expression
        | e -> e
        |> visitor.Visit
      let elementType = source.Type |> getEnumerableElementType
      Expression.Call (mEnumerableWhere.MakeGenericMethod elementType, source, predicate)

    let enumerableMethods =
      [ getMethodDefinition <@ Seq.filter : (int -> bool) -> seq<int> -> _ @>, transformSeqFilter ]
      |> Seq.map (fun (a, b) -> KeyValuePair (a, b))
      |> ImmutableDictionary.CreateRange

    let visitor =
      { new ExpressionVisitor () with
          override this.VisitMethodCall node =
            let m = node.Method
            match m.IsGenericMethod && m.DeclaringType = typeof<System.Linq.Enumerable> with
            | true ->
              let mutable transform = Unchecked.defaultof<_>
              match enumerableMethods.TryGetValue (m.GetGenericMethodDefinition (), &transform) with
              | true -> transform this node :> _
              | _    -> base.VisitMethodCall node
            | _    -> base.VisitMethodCall node
      }
    visitor.Visit : Expression -> Expression

  [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
  static let toLinqExprImpl (quotation : Expr) =
    QuotationToExpression quotation
    |> fixNewExpressions
    |> replaceSequenceMethods
    |> fsharpFuncToLambda

  [<CompiledName("ToLinqExpression1")>]
  static member toLinqExpr1 (quotation : Expr<'a -> 'b>) = toLinqExprImpl quotation :?> Expression<Func<'a, 'b>>

  [<CompiledName("ToLinqExpression2")>]
  static member toLinqExpr2 (quotation : Expr<'a -> 'b -> 'c>) = toLinqExprImpl quotation :?> Expression<Func<'a, 'b, 'c>>

  [<CompiledName("Filter")>]
  static member filter ([<ReflectedDefinition>] predicate : Expr<'a -> bool>) =
    let predicate' = Q.toLinqExpr1 predicate
    fun (queryable : IQueryable<'a>) -> queryable.Where predicate'

  [<CompiledName("Map")>]
  static member map ([<ReflectedDefinition>] selector : Expr<'a -> 'b>) =
    let selector' = Q.toLinqExpr1 selector
    fun (query : IQueryable<'a>) -> query.Select selector'

  [<CompiledName("Collect")>]
  static member collect ([<ReflectedDefinition>] selector : Expr<'a -> IEnumerable<'b>>) =
    let selector' = Q.toLinqExpr1 selector
    fun (query : IQueryable<'a>) -> query.SelectMany selector'

  [<CompiledName("Skip")>]
  static member skip n (query : IQueryable<_>) = query.Skip n

  [<CompiledName("Take")>]
  static member take n (query : IQueryable<_>) = query.Take n

  [<CompiledName("AsyncCount")>]
  static member asyncCount (query : IQueryable<_>) = Async.Adapt query.CountAsync

  [<CompiledName("AsyncToList")>]
  static member asyncToResizeArray (query : IQueryable<_>) = Async.Adapt query.ToListAsync

  [<CompiledName("AsyncToArray")>]
  static member asyncToArray (query : IQueryable<_>) = Async.Adapt query.ToArrayAsync

  [<CompiledName("ToAsyncSequence")>]
  static member toAsyncSeq (query : IQueryable<_>) =
    let enumerator = query.ExecuteAsync().GetEnumerator ()
    AsyncSeq.unfoldAsync
      (fun (enumerator : IAsyncEnumerator<_>) -> async {
        let! hasNext = Async.Adapt enumerator.MoveNext
        return
          match hasNext with
          | true -> Some (enumerator.Current, enumerator)
          | _ ->
            enumerator.Dispose ()
            None
      })
      enumerator

  [<CompiledName("AsyncExists")>]
  static member asyncExists predicate (query : IQueryable<_>) =
    let predicate' = Q.toLinqExpr1 predicate
    Async.Adapt (fun cancellationToken -> query.AnyAsync (predicate', cancellationToken))


  [<CompiledName("AsyncToFSharp")>]
  static member asyncToList query = Q.toAsyncSeq query |> AsyncSeq.toList

  [<CompiledName("AsyncIterate")>]
  static member asyncIter action query = Q.toAsyncSeq query |> AsyncSeq.iter action

  [<CompiledName("AsyncTryGetFirst")>]
  static member asyncTryFirst (query : IQueryable<_>) = async {
    let! value = Async.Adapt query.FirstOrDefaultAsync
    return Option.wrap value }

[<AutoOpen>]
module InterOp =

  [<CompiledName("ToAsyncEnumerable")>]
  let toAsyncEnumerable (asyncSeq : AsyncSeq<_>) =
    { new System.Collections.Generic.IAsyncEnumerable<_> with
        member __.GetEnumerator () =
          let enumerator = asyncSeq.GetEnumerator ()
          let current    = ref Unchecked.defaultof<_>
          { new System.Collections.Generic.IAsyncEnumerator<_> with
              member __.Current = !current
              member __.MoveNext cancellationToken =
                let computation = async {
                  let! next = enumerator.MoveNext ()
                  return
                    match next with
                    | Some value ->
                      current := value
                      true
                    | _ ->
                      current := Unchecked.defaultof<_>
                      false }
                Async.StartAsTask (computation, cancellationToken = cancellationToken)
              member __.Dispose () =
                current := Unchecked.defaultof<_>
                enumerator.Dispose ()
          }
    }