using System.Reactive.Linq;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Queries;

public record GetUserByEmailQuery
{
    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }

    public string Email { get; } = string.Empty;
}

public class GetUserByEmailQueryHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<GetUserByEmailQuery, GetUserResponse?>
{
    public IObservable<GetUserResponse?> Handle(GetUserByEmailQuery request)
    {
        return userRepository.GetByEmailAsync(request.Email)
            .Select(user => user == null
                ? null
                : new GetUserResponse(
                    user.Id,
                    user.Name,
                    user.Email,
                    user.CreatedAt,
                    user.UpdatedAt
                ));
    }
}