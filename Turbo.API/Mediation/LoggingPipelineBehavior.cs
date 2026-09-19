using System.Diagnostics;
using System.Reactive.Linq;

namespace Turbo.API.Mediation;

/// <summary>
///     Logs how long each request took, measured over the stream's execution rather than its construction.
/// </summary>
public class LoggingPipelineBehavior<TRequest, TResponse>(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IReactivePipelineBehavior<TRequest, TResponse>
{
    public IObservable<TResponse> Handle(TRequest request, Func<IObservable<TResponse>> next)
    {
        // Defer so the clock starts on subscription, once per subscriber, and never at build time.
        return Observable.Defer(() =>
        {
            var name = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            return next()
                .Do(
                    _ => { },
                    exception => logger.LogError(exception, "{Request} failed after {Elapsed}ms", name,
                        stopwatch.ElapsedMilliseconds),
                    () => logger.LogInformation("{Request} completed in {Elapsed}ms", name,
                        stopwatch.ElapsedMilliseconds));
        });
    }
}
