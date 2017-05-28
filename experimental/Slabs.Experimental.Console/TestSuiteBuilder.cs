using Microsoft.Extensions.Logging;
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

		public TestGroupBuilder Add<TTest>(string name) where TTest : ITest
		{
			var group = new List<TestEntity>
			{
				ToTestEntity(name, typeof(TTest))
			};
			_testGroups.Add(group);
			_currentParallelGroup = null;
			return this;
		}

		public TestGroupBuilder AddParallel<TTest>(string name) where TTest : ITest
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
		/// Builds tasks for the <see cref="TestGroupBuilder"/> - generally used by the <see cref="TestSuiteBuilder"/> itself to compose Test Groups.
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
		private readonly IServiceProvider _serviceProvider;
		private readonly ILoggerFactory _loggerFactory;
		private readonly List<TestGroupBuilder> _testGroups;

		public TestSuiteBuilder(string name, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
		{
			_name = name;
			_serviceProvider = serviceProvider;
			_loggerFactory = loggerFactory;
			_testGroups = new List<TestGroupBuilder>();
		}

		public TestSuiteBuilder Add(TestGroupBuilder testGroup)
		{
			_testGroups.Add(testGroup);
			return this;
		}

		public ITestSuite Build()
		{
			var testGroups = _testGroups.SelectMany(x => x.Build()).ToList();
			return new TestSuite(_serviceProvider, _loggerFactory, _name, testGroups);
		}
	}

	// ReSharper disable once ClassNeverInstantiated.Global
	public class TestSuiteBuilderFactory
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILoggerFactory _loggerFactory;

		public TestSuiteBuilderFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
		{
			_serviceProvider = serviceProvider;
			_loggerFactory = loggerFactory;
		}

		public TestSuiteBuilder Create(string name) => new TestSuiteBuilder(name, _serviceProvider, _loggerFactory);
	}

	public class TestEntity
	{
		public string Name { get; set; }
		public Type Type { get; set; }
	}
}