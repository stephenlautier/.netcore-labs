using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Slabs.AspnetCore.Infrastructure;

namespace Slabs.AspnetCore.Heroes
{
	public interface IHeroService
	{
		Task<ICollection<Hero>> GetAll();
		Task<Hero> GetByKey(string key);
		Task Add(Hero input);
	}

	public class HeroService : IHeroService
	{
		private readonly ICollection<Hero> _data = new List<Hero>
		{
			new Hero
			{
				Key = "azmodan",
				Name = "Azmodan",
				Title = "Lord of Sins"
			},
			new Hero
			{
				Key = "rexxar",
				Name = "Rexxar",
				Title = "Champion of the Horde"
			},
			new Hero
			{
				Key = "maiev",
				Name = "Maiev",
				Title = "The Warden"
			},
			new Hero
			{
				Key = "malthael",
				Name = "Malthael",
				Title = "Aspect of Death"
			},
			new Hero
			{
				Key = "garrosh",
				Name = "Garrosh",
				Title = "Son of Hellscream"
			},
		};

		public Task<ICollection<Hero>> GetAll() => Task.FromResult(_data);

		public Task<Hero> GetByKey(string key) => Task.FromResult(_data.FirstOrDefault(x => x.Key == key));

		public async Task Add(Hero input)
		{
			var result = await GetByKey(input.Key);
			if (result != null)
			{
				throw new ApiException(HttpStatusCode.BadRequest)
				{
					ErrorCode = "error.hero.key-already-exists"
				};
			}

			_data.Add(input);
		}
	}

	// use only for tenency sample
	public class SampleHeroService : IHeroService
	{
		private readonly ICollection<Hero> _data = new List<Hero>
		{
			new Hero{Key = "garrosh", Name = "Garrosh", Title = "Son of Hellscream"},
		};

		public Task<ICollection<Hero>> GetAll() => Task.FromResult(_data);

		public Task<Hero> GetByKey(string key) => Task.FromResult(_data.FirstOrDefault(x => x.Key == key));

		public async Task Add(Hero input)
		{
			var result = await GetByKey(input.Key);
			if (result != null)
			{
				throw new ApiException(HttpStatusCode.BadRequest)
				{
					ErrorCode = "error.hero.key-already-exists"
				};
			}
			_data.Add(input);
		}
	}
}