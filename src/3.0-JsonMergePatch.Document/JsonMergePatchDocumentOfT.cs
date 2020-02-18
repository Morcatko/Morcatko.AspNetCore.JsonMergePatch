using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Morcatko.AspNetCore.JsonMergePatch.Internal;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Morcatko.AspNetCore.JsonMergePatch
{
	public abstract class JsonMergePatchDocument
	{
		public const string ContentType = "application/merge-patch+json";
		public abstract IContractResolver ContractResolver { get; set; }
	}

	public class JsonMergePatchDocument<TModel> : JsonMergePatchDocument where TModel : class
	{
		private readonly JsonPatchDocument<TModel> _jsonPatchDocument = new JsonPatchDocument<TModel>();
		public TModel Model { get; internal set; }
		public List<Operation<TModel>> Operations => _jsonPatchDocument.Operations;
		public override IContractResolver ContractResolver
		{
			get => _jsonPatchDocument.ContractResolver;
			set => _jsonPatchDocument.ContractResolver = value;
		}

		internal JsonMergePatchDocument(TModel model)
		{
			this.Model = model;
		}

		bool clean = false;
		private void ClearAddOperation(object objectToApplyTo)
		{
			if (clean)
				throw new NotSupportedException("Cannot apply more than once");

			var addOperations = _jsonPatchDocument.Operations.Where(operation => operation.OperationType == OperationType.Add).ToArray();
			foreach (var addOperation in addOperations)
			{
				if (ReflectionHelper.Exist(objectToApplyTo, addOperation.path, ContractResolver))
				{
					_jsonPatchDocument.Operations.Remove(addOperation);
				}
			}
			clean = true;
		}

		public TModel ApplyTo(TModel objectToApplyTo)
		{
			this.ClearAddOperation(objectToApplyTo);
			_jsonPatchDocument.ApplyTo(objectToApplyTo);
			return objectToApplyTo;
		}

		public TOtherModel ApplyTo<TOtherModel>(TOtherModel objectToApplyTo) where TOtherModel : class
		{
			this.ClearAddOperation(objectToApplyTo);

			var newP = new JsonPatchDocument<TOtherModel>(
				_jsonPatchDocument
					.Operations
					.Select(o => new Operation<TOtherModel>(o.op, o.path, o.from, o.value))
					.ToList(),
				ContractResolver);

			newP.ApplyTo(objectToApplyTo);
			return objectToApplyTo;
		}
	}
}
