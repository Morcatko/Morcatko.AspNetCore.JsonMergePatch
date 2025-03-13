using System;
using System.Collections.Generic;
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

    [Fact]
    public void Build_FromJsonStringWithArraysOfObjects_ShouldCreatePatchDocument()
    {
        var jsonPatch = @"
{
  ""Quantity"": 23271,
  ""Models2"": [
    {
      ""Title"": ""Title9b041b1a-ca14-48c2-85be-30c65e09e1f4"",
      ""Models3"": [
        {
          ""Price"": 25523,
          ""Values"": [
            10582
          ]
        }
      ]
    },
    {
      ""Title"": ""Title4d9bf501-e443-43c9-b9df-b197790db6bf"",
      ""Models3"": [
        {
          ""Price"": 7043,
          ""Values"": []
        },
        {
          ""Price"": 7408,
          ""Values"": [
            25137,
            16491,
            730
          ]
        }
      ]
    },
    {
      ""Title"": ""Title773b30ae-4db9-4634-955b-be32f36b26ff"",
      ""Models3"": []
    }
  ]
}
";
        var patchDocument = PatchBuilder<ObjectArrayTestModel>.Build(jsonPatch);

        Assert.NotNull(patchDocument);
        Assert.Equal(2, patchDocument.Operations.Count);
        Assert.Equal("/Quantity", patchDocument.Operations[0].path);
        Assert.Equal("/Models2", patchDocument.Operations[1].path);
        Assert.Equal(23271, patchDocument.Operations[0].value);
        Assert.True(patchDocument.Operations[1].value is object[] array
            && array[0].GetType() == typeof(ObjectArrayTestModel2));
        Assert.True(patchDocument.Operations[1].value is object[] array2
            && ((ObjectArrayTestModel2)array2[1]).Models3[1].Values[2] == 730);
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

public class ObjectArrayTestModel
{
    public int Quantity { get; set; }
    public List<ObjectArrayTestModel2> Models2 { get; set; } = new List<ObjectArrayTestModel2>();
}

public class ObjectArrayTestModel2
{
    public string Title { get; set; }
    public ObjectArrayTestModel3[] Models3 { get; set; } = Array.Empty<ObjectArrayTestModel3>();
}

public class ObjectArrayTestModel3
{
    public decimal Price { get; set; }
    public int[] Values { get; set; } = Array.Empty<int>();
}
