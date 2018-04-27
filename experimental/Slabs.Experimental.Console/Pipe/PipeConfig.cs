using System;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	/// <summary>
	/// Pipe registry config.
	/// </summary>
	public class PipeConfig
	{
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
}