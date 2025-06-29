using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Moq;
using Turbo.API.DTOs;
using Turbo.API.Models;
using Turbo.API.Queries;
using Turbo.API.Repositories;
using Xunit;

namespace Turbo.API.Tests.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _handler = new GetUserByIdQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidUserId_ReturnsUserResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        
        var expectedUser = new User("John Doe", "john@example.com") { Id = userId };
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .Returns(Observable.Return(expectedUser));

        // Act
        var result = await _handler.Handle(query).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal(expectedUser.CreatedAt, result.CreatedAt);
        Assert.Equal(expectedUser.UpdatedAt, result.UpdatedAt);
        
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .Returns(Observable.Return<User?>(null));

        // Act
        var result = await _handler.Handle(query).ToTask();

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var expectedException = new InvalidOperationException("Database error");
        
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .Returns(Observable.Throw<User?>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query).ToTask());
        
        Assert.Equal("Database error", exception.Message);
    }

    [Fact]
    public async Task Handle_UserWithUpdatedAt_ReturnsCorrectData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        
        var expectedUser = new User("John Doe", "john@example.com") { Id = userId };
        expectedUser.Update("John Updated", "john.updated@example.com");
        
        _mockRepository.Setup(r => r.GetByIdAsync(userId))
            .Returns(Observable.Return(expectedUser));

        // Act
        var result = await _handler.Handle(query).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("John Updated", result.Name);
        Assert.Equal("john.updated@example.com", result.Email);
        Assert.NotNull(result.UpdatedAt);
    }
} 