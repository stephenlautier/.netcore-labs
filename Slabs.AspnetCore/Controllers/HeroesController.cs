using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Slabs.AspnetCore.Heroes;
using Slabs.AspnetCore.Infrastructure;

namespace Slabs.AspnetCore.Controllers
{
	[Route("api/[controller]")]
	public class HeroesController : Controller
	{
		private readonly HeroService _service;

		public HeroesController(HeroService service)
		{
			_service = service;
		}

		// GET api/heroes
		[HttpGet]
		public async Task<IEnumerable<Hero>> Get()
		{
			return await _service.GetAll();
		}

		// GET api/heroes/azmodan
		[HttpGet("{key}")]
		public async Task<IActionResult> Get(string key)
		{
			var hero = await _service.GetByKey(key);
			if (hero == null)
				return NotFound();
			return Ok(hero);
		}

		// PUT api/heroes/azmodan
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] [Required] Hero input)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				await _service.Add(input);
			}
			catch (ApiException ex)
			{
				return BadRequest(new
				{
					ex.ErrorCode
				});
			}
			return Ok(input);
		}
	}
}