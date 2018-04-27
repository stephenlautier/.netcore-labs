using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	/// <summary>
	/// Represents a method invocation.
	/// </summary>
	public class PipelineContext
	{
		internal Func<Task<object>> Func { get; set; }
		public PipelineOptions Options { get; set; }
	}

	/// <summary>
	/// Pipeline producer.
	/// </summary>
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

		/// <summary>
		/// Add pipe middleware, they will execute according to the order they are registered.
		/// </summary>
		/// <param name="args">Additional arguments to be used within the pipe ctor.</param>
		/// <returns></returns>
		public PipelineBuilder Add<T>(params object[] args)
			where T : IPipe
			=> Add(typeof(T), args);

		/// <summary>
		/// Add pipe middleware, they will execute according to the order they are registered.
		/// </summary>
		/// <param name="type">Pipe type which must implements <see cref="IPipe"/>.</param>
		/// <param name="args">Additional arguments to be used within the pipe ctor.</param>
		/// <returns></returns>
		public PipelineBuilder Add(Type type, params object[] args)
		{
			if(!typeof(IPipe).IsAssignableFrom(type))
				throw new ArgumentException($"Type '{type.FullName}' must implement {nameof(IPipe)}.", nameof(type));

			_pipes.Add(new PipeConfig(type, args));
			return this;
		}

		/// <summary>
		/// Build configured <see cref="Pipeline"/>.
		/// </summary>
		/// <returns></returns>
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

	/// <summary>
	/// Action invoker pipe, which actually triggers the users defined function. Generally invoked as the last pipe.
	/// </summary>
	public class ActionExecutePipe : IPipe
	{
		public ActionExecutePipe(PipeDelegate next)
		{
		}

		public async Task<object> Invoke(PipelineContext context) => await context.Func();
	}
}