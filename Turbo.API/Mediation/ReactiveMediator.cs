using System.Reactive.Linq;
using MediatR;
using Unit = System.Reactive.Unit;

namespace Turbo.API.Mediation;

public interface IReactiveMediator
{
    IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request);
    IObservable<Unit> Publish(object notification);
}

public class ReactiveMediator(IMediator mediator) : IReactiveMediator
{
    public IObservable<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        return Observable.FromAsync(ct => mediator.Send(request, ct));
    }

    public IObservable<Unit> Publish(object notification)
    {
        return Observable.FromAsync(async ct =>
        {
            await mediator.Publish(notification, ct);
            return Unit.Default;
        });
    }
}