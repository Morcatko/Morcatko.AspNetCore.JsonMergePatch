using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Server
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly IRepository _repository;

        public DataController(IRepository repository)
        {
            _repository = repository;
        }
            
        [HttpGet]
        public IEnumerable<TestModel> Get() => _repository.Values;

        [HttpGet]
        [Route("{id}")]
        public TestModel Get(int id) => _repository[id];

        [HttpPost]
        [Route("{id}")]
        public TestModel Post(int id, [FromBody]TestModel model)
        {
            model.Id = id;
            _repository[id] = model;
            return model;
        }

        [HttpPatch]
        [Route("{id}")]
        [Consumes(JsonMergePatchDocument.ContentType)]
        public TestModel Patch(int id, [FromBody] JsonMergePatchDocument<TestModel> patch)
        {
            var model = _repository[id];
            patch.ApplyTo(model);
            _repository[id] = model;
            model.Id = id;
            return model;
        }

        [HttpPatch]
        [Route("{id}")]
        [Consumes("application/json-patch+json")]
        public TestModel FullPatch(int id, [FromBody] JsonPatchDocument<TestModel> patch)
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
        public TestModel PatchWithModelValidation(int id, [FromBody] JsonMergePatchDocument<TestModel> patch)
        {
            if (!ModelState.IsValid)
                throw new ArgumentException("Model is invalid");

            return Patch(id, patch);
        }

        [HttpPatch]
        [Consumes(JsonMergePatchDocument.ContentType)]
        public IEnumerable<TestModel> Patch([FromBody] IEnumerable<JsonMergePatchDocument<TestModel>> patches)
        {
            foreach (var patch in patches)
            {
                Patch(patch.Model.Id, patch);
            }
            return Get();
        }
    }
}
