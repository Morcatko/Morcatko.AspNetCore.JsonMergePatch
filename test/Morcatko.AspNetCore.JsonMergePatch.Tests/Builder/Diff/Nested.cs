using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Builder.Diff
{
	public class Nested
	{
		class SimpleClass
		{
			public int Integer1 { get; set; } = 1;
			public int Integer2 { get; set; } = 2;
			public string String1 { get; set; } = null;
			public string String2 { get; set; } = "abcd";
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
			var patched = new NestedObject() { Id = 9 };

			var patch = JsonMergePatchDocument.Build(source, patched);
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(9, result.Id);
			Assert.Equal(1, result.Object.Integer1);
			Assert.Equal(2, result.Object.Integer2);
			Assert.Null(result.Object.String1);
			Assert.Equal("abcd", result.Object.String2);
		}

		[Fact]
		public void Deep()
		{
			var source = new NestedObject();
			var patched = new NestedObject();
			patched.Object.Integer2 = 19;

			var patch = JsonMergePatchDocument.Build(source, patched);
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(1, result.Id);
			Assert.Equal(1, result.Object.Integer1);
			Assert.Equal(19, result.Object.Integer2);
			Assert.Null(result.Object.String1);
			Assert.Equal("abcd", result.Object.String2);
		}

		[Fact]
		public void ObjectToNull()
		{
			var source = new NestedObject();
			var patched = new NestedObject();
			patched.Object = null;

			var patch = JsonMergePatchDocument.Build(source, patched);
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(1, result.Id);
			Assert.Null(result.Object);
		}

		/*
		
		//Not supported in JsonPatchDocument
		
		[Fact]
		public void NullToObject()
		{
			var source = new NestedObject();
			source.Object = null;
			var patched = new NestedObject();

			var patch = JsonMergePatchDocument.Build(source, patched);
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(1, result.Id);
			Assert.Equal(1, result.Object.Integer1);
			Assert.Equal(2, result.Object.Integer2);
			Assert.Null(result.Object.String1);
			Assert.Equal("abcd", result.Object.String2);
		}
		*/
	}
}