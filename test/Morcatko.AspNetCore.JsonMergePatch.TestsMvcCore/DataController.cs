using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Morcatko.AspNetCore.JsonMergePatch.TestsMvcCore
{
    [Route("api/[controller]")]
    public class DataController : ControllerBase
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
        public ObjectResult Get([FromRoute]int id)
        {
            return Ok(_repository[id]);
        }

        [HttpPost]
        [Route("{id}")]
        public ObjectResult Post([FromRoute]int id, [FromBody] TestModel model)
        {
            model.Id = id;
            _repository[id] = model;
            return Ok(model);
        }

        [HttpPatch]
        [Route("{id}")]
        public ObjectResult Patch([FromRoute]int id, [FromBody] JsonMergePatchDocument<TestModel> patch)
        {
            var model = _repository[id];
            patch.ApplyTo(model);
            _repository[id] = model;
            model.Id = id;
            return Ok(model);
        }
    }
}