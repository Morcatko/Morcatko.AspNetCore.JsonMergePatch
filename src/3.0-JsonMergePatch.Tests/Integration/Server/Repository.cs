using System.Collections.Generic;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.Integration.Server
{
	//Workaround - Static Dictionary would be enough, but we want new "static" instance per each test/instance of TestServer
	public interface IRepository<T> : IDictionary<int, T> where T : TestModelBase
	{ }

	class Repository<T> : Dictionary<int, T>, IRepository<T> where T : TestModelBase
	{ }
}
