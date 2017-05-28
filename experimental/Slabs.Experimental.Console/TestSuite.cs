using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient
{
	public interface ITestSuite
	{
		string Name { get; }
		Task Run();
	}

	public class TestSuite : ITestSuite
	{
		public string Name { get; }
		private readonly IEnumerable<List<TestEntity>> _tests;

		internal TestSuite(string name, IEnumerable<List<TestEntity>> tests)
		{
			Name = name;
			_tests = tests;
		}

		public async Task Run()
		{
			Console.WriteLine($"[TestSuite] Running test suite '{Name}'");
			foreach (var testGroup in _tests)
			{
				var promises = new List<Task>();
				foreach (var testEntity in testGroup)
				{
					Console.WriteLine($"[TestSuite] Running '{testEntity.Name}'");
					// todo: make tests to support DI.
					var test = (ITest)Activator.CreateInstance(testEntity.Type);
					var task = test.Execute();
					promises.Add(task);
				}
				// parallized test groups
				await Task.WhenAll(promises);
			}
		}
	}

	/// <summary>
	/// Describe a test unit.
	/// </summary>
	public interface ITest
	{
		/// <summary>
		/// Method which gets executed by the runner.
		/// </summary>
		/// <returns></returns>
		Task Execute();
	}

}
