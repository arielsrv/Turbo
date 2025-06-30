using System.Reactive.Linq;
using MediatR;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Commands;

public record UpdateUserCommand : IRequest<UserResponse>
{
    public UpdateUserCommand(Guid id, UpdateUserRequest request)
    {
        Id = id;
        Name = request.Name;
        Email = request.Email;
    }

    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public class UpdateUserCommandHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<UpdateUserCommand, UserResponse>
{
    public IObservable<UserResponse> Handle(UpdateUserCommand request)
    {
        return userRepository.GetByIdAsync(request.Id)
            .SelectMany(existingUser =>
            {
                if (existingUser == null) throw new InvalidOperationException($"User with id {request.Id} not found");

                existingUser.Update(request.Name, request.Email);
                return userRepository.UpdateAsync(existingUser);
            })
            .Select(updatedUser => new UserResponse(
                updatedUser.Id,
                updatedUser.Name,
                updatedUser.Email,
                updatedUser.CreatedAt,
                updatedUser.UpdatedAt
            ));
    }
}