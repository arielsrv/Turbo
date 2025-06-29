using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Moq;
using Turbo.API.Commands;
using Turbo.API.Repositories;

namespace Turbo.API.Tests.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly DeleteUserCommandHandler _handler;
    private readonly Mock<IUserRepository> _mockRepository;

    public DeleteUserCommandHandlerTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _handler = new DeleteUserCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _mockRepository.Setup(r => r.DeleteAsync(userId))
            .Returns(Observable.Return(true));

        // Act
        var result = await _handler.Handle(command).ToTask();

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _mockRepository.Setup(r => r.DeleteAsync(userId))
            .Returns(Observable.Return(false));

        // Act
        var result = await _handler.Handle(command).ToTask();

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository.Setup(r => r.DeleteAsync(userId))
            .Returns(Observable.Throw<bool>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command).ToTask());

        Assert.Equal("Database error", exception.Message);
    }
}