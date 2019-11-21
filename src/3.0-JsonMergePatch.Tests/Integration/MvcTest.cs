using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration
{
	public class MvcTest
	{
		private static TestModelBase GetTestModel(bool newtonsoft)
		{
			var result = newtonsoft
				? (TestModelBase)new NewtonsoftTestModel()
				: (TestModelBase)new SystemTextTestModel();
			result.Integer = 5;
			result.String = "string";
			result.Float = 1.5f;
			result.Boolean = false;
			result.Renamed = "some string";
			result.SimpleEnum = SimpleEnum.two;
			result.ValueEnum = ValueEnum.i;
			result.SubModel = new SubModel()
			{
				Value1 = "value 1",
				Value2 = "value 2",
				Numbers = new[] { 1, 2, 3 }
			};
			return result;
		}

		private static string GetUrl(bool newtonsoft, int? id = null)
			=> (newtonsoft
				? "api/data/newtonsoft"
				: "api/data/systemText")
			+ (id.HasValue ? $"/{id}" : null);

		public static IEnumerable<object[]> GetCombinations()
		{
			yield return new object[] { false, false };
			yield return new object[] { false, true };
			yield return new object[] { true, false };
			yield return new object[] { true, true };
		}

		private static async Task _SimplePatch<T>(bool core, bool newtonsoft) where T : TestModelBase
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync(GetUrl(newtonsoft, 0), GetTestModel(newtonsoft));

				await server.MergePatchAsync(GetUrl(newtonsoft, 0), new { integer = 7 });

				var patchedModel = await server.GetAsync<T>(GetUrl(newtonsoft, 0));
				var expected = GetTestModel(newtonsoft);
				expected.Integer = 7;
				Assert.Equal(expected, patchedModel);
			}
		}

		private static async Task _PatchIntegers<T>(bool core, bool newtonsoft) where T : TestModelBase
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync(GetUrl(newtonsoft, 0), GetTestModel(newtonsoft));
				await server.PostAsync(GetUrl(newtonsoft, 1), GetTestModel(newtonsoft));
				await server.PostAsync(GetUrl(newtonsoft, 2), GetTestModel(newtonsoft));

				await server.MergePatchAsync(GetUrl(newtonsoft), new[]
				{
					new { id = 1, integer = 7 },
					new { id = 2, integer = 9 }
				});

				var patchedModel = await server.GetAsync<T>(GetUrl(newtonsoft, 0));
				var expected = GetTestModel(newtonsoft);
				Assert.Equal(expected, patchedModel);

				patchedModel = await server.GetAsync<T>(GetUrl(newtonsoft, 1));
				expected = GetTestModel(newtonsoft);
				expected.Integer = 7;
				Assert.Equal(expected, patchedModel);

				patchedModel = await server.GetAsync<T>(GetUrl(newtonsoft, 2));
				expected = GetTestModel(newtonsoft);
				expected.Integer = 9;
				Assert.Equal(expected, patchedModel);
			}
		}

		private static async Task _PatchDateTimeOffsets<T>(bool core, bool newtonsoft) where T : TestModelBase
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync(GetUrl(newtonsoft, 0), GetTestModel(newtonsoft));
				await server.PostAsync(GetUrl(newtonsoft, 1), GetTestModel(newtonsoft));
				await server.PostAsync(GetUrl(newtonsoft, 2), GetTestModel(newtonsoft));

				var dateTime = new DateTimeOffset(2019, 10, 29, 9, 38, 0, 0, TimeSpan.FromHours(2));

				await server.MergePatchAsync(GetUrl(newtonsoft), new[]
				{
					new { id = 1, date = dateTime },
					new { id = 2, date = dateTime.AddDays(15) }
				});

				var patchedModel = await server.GetAsync<T>(GetUrl(newtonsoft, 0));
				var expected = GetTestModel(newtonsoft);
				Assert.Equal(expected, patchedModel);

				patchedModel = await server.GetAsync<T>(GetUrl(newtonsoft, 1));
				expected = GetTestModel(newtonsoft);
				expected.Date = dateTime;
				Assert.Equal(expected, patchedModel);

				patchedModel = await server.GetAsync<T>(GetUrl(newtonsoft, 2));
				expected = GetTestModel(newtonsoft);
				expected.Date = dateTime.AddDays(15);
				Assert.Equal(expected, patchedModel);
			}
		}

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public Task SimplePatch(bool core, bool newtonsoft)
			=> newtonsoft
				? _SimplePatch<NewtonsoftTestModel>(core, newtonsoft)
				: _SimplePatch<SystemTextTestModel>(core, newtonsoft);

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public Task PatchIntegers(bool core, bool newtonsoft)
			=> newtonsoft
				? _PatchIntegers<NewtonsoftTestModel>(core, newtonsoft)
				: _PatchIntegers<SystemTextTestModel>(core, newtonsoft);

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public Task PatchDateTimeOffsets(bool core, bool newtonsoft)
			=> newtonsoft
				? _PatchDateTimeOffsets<NewtonsoftTestModel>(core, newtonsoft)
				: _PatchDateTimeOffsets<SystemTextTestModel>(core, newtonsoft);

		[Theory(Skip = "Does not work")]
		[MemberData(nameof(GetCombinations))]
		public async Task MissingRequiredProperty(bool core, bool newtonsoft)
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync(GetUrl(newtonsoft, 0), GetTestModel(newtonsoft));

				var patchedModel = await server.MergePatchAsync<NewtonsoftTestModel>(GetUrl(newtonsoft, 0) + "/validate", new { integer = 8 });

				var expected = GetTestModel(newtonsoft);
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
				await server.PostAsync(GetUrl(newtonsoft, 0), GetTestModel(newtonsoft));

				var patchedModel = await server.MergePatchAsync<NewtonsoftTestModel>(GetUrl(newtonsoft, 0), new { valueEnum = "Feet" });

				var expected = GetTestModel(newtonsoft);
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
				await server.PostAsync(GetUrl(newtonsoft, 0), GetTestModel(newtonsoft));
				await server.JsonPatchAsync(GetUrl(newtonsoft, 0), new[] { new { op = "replace", path = "/valueEnum", value = "Feet" } });
			}
		}

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public async Task PostValueEnum(bool core, bool newtonsoft)
		{
			using (var server = Helper.CreateServer(core, newtonsoft))
			{
				await server.PostAsync(GetUrl(newtonsoft, 0), new { valueEnum = "Feet" });
			}
		}
		#endregion
	}
}
