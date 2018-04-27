using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	public class Hero
	{
		public string Name { get; set; }

		public override string ToString() => $"Name: '{Name}'";
	}
	
	/*
	 * - skip pipe (within run)
	 */
	public class PipeTestStartup
	{
		private readonly ILogger _logger;
		private readonly PipelineBuilderFactory _pipelineBuilderFactory;

		public PipeTestStartup(ILogger<PipeTestStartup> logger, PipelineBuilderFactory pipelineBuilderFactory)
		{
			_logger = logger;
			_pipelineBuilderFactory = pipelineBuilderFactory;
		}

		public async Task Run()
		{
			_logger.LogInformation("Init Pipe Test...");
			var pipeBuilder = _pipelineBuilderFactory.Create()
				.Add<TimerPipe>()
				.Add<CachePipe>()
				;

			var pipeline = pipeBuilder.Build();
			var r1 = await pipeline.Run(GetFruit, new PipelineOptions().SetCache("get-fruit"));
			var r2 = await pipeline.Run(GetFruit, new PipelineOptions().SetCache("get-fruit"));
			var hero = await pipeline.Run(GetHero, new PipelineOptions().SetCache("get-hero"));
			await pipeline.Run(SetFruit, new PipelineOptions().SetNoCache());

			_logger.LogInformation("[Pipe] Result={result1} R2={result2}, Hero: {@hero}", r1, r2, hero);
		}

		public async Task<string> GetFruit()
		{
			_logger.LogInformation($"[Service] Get fruit...");
			await Task.Delay(250);
			return "strawberry";
		}

		public async Task<Hero> GetHero()
		{
			_logger.LogInformation($"[Service] Get Hero...");
			await Task.Delay(250);
			return new Hero
			{
				Name = "Rexxar"
			};
		}

		public async Task SetFruit()
		{
			_logger.LogInformation($"[Service] Set fruit...");
			await Task.Delay(100);
			_logger.LogInformation($"[Service] Set fruit complete");
		}
	}
	

	public class TimerPipe : IPipe
	{
		private readonly PipeDelegate _next;
		private readonly ILogger _logger;

		public TimerPipe(PipeDelegate next, ILogger<TimerPipe> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task<object> Invoke(PipelineContext context)
		{
			if (!_logger.IsEnabled(LogLevel.Debug))
				return await _next(context);

			var watch = Stopwatch.StartNew();
			var result = await _next(context);
			var elapsed = watch.Elapsed;

			_logger.LogDebug("Executed action in {timeTakenMillis:n0}ms", elapsed.TotalMilliseconds);
			return result;
		}
	}
}