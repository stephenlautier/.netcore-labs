using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
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
		public PipelineOptions Options { get; set; }
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

	public interface IPipe
	{
		Task<object> Invoke(PipelineContext context);
	}

	public delegate Task<object> PipeDelegate(PipelineContext context);

	public class ActionExecutePipe : IPipe
	{
		public ActionExecutePipe(PipeDelegate next)
		{
		}

		public async Task<object> Invoke(PipelineContext context) => await context.Func();
	}
}