using Morcatko.AspNetCore.JsonMergePatch.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Builder.Json
{
	public class Simple
	{
		class SimpleClass
		{
			[JsonProperty("Number")]
			public int Integer { get; set; } = 1;
			public string String { get; set; } = "abc";
		}

		private readonly PatchBuilder<SimpleClass> builder = new PatchBuilder<SimpleClass>();

		[Fact]
		public void String()
		{
			var original = new SimpleClass();

			var patch = builder.Build("{ number: 3 }");
			var result = patch.ApplyTo(original);

			Assert.Equal(3, result.Integer);
		}

		[Fact]
		public void JObject()
		{
			var original = new SimpleClass();

			var patch = builder.Build(new JObject(new JProperty("number", 3)));
			var result = patch.ApplyTo(original);

			Assert.Equal(3, result.Integer);
		}

		[Fact]
		public void Diff()
		{
			var original= new SimpleClass();
			var patched = new SimpleClass();
			patched.Integer = 3;

			var patch = builder.Build(original, patched);
			var result = patch.ApplyTo(original);

			Assert.Equal(3, result.Integer);
		}

        [Fact]
        public void NoDiff()
        {
            var original = new SimpleClass();
            var patched = new SimpleClass();

            var patch = builder.Build(original, patched);
            
            Assert.Empty(patch.Operations);
        }
    }
}
