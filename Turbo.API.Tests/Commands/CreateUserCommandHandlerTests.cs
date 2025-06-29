using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Moq;
using Turbo.API.Commands;
using Turbo.API.DTOs;
using Turbo.API.Models;
using Turbo.API.Repositories;
using Xunit;

namespace Turbo.API.Tests.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _handler = new CreateUserCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsUserResponse()
    {
        // Arrange
        var request = new CreateUserRequest("John Doe", "john@example.com");
        var command = new CreateUserCommand(request);
        
        var expectedUser = new User("John Doe", "john@example.com");
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Observable.Return(expectedUser));

        // Act
        var result = await _handler.Handle(command).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
        Assert.Equal(expectedUser.Name, result.Name);
        Assert.Equal(expectedUser.Email, result.Email);
        Assert.Equal(expectedUser.CreatedAt, result.CreatedAt);
        Assert.Null(result.UpdatedAt);
        
        _mockRepository.Verify(r => r.AddAsync(It.Is<User>(u => 
            u.Name == "John Doe" && u.Email == "john@example.com")), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var request = new CreateUserRequest("John Doe", "john@example.com");
        var command = new CreateUserCommand(request);
        var expectedException = new InvalidOperationException("Email already exists");
        
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Observable.Throw<User>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command).ToTask());
        
        Assert.Equal("Email already exists", exception.Message);
    }

    [Fact]
    public async Task Handle_EmptyName_StillCreatesUser()
    {
        // Arrange
        var request = new CreateUserRequest("", "john@example.com");
        var command = new CreateUserCommand(request);
        
        var expectedUser = new User("", "john@example.com");
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Observable.Return(expectedUser));

        // Act
        var result = await _handler.Handle(command).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("", result.Name);
        Assert.Equal("john@example.com", result.Email);
    }
} 