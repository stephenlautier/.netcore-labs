using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
		public int TotalTestsCount { get; }
		private readonly IServiceProvider _serviceProvider;
		private readonly ICollection<List<TestEntity>> _tests;
		private readonly ILogger _logger;

		internal TestSuite(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, string name, ICollection<List<TestEntity>> tests)
		{
			_serviceProvider = serviceProvider;
			_logger = loggerFactory.CreateLogger($"[TestSuite::{name}]");
			Name = name;
			_tests = tests;
			TotalTestsCount = tests.Sum(x => x.Count);
		}

		public async Task Run()
		{
			_logger.LogInformation("Running test suite {Name} ({total})", Name, TotalTestsCount);
			int counter = 1;
			foreach (var testGroup in _tests)
			{
				_logger.LogInformation("{Name} ({counter}/{total})", Name, counter, TotalTestsCount);
				var promises = new List<Task>();
				foreach (var testEntity in testGroup)
				{
					_logger.LogInformation("Running {Name}", testEntity.Name);
					var testUnit = (ITest)ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, testEntity.Type);
					var task = testUnit.Execute();
					promises.Add(task);
					counter++;
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
