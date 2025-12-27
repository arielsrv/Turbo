using System.Reactive.Linq;
using Turbo.API.Commands;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Handlers.Commands;

public class UpdateUserCommandHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<UpdateUserCommand, GetUserResponse>
{
    public IObservable<GetUserResponse> Handle(UpdateUserCommand request)
    {
        return userRepository.GetByIdAsync(request.Id)
            .SelectMany(existingUser =>
            {
                if (existingUser == null) throw new InvalidOperationException($"User with id {request.Id} not found");

                existingUser.Update(request.Name, request.Email);
                return userRepository.UpdateAsync(existingUser);
            })
            .Select(updatedUser => new GetUserResponse(
                updatedUser.Id,
                updatedUser.Name,
                updatedUser.Email,
                updatedUser.CreatedAt,
                updatedUser.UpdatedAt
            ));
    }
}