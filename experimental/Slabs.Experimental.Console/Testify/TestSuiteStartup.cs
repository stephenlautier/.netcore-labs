using Microsoft.Extensions.Logging;
using Slabs.Experimental.ConsoleClient.Tests;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Testify
{
	public class TestSuiteStartup
	{
		private readonly ILogger<TestSuiteStartup> _logger;
		private readonly TestSuiteBuilderFactory _testSuiteBuilderFactory;

		public TestSuiteStartup(ILogger<TestSuiteStartup> logger, TestSuiteBuilderFactory testSuiteBuilderFactory)
		{
			_logger = logger;
			_testSuiteBuilderFactory = testSuiteBuilderFactory;
		}

		public async Task Run()
		{
			_logger.LogInformation("Init Test Suite...");

			var gamingTestGroup = new TestGroupBuilder()
				.Add<GetTeamsTest>("get-teams")
				.Add<AddTeamsTest>("add-teams")
				.Add<GetTeamDetailTest>("get-team-detail")
				.AddParallel<GetHeroesTest>("get-heroes")
				.AddParallel<GetMatchesTest>("get-matches")
				.Add<ResetTest>("reset");
			
			var gamingTestSuite = _testSuiteBuilderFactory.Create("gaming")
				.AddAuthTests()
				.Add(gamingTestGroup)
				.Build();

			await gamingTestSuite.Run();
		}
	}
}