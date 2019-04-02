# JSON Merge Patch support for ASP.NET Core 2.x

### JSON Merge Patch
- [RFC 7396](https://tools.ietf.org/html/rfc7396)
- performs partial resource update similar to JSON Patch
- Supports Swagger
- netstandard 2.0
```
C# object:
var backendModel = new Model()
{
    Name = "James Bond"
    Age = "45"
    Weapon = "Gun"
}

JSON Merge Patch:
{
    "Weapon": "Knife"
}

resulting C# object:
{
    Name = "James Bond"
    Age = "45"
    Weapon = "Knife"
}
```


### How to
See testApp2.0 for sample

1. Install [Morcatko.AspNetCore.JsonMergePatch](https://www.nuget.org/packages/Morcatko.AspNetCore.JsonMergePatch) nuget
2. Add to your startup class
```
using Morcatko.AspNetCore.JsonMergePatch;

public void ConfigureServices(IServiceCollection services)
{
    ...
    services
        .AddMvc()              // or .AddMvcCore()
        .AddJsonMergePatch();
    ...
}
```
3. Use in your controller
```
using Morcatko.AspNetCore.JsonMergePatch;

[HttpPatch]
[Consumes(JsonMergePatchDocument.ContentType)]
public void Patch([FromBody] JsonMergePatchDocument<Model> patch)
{
    ...
    patch.ApplyTo(backendModel);
    ...
}
```

4. Swagger config (optional)

copy & paste this class into your app - https://github.com/Morcatko/Morcatko.AspNetCore.JsonMergePatch/blob/master/test/testApp2.0/JsonMergePatchDocumentOperationFilter.cs
```
services.AddSwaggerGen(c =>
    {
        c.OperationFilter<JsonMergePatchDocumentOperationFilter>();
    });
```

### Options
```
    services
        .AddMvc()
        .AddJsonMergePatch(o => ....)
```
 * bool EnableDelete - Deletes items when target object is Dictionary and patched value is null

### How to - unit testing
See `Morcatko.AspNetCore.JsonMergePatch.Tests.Builder.Json.Simple` class for more examples
```
Morcatko.AspNetCore.JsonMergePatch.Tests.Builder.Json
public void UnitTest()
{
    var model = new Model();
    var patch1 = PatchBuidler.Build<Model>("{ integer: 1}");
    ...
    or
    ...
    var original = new Model();
    var patched = new Model() { Integer = 1};
    var patch2 = PatchBuilder.Build(original, patched);
}
```

### Known issues/Not working
- ModelState.IsValid is false when a required property is missing
- Enums with [EnumMember(Value = "....")] attribute