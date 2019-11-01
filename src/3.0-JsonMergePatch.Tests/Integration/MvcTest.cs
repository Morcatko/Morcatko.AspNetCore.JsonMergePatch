using System;
using System.Collections.Generic;
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

		public static IEnumerable<object[]> GetCombinations()
		{
			//yield return new object[] { false, false };
			yield return new object[] { false, true };
			//yield return new object[] { true, false };
			yield return new object[] { true, true };
		}

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public async Task PatchString(bool core, bool newtonsoft)
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync("api/data/0", GetTestModel());
				
				await server.MergePatchAsync("api/data/0", 
					new { String= "changed string" });

				var patchedModel = await server.GetAsync<TestModel>("api/data/0");
				var expected = GetTestModel();
				expected.String = "changed string";
				Assert.Equal(expected, patchedModel);
			}
		}

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public async Task PatchIntegers(bool core, bool newtonsoft)
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
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

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public async Task PatchDateTimeOffset(bool core, bool newtonsoft)
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync("api/data/0", GetTestModel());
				await server.PostAsync("api/data/1", GetTestModel());
				await server.PostAsync("api/data/2", GetTestModel());

				var dateTime = new DateTimeOffset(2019, 10, 29, 9, 38, 0, 0, TimeSpan.FromHours(2));

				await server.MergePatchAsync("api/data/2", 
					new { date = dateTime.AddDays(15) }
				);

				var patchedModel = await server.GetAsync<TestModel>("api/data/2");
				var expected = GetTestModel();
				expected.Date = dateTime.AddDays(15);
				Assert.Equal(expected, patchedModel);
			}
		}

		[Theory(Skip = "Does not work")]
		[MemberData(nameof(GetCombinations))]
		public async Task MissingRequiredProperty(bool core, bool newtonsoft)
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync("api/data/0", GetTestModel());

				var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0/validate", new { integer = 8 });

				var expected = GetTestModel();
				expected.Integer = 8;
				Assert.Equal(expected, patchedModel);
			}
		}

		#region ValueEnum
		[Theory(Skip = "Enums do not work - https://github.com/aspnet/Home/issues/2423")]
		[MemberData(nameof(GetCombinations))]
		public async Task PatchValueEnum(bool core, bool newtonsoft)
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync("api/data/0", GetTestModel());

				var patchedModel = await server.MergePatchAsync<TestModel>("api/data/0", new { valueEnum = "Feet" });

				var expected = GetTestModel();
				expected.ValueEnum = ValueEnum.ft;
				Assert.Equal(expected, patchedModel);
			}
		}

		[Theory(Skip = "JsonPatch (not merge) fails as well - https://github.com/aspnet/Home/issues/2423")]
		[MemberData(nameof(GetCombinations))]
		public async Task JsonPatchValueEnum(bool core, bool newtonsoft)
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync("api/data/0", GetTestModel());
				await server.JsonPatchAsync("api/data/0", new[] { new { op = "replace", path = "/valueEnum", value = "Feet" } });
			}
		}

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public async Task PostValueEnum(bool core, bool newtonsoft)
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync("api/data/0", new { valueEnum = "Feet" });
			}
		}
		#endregion
	}
}
