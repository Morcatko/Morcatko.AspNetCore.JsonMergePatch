using System.Linq;
using System.Text.Json;
using Morcatko.AspNetCore.JsonMergePatch.SystemText.Builders;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests;

public class PatchBuilderTests
{
    [Fact]
    public void Build_FromOriginalAndPatched_ShouldCreatePatchDocument()
    {
        var original = new TestModel { Name = "John", Age = 30 };
        var patched = new TestModel { Name = "John", Age = 31 };

        var patchDocument = PatchBuilder<TestModel>.Build(original, patched);

        Assert.NotNull(patchDocument);
        Assert.Single(patchDocument.Operations);
        Assert.Equal("/Age", patchDocument.Operations[0].path);
        Assert.Equal(31, patchDocument.Operations[0].value);
    }

    [Fact]
    public void Build_FromJsonString_ShouldCreatePatchDocument()
    {
        var jsonPatch = "{\"Age\":31}";
        var patchDocument = PatchBuilder<TestModel>.Build(jsonPatch);

        Assert.NotNull(patchDocument);
        Assert.Single(patchDocument.Operations);
        Assert.Equal("/Age", patchDocument.Operations[0].path);
        Assert.Equal(31, patchDocument.Operations[0].value);
    }

    [Fact]
    public void Build_FromObject_ShouldCreatePatchDocument()
    {
        var patchObject = new { Age = 31 };
        var patchDocument = PatchBuilder<TestModel>.Build(patchObject);

        Assert.NotNull(patchDocument);
        Assert.Single(patchDocument.Operations);
        Assert.Equal("/Age", patchDocument.Operations[0].path);
        Assert.Equal(31, patchDocument.Operations[0].value);
    }

    [Fact]
    public void Build_FromJsonElement_ShouldCreatePatchDocument()
    {
        var jsonElement = JsonDocument.Parse("{\"Age\":31}").RootElement;
        var patchDocument = PatchBuilder<TestModel>.Build(jsonElement);

        Assert.NotNull(patchDocument);
        Assert.Single(patchDocument.Operations);
        Assert.Equal("/Age", patchDocument.Operations[0].path);
        Assert.Equal(31, patchDocument.Operations[0].value);
    }

    [Fact]
    public void PatchNewsletter_ShouldApplyChangesCorrectly()
    {
        var originalModel = new TestModel
        {
            Name = "John",
            Surname = "Appleseed",
            Age = 30
        };

        var patchDto = new TestPatchDto
        {
            Name = "John", // Unchanged
            Surname = null, // Set to null
            Age = 31 // Changed
        };

        var patchDocument = PatchBuilder<TestPatchDto>.Build(new TestPatchDto
        {
            Name = originalModel.Name,
            Surname = originalModel.Surname,
            Age = originalModel.Age
        }, patchDto);

        Assert.NotNull(patchDocument);
        Assert.Equal(2, patchDocument.Operations.Count);  // Only 'TemplateName' and 'Age' should change

        var templateNameOperation = patchDocument.Operations.FirstOrDefault(op => op.path == "/Surname");
        var ageOperation = patchDocument.Operations.FirstOrDefault(op => op.path == "/Age");

        Assert.NotNull(templateNameOperation);
        Assert.Null(templateNameOperation.value);  // Set to null

        Assert.NotNull(ageOperation);
        Assert.Equal(31, ageOperation.value);  // Updated to 31

        patchDocument.ApplyToT(originalModel);

        Assert.Equal("John", originalModel.Name); // Unchanged
        Assert.Equal(31, originalModel.Age); // Updated
        Assert.Null(originalModel.Surname); // Set to null
    }
}

public class TestPatchDto
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public int? Age { get; set; }
}


public class TestModel
{
    public string Name { get; init; }
    public string Surname { get; init; }
    public int Age { get; init; }
}