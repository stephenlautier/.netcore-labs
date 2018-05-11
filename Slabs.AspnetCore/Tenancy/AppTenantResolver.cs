using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Slabs.AspnetCore.Infrastructure.Tenency;

namespace Slabs.AspnetCore.Tenancy
{
	public class AppTenantResolver<TTenant> : ITenantResolver<TTenant> where TTenant : class
	{
		private static readonly Dictionary<string, AppTenant> _domainMap = new Dictionary<string, AppTenant>
		{
			["local.cerberus.io"] = Tenants.Cerberus,
			["local.sketch7.io"] = Tenants.Sketch7,
		};

		public TTenant Resolve(string key)
		{
			_domainMap.TryGetValue(key, out var tenant);
			return tenant as TTenant;
		}

		public TTenant Resolve(HttpContext httpContext)
		{
			var host = httpContext.Request.Host.Host;
			return Resolve(host);
		}

	}
}