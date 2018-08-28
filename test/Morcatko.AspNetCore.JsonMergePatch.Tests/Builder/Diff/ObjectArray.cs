using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Builder.Diff
{
	public class Array
	{
		class Object
		{
			public int Integer { get; set; } = 1;
			public string String { get; set; } = "abc";
		}
		class ArrayClass
		{
			public Object[] Objects { get; set; } = new[] {
				new Object(),
				new Object()
			};
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
		public void ValueChanged()
		{
			var source = new ArrayClass();
			var patched = new ArrayClass();
			patched.Objects[1].Integer = 3;

			var patch = JsonMergePatchDocument.Build(source, patched);
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Equal(3, result.Objects[1].Integer);
		}
	}
}
