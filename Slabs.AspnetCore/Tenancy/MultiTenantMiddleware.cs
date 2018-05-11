using System;
using System.Threading.Tasks;
using Grace.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Slabs.AspnetCore.Tenancy;

namespace Slabs.AspnetCore.Tenancy
{
	public class MultiTenantMiddleware
	{
		private readonly RequestDelegate _next;

		public MultiTenantMiddleware(
			RequestDelegate next
		)
		{
			_next = next;
		}

		public async Task Invoke(
			HttpContext httpContext
		)
		{
			var host = httpContext.Request.Host.Host;
			var tenant = Tenants.ResolveByDomain(host);

			var locatorScope = httpContext.RequestServices.GetService<IExportLocatorScope>();
			var injectionScope = locatorScope.GetInjectionScope();

			using (var scope = injectionScope.CreateChildScope(c =>
			{
				// todo: get tenant container services + register
				c.ExportInstance(tenant).As<ITenant>();
			}, "tenant"))
			{
				//var t = scope.Locate<ITenant>();

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
		public static IApplicationBuilder UseMultiTenant(this IApplicationBuilder builder)
			=> builder.UseMiddleware<MultiTenantMiddleware>();
	}
}