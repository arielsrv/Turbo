using System.Reactive.Linq;
using MediatR;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Commands;

public record UpdateUserCommand : IRequest<GetUserResponse>
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