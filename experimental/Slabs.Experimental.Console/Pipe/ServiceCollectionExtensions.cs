using Slabs.Experimental.ConsoleClient.Pipe;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for setting up fluent HTTP services in an <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
	/// </summary>
	public static class PipeServiceCollectionExtensions
	{
		/// <summary>
		/// Adds fluently HTTP client services to the specified <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services"></param>
		/// <returns>Returns service collection for chaining.</returns>
		public static IServiceCollection AddPipes(this IServiceCollection services)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));

			services.AddSingleton<PipelineBuilderFactory>();

			return services;
		}
	}
}