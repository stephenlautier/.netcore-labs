using Microsoft.Extensions.Logging;
using Slabs.Experimental.ConsoleClient.Tests;
using System.Threading.Tasks;
using Slabs.Experimental.ConsoleClient.FluentHttp;

namespace Slabs.Experimental.ConsoleClient.Testify
{
	public class TestSuiteStartup
	{
		private readonly ILogger<TestSuiteStartup> _logger;
		private readonly TestSuiteBuilderFactory _testSuiteBuilderFactory;
		private readonly HttpClientFactory _httpClientFactory;

		public TestSuiteStartup(ILogger<TestSuiteStartup> logger, TestSuiteBuilderFactory testSuiteBuilderFactory, HttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_testSuiteBuilderFactory = testSuiteBuilderFactory;
			_httpClientFactory = httpClientFactory;
		}

		public async Task Run()
		{
			_logger.LogInformation("Init Test Suite...");
			SetupHttp();

			var gamingTestGroup = new TestGroupBuilder()
				.Add<GetTeamsTest>("get-teams")
				.Add<AddTeamsTest>("add-teams")
				.Add<GetTeamDetailTest>("get-team-detail")
				.AddParallel<GetHeroesTest>("get-heroes")
				.AddParallel<GetMatchesTest>("get-matches")
				.Add<ResetTest>("reset");

			var gamingTestSuite = _testSuiteBuilderFactory.Create("gaming")
				.AddCommonTests()
				.AddAuthTests()
				.Add(gamingTestGroup)
				.Build();

			await gamingTestSuite.Run();
		}

		private void SetupHttp()
		{
			_httpClientFactory.CreateBuilder("auth")
				// shared
				.AddHeader("user-agent", "slabs-testify")
				.AddHeader("Accept-Language", "en-GB")
				.SetTimeout(5)
				.AddMiddleware<TimerHttpMiddleware>()
				.AddMiddleware<LoggerHttpMiddleware>()

				// auth
				.SetBaseUrl("http://staging.api.cpm-odin.com:1001")
				.Register()

				// common
				.SetIdentifier("common")
				.SetBaseUrl("http://staging.api.cpm-odin.com:1002")
				.Register();
		}
	}
}