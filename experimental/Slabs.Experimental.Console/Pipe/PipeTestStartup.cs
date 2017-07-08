using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	public class PipeTestStartup
	{
		private readonly ILogger _logger;
		private readonly PipelineBuilderFactory _pipelineBuilderFactory;

		public PipeTestStartup(ILogger<PipeTestStartup> logger, PipelineBuilderFactory pipelineBuilderFactory)
		{
			_logger = logger;
			_pipelineBuilderFactory = pipelineBuilderFactory;
		}

		public async Task Run()
		{
			_logger.LogInformation("Init Pipe Test...");
			var pipeBuilder = _pipelineBuilderFactory.Create()
				.Add<TimerPipe>()
				.Add<CachePipe>()
				;

			var pipeline = pipeBuilder.Build();
			var r1 = await pipeline.Run(GetFruit);
			var r2 = await pipeline.Run(GetFruit);
			await pipeline.Run(SetFruit);

			_logger.LogInformation($"[Pipe] Result={r1} R2={r2}");
		}

		public async Task<string> GetFruit()
		{
			_logger.LogInformation($"[Service] Get fruit...");
			await Task.Delay(250);
			return "strawberry";
		}

		public async Task SetFruit()
		{
			_logger.LogInformation($"[Service] Set fruit...");
			await Task.Delay(100);
			_logger.LogInformation($"[Service] Set fruit complete");
		}
	}

	public class Pipeline
	{
		private readonly IPipe _pipeline;

		public Pipeline(IPipe pipline)
		{
			_pipeline = pipline;
		}

		public async Task<T> Run<T>(Func<Task<T>> action)
		{
			async Task<object> ToObjectFunc()
			{
				var r = await action();
				return r;
			}

			var result = await _pipeline.Invoke(new PipelineContext { Func = ToObjectFunc });
			return (T)result;
		}

		public async Task Run(Func<Task> action)
		{
			async Task<object> ToObjectFunc()
			{
				await action();
				return null;
			}

			await _pipeline.Invoke(new PipelineContext { Func = ToObjectFunc });
		}
	}

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
			var watch = Stopwatch.StartNew();
			var result = await _next(context);
			var elapsed = watch.Elapsed;

			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("Executed action in {timeTakenMillis}ms", elapsed.TotalMilliseconds);
			return result;
		}
	}
	public class CachePipe : IPipe
	{
		private readonly PipeDelegate _next;
		private readonly ILogger _logger;
		private readonly CacheService _cacheService = new CacheService();

		public CachePipe(PipeDelegate next, ILogger<CachePipe> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task<object> Invoke(PipelineContext context)
		{
			// todo: dont cache without result? e.g. Set
			// todo: build cache key from args
			string cacheKey = "";
			_logger.LogInformation("Cache key: {key}", cacheKey);

			var cacheValue = _cacheService.Get(cacheKey);
			if (cacheValue != null)
			{
				_logger.LogInformation("Return from cache", cacheKey);
				return cacheValue;
			}
			
			var result = await _next(context);
			_cacheService.Set(cacheKey, result);
			
			return result;
		}
	}

	/// <summary>
	/// Simple Caching for POC.
	/// </summary>
	public class CacheService
	{
		private readonly IDictionary<string, object> _items = new Dictionary<string, object>();

		public T Get<T>(string key)
		{
			if (_items.TryGetValue(key, out var value))
				return (T)value;
			return default(T);
		}

		public object Get(string key) => _items.TryGetValue(key, out var value) ? value : null;


		public void Set(string key, object value) => _items.Add(key, value);
	}

}