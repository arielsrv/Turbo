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
        var expectedResponse =
            new GetUserResponse(Guid.NewGuid(), "John Doe", "john@example.com", DateTime.UtcNow, null);

        _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reactiveMediator.Send(command).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.Name, result.Name);
        Assert.Equal(expectedResponse.Email, result.Email);

        _mockMediator.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_MediatorThrowsException_PropagatesException()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserRequest("John Doe", "john@example.com"));
        var expectedException = new InvalidOperationException("Handler not found");

        _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reactiveMediator.Send(command).ToTask());

        Assert.Equal("Handler not found", exception.Message);
        _mockMediator.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_WithCancellationToken_PassesTokenToMediator()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserRequest("John Doe", "john@example.com"));
        var expectedResponse =
            new GetUserResponse(Guid.NewGuid(), "John Doe", "john@example.com", DateTime.UtcNow, null);
        var cancellationToken = new CancellationToken();

        _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reactiveMediator.Send(command).ToTask(cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockMediator.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publish_ValidNotification_ReturnsUnit()
    {
        // Arrange
        var notification = new object();

        _mockMediator.Setup(m => m.Publish(notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _reactiveMediator.Publish(notification).ToTask();

        // Assert
        Assert.Equal(Unit.Default, result);
        _mockMediator.Verify(m => m.Publish(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publish_MediatorThrowsException_PropagatesException()
    {
        // Arrange
        var notification = new object();
        var expectedException = new InvalidOperationException("Notification handler error");

        _mockMediator.Setup(m => m.Publish(notification, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reactiveMediator.Publish(notification).ToTask());

        Assert.Equal("Notification handler error", exception.Message);
        _mockMediator.Verify(m => m.Publish(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Publish_WithCancellationToken_PassesTokenToMediator()
    {
        // Arrange
        var notification = new object();
        var cancellationToken = new CancellationToken();

        _mockMediator.Setup(m => m.Publish(notification, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _reactiveMediator.Publish(notification).ToTask(cancellationToken);

        // Assert
        Assert.Equal(Unit.Default, result);
        _mockMediator.Verify(m => m.Publish(notification, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_BoolResponse_ReturnsCorrectType()
    {
        // Arrange
        var command = new DeleteUserCommand(Guid.NewGuid());
        var expectedResponse = true;

        _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reactiveMediator.Send(command).ToTask();

        // Assert
        Assert.Equal(expectedResponse, result);
        _mockMediator.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_NullableResponse_ReturnsCorrectType()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.NewGuid());
        GetUserResponse? expectedResponse = null;

        _mockMediator.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reactiveMediator.Send(query).ToTask();

        // Assert
        Assert.Null(result);
        _mockMediator.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }
}