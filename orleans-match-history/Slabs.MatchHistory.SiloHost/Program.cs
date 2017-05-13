using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Slabs.MatchHistory.Silo
{
	class Program
	{
		static int Main(string[] args)
		{
			string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddCommandLine(args)
				.AddJsonFile("app-config.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"app-config.{environment}.json", optional: true)
				.AddEnvironmentVariables();

			var config = builder.Build();

			var serilog = ConfigureLogging();

			var serviceProvider = new ServiceCollection()
				.AddLogging()
				.AddSingleton<ClusterServer>()
				.BuildServiceProvider();

			var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
			loggerFactory.AddSerilog(serilog)
				.AddDebug();

			var logger = loggerFactory.CreateLogger<Program>();

			logger.LogInformation("Starting silo... Env={environment} Machine={machine}", environment, Environment.MachineName);
			var server = serviceProvider.GetService<ClusterServer>();

			var isStarted = server.Configure()
				.Start();

			int exitCode;
			if (isStarted)
			{
				exitCode = 0;
				logger.LogDebug("Press [Ctrl]-C to stop...");
				server.Wait();
			}
			else
				exitCode = 1;

			logger.LogInformation("Shutting down silo...");
			exitCode += server.Shutdown();

			return exitCode;
		}

		private static Serilog.ILogger ConfigureLogging() => new LoggerConfiguration()
				.Enrich.FromLogContext()
				.MinimumLevel.Debug()
				.WriteTo.ColoredConsole()
				.CreateLogger();
	}
}