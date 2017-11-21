using System;
using NullGuard;

namespace Slabs.FoldySample
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			SomeMethod(null);
		}

		public static void SomeMethod(string arg)
		{
			// throws ArgumentNullException if arg is null.
			Console.WriteLine($"Argument {arg.ToString()}");
		}

		public void AnotherMethod([AllowNull] string arg)
		{
			// arg may be null here
		}

	}
}