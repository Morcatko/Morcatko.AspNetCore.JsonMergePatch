# JSON Merge Patch support for ASP.NET Core 2, 3 and 5


[![Nuget](https://img.shields.io/nuget/v/Morcatko.AspNetCore.JsonMergePatch.svg)](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch) - [Morcatko.AspNetCore.JsonMergePatch](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch) (ASP.NET Core 2.x)  
[![Nuget](https://img.shields.io/nuget/v/Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson.svg)](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson) - [Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson) (ASP.NET Core 3+)  
[![Nuget](https://img.shields.io/nuget/v/Morcatko.AspNetCore.JsonMergePatch.SystemText.svg)](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch.SystemText) - [Morcatko.AspNetCore.JsonMergePatch.SystemText](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch.SystemText) (ASP.NET Core 3+)  
[![Nuget](https://img.shields.io/nuget/v/Morcatko.AspNetCore.JsonMergePatch.Document.svg)](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch.Document) - [Morcatko.AspNetCore.JsonMergePatch.Document](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch.Document) (ASP.NET Core 3+ - base package)

### JSON Merge Patch
- [RFC 7396](https://tools.ietf.org/html/rfc7396)
- performs partial resource update similar to JSON Patch
- Supports Swagger
- netstandard 2.0
 
C# object:
````csharp
var backendModel = new Model()
{
    Name = "James Bond"
    Age = "45"
    Weapon = "Gun"
}
````
JSON Merge Patch:
````csharp
 var mergePatch = new object() {
   "Weapon": "Knife"
 }
````
Resulting C# object:
````csharp
var newObject = new Model() {
  Name = "James Bond"
  Age = "45"
  Weapon = "Knife"
}
````

### How to
See `2.1-testApp`, `3.0 testApp` or `6.0/testApp` for sample
1. Install nuget (Use JsonMergePathch version same as your .NET) .
- ASP.NET Core 2.x [Morcatko.AspNetCore.JsonMergePatch](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch)
- ASP.NET Core 3+ (Newtonsoft.Json) [Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson)
- ASP.NET Core 3+ (System.Text) [Morcatko.AspNetCore.JsonMergePatch.SystemText](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch.SystemText) nuget

2. Add to your startup class
````csharp
using Morcatko.AspNetCore.JsonMergePatch;

public void ConfigureServices(IServiceCollection services)
{
    ...
    services
        .AddMvc()                         // or .AddMvcCore()
        //.AddJsonMergePatch();           // 2.x
        //.AddNewtonsoftJsonMergePatch(); // 3+ (Newtonsoft.Josn)
        //.AddSystemTextJsonMergePatch(); // 3+ (System.Text)
    ...
}
````
3. Use in your controller
````csharp
using Morcatko.AspNetCore.JsonMergePatch;

[HttpPatch]
[Consumes(JsonMergePatchDocument.ContentType)]
public void Patch([FromBody] JsonMergePatchDocument<Model> patch)
{
    ...
    patch.ApplyTo(backendModel);
    ...
}
````
You can apply a patch to a different Type (be carefull, all C# static typing is ignored) - see [#16](https://github.com/Morcatko/Morcatko.AspNetCore.JsonMergePatch/issues/16) for more details.
````csharp
BackendModel backendModel;
JsonMergePatch<DtoModel> patch;
patch.ApplyToT(backendModel)
````

4. Swagger config (optional)

copy & paste this class into your app 
 - 2.x - https://github.com/Morcatko/Morcatko.AspNetCore.JsonMergePatch/blob/master/src/2.1-testApp/JsonMergePatchDocumentOperationFilter.cs
 - 3+ - https://github.com/Morcatko/Morcatko.AspNetCore.JsonMergePatch/blob/master/src/6.0-testApp/JsonMergePatchDocumentOperationFilter.cs
````csharp
services.AddSwaggerGen(c =>
    {
        c.OperationFilter<JsonMergePatchDocumentOperationFilter>();
    });
````

### Options
````csharp
services
    .AddMvc()
    .AddJsonMergePatch(o => ....)
````
 * bool EnableDelete - Deletes items when target object is Dictionary and patched value is null

### How to - unit testing
See tests in `...Builder.Json.Simple` class for more examples
````csharp
Morcatko.AspNetCore.JsonMergePatch.Tests.Builder.Json

public void UnitTest()
{
    var model = new Model();
    var patch1 = PatchBuilder.Build<Model>("{ integer: 1}");
    ...
    or
    ...
    var original = new Model();
    var patched = new Model() { Integer = 1};
    var patch2 = PatchBuilder.Build(original, patched);
}
````

### Known issues/Not working
- ModelState.IsValid is false when a required property is missing
- Enums with [EnumMember(Value = "....")] attribute
