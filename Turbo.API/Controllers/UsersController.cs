using Microsoft.AspNetCore.Mvc;
using Turbo.API.Commands;
using Turbo.API.DTOs;
using Turbo.API.Exceptions;
using Turbo.API.Mediation;
using Turbo.API.Queries;
using Turbo.API.Streaming;

namespace Turbo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IReactiveMediator mediator) : ControllerBase
{
    // COMMANDS - Write operations
    [HttpPost]
    public async Task<ActionResult<GetUserResponse>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await mediator.SendAsync<CreateUserCommand, GetUserResponse>(new CreateUserCommand(request),
                cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GetUserResponse>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await mediator.SendAsync<UpdateUserCommand, GetUserResponse>(new UpdateUserCommand(id, request),
                cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync<DeleteUserCommand, bool>(new DeleteUserCommand(id), cancellationToken);

        if (!result)
            throw new NotFoundException("User", id);

        return NoContent();
    }

    // QUERIES - Read operations
    [HttpGet]
    public async Task<ActionResult<GetUsersResponse>> GetAllUsers(CancellationToken cancellationToken)
    {
        var result =
            await mediator.SendAsync<GetAllUsersQuery, GetUsersResponse>(new GetAllUsersQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    ///     Streams users as they are produced instead of buffering the whole collection.
    /// </summary>
    /// <remarks>
    ///     Because the response begins before the collection is known, a failure partway through cannot
    ///     become a ProblemDetails payload — the 200 and its headers are already on the wire. Clients
    ///     that need an error status for a partial failure should use <c>GET /api/Users</c> instead.
    /// </remarks>
    [HttpGet("stream")]
    public IAsyncEnumerable<GetUserResponse> StreamUsers(CancellationToken cancellationToken)
    {
        return mediator
            .Send<StreamUsersQuery, GetUserResponse>(new StreamUsersQuery())
            .ToAsyncEnumerable(cancellationToken: cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetUserResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var result =
            await mediator.SendAsync<GetUserByIdQuery, GetUserResponse?>(new GetUserByIdQuery(id), cancellationToken);

        if (result is null)
            throw new NotFoundException("User", id);

        return Ok(result);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<GetUserResponse>> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        var result =
            await mediator.SendAsync<GetUserByEmailQuery, GetUserResponse?>(new GetUserByEmailQuery(email),
                cancellationToken);

        return result is null ? throw new NotFoundException("User", email) : Ok(result);
    }
}