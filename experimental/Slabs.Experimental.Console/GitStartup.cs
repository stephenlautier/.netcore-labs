using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace Slabs.Experimental.ConsoleClient
{
	public class GitStartup
	{
		private readonly ILogger<GitStartup> _logger;

		public GitStartup(ILogger<GitStartup> logger)
		{
			_logger = logger;
		}

		public async Task Run()
		{
			_logger.LogInformation("Init...");

			Repository.Clone("https://github.com/CerberusTech/Odin.Config", Path.Combine(Directory.GetCurrentDirectory(), "clonehere"), new CloneOptions
			{
				CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = "username", Password = "xxx" }
			});

			await Task.CompletedTask;
		}
	}
}
