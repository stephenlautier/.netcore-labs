using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Tests
{
	public class Auth_LoginTest : ITest
	{
		private readonly ILogger _logger;

		public Auth_LoginTest(ILogger<Auth_LoginTest> logger)
		{
			_logger = logger;
		}

		public async Task Execute()
		{
			_logger.LogInformation("[{service}] Executing...", nameof(Auth_LoginTest));
			await Task.Delay(TimeSpan.FromSeconds(2));
			_logger.LogInformation("[{service}] complete", nameof(Auth_LoginTest));
		}
	}

	public static class AuthTests
	{
		public static TestGroupBuilder GetGroup()
		{
			return new TestGroupBuilder()
				.Add<Auth_LoginTest>("get-teams");
		}

		public static TestSuiteBuilder AddAuthTests(this TestSuiteBuilder testSuiteBuilder, int repeat = 1)
		{
			testSuiteBuilder.Add(GetGroup(), repeat);
			return testSuiteBuilder;
		}

	}
}
