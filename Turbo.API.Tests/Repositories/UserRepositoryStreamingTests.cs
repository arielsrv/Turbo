using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Turbo.API.Models;
using Turbo.API.Repositories;

namespace Turbo.API.Tests.Repositories;

/// <summary>
///     GetAllAsync used to return IObservable&lt;IEnumerable&lt;User&gt;&gt; — a stream of exactly one
///     list, which is a Task wearing a costume. These pin the streaming contract instead.
/// </summary>
public class UserRepositoryStreamingTests
{
    private readonly InMemoryUserRepository _repository = new();

    private async Task SeedAsync(params string[] names)
    {
        foreach (var name in names)
            await _repository.AddAsync(new User(name, $"{name}@example.com")).ToTask();
    }

    [Fact]
    public async Task GetAllAsync_WithUsers_EmitsOneItemPerUser()
    {
        await SeedAsync("ana", "beto", "carla");

        var emitted = await _repository.GetAllAsync().ToList().ToTask();

        Assert.Equal(3, emitted.Count);
        Assert.Equal(["ana", "beto", "carla"], emitted.Select(user => user.Name));
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepository_CompletesWithoutEmitting()
    {
        var emitted = await _repository.GetAllAsync().ToList().ToTask();

        Assert.Empty(emitted);
    }

    [Fact]
    public async Task GetAllAsync_IsCold_SoEachSubscriptionSeesCurrentState()
    {
        await SeedAsync("ana");
        var stream = _repository.GetAllAsync();

        var first = await stream.ToList().ToTask();
        await SeedAsync("beto");
        var second = await stream.ToList().ToTask();

        Assert.Single(first);
        Assert.Equal(2, second.Count);
    }

    [Fact]
    public async Task GetAllAsync_ComposesWithRxOperators()
    {
        await SeedAsync("ana", "beto", "carla");

        // The whole point of streaming: filtering happens per item, not on a materialised list.
        var names = await _repository.GetAllAsync()
            .Where(user => user.Name.StartsWith('b'))
            .Select(user => user.Name)
            .ToList().ToTask();

        Assert.Equal(["beto"], names);
    }
}
