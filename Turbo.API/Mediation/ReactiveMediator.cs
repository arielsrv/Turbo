using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace Turbo.API.Mediation;

public interface IReactiveMediator
{
    /// <summary>
    ///     Sends a request and returns an IObservable for reactive composition.
    /// </summary>
    IObservable<TResponse> Send<TRequest, TResponse>(TRequest request) where TRequest : notnull;

    /// <summary>
    ///     Sends a request and returns a Task directly (convenience method for controllers).
    /// </summary>
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : notnull;

    /// <summary>
    ///     Publishes a notification and returns an IObservable for reactive composition.
    /// </summary>
    IObservable<Unit> Publish<TNotification>(TNotification notification) where TNotification : notnull;

    /// <summary>
    ///     Publishes a notification and returns a Task directly (convenience method for controllers).
    /// </summary>
    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : notnull;
}

public class ReactiveMediator(IServiceProvider serviceProvider) : IReactiveMediator
{
    public IObservable<TResponse> Send<TRequest, TResponse>(TRequest request) where TRequest : notnull
    {
        var handler = serviceProvider.GetService<IReactiveRequestHandler<TRequest, TResponse>>();

        if (handler is null)
            throw new InvalidOperationException(
                $"No handler registered for {typeof(TRequest).Name}. " +
                $"Ensure IReactiveRequestHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> is registered.");

        return handler.Handle(request);
    }

    public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken = default) where TRequest : notnull
    {
        return Send<TRequest, TResponse>(request).ToTask(cancellationToken);
    }

    public IObservable<Unit> Publish<TNotification>(TNotification notification)
        where TNotification : notnull
    {
        var handlers = serviceProvider.GetServices<IReactiveNotificationHandler<TNotification>>();

        var observables = handlers.Select(h => h.Handle(notification));

        var enumerable = observables as IObservable<Unit>[] ?? observables.ToArray();
        return enumerable.Length != 0
            ? Observable.Merge(enumerable)
            : Observable.Return(Unit.Default);
    }

    public Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : notnull
    {
        return Publish(notification).ToTask(cancellationToken);
    }
}