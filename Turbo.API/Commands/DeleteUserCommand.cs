using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Commands;

public record DeleteUserCommand(Guid Id);

public class DeleteUserCommandHandler(IUserRepository userRepository) : IReactiveRequestHandler<DeleteUserCommand, bool>
{
    public IObservable<bool> Handle(DeleteUserCommand request)
    {
        return userRepository.DeleteAsync(request.Id);
    }
}