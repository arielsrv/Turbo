using System;
using System.Threading;
using Turbo.API.Models;
using Xunit;

namespace Turbo.API.Tests.Models;

public class UserTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesUserWithCorrectProperties()
    {
        // Arrange & Act
        var name = "John Doe";
        var email = "john@example.com";
        var user = new User(name, email);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(name, user.Name);
        Assert.Equal(email, user.Email);
        Assert.True(user.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.Null(user.UpdatedAt);
    }

    [Fact]
    public void Constructor_EmptyName_CreatesUserWithEmptyName()
    {
        // Arrange & Act
        var user = new User("", "john@example.com");

        // Assert
        Assert.Equal("", user.Name);
        Assert.Equal("john@example.com", user.Email);
    }

    [Fact]
    public void Constructor_EmptyEmail_CreatesUserWithEmptyEmail()
    {
        // Arrange & Act
        var user = new User("John Doe", "");

        // Assert
        Assert.Equal("John Doe", user.Name);
        Assert.Equal("", user.Email);
    }

    [Fact]
    public void Update_ValidParameters_UpdatesUserProperties()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");
        var originalCreatedAt = user.CreatedAt;
        var originalUpdatedAt = user.UpdatedAt;

        // Act
        Thread.Sleep(1); // Ensure time difference
        user.Update("John Updated", "john.updated@example.com");

        // Assert
        Assert.Equal("John Updated", user.Name);
        Assert.Equal("john.updated@example.com", user.Email);
        Assert.Equal(originalCreatedAt, user.CreatedAt);
        Assert.NotNull(user.UpdatedAt);
        Assert.True(user.UpdatedAt >= originalCreatedAt);
    }

    [Fact]
    public void Update_EmptyName_UpdatesWithEmptyName()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");

        // Act
        user.Update("", "john.updated@example.com");

        // Assert
        Assert.Equal("", user.Name);
        Assert.Equal("john.updated@example.com", user.Email);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void Update_EmptyEmail_UpdatesWithEmptyEmail()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");

        // Act
        user.Update("John Updated", "");

        // Assert
        Assert.Equal("John Updated", user.Name);
        Assert.Equal("", user.Email);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void Update_MultipleTimes_UpdatesUpdatedAtEachTime()
    {
        // Arrange
        var user = new User("John Doe", "john@example.com");

        // Act
        user.Update("John Updated", "john.updated@example.com");
        var firstUpdate = user.UpdatedAt;

        Thread.Sleep(10); // Ensure time difference

        user.Update("John Final", "john.final@example.com");
        var secondUpdate = user.UpdatedAt;

        // Assert
        Assert.NotNull(firstUpdate);
        Assert.NotNull(secondUpdate);
        Assert.True(secondUpdate > firstUpdate);
        Assert.Equal("John Final", user.Name);
        Assert.Equal("john.final@example.com", user.Email);
    }

    [Fact]
    public void Id_GeneratedId_IsUnique()
    {
        // Arrange & Act
        var user1 = new User("John Doe", "john@example.com");
        var user2 = new User("Jane Smith", "jane@example.com");

        // Assert
        Assert.NotEqual(user1.Id, user2.Id);
        Assert.NotEqual(Guid.Empty, user1.Id);
        Assert.NotEqual(Guid.Empty, user2.Id);
    }
}