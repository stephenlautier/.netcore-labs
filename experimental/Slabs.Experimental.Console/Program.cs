using System;
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
				.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
				.ReadFrom.Configuration(config)
					.Enrich.FromLogContext()
					.Enrich.WithMachineName()
					.Enrich.WithDemystifiedStackTraces()
				.CreateLogger()
				;

			var serviceProvider = new ServiceCollection()
				.AddLogging(x => x.AddSerilog(loggerConfig))
				.AddScoped<TestSuiteStartup>()
				.AddScoped<PipeTestStartup>()
				.AddScoped<GitStartup>()
				.AddSingleton<TestSuiteBuilderFactory>()
				.AddScoped<ISessionState, SessionState>()
				.AddFluentlyHttpClient()
				.AddPipes()
				.BuildServiceProvider();

			var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
			var logger = loggerFactory.CreateLogger<Program>();

			//var startup = serviceProvider.GetService<TestSuiteStartup>();
			//var startup = serviceProvider.GetService<GitStartup>();
			var startup = serviceProvider.GetService<PipeTestStartup>();
			try
			{
				startup.Run().Wait();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error while running.");
				throw;
			}

			logger.LogInformation("Press any key to stop...");
			//Console.ReadKey();
		}

	}

}