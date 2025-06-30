using System.Reactive.Linq;
using MediatR;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Queries;

public record GetAllUsersQuery : IRequest<UsersResponse>;

public class GetAllUsersQueryHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<GetAllUsersQuery, UsersResponse>
{
    public IObservable<UsersResponse> Handle(GetAllUsersQuery request)
    {
        return userRepository.GetAllAsync()
            .Select(users => new UsersResponse(
                users.Select(user => new UserResponse(
                    user.Id,
                    user.Name,
                    user.Email,
                    user.CreatedAt,
                    user.UpdatedAt
                ))
            ));
    }
}