using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Morcatko.AspNetCore.JsonMergePatch.SystemText;
using Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration
{
	class TestHelper : IDisposable
	{
		private readonly bool _newtonsoft;
		public ISerializer Serializer { get; }
		public TestServer Server { get; }

		public TestHelper(bool core, bool newtonsoft)
		{
			void ConfigureNewtonsoft(MvcNewtonsoftJsonOptions o)
			{
				o.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
				o.SerializerSettings.DateParseHandling = Newtonsoft.Json.DateParseHandling.DateTimeOffset;
				o.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
			}

			void ConfigureSystemText(JsonOptions o)
			{
				o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverterWithAttributeSupport());
			}

			_newtonsoft = newtonsoft;
			Serializer = newtonsoft
				? (ISerializer)new NewtonsoftSerializer()
				: (ISerializer)new SystemTextSerializer();
			Server = new TestServer(new WebHostBuilder()
				.ConfigureServices(services =>
				{
					if (core)
					{
						var builder = services.AddMvcCore();
						if (newtonsoft)
							builder
								.AddNewtonsoftJson(ConfigureNewtonsoft)
								.AddNewtonsoftJsonMergePatch();
						else
							builder
								.AddJsonOptions(ConfigureSystemText)
								.AddSystemTextJsonMergePatch();
					}
					else
					{
						var builder = services.AddMvc();
						if (newtonsoft)
							builder
								.AddNewtonsoftJson(ConfigureNewtonsoft)
								.AddNewtonsoftJsonMergePatch();
						else
							builder
								.AddJsonOptions(ConfigureSystemText)
								.AddSystemTextJsonMergePatch();
					}
				})
				.UseStartup<Startup>());


		}

		public TestModelBase GetTestModel()
		{
			var result = _newtonsoft
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

		private string GetUrl(string suffix = null)
			=> (_newtonsoft
				? "api/data/newtonsoft"
				: "api/data/systemText")
			+ (suffix != null ? $"/{suffix}" : null);

		public Task PostAsync(string urlSuffix, object data)
			=> Server.PostAsync(Serializer, GetUrl(urlSuffix), data);

		internal async Task<TestModelBase> MergePatchAsync(string urlSuffix, object data)
			=> _newtonsoft
				? (TestModelBase)(await Server.MergePatchAsync<NewtonsoftTestModel>(Serializer, GetUrl(urlSuffix), data))
				: (TestModelBase)(await Server.MergePatchAsync<SystemTextTestModel>(Serializer, GetUrl(urlSuffix), data));

		internal async Task<IEnumerable<TestModelBase>> MergePatchAsync(string urlSuffix, object[] data)
			=> _newtonsoft
				? (IEnumerable<TestModelBase>)(await Server.MergePatchAsync<List<NewtonsoftTestModel>>(Serializer, GetUrl(urlSuffix), data))
				: (IEnumerable<TestModelBase>)(await Server.MergePatchAsync<List<SystemTextTestModel>>(Serializer, GetUrl(urlSuffix), data));

		internal async Task<TestModelBase> GetAsync(string urlSuffix)
			=> _newtonsoft
				? (TestModelBase)(await Server.GetAsync<NewtonsoftTestModel>(Serializer, GetUrl(urlSuffix)))
				: (TestModelBase)(await Server.GetAsync<SystemTextTestModel>(Serializer, GetUrl(urlSuffix)));

		internal Task JsonPatchAsync(string urlSuffix, object data)
			=> Server.JsonPatchAsync(Serializer, GetUrl(urlSuffix), data);

		public void Dispose()
		{
		}

	}
}
