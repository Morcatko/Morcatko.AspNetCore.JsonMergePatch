using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Builder.Json
{
	public class Simple
	{
		class SimpleClass
		{
			public int Integer { get; set; } = 1;
			public string String { get; set; } = "abc";
		}

		[Fact]
		public void NoChange()
		{
			var model = new SimpleClass();

			var patch = JsonMergePatchDocument.Build<SimpleClass>("{ }");

			Assert.Empty(patch.JsonPatchDocument.Operations);
		}

		[Fact]
		public void ValueToValue()
		{
			var model = new SimpleClass();

			var patch = JsonMergePatchDocument.Build<SimpleClass>("{ integer: 3 }");
			var result = patch.ApplyTo(model);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(3, result.Integer);
			Assert.Equal("abc", result.String);
		}

		[Fact]
		public void ValueToNull()
		{
			var model = new SimpleClass();

			var patch = JsonMergePatchDocument.Build<SimpleClass>("{ string: null }");
			var result = patch.ApplyTo(model);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(1, result.Integer);
			Assert.Null(result.String);
		}
	}
}
