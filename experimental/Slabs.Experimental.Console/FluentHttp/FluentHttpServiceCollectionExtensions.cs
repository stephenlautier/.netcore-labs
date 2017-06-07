using Slabs.Experimental.ConsoleClient.FluentHttp;
using System;


// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods for setting up fluent http services in an <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
	/// </summary>
	public static class FluentHttpServiceCollectionExtensions
		{

			public static IServiceCollection AddFluentHttp(this IServiceCollection services)
			{
				if (services == null)
					throw new ArgumentNullException(nameof(services));

				services.AddSingleton<HttpClientFactory>();
				services.AddSingleton<IFluentHttpMiddlewareRunner, FluentHttpMiddlewareRunner>();

				return services;
			}
		}
	}
