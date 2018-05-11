using Grace.DependencyInjection;
using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slabs.AspnetCore.Heroes;
using Slabs.AspnetCore.Infrastructure;

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
			//services.AddSingleton<IHeroService, HeroService>();
			services.AddScoped<RequestContext>();

			services.AddMvc(options =>
			{
				//options.OutputFormatters.Clear();
				options.OutputFormatters.Add(new MessagePackOutputFormatter(ContractlessStandardResolver.Instance));
				//options.InputFormatters.Clear();
				options.InputFormatters.Add(new MessagePackInputFormatter(ContractlessStandardResolver.Instance));
			});
		}

		public void ConfigureContainer(IInjectionScope scope)
		{
			scope.Configure(c =>
			{
				//c.Export<RequestContext>().Lifestyle.SingletonPerRequest();
				//c.Export<RequestContext>().Lifestyle.SingletonPerScope();
				//c.Export<RequestContext>().Lifestyle.SingletonPerObjectGraph();
				c.Export<HeroService>().As<IHeroService>();
			});
			scope.WhatDoIHave();
			//scope.SetupMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMultiTenant();
			app.UseRequestContext();
			app.UseMvc();

		}
	}
}
