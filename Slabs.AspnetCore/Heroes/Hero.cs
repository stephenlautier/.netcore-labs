using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Slabs.AspnetCore.Heroes
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Hero
	{
		private string DebuggerDisplay => $"[{Key}] Name: '{Name}', Title: {Title}";

		[Required]
		public string Key { get; set; }

		[Required]
		public string Name { get; set; }
		public string Title { get; set; }

		public override string ToString() => DebuggerDisplay;
	}
}