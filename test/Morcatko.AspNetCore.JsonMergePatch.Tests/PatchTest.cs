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

        [Fact(Skip = "Does not work")]
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
        [Fact(Skip = "Enums do not work - https://github.com/aspnet/Home/issues/2423")]
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

        [Fact(Skip = "JsonPatch (not merge) fails as well - https://github.com/aspnet/Home/issues/2423")]
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
        public async Task PatchSubPropertyOfNotExistingObject()
        {
            using (var server = Helper.CreateServer())
            {
                var model = GetTestModel();
                model.SubModel = null;
                await server.PostAsync("api/data/0", model);

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { subModel = new { value1 = "new value 1" } });

                var expected = GetTestModel();
                expected.SubModel = new SubModel() { Value1 = "new value 1" };
                Assert.Equal(expected, patchedModel);
            }
        }


        [Fact]
        public async Task PatchTwoSubPropertiesOfNotExistingObject()
        {
            using (var server = Helper.CreateServer())
            {
                var model = GetTestModel();
                model.SubModel = null;
                await server.PostAsync("api/data/0", model);

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { subModel = new { value1 = "new value 1", numbers = new[] { 1, 3 } } });

                var expected = GetTestModel();
                expected.SubModel = new SubModel() { Value1 = "new value 1", Numbers = new[] { 1, 3 } };
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact]
        public async Task PatchAddAnObjectToANullProperty()
        {
            using (var server = Helper.CreateServer())
            {
                var model = GetTestModel();
                model.SubModel = null;
                await server.PostAsync("api/data/0", model);

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { subModel = new { } });

                var expected = GetTestModel();
                expected.SubModel = new SubModel();
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact]
        public async Task PatchSubSubObjectEmpty()
        {
            using (var server = Helper.CreateServer())
            {
                var model = GetTestModel();
                model.SubModel = null;
                await server.PostAsync("api/data/0", model);

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { subModel = new { subSubModel = new { } } });

                var expected = GetTestModel();
                expected.SubModel = new SubModel { SubSubModel = new SubSubModel() };
                Assert.Equal(expected, patchedModel);
            }
        }

        [Fact]
        public async Task PatchSubSubObjectProperty()
        {
            using (var server = Helper.CreateServer())
            {
                var model = GetTestModel();
                model.SubModel = null;
                await server.PostAsync("api/data/0", model);

                var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { subModel = new { subSubModel = new { value1 = "new value 1" } } });

                var expected = GetTestModel();
                expected.SubModel = new SubModel { SubSubModel = new SubSubModel { Value1 = "new value 1" } };
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
