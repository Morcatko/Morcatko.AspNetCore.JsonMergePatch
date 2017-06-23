using Newtonsoft.Json;

namespace testApp.Models
{
	public class TestModel
	{
		public int Integer { get; set; }
		public string String { get; set; }
		public float Float { get; set; }
		public bool Boolean { get; set; }

		[JsonProperty("NewName")]
		public string Renamed { get; set; }
		public SubModel SubModel { get; set; }
		
	}

	public class SubModel
	{
		public string Value1 { get; set; }
		public string Value2 { get; set; }
		public int[] Numbers { get; set; }
	}
}
