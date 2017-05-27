using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient
{
	internal class TestSuiteBuilder
	{
		private readonly string _name;

		private List<ITest> Tests { get; }


		public TestSuiteBuilder(string name)
		{
			Tests = new List<ITest>();
			_name = name;
		}

		public TestSuite Build()
		{
			return new TestSuite(_name, Tests);
		}

		public TestSuiteBuilder Add(string key)
		{
			Tests.Add(new TestEntity { Key = key });
			return this;
		}
	}

	internal class TestSuite
	{
		public string Name { get; }
		private readonly List<ITest> _tests;

		public TestSuite(string name, List<ITest> tests)
		{
			Name = name;
			_tests = tests;
		}

		public Task Run()
		{
			Console.WriteLine($"[TestSuite] Running tests for '{Name}'");
			foreach (var test in _tests)
			{
				Console.WriteLine($"[TestSuite] Running {test.Key}");
			}
			return Task.CompletedTask;
		}
	}

	internal interface ITest
	{
		string Key { get; set; }
		//Task Execute();
	}

	internal class TestEntity : ITest
	{
		public string Key { get; set; }
	}

}
