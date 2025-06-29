namespace Turbo.API.DTOs;

public record CreateUserRequest(string Name, string Email);
public record UpdateUserRequest(string Name, string Email);
public record UserResponse(Guid Id, string Name, string Email, DateTime CreatedAt, DateTime? UpdatedAt);
public record UsersResponse(IEnumerable<UserResponse> Users); 