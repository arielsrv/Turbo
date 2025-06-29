using System.Reactive;
using System.Reactive.Linq;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;
using MediatR;

namespace Turbo.API.Queries;

public record GetUserByIdQuery : IRequest<UserResponse?>
{
    public Guid Id { get; init; }

    public GetUserByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetUserByIdQueryHandler : IReactiveRequestHandler<GetUserByIdQuery, UserResponse?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IObservable<UserResponse?> Handle(GetUserByIdQuery request)
    {
        return _userRepository.GetByIdAsync(request.Id)
            .Select(user => user == null ? null : new UserResponse(
                user.Id,
                user.Name,
                user.Email,
                user.CreatedAt,
                user.UpdatedAt
            ));
    }
} 