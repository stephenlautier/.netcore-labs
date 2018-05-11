using Microsoft.AspNetCore.Mvc;
using Slabs.AspnetCore.Infrastructure;
using Slabs.AspnetCore.Tenancy;

namespace Slabs.AspnetCore.Controllers
{
	[Route("api/[controller]")]
	public class AdminController : Controller
	{
		private readonly ITenant _tenant;
		private readonly RequestContext _requestContext;

		public AdminController(ITenant tenant, RequestContext requestContext)
		{
			_tenant = tenant;
			_requestContext = requestContext;
		}

		// GET api/admin/tenant
		[HttpGet("tenant")]
		public ITenant GetTenant() => _tenant;

		// GET api/admin/tenant
		[HttpGet("request")]
		public RequestContext GetRequestContext() => _requestContext;
	}
}
