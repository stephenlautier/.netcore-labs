using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	// able to get args/result (cache pipe)
	// change Func<Task> (invoke param) to PipelineContext

	public class PipeTestStartup
	{
		private readonly ILogger _logger;
		private readonly PipeBuilderFactory _pipeBuilderFactory;

		public PipeTestStartup(ILogger<PipeTestStartup> logger, PipeBuilderFactory pipeBuilderFactory)
		{
			_logger = logger;
			_pipeBuilderFactory = pipeBuilderFactory;
		}

		public async Task Run()
		{
			_logger.LogInformation("Init Pipe Test...");
			var pipeBuilder = _pipeBuilderFactory.Create()
				.Add<TimerPipe>()
				// .Add<CachePipe>()
				;

			var result = await GetFruit();

			var pipeline = pipeBuilder.Build();
			await pipeline.Run(GetFruit);
			await pipeline.Run(GetFruit);
			//var result = await pipeline.Run(GetFruit);

			_logger.LogInformation($"[Pipe] Result={result}");
		}

		public async Task<object> GetFruit()
		{
			await Task.Delay(250);
			return "strawberry";
		}
	}

	public class PipeBuilderFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public PipeBuilderFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public PipeBuilder Create() => ActivatorUtilities.CreateInstance<PipeBuilder>(_serviceProvider);
	}

	public class PipeBuilder
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly List<PipeConfig> _pipes = new List<PipeConfig>();

		public PipeBuilder(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public PipeBuilder Add<T>(params object[] args)
		{
			_pipes.Add(new PipeConfig(typeof(T), args));
			return this;
		}

		public PipeRuntime Build()
		{
			if (_pipes.Count == 0)
				throw new InvalidOperationException("Cannot build with zero pipes.");

			Add<ActionExecutePipe>();
			IPipe previous = null;
			for (int i = _pipes.Count; i-- > 0;)
			{
				var pipe = _pipes[i];
				var isLast = _pipes.Count - 1 == i;
				var isFirst = i == 0;

				PipeDelegate next = isLast
					? (PipeDelegate)Stub
					: previous.Invoke;

				object[] ctor;
				if (pipe.Args == null)
					ctor = new object[] { next };
				else
				{
					ctor = new object[pipe.Args.Length + 1];
					ctor[0] = next;
					Array.Copy(pipe.Args, 0, ctor, 1, pipe.Args.Length);
				}

				IPipe instance = (IPipe)ActivatorUtilities.CreateInstance(_serviceProvider, pipe.Type, ctor);
				if (isFirst)
					return new PipeRuntime(instance);
				previous = instance;
			}
			throw new InvalidOperationException("Something went wrong!");

			Task Stub(Func<Task> action) => Task.CompletedTask;
		}
	}

	public class PipeRuntime
	{
		private readonly IPipe _pipeline;

		public PipeRuntime(IPipe pipline)
		{
			_pipeline = pipline;
		}

		public Task Run(Func<Task> action) => _pipeline.Invoke(action);
	}

	public class PipeConfig
	{
		public Type Type { get; set; }

		public object[] Args { get; set; }

		public PipeConfig()
		{
		}

		public PipeConfig(Type type, object[] args)
		{
			Type = type;
			Args = args;
		}

		/// <summary>
		/// Destructuring.
		/// </summary>
		public void Deconstruct(out Type type, out object[] args) { type = Type; args = Args; }
	}

	public interface IPipe
	{
		Task Invoke(Func<Task> action);
	}
	public delegate Task PipeDelegate(Func<Task> action);

	public class TimerPipe : IPipe
	{
		private readonly PipeDelegate _next;
		private readonly ILogger _logger;

		public TimerPipe(PipeDelegate next, ILogger<TimerPipe> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(Func<Task> action)
		{
			var watch = Stopwatch.StartNew();
			await _next(action);
			var elapsed = watch.Elapsed;

			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("Executed action in {timeTakenMillis}ms", elapsed.TotalMilliseconds);
		}
	}
	public class ActionExecutePipe : IPipe
	{
		public ActionExecutePipe(PipeDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(Func<Task> action)
		{
			await action();
		}
	}
}