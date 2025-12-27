using System.Reactive.Linq;
using Turbo.API.Commands;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Models;
using Turbo.API.Repositories;

namespace Turbo.API.Handlers.Commands;

public class CreateUserCommandHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<CreateUserCommand, GetUserResponse>
{
    public IObservable<GetUserResponse> Handle(CreateUserCommand request)
    {
        var user = new User(request.Name, request.Email);

        return userRepository.AddAsync(user)
            .Select(createdUser => new GetUserResponse(
                createdUser.Id,
                createdUser.Name,
                createdUser.Email,
                createdUser.CreatedAt,
                createdUser.UpdatedAt
            ));
    }
}