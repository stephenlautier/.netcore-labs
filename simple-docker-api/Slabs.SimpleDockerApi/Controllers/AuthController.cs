using Microsoft.AspNetCore.Mvc;
using System;

namespace Slabs.SimpleDockerApi.Controllers
{
	[Route("api/[controller]")]
	public class AuthController : Controller
	{

		// POST api/auth/login
		[HttpPost("[action]")]
		public IActionResult Login([FromBody]LoginInputModel value)
		{
			var result = new LoginResponse
			{
				ExpiresIn = 5,
				TokenType = "Basic",
				AccessToken = Guid.NewGuid().ToString()
			};

			return Ok(result);
		}
		// POST api/auth/keep-alive
		[HttpPatch("keep-alive")]
		public IActionResult KeepAlive([FromBody]string token)
		{
			var result = new LoginResponse
			{
				ExpiresIn = 10,
				TokenType = "Basic",
				AccessToken = token
			};

			return Ok(result);
		}

	}

	public class LoginInputModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}

	public class KeepAliveInputModel
	{
		public string Token { get; set; }
	}
	public class LoginResponse
	{
		public string AccessToken { get; set; }
		public int ExpiresIn { get; set; }
		public string TokenType { get; set; }
	}
}