using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient
{
	internal class TestSuiteBuilder
	{
		private readonly string _name;

		private List<List<ITest>> TestGroups { get; }
		private List<ITest> _currentParallelGroup;

		public TestSuiteBuilder(string name)
		{
			TestGroups = new List<List<ITest>>();
			_name = name;
		}

		public TestSuite Build()
		{
			return new TestSuite(_name, TestGroups);
		}

		public TestSuiteBuilder Add(string key)
		{
			var group = new List<ITest>
			{
				ToTestEntity(key)
			};
			TestGroups.Add(group);
			_currentParallelGroup = null;
			return this;
		}

		public TestSuiteBuilder AddParallel(string key)
		{
			var testEntity = ToTestEntity(key);
			if (_currentParallelGroup == null)
			{
				var group = new List<ITest>
				{
					testEntity
				};
				TestGroups.Add(group);
				_currentParallelGroup = group;
			}
			else
			{
				_currentParallelGroup.Add(testEntity);
			}
			
			return this;
		}

		TestEntity ToTestEntity(string key)
		{
			return new TestEntity
			{
				Key = key
			};
		}
	}

	internal class TestSuite
	{
		public string Name { get; }
		private readonly List<List<ITest>> _tests;

		public TestSuite(string name, List<List<ITest>> tests)
		{
			Name = name;
			_tests = tests;
		}

		public Task Run()
		{
			Console.WriteLine($"[TestSuite] Running tests for '{Name}'");
			foreach (var testGroup in _tests)
			{
				foreach (var test in testGroup)
				{
					Console.WriteLine($"[TestSuite] Running {test.Key}");
				}
			}
			return Task.CompletedTask;
		}
	}

	internal interface ITest
	{
		string Key { get; }
		//Task Execute();
	}

	internal class TestEntity : ITest
	{
		public string Key { get; set; }
	}

}
