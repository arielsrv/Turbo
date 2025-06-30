using System.Reactive.Linq;
using MediatR;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<UserResponse?>;

public class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<GetUserByIdQuery, UserResponse?>
{
    public IObservable<UserResponse?> Handle(GetUserByIdQuery request)
    {
        return userRepository.GetByIdAsync(request.Id)
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