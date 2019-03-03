using System;
using System.Diagnostics;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	/// <summary>
	/// Pipe registry config.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class PipeConfig
	{
		/// <summary>
		/// Debugger display.
		/// </summary>
		protected string DebuggerDisplay => $"Type: '{Type}', Args: '{Args}'";

		/// <summary>
		/// Get or set the pipe type to register.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Get or set additional arguments to instantiate pipe with.
		/// </summary>
		public object[] Args { get; set; }

		public PipeConfig()
		{
		}

		public PipeConfig(Type type, object[] args = null)
		{
			Type = type;
			Args = args;
		}

		/// <summary>
		/// Destructuring.
		/// </summary>
		public void Deconstruct(out Type type, out object[] args) { type = Type; args = Args; }
	}
}