using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.FluentHttp
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpClient
	{
		private string DebuggerDisplay => $"[{Identifier}] BaseUrl: '{BaseUrl}', MiddlewareCount: {_middleware.Count}";
		private readonly HttpClient _httpClient;
		public string Identifier { get; }
		public string BaseUrl { get; }
		public MediaTypeFormatterCollection Formatters { get; } = new MediaTypeFormatterCollection();

		private readonly IFluentHttpMiddlewareRunner _middlewareRunner;
		private readonly IList<Type> _middleware;

		public FluentHttpClient(FluentHttpClientOptions options, IServiceProvider serviceProvider, IFluentHttpMiddlewareRunner middlewareRunner)
		{
			_middlewareRunner = middlewareRunner;
			_httpClient = Configure(options);
			_middleware = options.Middleware;
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
			var response = await _GetAsResponse<T>(url);
			response.EnsureSuccessStatusCode();
			return response.Data;
		}

		public async Task<FluentHttpResponse<T>> GetAs<T>(string url)
		{
			var request = new FluentHttpRequest
			{
				Url = url,
				Method = HttpMethod.Get
			};

			var response = await _middlewareRunner.Run<T>(_middleware, request, async r => await _GetAsResponse<T>(r.Url));
			return (FluentHttpResponse<T>)response;
		}

		private async Task<T> ParseResult<T>(HttpResponseMessage response) => await response.Content.ReadAsAsync<T>(Formatters);

		private HttpClient Configure(FluentHttpClientOptions options)
		{
			var httpClient = new HttpClient
			{
				BaseAddress = new Uri(options.BaseUrl)
			};
			httpClient.DefaultRequestHeaders.Add("Accept", Formatters.SelectMany(x => x.SupportedMediaTypes).Select(x => x.MediaType));
			httpClient.Timeout = options.Timeout;

			foreach (var headerEntry in options.Headers)
				httpClient.DefaultRequestHeaders.Add(headerEntry.Key, headerEntry.Value);

			return httpClient;
		}

		private async Task<FluentHttpResponse<T>> _GetAsResponse<T>(string url)
		{
			var response = await _httpClient.GetAsync(url);

			var result = new FluentHttpResponse<T>(response);

			if (response.IsSuccessStatusCode)
				result.Data = await ParseResult<T>(response);

			return result;
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