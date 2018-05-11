using System.Collections.Generic;

namespace Slabs.AspnetCore.Tenancy
{
	public interface ITenantRegistry
	{
		TTenant Resolve<TTenant>(string key)
			where TTenant : class;
	}
	
	
	public class TenantRegistry : ITenantRegistry
	{
		private static readonly Dictionary<string, AppTenant> _tenantsMap = new Dictionary<string, AppTenant>
		{
			["local.cerberus.io"] = Tenants.Cerberus,
			["local.sketch7.io"] = Tenants.Sketch7,
		};

		public TTenant Resolve<TTenant>(string key)
			where TTenant : class
		{
			_tenantsMap.TryGetValue(key, out var tenant);
			return tenant as TTenant;
		}

	}
}