using System.Collections.Generic;

namespace Slabs.Experimental.ConsoleClient
{
	public interface ISessionState
	{
		T Get<T>(string key);
		void Set<T>(string key, T value);
	}

	public class SessionState : ISessionState
	{
		private readonly Dictionary<string, object> _data;

		public SessionState()
		{
			_data = new Dictionary<string, object>();
		}

		public T Get<T>(string key)
		{
			var exists = _data.TryGetValue(key, out object result);

			if (exists)
				return (T)result;
			throw new KeyNotFoundException($"Key '{key}' was not found!");
		}

		public void Set<T>(string key, T value) => _data[key] = value;
	}
}
