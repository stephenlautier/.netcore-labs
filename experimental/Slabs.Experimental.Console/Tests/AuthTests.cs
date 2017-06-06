using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NFluent;
using Slabs.Experimental.ConsoleClient.Testify;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Slabs.Experimental.ConsoleClient.FluentHttp;

namespace Slabs.Experimental.ConsoleClient.Tests
{
	public class JsonContent : StringContent
	{
		public JsonContent(object obj) :
			base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
		{ }
	}

	public class Auth_LoginTest : ITest
	{
		private readonly ILogger _logger;
		private readonly ISessionState _sessionState;

		private readonly FluentHttpClient _fluentHttpClient;
		//private string _authBaseUri = "http://staging.api.cpm-odin.com:1001";

		public Auth_LoginTest(ILogger<Auth_LoginTest> logger, ISessionState sessionState, HttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_sessionState = sessionState;
			_fluentHttpClient = httpClientFactory.Get("auth");
		}

		public async Task Execute()
		{
			//var httpClient = new HttpClient
			//{
			//	BaseAddress = new Uri(_authBaseUri)
			//};
			//httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			//httpClient.Timeout = TimeSpan.FromSeconds(5);
			var result = await _fluentHttpClient.Post<LoginResponse>("/api/auth/login", new
			{
				username = "test",
				password = "test"
			});

			//var response = await httpClient.PostAsync("/api/auth/login", new JsonContent(new
			//{
			//	username = "test",
			//	password = "test",
			//}));
			//response.EnsureSuccessStatusCode();
			//var responseContent = await response.Content.ReadAsStringAsync();
			//var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

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
		private string _authBaseUri = "http://staging.api.cpm-odin.com:1001";

		public Auth_KeepAliveTest(ILogger<Auth_KeepAliveTest> logger, ISessionState sessionState)
		{
			_logger = logger;
			_sessionState = sessionState;
		}

		public async Task Execute()
		{
			var httpClient = new HttpClient
			{
				BaseAddress = new Uri(_authBaseUri)
			};
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.Timeout = TimeSpan.FromSeconds(5);

			var token = _sessionState.Get<string>("auth:token");
			var response = await httpClient.PostAsync("/api/auth/keep-alive", new JsonContent(token));
			response.EnsureSuccessStatusCode();
			var responseContent = await response.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<KeepAliveResponse>(responseContent);

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
