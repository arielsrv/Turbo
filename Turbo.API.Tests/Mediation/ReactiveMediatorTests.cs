using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Turbo.API.Commands;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Queries;

namespace Turbo.API.Tests.Mediation;

public class ReactiveMediatorTests
{
    [Fact]
    public async Task Send_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserRequest("John Doe", "john@example.com"));
        var expectedResponse =
            new GetUserResponse(Guid.NewGuid(), "John Doe", "john@example.com", DateTime.UtcNow);

        var mockHandler = new Mock<IReactiveRequestHandler<CreateUserCommand, GetUserResponse>>();
        mockHandler.Setup(h => h.Handle(It.IsAny<CreateUserCommand>()))
            .Returns(Observable.Return(expectedResponse));

        var serviceProvider = CreateServiceProvider(mockHandler.Object);
        var reactiveMediator = new ReactiveMediator(serviceProvider);

        // Act
        var result = await reactiveMediator.Send<CreateUserCommand, GetUserResponse>(command).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.Name, result.Name);
        Assert.Equal(expectedResponse.Email, result.Email);

        mockHandler.Verify(h => h.Handle(It.IsAny<CreateUserCommand>()), Times.Once);
    }

    [Fact]
    public async Task Send_HandlerThrowsException_PropagatesException()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserRequest("John Doe", "john@example.com"));
        var expectedException = new InvalidOperationException("Handler error");

        var mockHandler = new Mock<IReactiveRequestHandler<CreateUserCommand, GetUserResponse>>();
        mockHandler.Setup(h => h.Handle(It.IsAny<CreateUserCommand>()))
            .Returns(Observable.Throw<GetUserResponse>(expectedException));

        var serviceProvider = CreateServiceProvider(mockHandler.Object);
        var reactiveMediator = new ReactiveMediator(serviceProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            reactiveMediator.Send<CreateUserCommand, GetUserResponse>(command).ToTask());

        Assert.Equal("Handler error", exception.Message);
        mockHandler.Verify(h => h.Handle(It.IsAny<CreateUserCommand>()), Times.Once);
    }

    [Fact]
    public void Send_NoHandlerRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserRequest("John Doe", "john@example.com"));
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var reactiveMediator = new ReactiveMediator(serviceProvider);

        // Act & Assert
        var exception =
            Assert.Throws<InvalidOperationException>(() =>
                reactiveMediator.Send<CreateUserCommand, GetUserResponse>(command));

        Assert.Contains("No handler registered for CreateUserCommand", exception.Message);
    }

    [Fact]
    public async Task SendAsync_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserRequest("John Doe", "john@example.com"));
        var expectedResponse =
            new GetUserResponse(Guid.NewGuid(), "John Doe", "john@example.com", DateTime.UtcNow);

        var mockHandler = new Mock<IReactiveRequestHandler<CreateUserCommand, GetUserResponse>>();
        mockHandler.Setup(h => h.Handle(It.IsAny<CreateUserCommand>()))
            .Returns(Observable.Return(expectedResponse));

        var serviceProvider = CreateServiceProvider(mockHandler.Object);
        var reactiveMediator = new ReactiveMediator(serviceProvider);

        // Act
        var result = await reactiveMediator.SendAsync<CreateUserCommand, GetUserResponse>(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
    }

    [Fact]
    public async Task Send_BoolResponse_ReturnsCorrectType()
    {
        // Arrange
        var command = new DeleteUserCommand(Guid.NewGuid());

        var mockHandler = new Mock<IReactiveRequestHandler<DeleteUserCommand, bool>>();
        mockHandler.Setup(h => h.Handle(It.IsAny<DeleteUserCommand>()))
            .Returns(Observable.Return(true));

        var serviceProvider = CreateServiceProvider(mockHandler.Object);
        var reactiveMediator = new ReactiveMediator(serviceProvider);

        // Act
        var result = await reactiveMediator.Send<DeleteUserCommand, bool>(command).ToTask();

        // Assert
        Assert.True(result);
        mockHandler.Verify(h => h.Handle(It.IsAny<DeleteUserCommand>()), Times.Once);
    }

    [Fact]
    public async Task Send_NullableResponse_ReturnsNull()
    {
        // Arrange
        var query = new GetUserByIdQuery(Guid.NewGuid());

        var mockHandler = new Mock<IReactiveRequestHandler<GetUserByIdQuery, GetUserResponse?>>();
        mockHandler.Setup(h => h.Handle(It.IsAny<GetUserByIdQuery>()))
            .Returns(Observable.Return<GetUserResponse?>(null));

        var serviceProvider = CreateServiceProvider(mockHandler.Object);
        var reactiveMediator = new ReactiveMediator(serviceProvider);

        // Act
        var result = await reactiveMediator.Send<GetUserByIdQuery, GetUserResponse?>(query).ToTask();

        // Assert
        Assert.Null(result);
        mockHandler.Verify(h => h.Handle(It.IsAny<GetUserByIdQuery>()), Times.Once);
    }

    [Fact]
    public async Task Publish_WithHandlers_CallsAllHandlers()
    {
        // Arrange
        var notification = new TestNotification("Test message");

        var mockHandler1 = new Mock<IReactiveNotificationHandler<TestNotification>>();
        mockHandler1.Setup(h => h.Handle(It.IsAny<TestNotification>()))
            .Returns(Observable.Return(Unit.Default));

        var mockHandler2 = new Mock<IReactiveNotificationHandler<TestNotification>>();
        mockHandler2.Setup(h => h.Handle(It.IsAny<TestNotification>()))
            .Returns(Observable.Return(Unit.Default));

        var services = new ServiceCollection();
        services.AddSingleton(mockHandler1.Object);
        services.AddSingleton(mockHandler2.Object);
        var serviceProvider = services.BuildServiceProvider();

        var reactiveMediator = new ReactiveMediator(serviceProvider);

        // Act
        await reactiveMediator.Publish(notification).ToTask();

        // Assert
        mockHandler1.Verify(h => h.Handle(It.IsAny<TestNotification>()), Times.Once);
        mockHandler2.Verify(h => h.Handle(It.IsAny<TestNotification>()), Times.Once);
    }

    [Fact]
    public async Task Publish_NoHandlers_ReturnsUnitDefault()
    {
        // Arrange
        var notification = new TestNotification("Test message");
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var reactiveMediator = new ReactiveMediator(serviceProvider);

        // Act
        var result = await reactiveMediator.Publish(notification).ToTask();

        // Assert
        Assert.Equal(Unit.Default, result);
    }

    [Fact]
    public async Task PublishAsync_WithHandlers_CompletesSuccessfully()
    {
        // Arrange
        var notification = new TestNotification("Test message");

        var mockHandler = new Mock<IReactiveNotificationHandler<TestNotification>>();
        mockHandler.Setup(h => h.Handle(It.IsAny<TestNotification>()))
            .Returns(Observable.Return(Unit.Default));

        var services = new ServiceCollection();
        services.AddSingleton(mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var reactiveMediator = new ReactiveMediator(serviceProvider);

        // Act
        await reactiveMediator.PublishAsync(notification);

        // Assert
        mockHandler.Verify(h => h.Handle(It.IsAny<TestNotification>()), Times.Once);
    }

    [Fact]
    public async Task Send_WhenCancelled_ThrowsOperationCancelledException()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserRequest("John Doe", "john@example.com"));
        var cts = new CancellationTokenSource();

        var mockHandler = new Mock<IReactiveRequestHandler<CreateUserCommand, GetUserResponse>>();
        mockHandler.Setup(h => h.Handle(It.IsAny<CreateUserCommand>()))
            .Returns(Observable.Never<GetUserResponse>());

        var serviceProvider = CreateServiceProvider(mockHandler.Object);
        var reactiveMediator = new ReactiveMediator(serviceProvider);

        // Act
        var task = reactiveMediator.SendAsync<CreateUserCommand, GetUserResponse>(command, cts.Token);
        await cts.CancelAsync();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    private static IServiceProvider CreateServiceProvider<THandler>(THandler handler) where THandler : class
    {
        var services = new ServiceCollection();
        services.AddSingleton(handler);
        return services.BuildServiceProvider();
    }
}

// Test notification class - must be public for Moq to create proxies
public record TestNotification(string Message);