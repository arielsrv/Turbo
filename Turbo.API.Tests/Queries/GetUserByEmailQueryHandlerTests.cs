using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Moq;
using Turbo.API.Handlers;
using Turbo.API.Handlers.Queries;
using Turbo.API.Models;
using Turbo.API.Queries;
using Turbo.API.Repositories;

namespace Turbo.API.Tests.Queries;

public class GetUserByEmailQueryHandlerTests
{
    private readonly GetUserByEmailQueryHandler _handler;
    private readonly Mock<IUserRepository> _mockRepository;

    public GetUserByEmailQueryHandlerTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _handler = new GetUserByEmailQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidEmail_ReturnsUserResponse()
    {
        // Arrange
        var email = "john@example.com";
        var query = new GetUserByEmailQuery(email);
        var expectedUser = new User("John Doe", email) { Id = Guid.NewGuid() };

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .Returns(Observable.Return(expectedUser));

        // Act
        var result = await _handler.Handle(query).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal(email, result.Email);
        Assert.Equal(expectedUser.CreatedAt, result.CreatedAt);
        Assert.Equal(expectedUser.UpdatedAt, result.UpdatedAt);

        _mockRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task Handle_EmailNotFound_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var query = new GetUserByEmailQuery(email);

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .Returns(Observable.Return<User?>(null));

        // Act
        var result = await _handler.Handle(query).ToTask();

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var email = "john@example.com";
        var query = new GetUserByEmailQuery(email);
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .Returns(Observable.Throw<User?>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query).ToTask());

        Assert.Equal("Database error", exception.Message);
    }

    [Fact]
    public async Task Handle_EmptyEmail_ReturnsNull()
    {
        // Arrange
        var email = "";
        var query = new GetUserByEmailQuery(email);

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .Returns(Observable.Return<User?>(null));

        // Act
        var result = await _handler.Handle(query).ToTask();

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task Handle_CaseInsensitiveEmail_ReturnsUser()
    {
        // Arrange
        var email = "JOHN@EXAMPLE.COM";
        var query = new GetUserByEmailQuery(email);
        var expectedUser = new User("John Doe", "john@example.com") { Id = Guid.NewGuid() };

        _mockRepository.Setup(r => r.GetByEmailAsync(email))
            .Returns(Observable.Return(expectedUser));

        // Act
        var result = await _handler.Handle(query).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@example.com", result.Email);
    }
}