using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	public class Hero
	{
		public string Name { get; set; }

		public override string ToString() => $"Name: '{Name}'";
	}
	
	/*
	 * - skip pipe (within run)
	 */
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
			var r1 = await pipeline.Run(GetFruit, new PipelineOptions().SetCache("get-fruit"));
			var r2 = await pipeline.Run(GetFruit, new PipelineOptions().SetCache("get-fruit"));
			var hero = await pipeline.Run(GetHero, new PipelineOptions().SetCache("get-hero"));
			await pipeline.Run(SetFruit, new PipelineOptions().SetNoCache());

			_logger.LogInformation("[Pipe] Result={r1} R2={r2}, Hero={hero}", r1, r2, hero.ToString());
		}

		public async Task<string> GetFruit()
		{
			_logger.LogInformation($"[Service] Get fruit...");
			await Task.Delay(250);
			return "strawberry";
		}

		public async Task<Hero> GetHero()
		{
			_logger.LogInformation($"[Service] Get Hero...");
			await Task.Delay(250);
			return new Hero
			{
				Name = "Rexxar"
			};
		}

		public async Task SetFruit()
		{
			_logger.LogInformation($"[Service] Set fruit...");
			await Task.Delay(100);
			_logger.LogInformation($"[Service] Set fruit complete");
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
			var options = context.Options.GetCache();
			if (options == null)
				throw new InvalidOperationException("CachePipe requires CacheOptions.");

			if(options.SkipCache)
				return await _next(context);

			if (string.IsNullOrEmpty(options.Key))
				throw new InvalidOperationException("CacheOptions key was not set.");

			// todo: build cache key from args
			string cacheKey = options.Key;
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


	public static class PipelineCacheOptionsExtensions
	{
		private const string CacheKey = "CACHE";
		
		public static PipelineOptions SetNoCache(this PipelineOptions pipelineOptions)
			=> pipelineOptions.SetCache(new CacheOptions
			{
				SkipCache = true
			});

		public static PipelineOptions SetCache(this PipelineOptions pipelineOptions, string key)
			=> pipelineOptions.SetCache(new CacheOptions
			{
				Key = key
			});

		public static PipelineOptions SetCache(this PipelineOptions pipelineOptions, CacheOptions options)
			=> pipelineOptions.Set(CacheKey, options);

		public static CacheOptions GetCache(this PipelineOptions pipelineOptions) => pipelineOptions.Get<CacheOptions>(CacheKey);
	}

	public class CacheOptions
	{
		public string Key { get; set; }
		public bool SkipCache { get; set; }
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