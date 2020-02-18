using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server
{
	public class DataControllerBase<T> : Controller where T: TestModelBase
	{
		private readonly IRepository<T> _repository;

		public DataControllerBase(IRepository<T> repository)
		{
			_repository = repository;
		}

		[HttpGet]
		public IEnumerable<T> Get() => _repository.Values;

		[HttpGet]
		[Route("{id}")]
		public T Get(int id) => _repository[id];

		[HttpPost]
		[Route("{id}")]
		public T Post(int id, [FromBody]T model)
		{
			model.Id = id;
			_repository[id] = model;
			return model;
		}

		[HttpPatch]
		[Route("{id}")]
		[Consumes(JsonMergePatchDocument.ContentType)]
		public T Patch(int id, [FromBody] JsonMergePatchDocument<T> patch)
		{
			var model = _repository[id];
			patch.ApplyTo(model);
			_repository[id] = model;
			model.Id = id;
			return model;
		}

		[HttpPatch]
		[Route("{id}/full")]
		[Consumes("application/json-patch+json")]
		public T FullPatch(int id, [FromBody] JsonPatchDocument<T> patch)
		{
			var model = _repository[id];
			patch.ApplyTo(model);
			_repository[id] = model;
			model.Id = id;
			return model;
		}

		[HttpPatch]
		[Route("{id}/validate")]
		[Consumes(JsonMergePatchDocument.ContentType)]
		public T PatchWithModelValidation(int id, [FromBody] JsonMergePatchDocument<T> patch)
		{
			if (!ModelState.IsValid)
				throw new ArgumentException("Model is invalid");

			return Patch(id, patch);
		}

		[HttpPatch]
		[Consumes(JsonMergePatchDocument.ContentType)]
		public IEnumerable<T> Patch([FromBody] IEnumerable<JsonMergePatchDocument<T>> patches)
		{
			foreach (var patch in patches)
			{
				Patch(patch.Model.Id, patch);
			}
			return Get();
		}
	}

	[Route("api/data/newtonsoft")]
	public class NewtonsoftDataController : DataControllerBase<NewtonsoftTestModel>
	{
		public NewtonsoftDataController(IRepository<NewtonsoftTestModel> repository)
			:base(repository)
		{ }
	}
	[Route("api/data/systemText")]
	public class SystemTextDataController : DataControllerBase<SystemTextTestModel>
	{
		public SystemTextDataController(IRepository<SystemTextTestModel> repository)
			: base(repository)
		{ }
	}
}
