using System.Reactive.Linq;
using MediatR;
using Unit = System.Reactive.Unit;

namespace Turbo.API.Mediation;

public interface IReactiveMediator
{
    IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    IObservable<Unit> Publish(object notification, CancellationToken cancellationToken = default);
}

public class ReactiveMediator(IMediator mediator) : IReactiveMediator
{
    public IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(() => mediator.Send(request, cancellationToken));
    }

    public IObservable<Unit> Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(async () =>
        {
            await mediator.Publish(notification, cancellationToken);
            return Unit.Default;
        });
    }
}