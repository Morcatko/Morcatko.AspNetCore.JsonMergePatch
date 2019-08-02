using System.Collections.Generic;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.NewtonsoftJson.Integration.Server
{
	//Workaround - Static Dictionary would be enough, but we want new "static" instance per each test/instance of TestServer
	public interface IRepository : IDictionary<int, TestModel>
	{ }

	class Repository : Dictionary<int, TestModel>, IRepository
	{ }
}
