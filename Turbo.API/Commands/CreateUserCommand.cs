using System.Reactive.Linq;
using MediatR;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Models;
using Turbo.API.Repositories;

namespace Turbo.API.Commands;

public record CreateUserCommand : IRequest<UserResponse>
{
    public CreateUserCommand(CreateUserRequest request)
    {
        Name = request.Name;
        Email = request.Email;
    }

    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public class CreateUserCommandHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<CreateUserCommand, UserResponse>
{
    public IObservable<UserResponse> Handle(CreateUserCommand request)
    {
        var user = new User(request.Name, request.Email);

        return userRepository.AddAsync(user)
            .Select(createdUser => new UserResponse(
                createdUser.Id,
                createdUser.Name,
                createdUser.Email,
                createdUser.CreatedAt,
                createdUser.UpdatedAt
            ));
    }
}