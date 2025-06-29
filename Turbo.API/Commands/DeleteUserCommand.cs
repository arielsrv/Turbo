using MediatR;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Commands;

public record DeleteUserCommand : IRequest<bool>
{
    public DeleteUserCommand(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; init; }
}

public class DeleteUserCommandHandler : IReactiveRequestHandler<DeleteUserCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IObservable<bool> Handle(DeleteUserCommand request)
    {
        return _userRepository.DeleteAsync(request.Id);
    }
}