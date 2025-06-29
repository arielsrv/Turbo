using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Turbo.API.Models;
using Turbo.API.Repositories;
using Xunit;

namespace Turbo.API.Tests.Repositories;

public class InMemoryUserRepositoryTests
{
    private readonly InMemoryUserRepository _repository;

    public InMemoryUserRepositoryTests()
    {
        _repository = new InMemoryUserRepository();
    }

    [Fact]
    public async Task AddAsync_ValidUser_ReturnsUser()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");

        // Act
        var result = await _repository.AddAsync(user).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task AddAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var user1 = new User("John Doe", "john@example.com");
        var user2 = new User("Jane Smith", "john@example.com");

        // Act & Assert
        await _repository.AddAsync(user1).ToTask();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.AddAsync(user2).ToTask());

        Assert.Equal("User with email john@example.com already exists", exception.Message);
    }

    [Fact]
    public async Task AddAsync_CaseInsensitiveEmail_ThrowsException()
    {
        // Arrange
        var user1 = new User("John Doe", "john@example.com");
        var user2 = new User("Jane Smith", "JOHN@EXAMPLE.COM");

        // Act & Assert
        await _repository.AddAsync(user1).ToTask();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.AddAsync(user2).ToTask());

        Assert.Equal("User with email JOHN@EXAMPLE.COM already exists", exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");
        await _repository.AddAsync(user).ToTask();

        // Act
        var result = await _repository.GetByIdAsync(user.Id).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId).ToTask();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync().ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithUsers_ReturnsAllUsers()
    {
        // Arrange
        var user1 = new User("John Doe", "john@example.com");
        var user2 = new User("Jane Smith", "jane@example.com");
        await _repository.AddAsync(user1).ToTask();
        await _repository.AddAsync(user2).ToTask();

        // Act
        var result = await _repository.GetAllAsync().ToTask();

        // Assert
        Assert.NotNull(result);
        var usersList = result.ToList();
        Assert.Equal(2, usersList.Count);
        Assert.Contains(usersList, u => u.Id == user1.Id);
        Assert.Contains(usersList, u => u.Id == user2.Id);
    }

    [Fact]
    public async Task UpdateAsync_ExistingUser_ReturnsUpdatedUser()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");
        await _repository.AddAsync(user).ToTask();
        user.Update("John Updated", "john.updated@example.com");

        // Act
        var result = await _repository.UpdateAsync(user).ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("John Updated", result.Name);
        Assert.Equal("john.updated@example.com", result.Email);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingUser_ThrowsException()
    {
        // Arrange
        var nonExistingUser = new User("John Doe", "john@example.com") { Id = Guid.NewGuid() };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.UpdateAsync(nonExistingUser).ToTask());

        Assert.Equal($"User with id {nonExistingUser.Id} not found", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var user1 = new User("John Doe", "john@example.com");
        var user2 = new User("Jane Smith", "jane@example.com");
        await _repository.AddAsync(user1).ToTask();
        await _repository.AddAsync(user2).ToTask();

        user2.Update("Jane Updated", "john@example.com"); // Try to use John's email

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.UpdateAsync(user2).ToTask());

        Assert.Equal("User with email john@example.com already exists", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUser_ReturnsTrue()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");
        await _repository.AddAsync(user).ToTask();

        // Act
        var result = await _repository.DeleteAsync(user.Id).ToTask();

        // Assert
        Assert.True(result);
        var retrievedUser = await _repository.GetByIdAsync(user.Id).ToTask();
        Assert.Null(retrievedUser);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingUser_ReturnsFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistingId).ToTask();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");
        await _repository.AddAsync(user).ToTask();

        // Act
        var result = await _repository.GetByEmailAsync("john@example.com").ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_CaseInsensitive_ReturnsUser()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");
        await _repository.AddAsync(user).ToTask();

        // Act
        var result = await _repository.GetByEmailAsync("JOHN@EXAMPLE.COM").ToTask();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
    {
        // Arrange
        var nonExistingEmail = "nonexistent@example.com";

        // Act
        var result = await _repository.GetByEmailAsync(nonExistingEmail).ToTask();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ConcurrentAccess_ThreadSafe()
    {
        // Arrange
        var tasks = new List<Task>();
        var userCount = 100;

        // Act - Add users concurrently
        for (int i = 0; i < userCount; i++)
        {
            var user = new User($"User{i}", $"user{i}@example.com");
            tasks.Add(_repository.AddAsync(user).ToTask());
        }

        await Task.WhenAll(tasks);

        // Assert
        var allUsers = await _repository.GetAllAsync().ToTask();
        Assert.Equal(userCount, allUsers.Count());
    }
} 