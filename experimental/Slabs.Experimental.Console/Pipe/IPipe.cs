using System.Threading.Tasks;

namespace Slabs.Experimental.ConsoleClient.Pipe
{
	public interface IPipe
	{
		Task<object> Invoke(PipelineContext context);
	}
	public interface IPipe<TContext, TResult>
	{
		Task<TResult> Invoke(TContext context);
	}

	public delegate Task<object> PipeDelegate(PipelineContext context);
	public delegate Task<TResult> PipeDelegate<in TContext, TResult>(TContext context);
}