using System.Net;
using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Turbo.API.Commands;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Models;
using Turbo.API.Queries;
using Turbo.API.Repositories;
using Xunit;

namespace Turbo.API.Tests.Controllers;

public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IReactiveMediator> _mockMediator;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _mockMediator = new Mock<IReactiveMediator>();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the real mediator with our mock
                services.AddSingleton(_mockMediator.Object);
            });
        });
    }

    [Fact]
    public async Task CreateUser_ValidRequest_ReturnsCreatedResponse()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateUserRequest("John Doe", "john@example.com");
        var expectedResponse = new UserResponse(Guid.NewGuid(), "John Doe", "john@example.com", DateTime.UtcNow, null);
        
        _mockMediator.Setup(m => m.Send<UserResponse>(It.IsAny<CreateUserCommand>(), default))
            .Returns(Observable.Return(expectedResponse));

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.Name, result.Name);
        Assert.Equal(expectedResponse.Email, result.Email);
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateUserRequest("John Doe", "john@example.com");
        var exception = new InvalidOperationException("User with email john@example.com already exists");
        
        _mockMediator.Setup(m => m.Send<UserResponse>(It.IsAny<CreateUserCommand>(), default))
            .Returns(Observable.Throw<UserResponse>(exception));

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("User with email john@example.com already exists", errorContent);
    }

    [Fact]
    public async Task GetUserById_ExistingUser_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var expectedResponse = new UserResponse(userId, "John Doe", "john@example.com", DateTime.UtcNow, null);
        
        _mockMediator.Setup(m => m.Send<UserResponse?>(It.IsAny<GetUserByIdQuery>(), default))
            .Returns(Observable.Return<UserResponse?>(expectedResponse));

        // Act
        var response = await client.GetAsync($"/api/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
    }

    [Fact]
    public async Task GetUserById_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        
        _mockMediator.Setup(m => m.Send<UserResponse?>(It.IsAny<GetUserByIdQuery>(), default))
            .Returns(Observable.Return<UserResponse?>(null));

        // Act
        var response = await client.GetAsync($"/api/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var users = new List<UserResponse>
        {
            new(Guid.NewGuid(), "John Doe", "john@example.com", DateTime.UtcNow, null),
            new(Guid.NewGuid(), "Jane Smith", "jane@example.com", DateTime.UtcNow, null)
        };
        var expectedResponse = new UsersResponse(users);
        
        _mockMediator.Setup(m => m.Send<UsersResponse>(It.IsAny<GetAllUsersQuery>(), default))
            .Returns(Observable.Return(expectedResponse));

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UsersResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Users.Count());
    }

    [Fact]
    public async Task UpdateUser_ValidRequest_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("John Updated", "john.updated@example.com");
        var expectedResponse = new UserResponse(userId, "John Updated", "john.updated@example.com", DateTime.UtcNow, DateTime.UtcNow);
        
        _mockMediator.Setup(m => m.Send<UserResponse>(It.IsAny<UpdateUserCommand>(), default))
            .Returns(Observable.Return(expectedResponse));

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{userId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(result);
        Assert.Equal("John Updated", result.Name);
        Assert.Equal("john.updated@example.com", result.Email);
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        
        _mockMediator.Setup(m => m.Send<bool>(It.IsAny<DeleteUserCommand>(), default))
            .Returns(Observable.Return(true));

        // Act
        var response = await client.DeleteAsync($"/api/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteUser_NonExistingUser_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        
        _mockMediator.Setup(m => m.Send<bool>(It.IsAny<DeleteUserCommand>(), default))
            .Returns(Observable.Return(false));

        // Act
        var response = await client.DeleteAsync($"/api/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserByEmail_ExistingUser_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = "john@example.com";
        var expectedResponse = new UserResponse(Guid.NewGuid(), "John Doe", email, DateTime.UtcNow, null);
        
        _mockMediator.Setup(m => m.Send<UserResponse?>(It.IsAny<GetUserByEmailQuery>(), default))
            .Returns(Observable.Return<UserResponse?>(expectedResponse));

        // Act
        var response = await client.GetAsync($"/api/users/email/{email}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }
} 