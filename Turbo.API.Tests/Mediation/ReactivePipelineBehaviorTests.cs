using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Turbo.API.Mediation;

namespace Turbo.API.Tests.Mediation;

public class ReactivePipelineBehaviorTests
{
    private record Ping(string Value);

    /// <summary>Appends a tag to the response so ordering is observable in the result itself.</summary>
    private sealed class TaggingBehavior(string tag) : IReactivePipelineBehavior<Ping, string>
    {
        public IObservable<string> Handle(Ping request, Func<IObservable<string>> next)
        {
            return next().Select(response => $"{response}>{tag}");
        }
    }

    private sealed class EchoHandler : IReactiveRequestHandler<Ping, string>
    {
        public IObservable<string> Handle(Ping request) => Observable.Return(request.Value);
    }

    private static ReactiveMediator MediatorWith(params IReactivePipelineBehavior<Ping, string>[] behaviors)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IReactiveRequestHandler<Ping, string>>(new EchoHandler());
        foreach (var behavior in behaviors) services.AddSingleton(behavior);
        return new ReactiveMediator(services.BuildServiceProvider());
    }

    [Fact]
    public async Task Send_WithSingleBehavior_BehaviorWrapsHandlerResponse()
    {
        var mediator = MediatorWith(new TaggingBehavior("outer"));

        var result = await mediator.Send<Ping, string>(new Ping("core")).ToTask();

        Assert.Equal("core>outer", result);
    }

    [Fact]
    public async Task Send_WithMultipleBehaviors_FirstRegisteredIsOutermost()
    {
        var mediator = MediatorWith(new TaggingBehavior("first"), new TaggingBehavior("second"));

        var result = await mediator.Send<Ping, string>(new Ping("core")).ToTask();

        // "second" is nearest the handler, so it tags first; "first" wraps it.
        Assert.Equal("core>second>first", result);
    }

    [Fact]
    public async Task Send_WithNoBehaviors_ReturnsHandlerResponseUnchanged()
    {
        var mediator = MediatorWith();

        var result = await mediator.Send<Ping, string>(new Ping("core")).ToTask();

        Assert.Equal("core", result);
    }

    /// <summary>
    ///     The reason this pipeline exists instead of MediatR's: a behavior must observe the stream
    ///     executing, not merely being constructed. Nothing may run before subscription.
    /// </summary>
    [Fact]
    public async Task Send_BeforeSubscription_NeitherHandlerNorBehaviorRuns()
    {
        var events = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IReactiveRequestHandler<Ping, string>>(
            new RecordingHandler(events));
        services.AddSingleton<IReactivePipelineBehavior<Ping, string>>(
            new RecordingBehavior(events));
        var mediator = new ReactiveMediator(services.BuildServiceProvider());

        var stream = mediator.Send<Ping, string>(new Ping("core"));
        Assert.Empty(events); // cold: building the pipeline must not execute it

        await stream.ToTask();

        Assert.Equal(["behavior:enter", "handler", "behavior:completed"], events);
    }

    private sealed class RecordingHandler(List<string> events) : IReactiveRequestHandler<Ping, string>
    {
        public IObservable<string> Handle(Ping request) =>
            Observable.Defer(() =>
            {
                events.Add("handler");
                return Observable.Return(request.Value);
            });
    }

    private sealed class RecordingBehavior(List<string> events) : IReactivePipelineBehavior<Ping, string>
    {
        public IObservable<string> Handle(Ping request, Func<IObservable<string>> next) =>
            Observable.Defer(() =>
            {
                events.Add("behavior:enter");
                return next().Finally(() => events.Add("behavior:completed"));
            });
    }
}
