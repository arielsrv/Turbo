using System.Reactive.Linq;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Queries;

public record GetAllUsersQuery;

public class GetAllUsersQueryHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<GetAllUsersQuery, GetUsersResponse>
{
    public IObservable<GetUsersResponse> Handle(GetAllUsersQuery request)
    {
        return userRepository.GetAllAsync()
            .Select(users => new GetUsersResponse(
                users.Select(user => new GetUserResponse(
                    user.Id,
                    user.Name,
                    user.Email,
                    user.CreatedAt,
                    user.UpdatedAt
                ))
            ));
    }
}