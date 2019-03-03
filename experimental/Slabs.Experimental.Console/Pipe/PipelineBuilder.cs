using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	/// <summary>
	/// Represents a method invocation.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class PipelineContext
	{
		/// <summary>
		/// Debugger display.
		/// </summary>
		protected string DebuggerDisplay => $"Options: {{ {Options} }}";

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
		public PipelineBuilder Add<T>(params object[] args)
			where T : IPipe
			=> Add(typeof(T), args);

		/// <summary>
		/// Add pipe middleware, they will execute according to the order they are registered.
		/// </summary>
		/// <param name="type">Pipe type which must implements <see cref="IPipe"/>.</param>
		/// <param name="args">Additional arguments to be used within the pipe ctor.</param>
		public PipelineBuilder Add(Type type, params object[] args)
		{
			if (!typeof(IPipe).IsAssignableFrom(type))
				throw new ArgumentException($"Type '{type.FullName}' must implement {nameof(IPipe)}.", nameof(type));

			_pipes.Add(new PipeConfig(type, args));
			return this;
		}

		/// <summary>
		/// Adds a collection from pipes configs.
		/// </summary>
		/// <param name="pipes">Pipe configs to add.</param>
		public PipelineBuilder AddRange(IEnumerable<PipeConfig> pipes)
		{
			_pipes.AddRange(pipes);
			return this;
		}

		/// <summary>
		/// Get all pipes configs.
		/// </summary>
		public IEnumerable<PipeConfig> GetAll() => _pipes.AsReadOnly();

		/// <summary>
		/// Build configured <see cref="Pipeline"/>.
		/// </summary>
		public Pipeline Build()
		{
			if (_pipes.Count == 0)
				throw new InvalidOperationException("Cannot build pipeline with zero pipes.");

			var pipes = _pipes.ToList();
			pipes.Add(new PipeConfig(typeof(ActionExecutePipe)));

			IPipe previous = null;
			for (int i = pipes.Count; i-- > 0;)
			{
				var pipe = pipes[i];
				var isLast = pipes.Count - 1 == i;
				var isFirst = i == 0;

				object[] ctor;
				if (!isLast)
				{
					PipeDelegate next = previous.Invoke;

					if (pipe.Args == null)
						ctor = new object[] { next };
					else
					{
						const int additionalCtorArgs = 1;
						ctor = new object[pipe.Args.Length + additionalCtorArgs];
						ctor[0] = next;
						Array.Copy(pipe.Args, 0, ctor, additionalCtorArgs, pipe.Args.Length);
					}
				}
				else
					ctor = new object[] { };
				var instance = (IPipe)ActivatorUtilities.CreateInstance(_serviceProvider, pipe.Type, ctor);
				if (isFirst)
					return new Pipeline(instance);
				previous = instance;
			}
			throw new InvalidOperationException("Pipeline was not build correctly!");
		}
	}

	/// <summary>
	/// Action invoker pipe, which actually triggers the users defined function. Generally invoked as the last pipe.
	/// </summary>
	public class ActionExecutePipe : IPipe
	{
		public async Task<object> Invoke(PipelineContext context) => await context.Func();
	}
}