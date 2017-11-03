# JSON Merge Patch support for ASP.NET Core 2.0

### JSON Merge Patch
- [RFC 7396](https://tools.ietf.org/html/rfc7396)
- performs partial resource update similar to JSON Patch
- Supports Swagger
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
        .AddMvc()
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
```
services.AddSwaggerGen(c =>
    {
        c.OperationFilter<JsonMergePatchDocumentOperationFilter>();
    });
```
copy & paste this class into your app - https://github.com/Morcatko/Morcatko.AspNetCore.JsonMergePatch/blob/master/test/testApp2.0/JsonMergePatchDocumentOperationFilter.cs

### Known issues/Not working
- ModelState.IsValid is false when a required property is missing
- Enums with [EnumMember(Value = "....")] attribute