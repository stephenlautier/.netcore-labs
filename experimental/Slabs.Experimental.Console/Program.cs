using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			RunTestSuite();
			Console.WriteLine("Press any key to stop...");
			Console.ReadKey();
		}

		private static async void RunTestSuite()
		{
			Console.WriteLine("Init TestSuite...");
			var gamingTestSuite = new TestSuiteBuilder("gaming")
				.Add("get-teams")
				.Add("add-teams")
				.Add("get-team-detail")
				.Build();

			await gamingTestSuite.Run();

		}

	}

	
}