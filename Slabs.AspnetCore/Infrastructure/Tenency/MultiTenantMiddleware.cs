using System;
using System.Threading.Tasks;
using Grace.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Slabs.AspnetCore.Infrastructure.Tenency
{
	public class MultiTenantMiddleware<TTenant> 
		where TTenant : class 
	{
		private readonly RequestDelegate _next;
		private readonly ITenantResolver<TTenant> _tenantResolver;

		public MultiTenantMiddleware(
			RequestDelegate next,
			ITenantResolver<TTenant> tenantResolver
		)
		{
			_next = next;
			_tenantResolver = tenantResolver;
		}

		public async Task Invoke(
			HttpContext httpContext,
			IExportLocatorScope locatorScope,
			Lazy<ITenantContainerBuilder<TTenant>> builder
		)
		{
			var tenant = _tenantResolver.Resolve(httpContext);

			using (var scope = await builder.Value.BuildAsync(tenant))
			{
				httpContext.RequestServices = scope.Locate<IServiceProvider>();
				await _next(httpContext);
			}
		}
	}

	public static class MultiTenantMiddlewareExtensions
	{
		public static IApplicationBuilder UseMultiTenant<TTenant>(this IApplicationBuilder builder) where TTenant : class
			=> builder.UseMiddleware<MultiTenantMiddleware<TTenant>>();
	}
}