using Slabs.Experimental.ConsoleClient.Tests;
using System;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			RunTestSuite().Wait();
			Console.WriteLine("Press any key to stop...");
			Console.ReadKey();
		}

		private static async Task RunTestSuite()
		{
			Console.WriteLine("Init Test Suite...");
			var gamingTestSuite = new TestSuiteBuilder("gaming")
				.Add<GetTeamsTest>("get-teams")
				.Add<AddTeamsTest>("add-teams")
				.Add<GetTeamDetailTest>("get-team-detail")
				.AddParallel<GetHeroesTest>("get-heroes")
				.AddParallel<GetMatchesTest>("get-matches")
				.Add<ResetTest>("reset")
				.Build();

			await gamingTestSuite.Run();
			
		}

	}

	
}