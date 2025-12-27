using System.Reactive.Linq;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Queries;
using Turbo.API.Repositories;

namespace Turbo.API.Handlers.Queries;

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