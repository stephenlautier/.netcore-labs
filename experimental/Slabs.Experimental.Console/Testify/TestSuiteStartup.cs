using FluentlyHttpClient;
using FluentlyHttpClient.Middleware;
using Microsoft.Extensions.Logging;
using Slabs.Experimental.ConsoleClient.Tests;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Testify
{
	public class TestSuiteStartup
	{
		private readonly ILogger<TestSuiteStartup> _logger;
		private readonly TestSuiteBuilderFactory _testSuiteBuilderFactory;
		private readonly IFluentHttpClientFactory _fluentHttpClientFactory;

		public TestSuiteStartup(ILogger<TestSuiteStartup> logger, TestSuiteBuilderFactory testSuiteBuilderFactory, IFluentHttpClientFactory fluentHttpClientFactory)
		{
			_logger = logger;
			_testSuiteBuilderFactory = testSuiteBuilderFactory;
			_fluentHttpClientFactory = fluentHttpClientFactory;
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
			_fluentHttpClientFactory.CreateBuilder("auth")
				// shared
				.WithHeader("user-agent", "slabs-testify")
				.WithHeader("locale", "en-GB")
				.WithTimeout(5)
				.UseTimer()
				.UseMiddleware<LoggerHttpMiddleware>()

				// auth
				//.WithBaseUrl("http://staging.api.cpm-odin.com:1001")
				.WithBaseUrl("http://localhost:2001")
				.Register()

				// common
				.WithIdentifier("common")
				.WithBaseUrl("http://staging.api.cpm-odin.com:1002")
				.Register();
		}
	}

	public static class FluentHttpClientFactoryExtensions
	{
		public static IFluentHttpClient GetAuthClient(this IFluentHttpClientFactory factory) => factory.Get("auth");
		public static IFluentHttpClient GetCommonClient(this IFluentHttpClientFactory factory) => factory.Get("common");
	}
}