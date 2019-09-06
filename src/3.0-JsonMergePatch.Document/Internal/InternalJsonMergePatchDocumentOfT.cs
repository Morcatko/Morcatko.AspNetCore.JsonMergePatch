using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;
using System;

namespace Morcatko.AspNetCore.JsonMergePatch.Internal
{
	public interface IInternalJsonMergePatchDocument
	{
		public abstract IContractResolver ContractResolver { get; set; }
		void AddOperation_Replace(string path, object value);
		void AddOperation_Remove(string path);
		void AddOperation_Add(string path);
	}

	public class InternalJsonMergePatchDocument<TModel> : JsonMergePatchDocument<TModel>, IInternalJsonMergePatchDocument where TModel : class
	{
		private static string replaceOp = OperationType.Replace.ToString();
		private static string addOp = OperationType.Add.ToString();
		private static string removeOp = OperationType.Remove.ToString();
		private readonly Type _modelType = typeof(TModel);

		internal InternalJsonMergePatchDocument(TModel model)
			: base(model)
		{
		}

		public virtual void AddOperation_Replace(string path, object value)
			=> base.Operations.Add(new Operation<TModel>(replaceOp, path, null, value));

		public virtual void AddOperation_Remove(string path)
			=> base.Operations.Add(new Operation<TModel>(removeOp, path, null, null));

		public virtual void AddOperation_Add(string path)
		{
			var propertyType = ReflectionHelper.GetPropertyTypeFromPath(_modelType, path, ContractResolver);
			base.Operations.Add(new Operation<TModel>(addOp, path, null, ContractResolver.ResolveContract(propertyType).DefaultCreator()));
		}

	}
}
