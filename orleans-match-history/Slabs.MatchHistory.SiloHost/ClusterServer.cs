using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using System;
using System.Collections.Generic;
using System.Text;
using Orleans.Runtime.Configuration;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Slabs.MatchHistory.Silo
{
	public class ClusterServer
	{
		private const int START_FAIL = 5000;
		private SiloHost _siloHost;
		private ClusterConfiguration _config;
		private ILogger<ClusterServer> _logger;

		public ClusterServer(ILogger<ClusterServer> logger)
		{
			_logger = logger;
		}

		internal bool Start()
		{
			_siloHost = new SiloHost(Dns.GetHostName(), _config);
			_siloHost.LoadOrleansConfig(); // todo: remove?

			try
			{
				_siloHost.InitializeOrleansSilo();
				if (!_siloHost.StartOrleansSilo(false))
				{
					// todo: remove?
					var exceptionMessage = $"Failed to start Orleans silo '{_siloHost.Name}' as a {_siloHost.Type} node.";
					_logger.LogError(exceptionMessage);
					throw new OrleansException(exceptionMessage);
				}

				_logger.LogInformation("Successfully started silo {siloName} as a {siloType} node.", _siloHost.Name, _siloHost.Type);

				return true;
			}
			catch (Exception ex)
			{
				_siloHost.ReportStartupError(ex);
				_logger.LogError(START_FAIL, ex, "Silo failed to start");
				return false;
			}
		}

		internal int Shutdown()
		{
			if (_siloHost == null) return 0;

			try
			{
				_siloHost.StopOrleansSilo();
				_siloHost.Dispose();
				_logger.LogInformation($"Silo '{_siloHost.Name}' shutdown.");
			}
			catch (Exception ex)
			{
				_siloHost.ReportStartupError(ex);
				_logger.LogInformation("Silo failed to shutdown.");
				_logger.LogError(ex.Message);
				return 1;
			}
			return 0;
		}

		internal void Wait()
		{
			if (_siloHost != null)
				_siloHost.WaitForOrleansSiloShutdown();
		}

		internal ClusterServer Configure()
		{
			var clusterConfig = ClusterConfiguration.LocalhostPrimarySilo();
			clusterConfig.AddMemoryStorageProvider();
			//config.AddAzureTableStorageProvider(connectionString: "UseDevelopmentStorage=true");

			_config = clusterConfig;

			return this;
		}
	}
}