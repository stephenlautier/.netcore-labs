using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient
{
	public interface ITestSuite
	{
		string Name { get; }
		Task<List<TestResult>> Run();
	}

	public class TestSuite : ITestSuite
	{
		public string Name { get; }
		public int TotalTestsCount { get; }
		private readonly IServiceProvider _serviceProvider;
		private readonly ICollection<List<TestEntity>> _tests;
		private readonly ILogger _logger;

		private readonly List<TestResult> _results;

		internal TestSuite(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, string name, ICollection<List<TestEntity>> tests)
		{
			_serviceProvider = serviceProvider;
			_logger = loggerFactory.CreateLogger($"[TestSuite::{name}]");
			Name = name;
			_tests = tests;
			TotalTestsCount = tests.Sum(x => x.Count);
			_results = new List<TestResult>(TotalTestsCount);
		}

		public async Task<List<TestResult>> Run()
		{
			_logger.LogInformation("Running test suite {Name} ({total})", Name, TotalTestsCount);
			var scope = _serviceProvider.CreateScope();
			
			int counter = 1;
			foreach (var testGroup in _tests)
			{
				_logger.LogInformation("{Name} ({counter}/{total})", Name, counter, TotalTestsCount);
				var executingTests = new List<(Task Promise, TestResult Result)>();
				foreach (var testEntity in testGroup)
				{
					_logger.LogInformation("Running {Name}", testEntity.Name);
					var result = new TestResult
					{
						Name = testEntity.Name,
						State = TestStateType.Executing
					};
					_results.Add(result);
					
					var testUnit = (ITest)ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, testEntity.Type);
					Task task = null;
					try
					{
						task = testUnit.Execute();
					}
					catch (Exception ex)
					{
						_logger.LogError("Test {Name} failed", result.Name);
						result.State = TestStateType.Failed;
						result.Exception = ex;
					}

					if (task != null)
						executingTests.Add((task, result));
					counter++;
				}
				
				try
				{
					// parallized test groups
					await Task.WhenAll(executingTests.Select(x => x.Promise));
				}
				catch (Exception)
				{
				}
				finally
				{
					foreach (var (promise, result) in executingTests)
					{
						if (promise.IsFaulted)
						{
							_logger.LogError("Test {Name} failed", result.Name);
							result.State = TestStateType.Failed;
							result.Exception = promise.Exception?.InnerException ?? promise.Exception;
						} else if (promise.IsCompleted)
						{
							result.State = TestStateType.Success;
						}
					}
				}
			}

			return _results;
		}

	}

	[DebuggerDisplay("{Name} [{State}]")]
	public class TestResult
	{
		public string Name { get; set; }
		public TestStateType State { get; set; }
		public Exception Exception { get; set; }
	}

	public enum TestStateType
	{
		Pending,
		Executing,
		Success,
		Failed
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
