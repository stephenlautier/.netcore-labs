using Microsoft.Extensions.Logging;
using NFluent;
using Slabs.Experimental.ConsoleClient.FluentHttp;
using Slabs.Experimental.ConsoleClient.Testify;
using System;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Tests
{

	public class Common_TermsAndConditionsTest : ITest
	{
		private readonly ILogger _logger;

		private readonly FluentHttpClient _fluentHttpClient;

		public Common_TermsAndConditionsTest(ILogger<Common_TermsAndConditionsTest> logger, FluentHttpClientFactory fluentHttpClientFactory)
		{
			_logger = logger;
			_fluentHttpClient = fluentHttpClientFactory.Get("common");
		}

		public async Task Execute()
		{
			var result = await _fluentHttpClient.Get<TermsAndConditionsResponse>("/api/profile/terms-and-conditions/latest");

			var response = await _fluentHttpClient.CreateRequest("/api/profile/terms-and-conditions/latest")
				.AsGet()
				.ReturnAsResponse<TermsAndConditionsResponse>();
			response.EnsureSuccessStatusCode();

			var timeTaken = response.GetTimeTaken();
			Check.That(timeTaken).IsLessThan(TimeSpan.FromMilliseconds(250));
			Check.That(result).IsNotNull();
			Check.That(result.Id).IsStrictlyGreaterThan(0);

			_logger.LogInformation("[{service}] complete", nameof(Common_TermsAndConditionsTest));
		}
	}


	public class TermsAndConditionsResponse
	{
		public int Id { get; set; }
		public DateTime PublicationDate { get; set; }
		public string Content { get; set; }
	}


	public static class CommonTests
	{
		public static TestGroupBuilder GetGroup()
		{
			return new TestGroupBuilder()
				.Add<Common_TermsAndConditionsTest>("terms-and-conditions")
				;
		}

		public static TestSuiteBuilder AddCommonTests(this TestSuiteBuilder testSuiteBuilder, int repeat = 1)
		{
			testSuiteBuilder.Add(GetGroup(), repeat);
			return testSuiteBuilder;
		}

	}
}
