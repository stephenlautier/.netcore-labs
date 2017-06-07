using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.FluentHttp
{
	public interface IFluentHttpMiddleware
	{
		Task<IFluentHttpResponse> Invoke(FluentHttpRequest request);
	}
}