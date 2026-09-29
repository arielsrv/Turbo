using System.Reactive.Linq;
using System.Reactive.Subjects;
using Turbo.API.Streaming;

namespace Turbo.API.Tests.Streaming;

public class ObservableStreamingExtensionsTests
{
    [Fact]
    public async Task ToAsyncEnumerable_EmitsEveryItemInOrder()
    {
        var source = new[] { 1, 2, 3 }.ToObservable();

        var received = new List<int>();
        await foreach (var item in source.ToAsyncEnumerable()) received.Add(item);

        Assert.Equal([1, 2, 3], received);
    }

    [Fact]
    public async Task ToAsyncEnumerable_EmptySource_YieldsNothing()
    {
        var received = new List<int>();
        await foreach (var item in Observable.Empty<int>().ToAsyncEnumerable()) received.Add(item);

        Assert.Empty(received);
    }

    [Fact]
    public async Task ToAsyncEnumerable_SourceFails_ThrowsOnEnumeration()
    {
        var source = Observable.Throw<int>(new InvalidOperationException("boom"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in source.ToAsyncEnumerable()) { }
        });

        Assert.Equal("boom", exception.Message);
    }

    [Fact]
    public async Task ToAsyncEnumerable_SourceFailsMidStream_YieldsItemsBeforeTheFailure()
    {
        var source = new[] { 1, 2 }.ToObservable()
            .Concat(Observable.Throw<int>(new InvalidOperationException("boom")));

        var received = new List<int>();
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var item in source.ToAsyncEnumerable()) received.Add(item);
        });

        Assert.Equal([1, 2], received);
    }

    [Fact]
    public async Task ToAsyncEnumerable_WhenCancelled_StopsEnumerating()
    {
        using var cts = new CancellationTokenSource();
        var received = new List<int>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in Observable.Range(1, 1_000).ToAsyncEnumerable(cancellationToken: cts.Token))
            {
                received.Add(item);
                if (received.Count == 2) await cts.CancelAsync();
            }
        });

        Assert.Equal([1, 2], received);
    }

    /// <summary>
    ///     A cold synchronous source runs entirely inside Subscribe. If the bridge subscribes on the
    ///     consumer's own thread, the producer fills the bounded channel and blocks before the read
    ///     loop ever starts — a deadlock. The repository is exactly such a source.
    /// </summary>
    [Fact]
    public async Task ToAsyncEnumerable_SynchronousSourceLargerThanCapacity_StreamsEveryItem()
    {
        var consumer = Task.Run(async () =>
        {
            var received = new List<int>();
            await foreach (var item in Observable.Range(1, 500).ToAsyncEnumerable(8)) received.Add(item);
            return received;
        });

        var finished = await Task.WhenAny(consumer, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.True(ReferenceEquals(finished, consumer), "the bridge deadlocked on a synchronous source");

        Assert.Equal(500, (await consumer).Count);
    }

    [Fact]
    public async Task ToAsyncEnumerable_UnsubscribesWhenTheConsumerStopsEarly()
    {
        var unsubscribed = new TaskCompletionSource();
        var source = Observable.Create<int>(observer =>
        {
            observer.OnNext(1);
            observer.OnNext(2);
            return () => unsubscribed.TrySetResult();
        });

        await foreach (var _ in source.ToAsyncEnumerable()) break;

        // SubscribeOn defers disposal to the scheduler, so await it rather than racing it.
        var finished = await Task.WhenAny(unsubscribed.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.True(ReferenceEquals(finished, unsubscribed.Task), "the source was never unsubscribed");
    }
}
