using System;
using System.Threading.Tasks;
using Grace.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Slabs.AspnetCore.Tenancy;

namespace Slabs.AspnetCore.Tenancy
{
	public class MultiTenantMiddleware<TTenant> 
		where TTenant : class 
	{
		private readonly RequestDelegate _next;
		private readonly ITenantRegistry _tenantRegistry;

		public MultiTenantMiddleware(
			RequestDelegate next,
			ITenantRegistry tenantRegistry
		)
		{
			_next = next;
			_tenantRegistry = tenantRegistry;
		}

		public async Task Invoke(
			HttpContext httpContext,
			IExportLocatorScope locatorScope,
			Lazy<ITenantContainerBuilder<TTenant>> builder
		)
		{
			var host = httpContext.Request.Host.Host;
			var tenant = _tenantRegistry.Resolve<TTenant>(host); // todo: create Resolver which takes HTTP context

			using (var scope = await builder.Value.BuildAsync(tenant))
			{
				httpContext.RequestServices = scope.Locate<IServiceProvider>();
				await _next(httpContext);
			}
		}
	}
}

namespace Microsoft.AspNetCore.Builder
{
	public static class MultiTenantMiddlewareExtensions
	{
		public static IApplicationBuilder UseMultiTenant<TTenant>(this IApplicationBuilder builder) where TTenant : class
			=> builder.UseMiddleware<MultiTenantMiddleware<TTenant>>();
	}
}