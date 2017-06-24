using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Slabs.Experimental.ConsoleClient.Testify;
using System;

namespace Slabs.Experimental.ConsoleClient
{
	class Program
	{
		static void Main(string[] args)
		{
			var serilog = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.MinimumLevel.Debug()
				.WriteTo.ColoredConsole()
				.CreateLogger();

			var serviceProvider = new ServiceCollection()
				.AddLogging()
				.AddScoped<TestSuiteStartup>()
				.AddSingleton<TestSuiteBuilderFactory>()
				.AddScoped<ISessionState, SessionState>()
				.AddFluentlyHttpClient()
				.BuildServiceProvider();

			var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
			loggerFactory.AddSerilog(serilog)
				.AddDebug();

			var logger = loggerFactory.CreateLogger<Program>();

			var testSuiteStartup = serviceProvider.GetService<TestSuiteStartup>();
			testSuiteStartup.Run().Wait();

			logger.LogInformation("Press any key to stop...");
			Console.ReadKey();
		}

	}

}