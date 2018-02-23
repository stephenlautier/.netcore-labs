using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slabs.AspnetCore.Controllers;
using Slabs.AspnetCore.Heroes;

namespace Slabs.AspnetCore
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<HeroService>();

			services.AddMvc(options =>
			{
				//options.OutputFormatters.Clear();
				options.OutputFormatters.Add(new MessagePackOutputFormatter(ContractlessStandardResolver.Instance));
				//options.InputFormatters.Clear();
				options.InputFormatters.Add(new MessagePackInputFormatter(ContractlessStandardResolver.Instance));
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseMvc();

		}
	}
}
