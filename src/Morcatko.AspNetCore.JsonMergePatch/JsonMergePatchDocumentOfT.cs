using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Morcatko.AspNetCore.JsonMergePatch.Builder;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Morcatko.AspNetCore.JsonMergePatch
{
	public abstract class JsonMergePatchDocument
	{
		public const string ContentType = "application/merge-patch+json";
		internal abstract void AddPatch(string path, object value);
		public abstract IContractResolver ContractResolver { get; set; }

		/// <summary>
		/// Returns Patch computed as "diff" of "patched - original"
		/// Warning: Only for tests - result.Model is not same as when user as InputFormatter
		/// </summary>
		public static JsonMergePatchDocument<TModel> Build<TModel>(TModel original, TModel patched) where TModel : class
			=> new PatchBuilder<TModel>().Build(original, patched);

		public static JsonMergePatchDocument<TModel> Build<TModel>(JObject jsonObject) where TModel : class
			=> new PatchBuilder<TModel>().Build(jsonObject);

		public static JsonMergePatchDocument<TModel> Build<TModel>(string jsonObject) where TModel : class
			=> new PatchBuilder<TModel>().Build(jsonObject);
	}

	public class JsonMergePatchDocument<TModel> : JsonMergePatchDocument where TModel : class
	{
		private static string replaceOp = OperationType.Replace.ToString();

		public TModel Model { get; internal set; }

		public JsonPatchDocument<TModel> JsonPatchDocument { get; } = new JsonPatchDocument<TModel>();

		public override IContractResolver ContractResolver
		{
			get => JsonPatchDocument.ContractResolver;
			set => JsonPatchDocument.ContractResolver = value;
		}

		internal JsonMergePatchDocument(TModel model)
		{
			this.Model = model;
		}

		internal override void AddPatch(string path, object value)
		{
			JsonPatchDocument.Operations.Add(new Operation<TModel>(replaceOp, path, null, value));
		}

		public TModel ApplyTo(TModel objectToApplyTo)
		{
			JsonPatchDocument.ApplyTo(objectToApplyTo);
			return objectToApplyTo;
		}
	}
}
