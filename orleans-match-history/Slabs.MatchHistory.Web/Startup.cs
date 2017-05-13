using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Slabs.MatchHistory.Web
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();

			Log.Logger = new LoggerConfiguration()
				//.MinimumLevel.Debug()
				//.ReadFrom.Configuration(Configuration)
				.Enrich.FromLogContext()
				.WriteTo.ColoredConsole()
				.CreateLogger();

			Log.Logger.Information("Starting up web... Env={environment} App={application}", env.EnvironmentName, env.ApplicationName);
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
		{
			//loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddSerilog()
				.AddDebug();

			app.UseMvc();

			// Ensure any buffered events are sent at shutdown
			appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
		}
	}
}
