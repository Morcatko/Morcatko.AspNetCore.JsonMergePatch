using Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson.Builders;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.NewtonsoftJson.Builders.Diff
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

			var diff = DiffBuilder.Build(source, patched);

			Assert.Null(diff);
		}

		[Fact]
		public void NullToValue()
		{
			var source = new SimpleClass();
			var patched = new SimpleClass() { String1 = "value" };

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
				 JObject.Parse("{String1:'value'}"),
				 diff));
		}

		[Fact]
		public void ValueToNull()
		{
			var source = new SimpleClass();
			var patched = new SimpleClass() { String2 = null };

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
				JObject.Parse("{String2:null}"),
				diff));
		}

		[Fact]
		public void Multiple()
		{
			var source = new SimpleClass();
			var patched = new SimpleClass() { Integer2 = 7, String2 = "Something" };

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
				JObject.Parse("{Integer2: 7, String2:'Something'}"),
				diff));
		}

	}
}
