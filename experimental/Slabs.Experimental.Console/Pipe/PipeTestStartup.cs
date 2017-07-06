using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Slabs.Experimental.ConsoleClient.Testify;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	// prebuilt pipe
	// able to get args
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
			var result = await GetFruit();

			var pipeBuilder = _pipeBuilderFactory.Create()
				.Add<TimerPipe>();

			await pipeBuilder.Run(GetFruit);
			await pipeBuilder.Run(GetFruit);

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

		public async Task Run(PipeDelegate action)
		{
			if (_pipes.Count == 0)
			{
				await action();
				return;
			}

			IPipe previous = null;
			for (int i = _pipes.Count; i-- > 0;)
			{
				var pipe = _pipes[i];
				var isLast = _pipes.Count - 1 == i;
				var isFirst = i == 0;

				var next = isLast
					? action
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
					await instance.Invoke(); // todo: arg
				else
					previous = instance;

			}
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

	public interface IPipe
	{
		Task Invoke();
	}
	public delegate Task PipeDelegate();
	
	public class TimerPipe : IPipe
	{
		private readonly PipeDelegate _next;
		private readonly ILogger _logger;
		
		public TimerPipe(PipeDelegate next, ILogger<TimerPipe> logger)
		{
			_next = next;
			_logger = logger;
		}
		
		public async Task Invoke()
		{
			var watch = Stopwatch.StartNew();
			await _next();
			var elapsed = watch.Elapsed;
			
			if (_logger.IsEnabled(LogLevel.Information))
				_logger.LogInformation("Executed action in {timeTakenMillis}ms", elapsed.TotalMilliseconds);	
		}
	}
}