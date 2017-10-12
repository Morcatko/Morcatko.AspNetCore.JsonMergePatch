using Microsoft.AspNetCore.Mvc;
using Morcatko.AspNetCore.JsonMergePatch.Tests.Server.Models;
using System.Collections.Generic;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Server.Controllers
{
    [Route("api/[controller]")]
    public class DataController
    {
        static Dictionary<int, TestModel> _data = new Dictionary<int, TestModel>();

        [HttpGet]
        public IEnumerable<TestModel> Get() => _data.Values;

        [HttpGet]
        [Route("{id}")]
        public TestModel Get(int id) => _data[id];

        [HttpPost]
        [Route("{id}")]
        public TestModel Post(int id, [FromBody]TestModel model) => _data[id] = model;

        [HttpPatch]
        [Route("{id}")]
        [Consumes(JsonMergePatchDocument.ContentType)]
        public TestModel Patch(int id, [FromBody] JsonMergePatchDocument<TestModel> patch)
        {
            var model = _data[id];
            patch.ApplyTo(model);
            _data[id] = model;
            return model;
        }
    }
}
