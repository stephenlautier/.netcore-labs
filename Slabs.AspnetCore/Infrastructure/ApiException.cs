using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace Slabs.AspnetCore.Infrastructure
{
	[Serializable]
	public class ApiException : Exception
	{
		public HttpStatusCode StatusCode { get; }
		public string ErrorCode { get; set; }
		public Dictionary<string, string> FieldErrors { get; set; }

		public ApiException(HttpStatusCode statusCode)
		{
			StatusCode = statusCode;
		}

		public ApiException(string message) : base(message)
		{
		}

		public ApiException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ApiException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}