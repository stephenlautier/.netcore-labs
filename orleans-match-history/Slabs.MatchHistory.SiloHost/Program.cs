using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Slabs.MatchHistory.SiloHost
{
	class Program
	{
		static void Main(string[] args)
		{
			string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddCommandLine(args)
				.AddJsonFile("app-config.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"app-config.{environment}.json", optional: true)
				.AddEnvironmentVariables();

			var config = builder.Build();

			var loggerFactory = new LoggerFactory();

			var serilog = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.MinimumLevel.Debug()
				.WriteTo.ColoredConsole()
				.CreateLogger();

			var logger = loggerFactory.AddSerilog(serilog)
					.AddDebug(LogLevel.Trace)
					.CreateLogger<Program>();

			logger.LogInformation("Starting silo... Env={environment} Machine={machine}", environment, Environment.MachineName);
			logger.LogDebug("Press [Ctrl]-C to stop...");
			Console.ReadKey();

			logger.LogInformation("Shutting down silo...");
		}
	}
}