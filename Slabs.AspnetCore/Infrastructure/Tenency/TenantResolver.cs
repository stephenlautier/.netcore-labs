using Microsoft.AspNetCore.Http;

namespace Slabs.AspnetCore.Infrastructure.Tenency
{
	public interface ITenantResolver<TTenant> 
		where TTenant : class
	{
		TTenant Resolve(HttpContext httpContext);
	}
}