using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Grace.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Slabs.AspnetCore.Infrastructure;
using Slabs.AspnetCore.Tenant;

namespace Slabs.AspnetCore.Tenant
{
	public interface ITenant
	{
		string Name { get; set; }
	}

	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Tenant : ITenant
	{
		private string DebuggerDisplay => $"Name: '{Name}'";

		public string Name { get; set; }

	}

	public static class Tenants
	{
		public static Tenant Cerberus = new Tenant
		{
			Name = "Cerberus"
		};

		public static Tenant Sketch7 = new Tenant
		{
			Name = "Sketch7"
		};

		private static readonly Dictionary<string, Tenant> _tenantsMap = new Dictionary<string, Tenant>
		{
			["local.cerberus.io"] = Cerberus,
			["local.sketch7.io"] = Sketch7,
		};

		public static Tenant ResolveByDomain(string domain)
		{
			_tenantsMap.TryGetValue(domain, out var tenant);
			return tenant;
		}
	}

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
			
			var r1 = locatorScope.Locate<RequestContext>();
			var r2 = locatorScope.Locate<RequestContext>();
			var scope = locatorScope.GetInjectionScope();
			scope.Configure(c => c.ExportInstance(tenant).As<ITenant>().Lifestyle.SingletonPerRequest());

			//using (var scope = locatorScope.GetInjectionScope().CreateChildScope(c =>
			//{

			//	c.ExportInstance(tenant).As<ITenant>();
			//}))
			//{
			var t = scope.Locate<ITenant>();

			httpContext.RequestServices = scope.Locate<IServiceProvider>();
			var r3 = locatorScope.Locate<RequestContext>();
			await _next(httpContext);
			//}
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