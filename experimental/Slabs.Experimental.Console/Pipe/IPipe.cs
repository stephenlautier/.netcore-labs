using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	public interface IPipe
	{
		Task<object> Invoke(PipelineContext context);
	}

	public delegate Task<object> PipeDelegate(PipelineContext context);
}