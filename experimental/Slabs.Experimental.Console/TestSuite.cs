using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient
{
	public class TestSuiteBuilder
	{
		private readonly string _name;

		private List<List<TestEntity>> TestGroups { get; }
		private List<TestEntity> _currentParallelGroup;

		public TestSuiteBuilder(string name)
		{
			TestGroups = new List<List<TestEntity>>();
			_name = name;
		}

		public ITestSuite Build()
		{
			return new TestSuite(_name, TestGroups);
		}

		internal TestSuiteBuilder Add<TTest>(string name) where TTest : ITest, new()
		{
			var group = new List<TestEntity>
			{
				ToTestEntity(name, typeof(TTest))
			};
			TestGroups.Add(group);
			_currentParallelGroup = null;
			return this;
		}
		
		public TestSuiteBuilder AddParallel<TTest>(string name) where TTest : ITest, new()
		{
			var testEntity = ToTestEntity(name, typeof(TTest));
			if (_currentParallelGroup == null)
			{
				var group = new List<TestEntity>
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

		static TestEntity ToTestEntity(string key, Type type)
		{
			return new TestEntity
			{
				Name = key,
				Type = type
			};
		}
	}

	public interface ITestSuite
	{
		string Name { get; }
		Task Run();
	}

	public class TestSuite : ITestSuite
	{
		public string Name { get; }
		private readonly List<List<TestEntity>> _tests;

		internal TestSuite(string name, List<List<TestEntity>> tests)
		{
			Name = name;
			_tests = tests;
		}

		public async Task Run()
		{
			Console.WriteLine($"[TestSuite] Running test suite '{Name}'");
			foreach (var testGroup in _tests)
			{
				IList<Task> promises = new List<Task>();
				foreach (var testEntity in testGroup)
				{
					Console.WriteLine($"[TestSuite] Running '{testEntity.Name}'");
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

	internal class TestEntity
	{
		public string Name { get; set; }
		public Type Type { get; set; }
	}

}
