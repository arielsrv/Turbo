using Turbo.API.DTOs;

namespace Turbo.API.Commands;

public record UpdateUserCommand
{
    public UpdateUserCommand(Guid id, UpdateUserRequest request)
    {
        Id = id;
        Name = request.Name;
        Email = request.Email;
    }

    public Guid Id { get; }
    public string Name { get; } = string.Empty;
    public string Email { get; } = string.Empty;
}