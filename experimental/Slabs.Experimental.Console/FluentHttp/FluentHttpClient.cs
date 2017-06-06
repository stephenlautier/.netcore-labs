using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Slabs.Experimental.ConsoleClient.FluentHttp
{
	public class FluentHttpClient
	{
		private readonly HttpClient _httpClient;
		public string Identifier { get; }
		public string BaseUrl { get; }

		public FluentHttpClient(FluentHttpClientOptions options)
		{
			_httpClient = Configure(options);
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
			var response = await _httpClient.GetAsync(url);

			// todo: implement this better
			response.EnsureSuccessStatusCode();

			var dataResult = await ParseResult<T>(response);
			return dataResult;
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

	public class FluentHttpClientOptions
	{
		public string BaseUrl { get; set; }
		public TimeSpan Timeout { get; set; }
		public string Identifier { get; set; }
		public Dictionary<string, string> Headers { get; set; }
	}

	public class JsonContent : StringContent
	{
		public JsonContent(object obj) :
			base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
		{
		}
	}
}