using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace Slabs.SimpleDockerApi.Infrastructure
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class HealthMiddleware
	{
		// ReSharper disable once NotAccessedField.Local
		private readonly RequestDelegate _next;

		public HealthMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext httpContext)
		{
			httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
			await httpContext.Response.WriteAsync("healthy");
		}
	}

	public static class HealthMiddlewareExtensions
	{
		// ReSharper disable once UnusedMethodReturnValue.Global
		public static IApplicationBuilder UseHealthMiddleware(this IApplicationBuilder builder, string path = "/health")
		{
			return builder.Map(path, subApp => subApp.UseMiddleware<HealthMiddleware>());
		}
	}
}