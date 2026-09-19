using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Turbo.API.Mediation;

namespace Turbo.API.Tests.Mediation;

public class LoggingPipelineBehaviorTests
{
    private record Ping(string Value);

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception));
    }

    [Fact]
    public async Task Handle_WhenStreamCompletes_LogsRequestName()
    {
        var logger = new CapturingLogger<LoggingPipelineBehavior<Ping, string>>();
        var behavior = new LoggingPipelineBehavior<Ping, string>(logger);

        await behavior.Handle(new Ping("x"), () => Observable.Return("done")).ToTask();

        Assert.Contains(logger.Messages, m => m.Contains("Ping"));
    }

    /// <summary>
    ///     The point of an Rx-native pipeline: the behaviour must log when the stream finishes,
    ///     not when it is constructed. A Task-shaped pipeline would log immediately here.
    /// </summary>
    [Fact]
    public void Handle_BeforeStreamCompletes_LogsNothing()
    {
        var logger = new CapturingLogger<LoggingPipelineBehavior<Ping, string>>();
        var behavior = new LoggingPipelineBehavior<Ping, string>(logger);
        var gate = new Subject<string>();

        var stream = behavior.Handle(new Ping("x"), () => gate);
        Assert.Empty(logger.Messages); // not built yet

        using var subscription = stream.Subscribe();
        Assert.Empty(logger.Messages); // subscribed, but the stream has not finished

        gate.OnNext("value");
        Assert.Empty(logger.Messages); // a value flowed, still not complete

        gate.OnCompleted();
        Assert.Single(logger.Messages); // only now
    }

    [Fact]
    public async Task Handle_WhenStreamFails_LogsTheFailure()
    {
        var logger = new CapturingLogger<LoggingPipelineBehavior<Ping, string>>();
        var behavior = new LoggingPipelineBehavior<Ping, string>(logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new Ping("x"),
                () => Observable.Throw<string>(new InvalidOperationException("boom"))).ToTask());

        Assert.Contains(logger.Messages, m => m.Contains("failed"));
    }
}
