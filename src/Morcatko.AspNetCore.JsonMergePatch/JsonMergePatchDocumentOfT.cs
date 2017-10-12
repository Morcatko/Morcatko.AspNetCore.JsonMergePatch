using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Morcatko.AspNetCore.JsonMergePatch
{
    public abstract class JsonMergePatchDocument
	{
		public const string ContentType = "application/merge-patch+json";
		public abstract void AddOperation(OperationType operationType, string path, object value);
	}

	public class JsonMergePatchDocument<TModel> : JsonMergePatchDocument where TModel : class
	{
		public TModel Model { get; }

		readonly JsonPatchDocument<TModel> _jsonPatchDocument = new JsonPatchDocument<TModel>();

		public JsonMergePatchDocument(TModel model)
		{
			this.Model = model;
		}

		public override void AddOperation(OperationType operationType, string path, object value)
		{
			_jsonPatchDocument.Operations.Add(new Operation<TModel>(operationType.ToString(), path, null, value));
		}

		public void ApplyTo(TModel objectToApplyTo)
		{
			_jsonPatchDocument.ApplyTo(objectToApplyTo);
		}
	}
}
