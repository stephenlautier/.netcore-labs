using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	public class PipelineOptions
	{
		private readonly IDictionary<string, object> _options = new Dictionary<string, object>();

		public PipelineOptions Set(string key, object value)
		{
			_options[key] = value;
			return this;
		}

		public object Get(string key)
		{
			_options.TryGetValue(key, out var value);
			return value;
		}

		public T Get<T>(string key)
		{
			var value = Get(key);
			return (T)value;
		}
	}

	/// <summary>
	/// Pipeline runner/executor.
	/// </summary>
	public class Pipeline
	{
		private readonly IPipe _pipeline;
		private readonly PipelineOptions _defaultOptions = new PipelineOptions();

		public Pipeline(IPipe pipeline)
		{
			_pipeline = pipeline;
		}

		public Task<T> Run<T>(Func<Task<T>> action, Action<PipelineOptions> configure)
		{
			var options = new PipelineOptions();
			configure?.Invoke(options);
			return Run(action, options);
		}

		public async Task<T> Run<T>(Func<Task<T>> action, PipelineOptions options = null)
		{
			options = options ?? _defaultOptions;
			async Task<object> ToObjectFunc()
			{
				var r = await action();
				return r;
			}

			var result = await _pipeline.Invoke(new PipelineContext { Func = ToObjectFunc, Options = options });
			return (T)result;
		}

		public Task Run(Func<Task> action, Action<PipelineOptions> configure)
		{
			var options = new PipelineOptions();
			configure?.Invoke(options);
			return Run(action, options);
		}

		public async Task Run(Func<Task> action, PipelineOptions options = null)
		{
			options = options ?? _defaultOptions;
			async Task<object> ToObjectFunc()
			{
				await action();
				return null;
			}
			await _pipeline.Invoke(new PipelineContext { Func = ToObjectFunc, Options = options });
		}
	}
}