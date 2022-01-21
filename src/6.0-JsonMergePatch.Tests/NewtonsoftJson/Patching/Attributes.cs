using Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson.Builders;
using Newtonsoft.Json;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.NewtonsoftJson.Patching
{
	public class Attributes
	{
		class ClassOne
		{
			[JsonProperty("id")]
			public int Id { get; set; } = 1;

			[JsonProperty("rec_id")]
			public string RecId { get; set; } = "abc";
		}

		class ClassTwo
		{
			[JsonProperty("first_name")]
			public string FirstName { get; set; }

			[JsonProperty("last_name")]
			public string LastName { get; set; }

			[JsonProperty("obj_property")]
			public ClassOne ObjProperty { get; set; } = new ClassOne();
		}


		private readonly PatchBuilder<ClassTwo> _patchBuilder = new PatchBuilder<ClassTwo>();

		[Fact]
		public void CanPatchTopLevelProperties()
		{
			var original = new ClassTwo();

			var patch = _patchBuilder.Build("{'first_name': 'MyFirstName'}");
			var result = patch.ApplyTo(original);

			Assert.Equal("MyFirstName", result.FirstName);
		}

		[Fact]
		public void CannotPatchSubObject()
		{
			var original = new ClassTwo();

			var patch = _patchBuilder.Build("{'obj_property': { 'id': 3 }}");
			var result = patch.ApplyTo(original);

			Assert.Equal(3, result.ObjProperty.Id);
		}
	}
}
