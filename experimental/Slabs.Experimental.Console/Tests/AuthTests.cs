using FluentlyHttp;
using Microsoft.Extensions.Logging;
using NFluent;
using Slabs.Experimental.ConsoleClient.Testify;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Tests
{

	public class Auth_LoginTest : ITest
	{
		private readonly ILogger _logger;
		private readonly ISessionState _sessionState;

		private readonly FluentHttpClient _fluentHttpClient;

		public Auth_LoginTest(ILogger<Auth_LoginTest> logger, ISessionState sessionState, FluentHttpClientFactory fluentHttpClientFactory)
		{
			_logger = logger;
			_sessionState = sessionState;
			_fluentHttpClient = fluentHttpClientFactory.Get("auth");
		}

		public async Task Execute()
		{
			// non fluent
			var result = await _fluentHttpClient.Post<LoginResponse>("/api/auth/login", new
			{
				username = "test",
				password = "test"
			});

			// fluent
			var fluentResult = await _fluentHttpClient.CreateRequest("/api/auth/login")
				.AsPost()
				.WithBody(new
				{
					username = "test",
					password = "test"
				})
				.Return<LoginResponse>();

			Check.That(result).IsNotNull();
			Check.That(result.AccessToken).IsNotNull();

			_sessionState.Set("auth:token", result.AccessToken);

			_logger.LogInformation("[{service}] complete", nameof(Auth_LoginTest));
		}
	}

	public class Auth_KeepAliveTest : ITest
	{
		private readonly ILogger _logger;
		private readonly ISessionState _sessionState;
		private readonly FluentHttpClient _fluentHttpClient;

		public Auth_KeepAliveTest(ILogger<Auth_KeepAliveTest> logger, ISessionState sessionState, FluentHttpClientFactory fluentHttpClientFactory)
		{
			_logger = logger;
			_sessionState = sessionState;
			_fluentHttpClient = fluentHttpClientFactory.Get("auth");
		}

		public async Task Execute()
		{
			var token = _sessionState.Get<string>("auth:token");
			var result = await _fluentHttpClient.Patch<KeepAliveResponse>("/api/auth/keep-alive", token);

			Check.That(result).IsNotNull();
			Check.That(result.AccessToken).IsNotNull();

			_logger.LogInformation("[{service}] complete", nameof(Auth_KeepAliveTest));
		}
	}

	public class LoginResponse
	{
		public string AccessToken { get; set; }
		public int ExpiresIn { get; set; }
		public string TokenType { get; set; }
	}

	public class KeepAliveResponse : LoginResponse
	{
	}

	public static class AuthTests
	{
		public static TestGroupBuilder GetGroup()
		{
			return new TestGroupBuilder()
				.Add<Auth_LoginTest>("login")
				.Add<Auth_KeepAliveTest>("keep-alive")
				;
		}

		public static TestSuiteBuilder AddAuthTests(this TestSuiteBuilder testSuiteBuilder, int repeat = 1)
		{
			testSuiteBuilder.Add(GetGroup(), repeat);
			return testSuiteBuilder;
		}

	}
}
