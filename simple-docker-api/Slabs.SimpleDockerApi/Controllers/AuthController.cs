using Microsoft.AspNetCore.Mvc;
using System;

namespace Slabs.SimpleDockerApi.Controllers
{
	[Route("api/[controller]")]
	public class AuthController : Controller
	{

		// POST api/login
		[HttpPost("login")]
		public IActionResult Post([FromBody]LoginInputModel value)
		{
			var result = new LoginResponse
			{
				ExpiresIn = 5,
				TokenType = "Basic",
				AccessToken = Guid.NewGuid().ToString()
			};

			return Ok(result);
		}

	}

	public class LoginInputModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}
	public class LoginResponse
	{
		public string AccessToken { get; set; }
		public int ExpiresIn { get; set; }
		public string TokenType { get; set; }
	}
}