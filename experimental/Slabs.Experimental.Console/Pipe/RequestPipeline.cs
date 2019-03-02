using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	public delegate Task<Response> RequestPipeDelegate(RequestPipeContext context);

	public class Request
	{
		public string Url { get; set; }
	}
	public class Response
	{
		public string Status { get; set; }
	}

	public class RequestPipeContext : PipelineContext<Response>
	{
		public Request Request { get; }
	}

	public class RequestPipeline : Pipeline<RequestPipeContext, Response, IRequestPipe>
	{
		public RequestPipeline(IRequestPipe pipeline) : base(pipeline)
		{
		}
	}

	public interface IRequestPipe : IPipe<RequestPipeContext, Response>
	{
		Task<Response> Invoke(RequestPipeContext context);

	}

	public class RequestLogPipe : IRequestPipe
	{
		//private readonly RequestPipeDelegate _next;

		public RequestLogPipe(PipeDelegate<RequestPipeContext, Response> next)
		{
			//_next = next;
		}

		public Task<Response> Invoke(RequestPipeContext context)
		{

			throw new NotImplementedException();
		}
	}

	public class RequestPipelineBuilder : PipelineBuilder<RequestPipeContext, Response, RequestPipeline, IRequestPipe>
	{
		public RequestPipelineBuilder(IServiceProvider serviceProvider) : base(serviceProvider)
		{
		}
	}

	public class RequestPipelineBuilderFactory : PipelineBuilderFactory<RequestPipelineBuilder, RequestPipeContext,
		Response, RequestPipeline, IRequestPipe>
	{
		public RequestPipelineBuilderFactory(IServiceProvider serviceProvider) : base(serviceProvider)
		{
		}
	}

	public class RequestPipelineTest
	{
		private readonly ILogger<RequestPipelineTest> _logger;
		private readonly RequestPipelineBuilderFactory _pipelineBuilderFactory;

		public RequestPipelineTest(ILogger<RequestPipelineTest> logger, RequestPipelineBuilderFactory pipelineBuilderFactory)
		{
			_logger = logger;
			_pipelineBuilderFactory = pipelineBuilderFactory;
		}
		public async Task Run()
		{
			var pipelineBuilder = _pipelineBuilderFactory.Create();
			var pipeline = pipelineBuilder.Add<RequestLogPipe>()
							.Build()
				;

			var r = pipeline.Run(async () =>
			 {
				 await Task.Delay(1000);
				 return new Response { Status = "Yay!" };
			 });




			await Task.CompletedTask;
		}

	}
}
