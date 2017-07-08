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
			if (_options.ContainsKey(key))
				_options[key] = value;
			else
				_options.Add(key, value);
			return this;
		}
		public object Get(string key) => _options.ContainsKey(key) ? _options[key] : null;

		public T Get<T>(string key)
		{
			var value = Get(key);
			return (T)value;
		}
	}


	public class Pipeline
	{
		private readonly IPipe _pipeline;
		private readonly PipelineOptions _defaultOptions = new PipelineOptions();

		public Pipeline(IPipe pipline)
		{
			_pipeline = pipline;
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