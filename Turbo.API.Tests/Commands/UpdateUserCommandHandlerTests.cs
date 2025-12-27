using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Moq;
using Turbo.API.Commands;
using Turbo.API.DTOs;
using Turbo.API.Handlers;
using Turbo.API.Handlers.Commands;
using Turbo.API.Models;
using Turbo.API.Repositories;

namespace Turbo.API.Tests.Commands;

public class UpdateUserCommandHandlerTests
{
    private readonly UpdateUserCommandHandler _handler;
    private readonly Mock<IUserRepository> _mockRepository;

    public UpdateUserCommandHandlerTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _handler = new UpdateUserCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsUpdatedUserResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("John Updated", "john.updated@example.com");
        var command = new UpdateUserCommand(userId, request);

        var existingUser = new User("John Doe", "john@example.com") { Id = userId };
        var updatedUser = new User("John Updated", "john.updated@example.com") { Id = userId };

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .Returns(Observable.Return(existingUser));
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Observable.Return(updatedUser));

        // Act
        var result = await _handler.Handle(command).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("John Updated", result.Name);
        Assert.Equal("john.updated@example.com", result.Email);

        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<User>(u =>
            u.Id == userId && u.Name == "John Updated" && u.Email == "john.updated@example.com")), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("John Updated", "john.updated@example.com");
        var command = new UpdateUserCommand(userId, request);

        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .Returns(Observable.Return<User?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command).ToTask());

        Assert.Equal($"User with id {userId} not found", exception.Message);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("John Updated", "john.updated@example.com");
        var command = new UpdateUserCommand(userId, request);
        var expectedException = new InvalidOperationException("Email already exists");

        var existingUser = new User("John Doe", "john@example.com") { Id = userId };
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .Returns(Observable.Return(existingUser));
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Observable.Throw<User>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command).ToTask());

        Assert.Equal("Email already exists", exception.Message);
    }
}