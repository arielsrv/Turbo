using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MediatR;

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
    IObservable<System.Reactive.Unit> Publish(object notification);

    /// <summary>
    /// Publishes a notification and returns a Task directly (convenience method for controllers).
    /// </summary>
    Task PublishAsync(object notification, CancellationToken cancellationToken = default);
}

public class ReactiveMediator(IMediator mediator) : IReactiveMediator
{
    public IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        return Observable.FromAsync(cancellationToken => mediator.Send(request, cancellationToken));
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return Send(request).ToTask(cancellationToken);
    }

    public IObservable<System.Reactive.Unit> Publish(object notification)
    {
        return Observable.FromAsync(async cancellationToken =>
        {
            await mediator.Publish(notification, cancellationToken);
            return System.Reactive.Unit.Default;
        });
    }

    public Task PublishAsync(object notification, CancellationToken cancellationToken = default)
    {
        return Publish(notification).ToTask(cancellationToken);
    }
}