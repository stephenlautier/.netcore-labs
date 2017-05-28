using System;
using System.Collections.Generic;

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

		public TestSuiteBuilder Add<TTest>(string name) where TTest : ITest, new()
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

	internal class TestEntity
	{
		public string Name { get; set; }
		public Type Type { get; set; }
	}
}