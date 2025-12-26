using System.Reactive.Threading.Tasks;
using System.Reflection;
using Turbo.API.Models;
using Turbo.API.Repositories;

namespace Turbo.API.Tests.Repositories;

public class InMemoryUserRepositoryTests
{
    private readonly InMemoryUserRepository _repository = new();

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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.AddAsync(user2).ToTask());

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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.AddAsync(user2).ToTask());

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
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _repository.UpdateAsync(nonExistingUser).ToTask());

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
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateAsync(user2).ToTask());

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
        for (var i = 0; i < userCount; i++)
        {
            var user = new User($"User{i}", $"user{i}@example.com");
            tasks.Add(_repository.AddAsync(user).ToTask());
        }

        await Task.WhenAll(tasks);

        // Assert
        var allUsers = await _repository.GetAllAsync().ToTask();
        Assert.Equal(userCount, allUsers.Count());
    }

    [Fact]
    public async Task AddAsync_WhenExceptionThrown_PropagatesError()
    {
        var repo = new InMemoryUserRepository();
        var user = new ExplodingUser();
        var observable = repo.AddAsync(user);
        var ex = await Assert.ThrowsAsync<Exception>(() => observable.ToTask());
        Assert.Equal("Exploded!", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExceptionThrown_PropagatesError()
    {
        var repo = new InMemoryUserRepository();
        // Forzamos excepción usando un mock de lista interna (no posible aquí), así que lanzamos manualmente
        // Simulamos con un usuario que explota en Equals
        var id = Guid.NewGuid();
        var user = new ExplodingUser();
        typeof(User).GetProperty("Id")!.SetValue(user, id);
        // Insertamos el usuario en la lista interna usando reflexión para forzar el error
        var usersField = typeof(InMemoryUserRepository).GetField("_users",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var usersList = (List<User>)usersField!.GetValue(repo)!;
        usersList.Add(user);
        // Ahora llamamos y forzamos la excepción
        var observable = repo.GetByIdAsync(id);
        var ex = await Assert.ThrowsAsync<Exception>(() => observable.ToTask());
        Assert.Equal("Exploded!", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_WhenExceptionThrown_PropagatesError()
    {
        var repo = new InMemoryUserRepository();
        var user = new ExplodingUser();
        // Insertamos el usuario en la lista interna
        var usersField = typeof(InMemoryUserRepository).GetField("_users",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var usersList = (List<User>)usersField!.GetValue(repo)!;
        usersList.Add(user);
        // Ahora llamamos y forzamos la excepción
        var observable = repo.UpdateAsync(user);
        var ex = await Assert.ThrowsAsync<Exception>(() => observable.ToTask());
        Assert.Equal("Exploded!", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_WhenExceptionThrown_PropagatesError()
    {
        var repo = new InMemoryUserRepository();
        // Insertamos un usuario que explota y le asignamos el mismo Id
        var user = new ExplodingUser();
        var id = Guid.NewGuid();
        typeof(User).GetProperty("Id")!.SetValue(user, id);
        var usersField = typeof(InMemoryUserRepository).GetField("_users",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var usersList = (List<User>)usersField!.GetValue(repo)!;
        usersList.Add(user);
        // Ahora llamamos y forzamos la excepción
        var observable = repo.DeleteAsync(id);
        var ex = await Assert.ThrowsAsync<Exception>(() => observable.ToTask());
        Assert.Equal("Exploded!", ex.Message);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenExceptionThrown_PropagatesError()
    {
        var repo = new InMemoryUserRepository();
        // Insertamos un usuario que explota
        var user = new ExplodingUser();
        var usersField = typeof(InMemoryUserRepository).GetField("_users",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var usersList = (List<User>)usersField!.GetValue(repo)!;
        usersList.Add(user);
        // Ahora llamamos y forzamos la excepción
        var observable = repo.GetByEmailAsync(user.Email);
        var ex = await Assert.ThrowsAsync<Exception>(() => observable.ToTask());
        Assert.Equal("Exploded!", ex.Message);
    }

    // --- TESTS PARA FORZAR EXCEPCIONES Y CUBRIR BLOQUES CATCH ---
    private class ExplodingUser : User
    {
        public ExplodingUser() : base("Explode", "explode@example.com")
        {
        }

        public override string Name => throw new Exception("Exploded!");
    }
}