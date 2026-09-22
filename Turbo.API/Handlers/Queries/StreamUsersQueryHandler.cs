using System.Reactive.Linq;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Queries;
using Turbo.API.Repositories;

namespace Turbo.API.Handlers.Queries;

/// <summary>
///     Emits one <see cref="GetUserResponse" /> per user. Note this uses the same handler interface as
///     every single-valued query: an observable covers one result or many, so streaming needs no
///     parallel abstraction.
/// </summary>
public class StreamUsersQueryHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<StreamUsersQuery, GetUserResponse>
{
    public IObservable<GetUserResponse> Handle(StreamUsersQuery request)
    {
        return userRepository.GetAllAsync()
            .Select(user => new GetUserResponse(
                user.Id,
                user.Name,
                user.Email,
                user.CreatedAt,
                user.UpdatedAt
            ));
    }
}
