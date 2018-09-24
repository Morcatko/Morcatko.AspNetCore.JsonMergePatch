using System.Collections.Generic;

namespace Morcatko.AspNetCore.JsonMergePatch.TestsMvcCore
{
    //Workaround - Static Dictionary would be enough, but we want new "static" instance per each test/instance of TestServer
    public interface IRepository : IDictionary<int, TestModel>
    { }

    public class Repository : Dictionary<int, TestModel>, IRepository
    { }
}
