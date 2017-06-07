using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.FluentHttp
{
	public class FluentHttpClient
	{
		private readonly HttpClient _httpClient;
		public string Identifier { get; }
		public string BaseUrl { get; }

		private readonly IFluentHttpMiddlewareRunner _middlewareRunner;
		private readonly IList<Type> _middlewares;

		public FluentHttpClient(FluentHttpClientOptions options, IServiceProvider serviceProvider, IFluentHttpMiddlewareRunner middlewareRunner)
		{
			_middlewareRunner = middlewareRunner;
			_httpClient = Configure(options);
			_middlewares = options.Middleware;
			Identifier = options.Identifier;
			BaseUrl = options.BaseUrl;
		}

		public async Task<T> Post<T>(string url, object data)
		{
			var response = await _httpClient.PostAsync(url, new JsonContent(data));

			// todo: implement this better
			response.EnsureSuccessStatusCode();

			var dataResult = await ParseResult<T>(response);
			return dataResult;
		}

		public async Task<T> Get<T>(string url)
		{
			var response = await GetAsHttp<T>(url);
			response.EnsureSuccessStatusCode();
			return response.Data;
		}

		public async Task<FluentHttpResponse<T>> GetAsHttp<T>(string url)
		{
			var response = await _httpClient.GetAsync(url);

			var result = new FluentHttpResponse<T>(response);

			if (response.IsSuccessStatusCode)
				result.Data = await ParseResult<T>(response);

			return result;
		}

		public async Task<FluentHttpResponse<T>> GetAsHttpWithMiddleware<T>(string url)
		{
			var request = new FluentHttpRequest
			{
				Url = url,
				Method = "GET"
			};

			var response = await _middlewareRunner.Run<T>(_middlewares, request, async r => await GetAsHttp<T>(r.Url));
			return (FluentHttpResponse<T>)response;
		}

		private async Task<T> ParseResult<T>(HttpResponseMessage response)
		{
			var responseContent = await response.Content.ReadAsStringAsync();
			// todo: settings options
			return JsonConvert.DeserializeObject<T>(responseContent);
		}

		private HttpClient Configure(FluentHttpClientOptions options)
		{
			var httpClient = new HttpClient
			{
				BaseAddress = new Uri(options.BaseUrl)
			};
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.Timeout = options.Timeout;

			foreach (var headerEntry in options.Headers)
			{
				httpClient.DefaultRequestHeaders.Add(headerEntry.Key, headerEntry.Value);
			}

			return httpClient;
		}
	}

	public interface IFluentHttpMiddlewareRunner
	{
		Task<IFluentHttpResponse> Run<T>(IList<Type> middleware, FluentHttpRequest request, FluentHttpRequestDelegate send);
	}

	public class FluentHttpMiddlewareRunner : IFluentHttpMiddlewareRunner
	{
		private readonly IServiceProvider _serviceProvider;

		public FluentHttpMiddlewareRunner(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task<IFluentHttpResponse> Run<T>(IList<Type> middleware, FluentHttpRequest request, FluentHttpRequestDelegate send)
		{
			if (middleware.Count == 0)
				return await send(request);

			IFluentHttpResponse httpResult = null;
			IFluentHttpMiddleware previousMiddleware = null;

			for (int i = middleware.Count; i-- > 0;)
			{
				var type = middleware[i];

				var isLast = middleware.Count - 1 == i;
				var isFirst = i == 0;
				var next = isLast
					? send
					: previousMiddleware.Invoke;
				var instance = (IFluentHttpMiddleware)ActivatorUtilities.CreateInstance(_serviceProvider, type, next);

				if (isFirst)
					httpResult = await instance.Invoke(request);
				else
					previousMiddleware = instance;
			}
			return httpResult;
		}

	}

	public class FluentHttpClientOptions
	{
		public string BaseUrl { get; set; }
		public TimeSpan Timeout { get; set; }
		public string Identifier { get; set; }
		public Dictionary<string, string> Headers { get; set; }
		public List<Type> Middleware { get; set; }
	}

	public class JsonContent : StringContent
	{
		public JsonContent(object obj) :
			base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
		{
		}
	}
}