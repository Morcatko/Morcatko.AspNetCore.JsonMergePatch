using System.Threading.Tasks;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration
{
	public class MvcTest

	{
		private TestModel GetTestModel() =>
			new TestModel()
			{
				Integer = 5,
				String = "string",
				Float = 1.5f,
				Boolean = false,
				Renamed = "some string",
				SimpleEnum = SimpleEnum.two,
				ValueEnum = ValueEnum.i,
				SubModel = new SubModel()
				{
					Value1 = "value 1",
					Value2 = "value 2",
					Numbers = new[] { 1, 2, 3 }
				}
			};

		[Fact]
		public async Task PatchIntegers()
		{
			using (var server = Helper.CreateMvcServer())
			{
				await server.PostAsync("api/data/0", GetTestModel());
				await server.PostAsync("api/data/1", GetTestModel());
				await server.PostAsync("api/data/2", GetTestModel());

				await server.MergePatchAsync("api/data", new[]
				{
					new { id = 1, integer = 7 },
					new { id = 2, integer = 9 }
				});

				var patchedModel = await server.GetAsync<TestModel>("api/data/0");
				var expected = GetTestModel();
				Assert.Equal(expected, patchedModel);

				patchedModel = await server.GetAsync<TestModel>("api/data/1");
				expected = GetTestModel();
				expected.Integer = 7;
				Assert.Equal(expected, patchedModel);

				patchedModel = await server.GetAsync<TestModel>("api/data/2");
				expected = GetTestModel();
				expected.Integer = 9;
				Assert.Equal(expected, patchedModel);
			}
		}

		[Fact(Skip = "Does not work")]
		public async Task MissingRequiredProperty()
		{
			using (var server = Helper.CreateMvcServer())
			{
				await server.PostAsync("api/data/0", GetTestModel());

				var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0/validate", new { integer = 8 });

				var expected = GetTestModel();
				expected.Integer = 8;
				Assert.Equal(expected, patchedModel);
			}
		}

		#region ValueEnum
		[Fact(Skip = "Enums do not work - https://github.com/aspnet/Home/issues/2423")]
		public async Task PatchValueEnum()
		{
			using (var server = Helper.CreateMvcServer())
			{
				await server.PostAsync("api/data/0", GetTestModel());

				var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { valueEnum = "Feet" });

				var expected = GetTestModel();
				expected.ValueEnum = ValueEnum.ft;
				Assert.Equal(expected, patchedModel);
			}
		}

		[Fact(Skip = "JsonPatch (not merge) fails as well - https://github.com/aspnet/Home/issues/2423")]
		public async Task JsonPatchValueEnum()
		{
			using (var server = Helper.CreateMvcServer())
			{
				await server.PostAsync("api/data/0", GetTestModel());
				await server.JsonPatchAsync("api/data/0", new[] { new { op = "replace", path = "/valueEnum", value = "Feet" } });
			}
		}

		[Fact] //Post works
		public async Task PostValueEnum()
		{
			using (var server = Helper.CreateMvcServer())
			{
				await server.PostAsync("api/data/0", new { valueEnum = "Feet" });
			}
		}
		#endregion
	}
}
