namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration
{
	interface ISerializer
	{
		string Serialize(object data);
		T Deserialize<T>(string json);
	}

	class NewtonsoftSerializer : ISerializer
	{
		public T Deserialize<T>(string json)
			=> Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);

		public string Serialize(object data)
			=> Newtonsoft.Json.JsonConvert.SerializeObject(data);
	}

	class SystemTextSerializer : ISerializer
	{
		private readonly System.Text.Json.JsonSerializerOptions _options;

		public SystemTextSerializer()
		{
			_options = new System.Text.Json.JsonSerializerOptions()
			{
				PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
			};
			_options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverterWithAttributeSupport());
		}

		public T Deserialize<T>(string json)
			=> System.Text.Json.JsonSerializer.Deserialize<T>(json, _options);

		public string Serialize(object data)
			=> System.Text.Json.JsonSerializer.Serialize(data, data.GetType(), _options);
	}
}
