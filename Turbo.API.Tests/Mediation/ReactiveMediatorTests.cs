using System.Reactive.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Turbo.API.Commands;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Queries;
using Unit = System.Reactive.Unit;

namespace Turbo.API.Tests.Mediation;

public class ReactiveMediatorTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly ReactiveMediator _reactiveMediator;

    public ReactiveMediatorTests()
    {
        _mockMediator = new Mock<IMediator>();
        _reactiveMediator = new ReactiveMediator(_mockMediator.Object);

        // Setup service provider for testing
        var services = new ServiceCollection();
        services.AddSingleton(_mockMediator.Object);
        services.BuildServiceProvider();
    }

    [Fact]
    public async Task Send_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserRequest("John Doe", "john@example.com"));
        var expectedResponse = new UserResponse(Guid.NewGuid(), "John Doe", "john@example.com", DateTime.UtcNow, null);

        _mockMediator.Setup(m => m.Send(command, CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reactiveMediator.Send(command).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.Name, result.Name);
        Assert.Equal(expectedResponse.Email, result.Email);

        _mockMediator.Verify(m => m.Send(command, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Send_MediatorThrowsException_PropagatesException()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserRequest("John Doe", "john@example.com"));
        var expectedException = new InvalidOperationException("Handler not found");

        _mockMediator.Setup(m => m.Send(command, CancellationToken.None))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reactiveMediator.Send(command).ToTask());

        Assert.Equal("Handler not found", exception.Message);
        _mockMediator.Verify(m => m.Send(command, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Send_WithCancellationToken_PassesTokenToMediator()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserRequest("John Doe", "john@example.com"));
        var expectedResponse = new UserResponse(Guid.NewGuid(), "John Doe", "john@example.com", DateTime.UtcNow, null);
        var cancellationToken = CancellationToken.None;

        _mockMediator.Setup(m => m.Send(command, cancellationToken))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reactiveMediator.Send(command, cancellationToken).ToTask(cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMediator.Verify(m => m.Send(command, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Publish_ValidNotification_ReturnsUnit()
    {
        // Arrange
        var notification = new object();

        _mockMediator.Setup(m => m.Publish(notification, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _reactiveMediator.Publish(notification).ToTask();

        // Assert
        Assert.Equal(Unit.Default, result);
        _mockMediator.Verify(m => m.Publish(notification, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Publish_MediatorThrowsException_PropagatesException()
    {
        // Arrange
        var notification = new object();
        var expectedException = new InvalidOperationException("Notification handler error");

        _mockMediator.Setup(m => m.Publish(notification, CancellationToken.None))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reactiveMediator.Publish(notification).ToTask());

        Assert.Equal("Notification handler error", exception.Message);
        _mockMediator.Verify(m => m.Publish(notification, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Publish_WithCancellationToken_PassesTokenToMediator()
    {
        // Arrange
        var notification = new object();
        var cancellationToken = CancellationToken.None;

        _mockMediator.Setup(m => m.Publish(notification, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _reactiveMediator.Publish(notification, cancellationToken).ToTask(cancellationToken);

        // Assert
        Assert.Equal(Unit.Default, result);
        _mockMediator.Verify(m => m.Publish(notification, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Send_BoolResponse_ReturnsCorrectType()
    {
        // Arrange
        var command = new DeleteUserCommand(Guid.NewGuid());
        var expectedResponse = true;

        _mockMediator.Setup(m => m.Send(command, CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reactiveMediator.Send(command).ToTask();

        // Assert
        Assert.Equal(expectedResponse, result);
        _mockMediator.Verify(m => m.Send(command, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Send_NullableResponse_ReturnsCorrectType()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.NewGuid());
        UserResponse? expectedResponse = null;

        _mockMediator.Setup(m => m.Send(query, CancellationToken.None))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reactiveMediator.Send(query).ToTask();

        // Assert
        Assert.Null(result);
        _mockMediator.Verify(m => m.Send(query, CancellationToken.None), Times.Once);
    }
}