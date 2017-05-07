using Microsoft.AspNetCore.Mvc;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Slabs.SimpleDockerApi.Controllers
{
	[Route("api/[controller]")]
	public class AppController : Controller
	{
		// GET: api/values
		[HttpGet]
		public dynamic Get()
		{
			return new
			{
				LocalIp = $"{HttpContext.Connection.LocalIpAddress}:{HttpContext.Connection.LocalPort}",
				RemoteIp = $"{HttpContext.Connection.RemoteIpAddress}:{HttpContext.Connection.RemotePort}",
				Issuer = Environment.MachineName
			};
		}
	}
}
