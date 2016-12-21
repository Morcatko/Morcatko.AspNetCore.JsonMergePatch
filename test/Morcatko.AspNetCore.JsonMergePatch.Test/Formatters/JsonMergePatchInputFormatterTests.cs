using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Moq;
using Morcatko.AspNetCore.JsonMergePatch.Formatters;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Test.Formatters
{
    public class JsonMergePatchInputFormatterTests
    {
		//private JsonMergePatchInputFormatter CreateFormattter()
		//{
		//	return new JsonMergePatchInputFormatter(
		//		Mock.Of<ILogger>(),
		//		new JsonSerializerSettings(),
		//		ArrayPool<char>.Create(),
		//		ObjectPoolProvider.
		//}


		[Fact]
		public void CanRead()
		{
		}
    }
}
