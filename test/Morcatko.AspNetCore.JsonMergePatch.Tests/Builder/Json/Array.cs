using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Builder.Json
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
		public void ValueChanged()
		{
			var source = new ArrayClass();;

			var patch = JsonMergePatchDocument.Build<ArrayClass>("{ objects: [{ integer: 2, string: 'xx'}]}");
			var result = patch.ApplyTo(source);

			Assert.Single(patch.JsonPatchDocument.Operations);
			Assert.Single(result.Objects);
			Assert.Equal(2, result.Objects[0].Integer);
			Assert.Equal("xx", result.Objects[0].String);
		}
	}
}
