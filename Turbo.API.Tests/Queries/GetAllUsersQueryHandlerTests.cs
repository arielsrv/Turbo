using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Moq;
using Turbo.API.Models;
using Turbo.API.Queries;
using Turbo.API.Repositories;

namespace Turbo.API.Tests.Queries;

public class GetAllUsersQueryHandlerTests
{
    private readonly GetAllUsersQueryHandler _handler;
    private readonly Mock<IUserRepository> _mockRepository;

    public GetAllUsersQueryHandlerTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _handler = new GetAllUsersQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_UsersExist_ReturnsUsersResponse()
    {
        // Arrange
        var query = new GetAllUsersQuery();
        var users = new List<User>
        {
            new("John Doe", "john@example.com") { Id = Guid.NewGuid() },
            new("Jane Smith", "jane@example.com") { Id = Guid.NewGuid() }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .Returns(Observable.Return<IEnumerable<User>>(users));

        // Act
        var result = await _handler.Handle(query).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Users);
        var usersList = result.Users.ToList();
        Assert.Equal(2, usersList.Count);

        Assert.Equal(users[0].Id, usersList[0].Id);
        Assert.Equal(users[0].Name, usersList[0].Name);
        Assert.Equal(users[0].Email, usersList[0].Email);

        Assert.Equal(users[1].Id, usersList[1].Id);
        Assert.Equal(users[1].Name, usersList[1].Name);
        Assert.Equal(users[1].Email, usersList[1].Email);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_NoUsers_ReturnsEmptyResponse()
    {
        // Arrange
        var query = new GetAllUsersQuery();
        var emptyUsers = new List<User>();

        _mockRepository.Setup(r => r.GetAllAsync())
            .Returns(Observable.Return<IEnumerable<User>>(emptyUsers));

        // Act
        var result = await _handler.Handle(query).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Users);
        Assert.Empty(result.Users);

        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var query = new GetAllUsersQuery();
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository.Setup(r => r.GetAllAsync())
            .Returns(Observable.Throw<IEnumerable<User>>(expectedException));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query).ToTask());

        Assert.Equal("Database error", exception.Message);
    }

    [Fact]
    public async Task Handle_SingleUser_ReturnsCorrectResponse()
    {
        // Arrange
        var query = new GetAllUsersQuery();
        var user = new User("John Doe", "john@example.com") { Id = Guid.NewGuid() };
        var users = new List<User> { user };

        _mockRepository.Setup(r => r.GetAllAsync())
            .Returns(Observable.Return<IEnumerable<User>>(users));

        // Act
        var result = await _handler.Handle(query).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Users);
        var usersList = result.Users.ToList();
        Assert.Single(usersList);

        Assert.Equal(user.Id, usersList[0].Id);
        Assert.Equal(user.Name, usersList[0].Name);
        Assert.Equal(user.Email, usersList[0].Email);
    }
}