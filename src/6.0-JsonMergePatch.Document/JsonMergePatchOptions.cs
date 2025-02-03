namespace Morcatko.AspNetCore.JsonMergePatch
{
	public class JsonMergePatchOptions
	{
		/// <summary>
		/// Allow to delete property when setting null on a dictionary type
		/// </summary>
		public bool EnableDelete { get; set; }
	}
}
