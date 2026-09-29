namespace Turbo.API.Mediation;

/// <summary>
///     Wraps a request's execution to add cross-cutting behaviour: logging, validation, retries.
/// </summary>
/// <remarks>
///     <paramref name="next" /> is a factory rather than an already-built stream so a behaviour can
///     decide when — and how often — the rest of the pipeline runs. Returning
///     <c>Observable.Defer(() => next())</c> keeps the pipeline cold, so nothing executes until a
///     subscriber arrives and a behaviour can observe the stream running rather than merely being built.
/// </remarks>
public interface IReactivePipelineBehavior<in TRequest, TResponse>
{
    IObservable<TResponse> Handle(TRequest request, Func<IObservable<TResponse>> next);
}
