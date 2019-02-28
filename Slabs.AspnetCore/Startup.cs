using Grace.DependencyInjection;
using MessagePack.AspNetCoreMvcFormatter;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slabs.AspnetCore.Heroes;
using Slabs.AspnetCore.Infrastructure;
using Slabs.AspnetCore.Infrastructure.Tenency;
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
			services.AddSingleton(typeof(ITenantResolver<>), typeof(AppTenantResolver<>));
			services.AddScoped<RequestContext>();

			services.AddMvc(options =>
			{
				//options.OutputFormatters.Clear();
				options.OutputFormatters.Add(new MessagePackOutputFormatter(ContractlessStandardResolver.Instance));
				//options.InputFormatters.Clear();
				options.InputFormatters.Add(new MessagePackInputFormatter(ContractlessStandardResolver.Instance));
			}).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);;
		}

		public void ConfigureContainer(IInjectionScope scope)
		{
			scope.Configure(c =>
			{
				c.Export<HeroService>().As<IHeroService>().Lifestyle.Singleton();
			});

			scope.ConfigureTenants<AppTenant>((tenant, c) =>
			{
				if (tenant.Key == "sketch7")
					c.Export<SampleHeroService>().As<IHeroService>().Lifestyle.Singleton();
			});

			//scope.SetupMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			//app.UseHttpsRedirection();
			app.UseMultiTenant<AppTenant>();
			app.UseRequestContext();
			app.UseMvc();
		}
	}
}
