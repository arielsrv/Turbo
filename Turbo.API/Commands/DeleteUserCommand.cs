using System.Reactive;
using System.Reactive.Linq;
using Turbo.API.Mediation;
using Turbo.API.Repositories;
using MediatR;

namespace Turbo.API.Commands;

public record DeleteUserCommand : IRequest<bool>
{
    public Guid Id { get; init; }

    public DeleteUserCommand(Guid id)
    {
        Id = id;
    }
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