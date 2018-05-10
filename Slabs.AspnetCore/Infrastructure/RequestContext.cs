using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Slabs.AspnetCore.Infrastructure;

namespace Slabs.AspnetCore.Infrastructure
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class RequestContext
	{
		private string DebuggerDisplay => $"TraceId: '{TraceId}', UserAgent: '{UserAgent}'";

		public string TraceId { get; set; }
		public string UserAgent { get; set; }
	}

	public class RequestContextMiddleware
	{
		private readonly RequestDelegate _next;

		public RequestContextMiddleware(
			RequestDelegate next
		)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext httpContext, RequestContext requestContext)
		{
			requestContext.TraceId = httpContext.TraceIdentifier;
			if (httpContext.Request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgent))
				requestContext.UserAgent = userAgent;

			await _next(httpContext);
		}
	}
}

namespace Microsoft.AspNetCore.Builder
{
	public static class RequestContextMiddlewareExtensions
	{
		public static IApplicationBuilder UseRequestContext(this IApplicationBuilder builder)
		=> builder.UseMiddleware<RequestContextMiddleware>();
	}
}