using Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.NewtonsoftJson.Builders.Diff
{
	public class Array
	{
		class Object
		{
			public int Integer { get; set; } = 1;
			public string String { get; set; } = "abc";
		}
		class ObjectArray
		{
			public Object[] Objects { get; set; } = new[] {
				new Object(),
				new Object()
			};
		}
		class ValueArray
		{
			public int[] Integers1 { get; set; } = new[] { 1, 2, 3 };
			public int[] Integers2 { get; set; }
		}

		[Fact]
		public void NoChange()
		{
			var source = new ObjectArray();
			var patched = new ObjectArray();

			var diff = DiffBuilder.Build(source, patched);

			Assert.Null(diff);
		}

		[Fact]
		public void ValueChanged()
		{
			var source = new ObjectArray();
			var patched = new ObjectArray();
			patched.Objects[1].Integer = 3;

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
				 JObject.Parse($"{{Objects:{JsonConvert.SerializeObject(patched.Objects)}}}"),
				 diff));
		}

		[Fact]
		public void NullToValue()
		{
			var source = new ValueArray();
			var patched = new ValueArray() { Integers2 = new[] { 2, 3, 4 } };

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
				 JObject.Parse($"{{Integers2:{JsonConvert.SerializeObject(patched.Integers2)}}}"),
				 diff));
		}

		[Fact]
		public void ValueToNull()
		{
			var source = new ValueArray();
			var patched = new ValueArray() { Integers1 = null };

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
				 JObject.Parse("{Integers1:null}"),
				 diff));
		}

		[Fact]
		public void ValueToValue()
		{
			var source = new ValueArray();
			var patched = new ValueArray() { Integers1 = new[] { 5, 6, 7 } };

			var diff = DiffBuilder.Build(source, patched);

			Assert.True(JObject.DeepEquals(
				 JObject.Parse($"{{Integers1:{JsonConvert.SerializeObject(patched.Integers1)}}}"),
				 diff));
		}

	}
}
