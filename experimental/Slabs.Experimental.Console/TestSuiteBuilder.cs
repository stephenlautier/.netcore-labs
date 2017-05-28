using System;
using System.Collections.Generic;
using System.Linq;

namespace Slabs.Experimental.ConsoleClient
{
	/// <summary>
	/// Builds a set of tests to be executed, which will be added to <see cref="TestSuiteBuilder"/>.
	/// </summary>
	public class TestGroupBuilder
	{
		private readonly List<List<TestEntity>> _testGroups;
		private List<TestEntity> _currentParallelGroup;

		public TestGroupBuilder()
		{
			_testGroups = new List<List<TestEntity>>();
		}

		public TestGroupBuilder Add<TTest>(string name) where TTest : ITest, new()
		{
			var group = new List<TestEntity>
			{
				ToTestEntity(name, typeof(TTest))
			};
			_testGroups.Add(group);
			_currentParallelGroup = null;
			return this;
		}

		public TestGroupBuilder AddParallel<TTest>(string name) where TTest : ITest, new()
		{
			var testEntity = ToTestEntity(name, typeof(TTest));
			if (_currentParallelGroup == null)
			{
				var group = new List<TestEntity>
				{
					testEntity
				};
				_testGroups.Add(group);
				_currentParallelGroup = group;
			}
			else
				_currentParallelGroup.Add(testEntity);

			return this;
		}

		/// <summary>
		/// Builds tasks for the <see cref="TestGroupBuilder"/> - generally should be used by the <see cref="TestSuiteBuilder"/> to compose TestGroups.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<List<TestEntity>> Build() => _testGroups;

		private static TestEntity ToTestEntity(string key, Type type)
		{
			return new TestEntity
			{
				Name = key,
				Type = type
			};
		}
	}

	public class TestSuiteBuilder
	{
		private readonly string _name;
		private readonly List<TestGroupBuilder> _testGroups;

		public TestSuiteBuilder(string name)
		{
			_name = name;
			_testGroups = new List<TestGroupBuilder>();
		}

		public TestSuiteBuilder Add(TestGroupBuilder testGroup)
		{
			_testGroups.Add(testGroup);
			return this;
		}

		public ITestSuite Build()
		{
			var testGroups = _testGroups.SelectMany(x => x.Build());
			return new TestSuite(_name, testGroups);
		}
	}

	public class TestEntity
	{
		public string Name { get; set; }
		public Type Type { get; set; }
	}
}