using Turbo.API.DTOs;

namespace Turbo.API.Commands;

public record CreateUserCommand
{
    public CreateUserCommand(CreateUserRequest request)
    {
        Name = request.Name;
        Email = request.Email;
    }

    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}