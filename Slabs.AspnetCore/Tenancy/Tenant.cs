using System.Collections.Generic;
using System.Diagnostics;

namespace Slabs.AspnetCore.Tenancy
{
	public interface ITenant
	{
		string Key { get; set; }
		string Name { get; set; }
	}

	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Tenant : ITenant
	{
		private string DebuggerDisplay => $"Key: '{Key}', Name: '{Name}'";

		public string Key { get; set; }
		public string Name { get; set; }

	}

	public static class Tenants
	{
		public static Tenant Cerberus = new Tenant
		{
			Key = "cerberus",
			Name = "Cerberus"
		};

		public static Tenant Sketch7 = new Tenant
		{
			Key = "sketch7",
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
}
