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
		public T Deserialize<T>(string json)
			=> System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

		public string Serialize(object data)
			=> System.Text.Json.JsonSerializer.Serialize(data, data.GetType());
	}
}
