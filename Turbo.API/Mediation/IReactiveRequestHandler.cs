using System.Reactive;

namespace Turbo.API.Mediation;

public interface IReactiveRequestHandler<in TRequest, TResponse>
{
    IObservable<TResponse> Handle(TRequest request);
}

public interface IReactiveNotificationHandler<in TNotification>
{
    IObservable<Unit> Handle(TNotification notification);
}