using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.FluentHttp
{
	public class LoggerHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;

		private readonly ILogger _logger;

		public LoggerHttpMiddleware(FluentHttpRequestDelegate next, ILogger<LoggerHttpMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task<IFluentHttpResponse> Invoke(FluentHttpRequest request)
		{
			_logger.LogInformation("Pre-request... [{method}] {url}", request.Method, request.Url);
			var response = await _next(request);
			_logger.LogInformation("Post-request... {status}", response.StatusCode);
			return response;
		}
	}
	public class TimerHttpMiddleware : IFluentHttpMiddleware
	{
		private readonly FluentHttpRequestDelegate _next;

		private readonly ILogger _logger;

		public TimerHttpMiddleware(FluentHttpRequestDelegate next, ILogger<TimerHttpMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task<IFluentHttpResponse> Invoke(FluentHttpRequest request)
		{
			var watch = Stopwatch.StartNew();
			var response = await _next(request);
			_logger.LogInformation("{duration}", watch.Elapsed);
			return response;
		}
	}
}
