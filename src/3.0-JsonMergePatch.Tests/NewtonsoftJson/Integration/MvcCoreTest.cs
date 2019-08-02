using System.Threading.Tasks;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.NewtonsoftJson.Integration
{
	public class MvcCoreTest

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
	}
}
