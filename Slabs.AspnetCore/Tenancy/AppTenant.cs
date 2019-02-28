using System.Diagnostics;

namespace Slabs.AspnetCore.Tenancy
{
	public interface ITenant
	{
		string Key { get; set; }
		string Name { get; set; }
	}

	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AppTenant : ITenant
	{
		private string DebuggerDisplay => $"Key: '{Key}', Name: '{Name}'";

		public string Key { get; set; }
		public string Name { get; set; }
	}

	public static class Tenants
	{
		public static AppTenant Chiko = new AppTenant
		{
			Key = "chiko",
			Name = "Chiko"
		};

		public static AppTenant Sketch7 = new AppTenant
		{
			Key = "sketch7",
			Name = "Sketch7"
		};
	}
}