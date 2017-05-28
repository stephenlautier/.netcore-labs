using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Slabs.Experimental.ConsoleClient.Tests
{
	public class GetTeamsTest : ITest
	{
		private readonly ILogger _logger;

		public GetTeamsTest(ILogger<GetTeamsTest> logger)
		{
			_logger = logger;
		}

		public async Task Execute()
		{
			_logger.LogInformation("[{service}] Executing...", nameof(GetTeamsTest));
			await Task.Delay(TimeSpan.FromSeconds(2));
			_logger.LogInformation("[{service}] complete", nameof(GetTeamsTest));
		}
	}

	public class AddTeamsTest : ITest
	{
		private readonly ILogger _logger;

		public AddTeamsTest(ILogger<AddTeamsTest> logger)
		{
			_logger = logger;
		}

		public async Task Execute()
		{
			_logger.LogInformation("[{service}] Executing...", nameof(AddTeamsTest));
			await Task.Delay(TimeSpan.FromSeconds(1));
		}
	}

	public class GetTeamDetailTest : ITest
	{
		private readonly ILogger _logger;

		public GetTeamDetailTest(ILogger<AddTeamsTest> logger)
		{
			_logger = logger;
		}

		public Task Execute()
		{
			_logger.LogInformation("[{service}] Executing...", nameof(GetTeamDetailTest));
			return Task.CompletedTask;
		}
	}

	// parallel
	public class GetHeroesTest : ITest
	{
		private readonly ILogger _logger;

		public GetHeroesTest(ILogger<GetHeroesTest> logger)
		{
			_logger = logger;
		}

		public async Task Execute()
		{
			_logger.LogInformation("[{service}] Executing...", nameof(GetHeroesTest));
			await Task.Delay(TimeSpan.FromSeconds(2));
			throw new Exception("Because it crashed ee");
			_logger.LogInformation("[{service}] complete!", nameof(GetHeroesTest));
		}
	}

	// parallel
	public class GetMatchesTest : ITest
	{
		private readonly ILogger _logger;

		public GetMatchesTest(ILogger<GetMatchesTest> logger)
		{
			_logger = logger;
		}

		public async Task Execute()
		{
			_logger.LogInformation("[{service}] Executing...", nameof(GetMatchesTest));
			await Task.Delay(TimeSpan.FromSeconds(2));
			_logger.LogInformation("[{service}] complete!", nameof(GetMatchesTest));
		}
	}

	public class ResetTest : ITest
	{
		private readonly ILogger _logger;

		public ResetTest(ILogger<ResetTest> logger)
		{
			_logger = logger;
		}

		public Task Execute()
		{
			_logger.LogInformation("[{service}] Executing...", nameof(ResetTest));
			return Task.CompletedTask;
		}
	}
}
