using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Builder.Json
{
	public class Nested
	{
		class SimpleClass
		{
			public int Integer { get; set; } = 1;
			public string String { get; set; } = "abcd";
		}

		class NestedObject
		{
			public int Id { get; set; } = 1;
			public SimpleClass Object { get; set; } = new SimpleClass();
		}

		[Fact]
		public void RootOnly()
		{
			var source = new NestedObject();

			var patch = JsonMergePatchDocument.Build<NestedObject>("{ id: 5 }");
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(5, result.Id);
			Assert.Equal(1, result.Object.Integer);
			Assert.Equal("abcd", result.Object.String);
		}

		[Fact]
		public void Deep()
		{
			var source = new NestedObject();

			var patch = JsonMergePatchDocument.Build<NestedObject>("{ object: { integer: 7} }");
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(1, result.Id);
			Assert.Equal(7, result.Object.Integer);
			Assert.Equal("abcd", result.Object.String);
		}

		[Fact]
		public void ObjectToNull()
		{
			var source = new NestedObject();

			var patch = JsonMergePatchDocument.Build<NestedObject>("{ object: null}");
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(1, result.Id);
			Assert.Null(result.Object);
		}
	}
}
