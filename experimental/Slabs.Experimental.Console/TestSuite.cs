using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
		private readonly IServiceProvider _serviceProvider;
		private readonly IEnumerable<List<TestEntity>> _tests;
		private readonly ILogger _logger;

		internal TestSuite(string name, IServiceProvider serviceProvider, IEnumerable<List<TestEntity>> tests, ILoggerFactory loggerFactory)
		{
			Name = name;
			_serviceProvider = serviceProvider;
			_tests = tests;
			_logger = loggerFactory.CreateLogger($"[TestSuite::{name}]");
		}

		public async Task Run()
		{
			_logger.LogInformation("Running test suite {Name}", Name);
			foreach (var testGroup in _tests)
			{
				var promises = new List<Task>();
				foreach (var testEntity in testGroup)
				{
					_logger.LogInformation("Running {Name}", testEntity.Name);
					var testUnit = (ITest)ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, testEntity.Type);
					var task = testUnit.Execute();
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
