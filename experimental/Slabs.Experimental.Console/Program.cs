using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Slabs.Experimental.ConsoleClient.Pipe;
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
				.AddScoped<PipeTestStartup>()
				.AddSingleton<TestSuiteBuilderFactory>()
				.AddScoped<ISessionState, SessionState>()
				.AddFluentlyHttpClient()
				.AddPipes()
				.BuildServiceProvider();

			var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
			loggerFactory.AddSerilog(serilog)
				.AddDebug();

			var logger = loggerFactory.CreateLogger<Program>();

			//var startup = serviceProvider.GetService<TestSuiteStartup>();
			var startup = serviceProvider.GetService<PipeTestStartup>();
			startup.Run().Wait();

			logger.LogInformation("Press any key to stop...");
			Console.ReadKey();
		}

	}

}