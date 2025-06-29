using MediatR;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace Turbo.API.Mediation;

public interface IReactiveMediator
{
    IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    IObservable<System.Reactive.Unit> Publish(object notification, CancellationToken cancellationToken = default);
}

public class ReactiveMediator : IReactiveMediator
{
    private readonly IMediator _mediator;

    public ReactiveMediator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(() => _mediator.Send(request, cancellationToken));
    }

    public IObservable<System.Reactive.Unit> Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Observable.FromAsync(async () =>
        {
            await _mediator.Publish(notification, cancellationToken);
            return System.Reactive.Unit.Default;
        });
    }
}