using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
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

			if (options.SkipCache)
				return await _next(context);

			if (string.IsNullOrEmpty(options.Key))
				throw new InvalidOperationException("CacheOptions key was not set.");

			// todo: build cache key from args
			string cacheKey = options.Key;
			_logger.LogInformation("Cache key: {key}", cacheKey);

			var cacheValue = _cacheService.Get(cacheKey);
			if (cacheValue != null)
			{
				_logger.LogInformation("Return from cache for {key}", cacheKey);
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