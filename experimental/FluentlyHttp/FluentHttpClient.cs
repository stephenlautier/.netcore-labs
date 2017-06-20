using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FluentlyHttp
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

		private readonly IServiceProvider _serviceProvider;
		private readonly IFluentHttpMiddlewareRunner _middlewareRunner;
		private readonly IList<Type> _middleware;

		public FluentHttpClient(FluentHttpClientOptions options, IServiceProvider serviceProvider, IFluentHttpMiddlewareRunner middlewareRunner)
		{
			_serviceProvider = serviceProvider;
			_middlewareRunner = middlewareRunner;
			RawHttpClient = Configure(options);
			Headers = RawHttpClient.DefaultRequestHeaders;
			_middleware = options.Middleware;
			Identifier = options.Identifier;
			BaseUrl = options.BaseUrl;
		}

		public Task<T> Post<T>(string url, object data, MediaTypeHeaderValue contentType = null)
		{
			return CreateRequest(url)
				.AsPost()
				.WithBody(data, contentType)
				.Return<T>();
		}

		public Task<T> Patch<T>(string url, object data, MediaTypeHeaderValue contentType = null)
		{
			return CreateRequest(url)
				.AsPatch()
				.WithBody(data, contentType)
				.Return<T>();
		}

		public Task<T> Get<T>(string url)
		{
			return CreateRequest(url)
				.AsGet()
				.Return<T>();
		}

		/// <summary>Get the formatter for an HTTP content type.</summary>
		/// <param name="contentType">The HTTP content type (or <c>null</c> to automatically select one).</param>
		/// <exception cref="System.InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
		public MediaTypeFormatter GetFormatter(MediaTypeHeaderValue contentType = null)
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

		public FluentHttpRequestBuilder CreateRequest(string uri = null)
		{
			var builder = ActivatorUtilities.CreateInstance<FluentHttpRequestBuilder>(_serviceProvider, this);
			if (uri != null)
				builder.WithUri(uri);
			return builder;
		}

		private HttpClient Configure(FluentHttpClientOptions options)
		{
			var httpClient = new HttpClient
			{
				BaseAddress = new Uri(options.BaseUrl)
			};
			httpClient.DefaultRequestHeaders.Add("Accept", Formatters.SelectMany(x => x.SupportedMediaTypes).Select(x => x.MediaType));
			httpClient.Timeout = options.Timeout;

			foreach (var headerEntry in options.Headers)
				httpClient.DefaultRequestHeaders.Add((string) headerEntry.Key, (string) headerEntry.Value);

			return httpClient;
		}

		public async Task<FluentHttpResponse<T>> Send<T>(FluentHttpRequestBuilder builder)
		{
			var fluentRequest = builder.Build();

			var response = await _middlewareRunner.Run<T>(_middleware, fluentRequest, async request =>
			{
				var result = await RawHttpClient.SendAsync(request.RawRequest);
				return ToFluentResponse<T>(result);
			});

			// todo: implement this better
			response.EnsureSuccessStatusCode();

			return (FluentHttpResponse<T>)response;
		}

		private static FluentHttpResponse<T> ToFluentResponse<T>(HttpResponseMessage response) => new FluentHttpResponse<T>(response);

	}

	public class FluentHttpClientOptions
	{
		public string BaseUrl { get; set; }
		public TimeSpan Timeout { get; set; }
		public string Identifier { get; set; }
		public Dictionary<string, string> Headers { get; set; }
		public List<Type> Middleware { get; set; }
	}

	public class FluentHttpRequestBuilder
	{
		public HttpMethod HttpMethod { get; private set; }
		public string Uri { get; private set; }

		private readonly FluentHttpClient _fluentHttpClient;
		private static readonly HttpMethod HttpMethodPatch = new HttpMethod("Patch");
		private HttpContent _httpBody;

		public FluentHttpRequestBuilder(FluentHttpClient fluentHttpClient)
		{
			_fluentHttpClient = fluentHttpClient;
		}

		public FluentHttpRequestBuilder AsGet()
		{
			HttpMethod = HttpMethod.Get;
			return this;
		}

		public FluentHttpRequestBuilder AsPost()
		{
			HttpMethod = HttpMethod.Post;
			return this;
		}

		public FluentHttpRequestBuilder AsPatch()
		{
			HttpMethod = HttpMethodPatch;
			return this;
		}

		public FluentHttpRequestBuilder WithMethod(HttpMethod method)
		{
			HttpMethod = method;
			return this;
		}

		public FluentHttpRequestBuilder WithUri(string uri, object interpolationData = null)
		{
			Uri = uri;
			// todo: interpolation
			return this;
		}


		/// <summary>Set the body content of the HTTP request.</summary>
		/// <param name="body">Value to serialize into the HTTP body content.</param>
		/// <param name="contentType">Request body format (or <c>null</c> to use the first supported Content-Type in the <see cref="Client.IRequest.Formatters"/>).</param>
		/// <returns>Returns the request builder for chaining.</returns>
		/// <exception cref="InvalidOperationException">No MediaTypeFormatters are available on the API client for this content type.</exception>
		public FluentHttpRequestBuilder WithBody<T>(T body, MediaTypeHeaderValue contentType = null)
		{
			MediaTypeFormatter formatter = _fluentHttpClient.GetFormatter(contentType);
			string mediaType = contentType?.MediaType;
			return WithBody(body, formatter, mediaType);
		}

		public FluentHttpRequestBuilder WithBody(object body, MediaTypeHeaderValue contentType = null)
		{
			MediaTypeFormatter formatter = _fluentHttpClient.GetFormatter(contentType);
			string mediaType = contentType?.MediaType;
			return WithBody(body, formatter, mediaType);
		}

		/// <summary>Set the body content of the HTTP request.</summary>
		/// <param name="body">Value to serialize into the HTTP body content.</param>
		/// <param name="formatter">Media type formatter with which to format the request body format.</param>
		/// <param name="mediaType">HTTP media type (or <c>null</c> for the <paramref name="formatter"/>'s default).</param>
		/// <returns>Returns the request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithBody(object body, MediaTypeFormatter formatter, string mediaType = null)
		{
			return WithBodyContent(new ObjectContent(body.GetType(), body, formatter));
		}

		/// <summary>Set the body content of the HTTP request.</summary>
		/// <param name="body">Value to serialize into the HTTP body content.</param>
		/// <param name="formatter">Media type formatter with which to format the request body format.</param>
		/// <param name="mediaType">HTTP media type (or <c>null</c> for the <paramref name="formatter"/>'s default).</param>
		/// <returns>Returns the request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithBody<T>(T body, MediaTypeFormatter formatter, string mediaType = null)
		{
			return WithBodyContent(new ObjectContent<T>(body, formatter, mediaType));
		}

		/// <summary>Set the body content of the HTTP request.</summary>
		/// <param name="body">Formatted HTTP body content.</param>
		/// <returns>Returns the request builder for chaining.</returns>
		public FluentHttpRequestBuilder WithBodyContent(HttpContent body)
		{
			_httpBody = body;
			return this;
		}
		

		public async Task<T> Return<T>()
		{
			var response = await ReturnAsResponse<T>();
			return response.Data;
		}

		public async Task<FluentHttpResponse<T>> ReturnAsResponse<T>()
		{
			var response = await _fluentHttpClient.Send<T>(this);
			response.Data = await response.RawResponse.Content.ReadAsAsync<T>(_fluentHttpClient.Formatters);
			return response;
		}

		public FluentHttpRequest Build()
		{
			var httpRequest = new HttpRequestMessage(HttpMethod, Uri);

			if (_httpBody != null)
				httpRequest.Content = _httpBody;

			var fluentRequest = new FluentHttpRequest(httpRequest);
			return fluentRequest;
		}
	}

}