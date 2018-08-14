using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Builder
{
	public class Flat
	{
		class SimpleClass
		{
			public int Integer1 { get; set; } = 1;
			public int Integer2 { get; set; } = 2;
			public string String1 { get; set; } = null;
			public string String2 { get; set; } = "abcd";
		}

		[Fact]
		public void NoChange()
		{
			var source = new SimpleClass();
			var patched = new SimpleClass();

			var patch = JsonMergePatchDocument.Build(source, patched);

			Assert.Empty(patch.JsonPatchDocument.Operations);
		}

		[Fact]
		public void NullToValue()
		{
			var source = new SimpleClass();
			var patched = new SimpleClass() { String1 = "value" };

			var patch = JsonMergePatchDocument.Build(source, patched);
			var result = patch.ApplyTo(source);

			Assert.Equal("value", result.String1);
		}

		[Fact]
		public void ValueToNull()
		{
			var source = new SimpleClass();
			var patched = new SimpleClass() { String2 = null };

			var patch = JsonMergePatchDocument.Build(source, patched);
			var result = patch.ApplyTo(source);

			Assert.Null(result.String2);
		}

		[Fact]
		public void Multiple()
		{
			var source = new SimpleClass();
			var patched = new SimpleClass() { Integer2 = 7, String2 = "Something" };

			var patch = JsonMergePatchDocument.Build(source, patched);

			var result = patch.ApplyTo(source);

			Assert.Equal(1, result.Integer1);
			Assert.Equal(7, result.Integer2);
			Assert.Null(result.String1);
			Assert.Equal("Something", result.String2);
		}
	}
}
