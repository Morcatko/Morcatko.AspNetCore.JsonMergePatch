using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration
{
	public class MvcTest
	{
		public static IEnumerable<object[]> GetCombinations()
		{
			yield return new object[] { false, false };
			yield return new object[] { false, true };
			yield return new object[] { true, false };
			yield return new object[] { true, true };
		}


		[Theory]
		[MemberData(nameof(GetCombinations))]
		public async Task SimplePatch(bool core, bool newtonsoft)
		{
			using (var p = new TestHelper(core, newtonsoft))
			{
				await p.PostAsync("0", p.GetTestModel());

				await p.MergePatchAsync("0", new { Integer = 7 });

				var patchedModel = await p.GetAsync("0");
				var expected = p.GetTestModel();
				expected.Integer = 7;
				Assert.Equal(expected, patchedModel);
			}
		}

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public async Task PatchIntegers(bool core, bool newtonsoft)
		{
			using (var p = new TestHelper(core, newtonsoft))
			{
				await p.PostAsync("0", p.GetTestModel());
				await p.PostAsync("1", p.GetTestModel());
				await p.PostAsync("2", p.GetTestModel());

				await p.MergePatchAsync(null, new[]
				{
					new { Id = 1, Integer = 7 },
					new { Id = 2, Integer = 9 }
				});

				var patchedModel = await p.GetAsync("0");
				var expected = p.GetTestModel();
				Assert.Equal(expected, patchedModel);

				patchedModel = await p.GetAsync("1");
				expected = p.GetTestModel();
				expected.Integer = 7;
				Assert.Equal(expected, patchedModel);

				patchedModel = await p.GetAsync("2");
				expected = p.GetTestModel();
				expected.Integer = 9;
				Assert.Equal(expected, patchedModel);
			}
		}

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public async Task PatchDateTimeOffsets(bool core, bool newtonsoft)
		{
			using (var p = new TestHelper(core, newtonsoft))
			{
				await p.PostAsync("0", p.GetTestModel());
				await p.PostAsync("1", p.GetTestModel());
				await p.PostAsync("2", p.GetTestModel());

				var dateTime = new DateTimeOffset(2019, 10, 29, 9, 38, 0, 0, TimeSpan.FromHours(2));

				await p.MergePatchAsync(null, new[]
				{
					new { Id = 1, Date = dateTime },
					new { Id = 2, Date = dateTime.AddDays(15) }
				});

				var patchedModel = await p.GetAsync("0");
				var expected = p.GetTestModel();
				Assert.Equal(expected, patchedModel);

				patchedModel = await p.GetAsync("1");
				expected = p.GetTestModel();
				expected.Date = dateTime;
				Assert.Equal(expected, patchedModel);

				patchedModel = await p.GetAsync("2");
				expected = p.GetTestModel();
				expected.Date = dateTime.AddDays(15);
				Assert.Equal(expected, patchedModel);
			}
		}

		[Theory(Skip = "Does not work")]
		[MemberData(nameof(GetCombinations))]
		public async Task MissingRequiredProperty(bool core, bool newtonsoft)
		{
			using (var p = new TestHelper(core, newtonsoft))
			{
				await p.PostAsync("0", p.GetTestModel());

				var patchedModel = await p.MergePatchAsync("0/validate", new { integer = 8 });

				var expected = p.GetTestModel();
				expected.Integer = 8;
				Assert.Equal(expected, patchedModel);
			}
		}

		#region ValueEnum
		[Theory(Skip = "Enums do not work - https://github.com/aspnet/Home/issues/2423")]
		[MemberData(nameof(GetCombinations))]
		public async Task PatchValueEnum(bool core, bool newtonsoft)
		{
			using (var p = new TestHelper(core, newtonsoft))
			{
				await p.PostAsync("0", p.GetTestModel());

				var patchedModel = await p.MergePatchAsync("0", new { valueEnum = "Feet" });

				var expected = p.GetTestModel();
				expected.ValueEnum = ValueEnum.ft;
				Assert.Equal(expected, patchedModel);
			}
		}

		[Theory(Skip = "JsonPatch (not merge) fails as well - https://github.com/aspnet/Home/issues/2423")]
		[MemberData(nameof(GetCombinations))]
		public async Task JsonPatchValueEnum(bool core, bool newtonsoft)
		{
			using (var p = new TestHelper(core, newtonsoft))
			{
				await p.PostAsync("0", p.GetTestModel());
				await p.JsonPatchAsync("0", new[] { new { op = "replace", path = "/valueEnum", value = "Feet" } });
			}
		}

		[Theory]
		[MemberData(nameof(GetCombinations))]
		public async Task PostValueEnum(bool core, bool newtonsoft)
		{
			using (var p = new TestHelper(core, newtonsoft))
			{
				await p.PostAsync("0", new { valueEnum = "Feet" });
			}
		}
		#endregion
	}
}
