using Grace.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Slabs.AspnetCore
{
	public class Program
	{
		public static void Main(string[] args)
		{
			BuildWebHost(args).Run();
		}

		public static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.UseGrace()
				.UseUrls("http://*:5001")
				.Build();
	}
}
