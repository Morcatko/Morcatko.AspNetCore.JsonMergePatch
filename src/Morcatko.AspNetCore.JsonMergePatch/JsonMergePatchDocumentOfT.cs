﻿using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Morcatko.AspNetCore.JsonMergePatch.Builder;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Morcatko.AspNetCore.JsonMergePatch
{
    public abstract class JsonMergePatchDocument
    {
        public const string ContentType = "application/merge-patch+json";
        internal abstract void AddPatch(string path, object value);
        internal abstract void AddObject(string path);
        internal abstract void Delete(string path);
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
        private static string addOp = OperationType.Add.ToString();
        private static string removeOp = OperationType.Remove.ToString();
        
        private static char[] pathSplitter = new[] { '/' };
        private Type Type = typeof(TModel);

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

        internal override void Delete(string path)
        {
            JsonPatchDocument.Operations.Add(new Operation<TModel>(removeOp, path, null,  null));
        }

        internal override void AddObject(string path)
        {
            var propertyType = this.RetrievePropertyTypeFromPath(Type, path);
            JsonPatchDocument.Operations.Add(new Operation<TModel>(addOp, path, null, ContractResolver.ResolveContract(propertyType).DefaultCreator()));
        }

        private Type RetrievePropertyTypeFromPath(Type type, string path)
        {
            var currentType = type;
            foreach (var propertyName in path.Split(pathSplitter, StringSplitOptions.RemoveEmptyEntries))
            {
                var jsonContract = ContractResolver.ResolveContract(currentType);
                if (jsonContract is JsonDictionaryContract jsonDictionaryContract)
                {
                    currentType = jsonDictionaryContract.DictionaryValueType;
                    continue;
                }
                var currentProperty = GetPropertyInfo(currentType, propertyName);
                currentType = currentProperty.PropertyType;
            }
            return currentType;
        }


        private PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            return type.GetProperties().Single(property => property.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
        }


        private bool Exist(object value, IEnumerable<string> paths)
        {
            if (value == null) return false;
            var currentPath = paths.FirstOrDefault();
            if (currentPath == null)
            {
                return value != null;
            }

            object currentValue;

            var jsonContract = ContractResolver.ResolveContract(value.GetType());
            if (jsonContract is JsonDictionaryContract)
            {
                try
                {
                    currentValue = value.GetType().GetProperty("Item")
                             .GetValue(value, new[] { currentPath });
                }
                catch (Exception)
                {
                    return false;
                }

            }
            else
            {
                currentValue = GetPropertyInfo(value.GetType(), currentPath).GetValue(value);
            }

            
            return Exist(currentValue, paths.Skip(1));
        }

        public TModel ApplyTo(TModel objectToApplyTo)
        {
            this.ClearAddOperation(objectToApplyTo);
            JsonPatchDocument.ApplyTo(objectToApplyTo);
            return objectToApplyTo;
        }

        private void ClearAddOperation(TModel objectToApplyTo)
        {
            var addOperations = this.JsonPatchDocument.Operations.Where(operation => operation.OperationType == OperationType.Add).ToArray();
            foreach (var addOperation in addOperations)
            {
                if (Exist(objectToApplyTo, addOperation.path.Split(pathSplitter, StringSplitOptions.RemoveEmptyEntries)))
                {
                    JsonPatchDocument.Operations.Remove(addOperation);
                }
            }
        }
    }
}
