using System.Reactive.Linq;
using MediatR;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Queries;

public record GetUserByEmailQuery : IRequest<UserResponse?>
{
    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }

    public string Email { get; init; } = string.Empty;
}

public class GetUserByEmailQueryHandler : IReactiveRequestHandler<GetUserByEmailQuery, UserResponse?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IObservable<UserResponse?> Handle(GetUserByEmailQuery request)
    {
        return _userRepository.GetByEmailAsync(request.Email)
            .Select(user => user == null
                ? null
                : new UserResponse(
                    user.Id,
                    user.Name,
                    user.Email,
                    user.CreatedAt,
                    user.UpdatedAt
                ));
    }
}