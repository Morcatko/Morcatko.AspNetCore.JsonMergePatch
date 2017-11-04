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
        public async Task PatchInteger()
        {
            using (var server = Helper.CreateServer())
            {
                await server.PostAsync("api/data/0", GetTestModel());

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { integer = 8 });

                var expected = GetTestModel();
                expected.Integer = 8;
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact]
#warning this test fails - Need to fix somehow
        public async Task MissingRequiredProperty()
        {
            using (var server = Helper.CreateServer())
            {
                await server.PostAsync("api/data/0", GetTestModel());

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0/validate", new { integer = 8 });

                var expected = GetTestModel();
                expected.Integer = 8;
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact]
        public async Task PatchSimpleEnum()
        {
            using (var server = Helper.CreateServer())
            {
                await server.PostAsync("api/data/0", GetTestModel());

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { simpleEnum = "one" });

                var expected = GetTestModel();
                expected.SimpleEnum = SimpleEnum.one;
                Assert.Equal(expected, patchedModel);
            }
        }

        #region ValueEnum
        [Fact]
#warning this test fails - Need to fix somehow
        public async Task PatchValueEnum()
        {
            using (var server = Helper.CreateServer())
            {
                await server.PostAsync("api/data/0", GetTestModel());

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { valueEnum = "Feet" });

                var expected = GetTestModel();
                expected.ValueEnum = ValueEnum.ft;
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact] //Original JsonPatchDocument fails on ValueEnums as well
        public async Task JsonPatchValueEnum()
        {   
            using (var server = Helper.CreateServer())
            {
                await server.PostAsync("api/data/0", GetTestModel());
                await server.JsonPatchAsync("api/data/0", new[] { new { op = "replace", path = "/valueEnum", value = "Feet" } });
            }
        }

        [Fact] //Post works
        public async Task PostValueEnum()
        {
            using (var server = Helper.CreateServer())
            {
                await server.PostAsync("api/data/0", new { valueEnum = "Feet" });
            }
        }
        #endregion

        [Fact]
        public async Task PatchSubProperty()
        {
            using (var server = Helper.CreateServer())
            {
                await server.PostAsync("api/data/0", GetTestModel());

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { subModel = new { value1 = "new value 1" } });

                var expected = GetTestModel();
                expected.SubModel.Value1 = "new value 1";
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact]
        public async Task PatchObject()
        {
            using (var server = Helper.CreateServer())
            {
                await server.PostAsync("api/data/0", GetTestModel());

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { subModel = new SubModel() { Value1 = "All others are null" } });

                var expected = GetTestModel();
                expected.SubModel = new SubModel() { Value1 = "All others are null" };
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact]
        public async Task NullObject()
        {
            using (var server = Helper.CreateServer())
            {
                await server.PostAsync("api/data/0", GetTestModel());

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { subModel = null as SubModel });

                var expected = GetTestModel();
                expected.SubModel = null;
                Assert.Equal(expected, patchedModel);
            }
        }
    }
}
