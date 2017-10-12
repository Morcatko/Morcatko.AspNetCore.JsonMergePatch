using Morcatko.AspNetCore.JsonMergePatch.Tests.Server;
using System.Threading.Tasks;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests
{
    public class PatchTest
    {
        private TestModel GetTestModel() =>
            new TestModel()
            {
                Integer = 5,
                String = "string",
                Float = 1.5f,
                Boolean = false,
                Renamed = "some string",
                SubModel = new SubModel()
                {
                    Value1 = "value 1",
                    Value2 = "value 2",
                    Numbers = new[] { 1, 2, 3 }
                }
            };


        [Fact]
        public async Task PatchInteger()
        {
            using (var server = Helper.CreateServer())
            using (var client = server.CreateClient())
            {
                await client.PostAsync("api/data/0", GetTestModel());

                await server.PatchAsync("api/data/0", new { integer = 8 });

                var patchedModel = await client.GetAsync<TestModel>("api/data/0");

                var expected = GetTestModel();
                expected.Integer = 8;
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact]
        public async Task PatchSubProperty()
        {
            using (var server = Helper.CreateServer())
            using (var client = server.CreateClient())
            {
                await client.PostAsync("api/data/0", GetTestModel());

                await server.PatchAsync("api/data/0", new { subModel = new { value1 = "new value 1" } });

                var patchedModel = await client.GetAsync<TestModel>("api/data/0");

                var expected = GetTestModel();
                expected.SubModel.Value1 = "new value 1";
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact]
        public async Task PatchObject()
        {
            using (var server = Helper.CreateServer())
            using (var client = server.CreateClient())
            {
                await client.PostAsync("api/data/0", GetTestModel());

                await server.PatchAsync("api/data/0", new { subModel = new SubModel() { Value1 = "All others are null" }});

                var patchedModel = await client.GetAsync<TestModel>("api/data/0");

                var expected = GetTestModel();
                expected.SubModel = new SubModel() { Value1 = "All others are null" };
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact]
        public async Task NullObject()
        {
            using (var server = Helper.CreateServer())
            using (var client = server.CreateClient())
            {
                await client.PostAsync("api/data/0", GetTestModel());

                await server.PatchAsync("api/data/0", new { subModel = null as SubModel });

                var patchedModel = await client.GetAsync<TestModel>("api/data/0");

                var expected = GetTestModel();
                expected.SubModel = null;
                Assert.Equal(expected, patchedModel);
            }
        }
    }
}
