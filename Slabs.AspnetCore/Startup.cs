using System;
using System.ComponentModel;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Extensions;
using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slabs.AspnetCore.Heroes;
using Slabs.AspnetCore.Infrastructure;
using Slabs.AspnetCore.Tenancy;

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
			services.AddSingleton<ITenantRegistry, TenantRegistry>();
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
				c.Export<HeroService>().As<IHeroService>();
			});

			scope.ConfigureTenants<AppTenant>((tenant, c) =>
			{
				if (tenant.Key == "sketch7")
					c.Export<SampleHeroService>().As<IHeroService>();
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

			app.UseMultiTenant<AppTenant>();
			app.UseRequestContext();
			app.UseMvc();

		}
	}
}
