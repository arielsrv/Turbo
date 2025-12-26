using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MediatR;
using Unit = System.Reactive.Unit;

namespace Turbo.API.Mediation;

public interface IReactiveMediator
{
    /// <summary>
    /// Sends a request and returns an IObservable for reactive composition.
    /// </summary>
    IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request);
    
    /// <summary>
    /// Sends a request and returns a Task directly (convenience method for controllers).
    /// </summary>
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Publishes a notification and returns an IObservable for reactive composition.
    /// </summary>
    IObservable<Unit> Publish(object notification);
    
    /// <summary>
    /// Publishes a notification and returns a Task directly (convenience method for controllers).
    /// </summary>
    Task PublishAsync(object notification, CancellationToken cancellationToken = default);
}

public class ReactiveMediator(IMediator mediator) : IReactiveMediator
{
    public IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        return Observable.FromAsync(ct => mediator.Send(request, ct));
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return Send(request).ToTask(cancellationToken);
    }

    public IObservable<Unit> Publish(object notification)
    {
        return Observable.FromAsync(async ct =>
        {
            await mediator.Publish(notification, ct);
            return Unit.Default;
        });
    }

    public Task PublishAsync(object notification, CancellationToken cancellationToken = default)
    {
        return Publish(notification).ToTask(cancellationToken);
    }
}