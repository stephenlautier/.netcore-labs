using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Slabs.Experimental.ConsoleClient.Pipe;
using Slabs.Experimental.ConsoleClient.Testify;

namespace Slabs.Experimental.ConsoleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			var configBuilder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddCommandLine(args)
				.AddJsonFile("config.json");

			var config = configBuilder.Build();

			var loggerConfig = new LoggerConfiguration()
				.WriteTo.Console()
				.ReadFrom.Configuration(config)
				.CreateLogger()
				;

			var serviceProvider = new ServiceCollection()
				.AddLogging(x => x.AddSerilog(loggerConfig))
				.AddScoped<TestSuiteStartup>()
				.AddScoped<PipeTestStartup>()
				.AddSingleton<TestSuiteBuilderFactory>()
				.AddScoped<ISessionState, SessionState>()
				.AddFluentlyHttpClient()
				.AddPipes()
				.BuildServiceProvider();

			var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger<Program>();

			//var startup = serviceProvider.GetService<TestSuiteStartup>();
			var startup = serviceProvider.GetService<PipeTestStartup>();
			startup.Run().Wait();

			logger.LogInformation("Press any key to stop...");
			//Console.ReadKey();
		}

	}

}