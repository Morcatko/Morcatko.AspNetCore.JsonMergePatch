using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Builder
{
	public class ValueArray
	{
		class ArrayClass
		{
			public int[] Integers1 { get; set; } = new[] { 1, 2, 3 };
			public int[] Integers2 { get; set; }
		}

		[Fact]
		public void NoChange()
		{
			var source = new ArrayClass();
			var patched = new ArrayClass();

			var patch = JsonMergePatchDocument.Build(source, patched);

			Assert.Empty(patch.JsonPatchDocument.Operations);
		}

		[Fact]
		public void NullToValue()
		{
			var source = new ArrayClass();
			var patched = new ArrayClass() { Integers2 = new[] { 2, 3, 4 } };

			var patch = JsonMergePatchDocument.Build(source, patched);
			var result = patch.ApplyTo(source);
			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(new[] { 1, 2, 3 }, result.Integers1);
			Assert.Equal(new[] { 2, 3, 4 }, result.Integers2);
		}

		[Fact]
		public void ValueToNull()
		{
			var source = new ArrayClass();
			var patched = new ArrayClass() { Integers1 = null };

			var patch = JsonMergePatchDocument.Build(source, patched);
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Null(result.Integers1);
			Assert.Null(result.Integers2);
		}

		[Fact]
		public void ValueToValue()
		{
			var source = new ArrayClass();
			var patched = new ArrayClass() { Integers1 = new[] { 5, 6, 7 } };

			var patch = JsonMergePatchDocument.Build(source, patched);
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(new[] { 5, 6, 7 }, result.Integers1);
			Assert.Null(result.Integers2);
		}
	}
}
