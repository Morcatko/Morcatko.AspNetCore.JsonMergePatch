using Morcatko.AspNetCore.JsonMergePatch.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Builders.Diff
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
		public void NoChange()
		{
			var source = new NestedObject();
			var patched = new NestedObject();

			var diff = DiffBuilder.Build(source, patched);

			Assert.Null(diff);
		}

		[Fact]
		public void RootOnly()
		{
			var source = new NestedObject();
			var patched = new NestedObject() { Id = 9 };

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
				 JObject.Parse("{Id: 9}"),
				 diff));
		}

		[Fact]
		public void Deep()
		{
			var source = new NestedObject();
			var patched = new NestedObject();
			patched.Object.Integer2 = 19;

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
							 JObject.Parse("{Object:{Integer2: 19}}"),
							 diff));
		}

		[Fact]
		public void ObjectToNull()
		{
			var source = new NestedObject();
			var patched = new NestedObject();
			patched.Object = null;

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
							 JObject.Parse("{Object:null}"),
							 diff));
		}

		//Not supported in JsonPatchDocument
		[Fact]
		public void NullToObject()
		{
			var source = new NestedObject();
			source.Object = null;
			var patched = new NestedObject();

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
							 JObject.Parse($"{{Object:{JsonConvert.SerializeObject(patched.Object)}}}"),
							 diff));
		}
	}
}