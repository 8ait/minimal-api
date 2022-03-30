namespace MinimapApiExample.ApiExtentions;

public interface IHandler<in TIn>
{
    public Task<object?> RunAsync(TIn action);
}

public abstract class Handler<TIn, TOut> : IHandler<TIn>
    where TIn: IRequest<TOut>
{
    public abstract Task<TOut?> Run(TIn action);

    public async Task<object?> RunAsync(TIn action)
    {
        var result = await Run(action);
        return result;
    }
}