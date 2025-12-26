namespace Turbo.API.DTOs;

public record GetUserResponse(Guid Id, string Name, string Email, DateTime CreatedAt, DateTime? UpdatedAt);