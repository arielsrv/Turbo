using System.Reactive;
using System.Reactive.Linq;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Models;
using Turbo.API.Repositories;
using MediatR;

namespace Turbo.API.Commands;

public record CreateUserCommand : IRequest<UserResponse>
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public CreateUserCommand(CreateUserRequest request)
    {
        Name = request.Name;
        Email = request.Email;
    }
}

public class CreateUserCommandHandler : IReactiveRequestHandler<CreateUserCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IObservable<UserResponse> Handle(CreateUserCommand request)
    {
        var user = new User(request.Name, request.Email);
        
        return _userRepository.AddAsync(user)
            .Select(createdUser => new UserResponse(
                createdUser.Id,
                createdUser.Name,
                createdUser.Email,
                createdUser.CreatedAt,
                createdUser.UpdatedAt
            ));
    }
} 