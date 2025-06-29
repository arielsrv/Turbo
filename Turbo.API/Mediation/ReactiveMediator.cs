using System.Reactive.Linq;
using MediatR;
using Unit = System.Reactive.Unit;

namespace Turbo.API.Mediation;

public interface IReactiveMediator
{
    IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    IObservable<Unit> Publish(object notification, CancellationToken cancellationToken = default);
}

public class ReactiveMediator : IReactiveMediator
{
    private readonly IMediator _mediator;

    public ReactiveMediator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(() => _mediator.Send(request, cancellationToken));
    }

    public IObservable<Unit> Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(async () =>
        {
            await _mediator.Publish(notification, cancellationToken);
            return Unit.Default;
        });
    }
}