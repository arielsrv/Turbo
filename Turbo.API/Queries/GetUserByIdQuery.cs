using System.Reactive.Linq;
using MediatR;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<GetUserResponse?>;

public class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IReactiveRequestHandler<GetUserByIdQuery, GetUserResponse?>
{
    public IObservable<GetUserResponse?> Handle(GetUserByIdQuery request)
    {
        return userRepository.GetByIdAsync(request.Id)
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