using Microsoft.AspNetCore.Mvc;
using Morcatko.AspNetCore.JsonMergePatch;
using testApp.Models;

namespace testApp.Controllers
{
	[Route("api/[controller]")]
	public class TestController
	{
		static TestModel _model;

		static TestController()
		{
			_model = new TestModel()
			{
				SubModel = new SubModel()
				{
					Numbers = new[] { 1, 2, 3 }
				}
			};
		}


		[HttpPut]
		public void Put([FromBody]TestModel model) => _model = model;

		[HttpGet]
		public TestModel Get() => _model;


		[HttpPatch]
		[Consumes(JsonMergePatchDocument.ContentType)]
		public TestModel Patch([FromBody] JsonMergePatchDocument<TestModel> model)
		{
			model.ApplyTo(_model);
			return _model;
		}
	}
}
