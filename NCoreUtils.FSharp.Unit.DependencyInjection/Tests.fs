module NCoreUtils.DependencyInjectionTests

open Xunit
open Microsoft.Extensions.DependencyInjection

[<Literal>]
let private ParName = "name"

let private globalServices =
  ServiceCollection()
    .BuildServiceProvider()

let private scoped action =
  use scope = globalServices.CreateScope ()
  action scope.ServiceProvider

let private asyncScoped action = async {
  use scope = globalServices.CreateScope ()
  return! action scope.ServiceProvider }

type RecordWithExplicitlyNamedProperty = {
  [<ParameterName(ParName)>]
  Name : string }

type RecordWithImplicitlyNamedProperty = { name : string }


[<Fact>]
let ``Named parameter binding via descriptor`` () =
  let tryGetParameters =
    let parameters = Map.ofList [ ParName, [ "x" ] ]
    fun name -> Map.tryFind name parameters
  let descriptor =
    { Path = ParName
      Type = typeof<string>
      Attributes = ParameterBinding.noCustomAttributes }
  asyncScoped
    (fun serviceProvider -> async {
      let! value = ParameterBinding.asyncBindParameter serviceProvider tryGetParameters descriptor
      Assert.NotNull value
      let stringValue = Assert.IsType<string> value
      Assert.Equal ("x", stringValue)
    })

[<Fact>]
let ``Implicitly named record property binding`` () =
  let tryGetParameters =
    let parameters = Map.ofList [ ParName, [ "x" ] ]
    fun name -> Map.tryFind name parameters
  let descriptor =
    { Path = null
      Type = typeof<RecordWithImplicitlyNamedProperty>
      Attributes = ParameterBinding.noCustomAttributes }
  asyncScoped
    (fun serviceProvider -> async {
      let! value = ParameterBinding.asyncBindParameter serviceProvider tryGetParameters descriptor
      Assert.NotNull value
      let record = Assert.IsType<RecordWithImplicitlyNamedProperty> value
      Assert.NotNull record.name
      Assert.Equal ("x", record.name)
    })

[<Fact>]
let ``Explicitly named record property binding`` () =
  let tryGetParameters =
    let parameters = Map.ofList [ ParName, [ "x" ] ]
    fun name -> Map.tryFind name parameters
  let descriptor =
    { Path = null
      Type = typeof<RecordWithExplicitlyNamedProperty>
      Attributes = ParameterBinding.noCustomAttributes }
  asyncScoped
    (fun serviceProvider -> async {
      let! value = ParameterBinding.asyncBindParameter serviceProvider tryGetParameters descriptor
      Assert.NotNull value
      let record = Assert.IsType<RecordWithExplicitlyNamedProperty> value
      Assert.NotNull record.Name
      Assert.Equal ("x", record.Name)
    })


