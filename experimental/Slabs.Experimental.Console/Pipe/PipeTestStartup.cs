using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	// able to get args/result (cache pipe)

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
				// .Add<CachePipe>()
				;

			var pipeline = pipeBuilder.Build();
			var r1 = await pipeline.Run(GetFruit);
			var r2 = await pipeline.Run(GetFruit);
			await pipeline.Run(SetFruit);

			_logger.LogInformation($"[Pipe] Result={r1} R2={r2}");
		}

		public async Task<string> GetFruit()
		{
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

	public class PipelineBuilderFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public PipelineBuilderFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public PipelineBuilder Create() => ActivatorUtilities.CreateInstance<PipelineBuilder>(_serviceProvider);
	}

	public class PipelineBuilder
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly List<PipeConfig> _pipes = new List<PipeConfig>();

		public PipelineBuilder(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public PipelineBuilder Add<T>(params object[] args)
		{
			_pipes.Add(new PipeConfig(typeof(T), args));
			return this;
		}

		public Pipeline Build()
		{
			if (_pipes.Count == 0)
				throw new InvalidOperationException("Cannot build pipeline with zero pipes.");

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
					return new Pipeline(instance);
				previous = instance;
			}
			throw new InvalidOperationException("Something went wrong!");

			Task<object> Stub(PipelineContext context) => Task.FromResult<object>(null);
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

	public class PipelineContext
	{
		public Func<Task<object>> Func { get; set; }
	}

	public interface IPipe
	{
		Task<object> Invoke(PipelineContext context);
	}

	public delegate Task<object> PipeDelegate(PipelineContext context);

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

	public class ActionExecutePipe : IPipe
	{
		public ActionExecutePipe(PipeDelegate next)
		{
		}

		public async Task<object> Invoke(PipelineContext context) => await context.Func();
	}
}