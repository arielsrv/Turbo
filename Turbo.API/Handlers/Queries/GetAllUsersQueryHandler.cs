using System.Reactive.Linq;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Queries;
using Turbo.API.Repositories;

namespace Turbo.API.Handlers.Queries;

public class GetAllUsersQueryHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<GetAllUsersQuery, GetUsersResponse>
{
    public IObservable<GetUsersResponse> Handle(GetAllUsersQuery request)
    {
        return userRepository.GetAllAsync()
            .Select(user => new GetUserResponse(
                user.Id,
                user.Name,
                user.Email,
                user.CreatedAt,
                user.UpdatedAt
            ))
            .ToList()
            .Select(users => new GetUsersResponse(users));
    }
}