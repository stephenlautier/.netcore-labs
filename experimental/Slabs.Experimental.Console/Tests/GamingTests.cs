using System;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Tests
{
	public class GetTeamsTest : ITest
	{
		public Task Execute()
		{
			Console.WriteLine($"[{nameof(GetTeamsTest)}] Executing...");
			return Task.Delay(TimeSpan.FromSeconds(2));
		}
	}

	public class AddTeamsTest : ITest
	{
		public Task Execute()
		{
			Console.WriteLine($"[{nameof(AddTeamsTest)}] Executing...");
			return Task.CompletedTask;
		}
	}

	public class GetTeamDetailTest : ITest
	{
		public Task Execute()
		{
			Console.WriteLine($"[{nameof(GetTeamDetailTest)}] Executing...");
			return Task.CompletedTask;
		}
	}

	// parallel
	public class GetHeroesTest : ITest
	{
		public async Task Execute()
		{
			Console.WriteLine($"[{nameof(GetHeroesTest)}] Executing...");
			await Task.Delay(TimeSpan.FromSeconds(2));
			Console.WriteLine($"[{nameof(GetHeroesTest)}] Complete!");
		}
	}

	// parallel
	public class GetMatchesTest : ITest
	{
		public async Task Execute()
		{
			Console.WriteLine($"[{nameof(GetMatchesTest)}] Executing...");
			await Task.Delay(TimeSpan.FromSeconds(2));
			Console.WriteLine($"[{nameof(GetMatchesTest)}] Complete!");
		}
	}

	public class ResetTest : ITest
	{
		public Task Execute()
		{
			Console.WriteLine($"[{nameof(ResetTest)}] Executing...");
			return Task.CompletedTask;
		}
	}
}
