using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Channels;

namespace Turbo.API.Streaming;

public static class ObservableStreamingExtensions
{
    /// <summary>
    ///     Bridges a push-based <see cref="IObservable{T}" /> to the pull-based
    ///     <see cref="IAsyncEnumerable{T}" /> that ASP.NET Core serialises incrementally.
    /// </summary>
    /// <remarks>
    ///     Rx only offers <c>ToEnumerable</c>, which blocks the calling thread — unusable on a request
    ///     path. A bounded channel is where push meets pull: once <paramref name="capacity" /> items are
    ///     waiting, the producer is made to wait, so a fast source cannot grow an unbounded buffer
    ///     against a slow client. That wait is synchronous, which suits the cold, in-process sources
    ///     this API streams, because the producer is moved onto the thread pool before it runs.
    /// </remarks>
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
        this IObservable<T> source,
        int capacity = 32,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        });

        // Subscribe off the consumer's thread. A cold synchronous source runs to completion inside
        // Subscribe, so on this thread it would fill the channel and block before the read loop below
        // ever starts. On the pool the producer blocks there instead, which is the backpressure we want.
        using var subscription = source.SubscribeOn(TaskPoolScheduler.Default).Subscribe(
            item =>
            {
                if (channel.Writer.TryWrite(item)) return;
                // Channel is full: block the producer until the consumer drains one. This is the
                // backpressure signal travelling back up the stream.
                channel.Writer.WriteAsync(item, cancellationToken).AsTask().GetAwaiter().GetResult();
            },
            error => channel.Writer.TryComplete(error),
            () => channel.Writer.TryComplete());

        await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            // ReadAllAsync drains whatever is already buffered without re-checking the token, so a
            // cancelled client would still be served the rest of the buffer. Check per item instead.
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }
}
