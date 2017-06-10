using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.FluentHttp
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class FluentHttpClient
	{
		private string DebuggerDisplay => $"[{Identifier}] BaseUrl: '{BaseUrl}', MiddlewareCount: {_middleware.Count}";

		/// <summary>
		/// Get the identifier (key) for this instance, which is registered within the factory as.
		/// </summary>
		public string Identifier { get; }
		public string BaseUrl { get; }

		/// <summary>
		/// Raw http client. This should be avoided from being used.
		/// However if something is not exposed and its really needed, it can be used from here.
		/// </summary>
		public HttpClient RawHttpClient { get; }
		public MediaTypeFormatterCollection Formatters { get; } = new MediaTypeFormatterCollection();
		public HttpRequestHeaders Headers { get; }

		private readonly IFluentHttpMiddlewareRunner _middlewareRunner;
		private readonly IList<Type> _middleware;
		private static readonly HttpMethod HttpMethodPatch = new HttpMethod("Patch");
		
		public FluentHttpClient(FluentHttpClientOptions options, IServiceProvider serviceProvider, IFluentHttpMiddlewareRunner middlewareRunner)
		{
			_middlewareRunner = middlewareRunner;
			RawHttpClient = Configure(options);
			Headers = RawHttpClient.DefaultRequestHeaders;
			_middleware = options.Middleware;
			Identifier = options.Identifier;
			BaseUrl = options.BaseUrl;
		}

		public async Task<T> Post<T>(string url, object data, MediaTypeHeaderValue contentType = null)
		{
			var formatter = GetFormatter(contentType);

			var response = await RawHttpClient.PostAsync(url, new ObjectContent(data.GetType(), data, formatter));

			// todo: implement this better
			response.EnsureSuccessStatusCode();

			var dataResult = await ParseResult<T>(response);
			return dataResult;
		}

		public async Task<T> Patch<T>(string url, object data, MediaTypeHeaderValue contentType = null)
		{
			var formatter = GetFormatter(contentType);

			var request = new HttpRequestMessage(HttpMethodPatch, url)
			{
				Content = new ObjectContent(data.GetType(), data, formatter)
			};
			var response = await RawHttpClient.SendAsync(request);

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
			var response = await RawHttpClient.GetAsync(url);

			var result = new FluentHttpResponse<T>(response);

			if (response.IsSuccessStatusCode)
				result.Data = await ParseResult<T>(response);

			return result;
		}

		/// <summary>Get the formatter for an HTTP content type.</summary>
		/// <param name="contentType">The HTTP content type (or <c>null</c> to automatically select one).</param>
		/// <exception cref="InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
		private MediaTypeFormatter GetFormatter(MediaTypeHeaderValue contentType = null)
		{
			if (!Formatters.Any())
				throw new InvalidOperationException("No media type formatters available.");

			MediaTypeFormatter formatter = contentType != null
				? Formatters.FirstOrDefault(x => x.SupportedMediaTypes.Any(m => m.MediaType == contentType.MediaType))
				: Formatters.FirstOrDefault();
			if (formatter == null)
				throw new InvalidOperationException($"No media type formatters are available for '{contentType}' content-type.");

			return formatter;
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

}