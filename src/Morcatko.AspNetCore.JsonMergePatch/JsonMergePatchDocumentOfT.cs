using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Morcatko.AspNetCore.JsonMergePatch.Builder;
using Newtonsoft.Json.Serialization;

namespace Morcatko.AspNetCore.JsonMergePatch
{
	public abstract class JsonMergePatchDocument
	{
		public const string ContentType = "application/merge-patch+json";
		internal abstract void AddPatch(string path, object value);
		public abstract IContractResolver ContractResolver { get; set; }

		//"patched" is full patched object, but it is only the diff-object when created from JSON/by InputFormatter
		public static JsonMergePatchDocument<TModel> Build<TModel>(TModel original, TModel patched) where TModel : class
			=> new PatchBuilder<TModel>().Build(original, patched);
	}

	public class JsonMergePatchDocument<TModel> : JsonMergePatchDocument where TModel : class
	{
		private static string replaceOp = OperationType.Replace.ToString();

		public TModel Model { get; internal set; }

		private readonly JsonPatchDocument<TModel> _jsonPatchDocument = new JsonPatchDocument<TModel>();

		public JsonPatchDocument<TModel> JsonPatchDocument => _jsonPatchDocument;

		public override IContractResolver ContractResolver
		{
			get => _jsonPatchDocument.ContractResolver;
			set => _jsonPatchDocument.ContractResolver = value;
		}

		public JsonMergePatchDocument() { }
		internal JsonMergePatchDocument(TModel model)
		{
			this.Model = model;
		}

		internal override void AddPatch(string path, object value)
		{
			_jsonPatchDocument.Operations.Add(new Operation<TModel>(replaceOp, path, null, value));
		}

		public TModel ApplyTo(TModel objectToApplyTo)
		{
			_jsonPatchDocument.ApplyTo(objectToApplyTo);
			return objectToApplyTo;
		}
	}
}
