using System;
using System.Threading.Tasks;
using Grace.DependencyInjection;
using Slabs.AspnetCore.Tenancy;

namespace Slabs.AspnetCore.Infrastructure.Tenency
{
	public interface ITenantContainerBuilder<in TTenant> 
		where TTenant : class
	{
		Task<IInjectionScope> BuildAsync(TTenant tenant);
	}

	// reference: https://github.com/saaskit/saaskit/
	public class TenantContainerBuilder<TTenant> : ITenantContainerBuilder<TTenant> 
		where TTenant : class
	{
		public TenantContainerBuilder(IInjectionScope container, Action<TTenant, IExportRegistrationBlock> configure)
		{
			Container = container;
			Configure = configure;
		}

		protected IInjectionScope Container { get; }
		protected Action<TTenant, IExportRegistrationBlock> Configure { get; }

		public virtual Task<IInjectionScope> BuildAsync(TTenant tenant)
		{
			if (tenant == null) throw new ArgumentNullException(nameof(tenant));

			var tenantContainer = Container.CreateChildScope(config =>
			{
				config.ExportInstance(tenant).As<ITenant>();
				Configure(tenant, config);
			}, scopeName: "tenant");

			return Task.FromResult(tenantContainer);
		}
	}

	public static class MultitenancyContainerExtensions
	{
		public static void ConfigureTenants<TTenant>(this IInjectionScope container, Action<IExportRegistrationBlock> configure) 
			where TTenant : class
		{

			container.Configure(c =>
				c.ExportInstance(new TenantContainerBuilder<TTenant>(container, (tenant, config) => configure(config)))
					.As<ITenantContainerBuilder<TTenant>>()
			);
		}

		public static void ConfigureTenants<TTenant>(this IInjectionScope container, Action<TTenant, IExportRegistrationBlock> configure) 
			where TTenant : class
		{
			container.Configure(c =>
				c.ExportInstance(new TenantContainerBuilder<TTenant>(container, configure))
					.As<ITenantContainerBuilder<TTenant>>()
			);
		}
	}
}
