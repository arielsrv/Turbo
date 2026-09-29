using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Turbo.API.DTOs;

namespace Turbo.API.Tests.Controllers;

/// <summary>
///     Exercises the real pipeline end to end: no mocked mediator, so the stream actually flows
///     from the repository through the handler and out over HTTP.
/// </summary>
public class UsersStreamEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private async Task<HttpClient> SeededClientAsync(params string[] names)
    {
        var client = factory.CreateClient();
        foreach (var name in names)
        {
            var response = await client.PostAsJsonAsync("/api/Users",
                new CreateUserRequest(name, $"{name}-{Guid.NewGuid():N}@example.com"));
            response.EnsureSuccessStatusCode();
        }

        return client;
    }

    [Fact]
    public async Task GetStream_ReturnsEveryUserAsAJsonArray()
    {
        var client = await SeededClientAsync("ana", "beto", "carla");

        var response = await client.GetAsync("/api/Users/stream");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<GetUserResponse>>();
        Assert.NotNull(users);
        Assert.Contains(users, user => user.Name == "ana");
        Assert.Contains(users, user => user.Name == "beto");
        Assert.Contains(users, user => user.Name == "carla");
    }

    [Fact]
    public async Task GetStream_ServesJson()
    {
        var client = await SeededClientAsync("ana");

        var response = await client.GetAsync("/api/Users/stream");

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetStream_StartsRespondingBeforeTheBodyIsComplete()
    {
        var client = await SeededClientAsync("ana", "beto");

        // HttpCompletionOption.ResponseHeadersRead proves the status line arrives before the payload
        // is buffered — the property a buffered endpoint cannot offer.
        var response = await client.GetAsync("/api/Users/stream", HttpCompletionOption.ResponseHeadersRead);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var body = await response.Content.ReadAsStreamAsync();
        Assert.True(body.CanRead);
    }
}
