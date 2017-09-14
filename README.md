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
2. Startup class
```
using Morcatko.AspNetCore.JsonMergePatch;

public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddJsonMergePatch();
    ...
}
```
3. Controller
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

### Warning
There are no tests in this project. Feeld free to add it, or completely takeover the project
