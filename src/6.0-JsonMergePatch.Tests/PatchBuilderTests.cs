using System.Text.Json;
using Morcatko.AspNetCore.JsonMergePatch.SystemText.Builders;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests;

public class PatchBuilderTests
{
    private class TestModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

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
}