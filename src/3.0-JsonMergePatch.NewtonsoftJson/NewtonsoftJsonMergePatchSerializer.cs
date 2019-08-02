using Morcatko.AspNetCore.JsonMergePatch.Internal;
using Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Linq;

namespace Morcatko.AspNetCore.JsonMergePatch
{
	class NewtonsoftJsonMergePatchSerializer : JsonSerializer
	{
		private readonly IList _listContainer;
		private readonly Type _innerModelType;
		private readonly JsonSerializerSettings _serializerSettings;
		private readonly JsonMergePatchOptions _jsonMergePatchOptions;
		private readonly JsonSerializer _innerjsonSerializer;

		public NewtonsoftJsonMergePatchSerializer(
			IList listContainer,
			Type innerModelType,
			JsonSerializer innerJsonSerializer,
			JsonSerializerSettings serializerSettings,
			JsonMergePatchOptions jsonMergePatchOptions)
		{
			_listContainer = listContainer;
			_innerModelType = innerModelType;
			_serializerSettings = serializerSettings;
			_jsonMergePatchOptions = jsonMergePatchOptions;
			_innerjsonSerializer = innerJsonSerializer;
		}

		private IInternalJsonMergePatchDocument CreatePatchDocument(JObject jObject, JsonSerializer jsonSerializer)
		{
			var jsonMergePatchDocument = PatchBuilder.CreatePatchDocument(_innerModelType, jObject, jsonSerializer, _jsonMergePatchOptions);
			jsonMergePatchDocument.ContractResolver = _serializerSettings.ContractResolver;
			return jsonMergePatchDocument;
		}

		public new object Deserialize(JsonReader reader, Type objectType)
		{
			var jToken = JToken.Load(reader);

			switch (jToken)
			{
				case JObject jObject:
					if (_listContainer != null)
						throw new ArgumentException("Received object when array was expected"); //This could be handled by returnin list with single item

					return CreatePatchDocument(jObject, _innerjsonSerializer);
				case JArray jArray:
					if (_listContainer == null)
						throw new ArgumentException("Received array when object was expected");

					foreach (var jObject in jArray.OfType<JObject>())
					{
						_listContainer.Add(CreatePatchDocument(jObject, _innerjsonSerializer));
					}
					return _listContainer;
			}

			throw new NotSupportedException("Unknown jToken type");
		}
	}
}
