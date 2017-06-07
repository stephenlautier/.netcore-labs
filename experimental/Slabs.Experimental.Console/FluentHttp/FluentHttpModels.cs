using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.FluentHttp
{
	public delegate Task<IFluentHttpResponse> FluentHttpRequestDelegate(FluentHttpRequest request);

	public class FluentHttpRequest
	{
		public string Method { get; set; }
		public string Url { get; set; }
		public object Data { get; set; }
	}

	public interface IFluentHttpResponse
	{
		HttpStatusCode StatusCode { get; }
		bool IsSuccessStatusCode { get; }
		void EnsureSuccessStatusCode();
		string ReasonPhrase { get; }
		HttpResponseHeaders Headers { get; }
		IDictionary<object, object> Items { get; set; }
	}

	public class FluentHttpResponse<T> : IFluentHttpResponse
	{
		private readonly HttpResponseMessage _response;

		public FluentHttpResponse(HttpResponseMessage response)
		{
			_response = response;
		}

		public T Data { get; set; }

		public HttpStatusCode StatusCode => _response.StatusCode;
		public bool IsSuccessStatusCode => _response.IsSuccessStatusCode;
		public void EnsureSuccessStatusCode() => _response.EnsureSuccessStatusCode();
		public string ReasonPhrase => _response.ReasonPhrase;
		public HttpResponseHeaders Headers => _response.Headers;
		/// <summary>
		/// Gets or sets a key/value collection that can be used to share data within the scope of this request/response.
		/// </summary>
		public IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
	}

	
}
