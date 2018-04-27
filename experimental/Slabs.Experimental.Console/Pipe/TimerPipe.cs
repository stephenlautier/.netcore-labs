using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	/// <summary>
	/// Timer pipe middleware, which tracks how long an exception executes.
	/// </summary>
	public class TimerPipe : IPipe
	{
		private readonly PipeDelegate _next;
		private readonly ILogger _logger;

		public TimerPipe(PipeDelegate next, ILogger<TimerPipe> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task<object> Invoke(PipelineContext context)
		{
			if (!_logger.IsEnabled(LogLevel.Debug))
				return await _next(context);

			var watch = Stopwatch.StartNew();
			var result = await _next(context);
			var elapsed = watch.Elapsed;

			_logger.LogDebug("Executed action in {timeTakenMillis:n0}ms", elapsed.TotalMilliseconds);
			return result;
		}
	}
}