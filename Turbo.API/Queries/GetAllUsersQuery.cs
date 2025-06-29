using System;
using System.Reactive.Linq;
using MediatR;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Repositories;

namespace Turbo.API.Queries;

public record GetAllUsersQuery : IRequest<UsersResponse>;

public class GetAllUsersQueryHandler : IReactiveRequestHandler<GetAllUsersQuery, UsersResponse>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IObservable<UsersResponse> Handle(GetAllUsersQuery request)
    {
        return _userRepository.GetAllAsync()
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