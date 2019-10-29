using System;
using System.Threading.Tasks;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration
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

        [Fact]
        public async Task PatchDateTimeOffset()
        {
            using (var server = Helper.CreateMvcServer())
            {
                await server.PostAsync("api/data/0", GetTestModel());
                await server.PostAsync("api/data/1", GetTestModel());
                await server.PostAsync("api/data/2", GetTestModel());

                var dateTime = new DateTimeOffset(2019, 10, 29, 9, 38, 0, 0, TimeSpan.FromHours(2));

                await server.MergePatchAsync("api/data", new[]
                {
                    new { id = 1, date = dateTime },
                    new { id = 2, date = dateTime.AddDays(15) }
                });

                var patchedModel = await server.GetAsync<TestModel>("api/data/0");
                var expected = GetTestModel();
                Assert.Equal(expected, patchedModel);

                patchedModel = await server.GetAsync<TestModel>("api/data/1");
                expected = GetTestModel();
                expected.Date = dateTime;
                Assert.Equal(expected, patchedModel);

                patchedModel = await server.GetAsync<TestModel>("api/data/2");
                expected = GetTestModel();
                expected.Date = dateTime.AddDays(15);
                Assert.Equal(expected, patchedModel);
            }
        }
    }
}
