using Microsoft.AspNetCore.Mvc;
using Turbo.API.Commands;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Queries;

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
        try
        {
            var result = await mediator.SendAsync(new CreateUserCommand(request), cancellationToken);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GetUserResponse>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.SendAsync(new UpdateUserCommand(id, request), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<bool>> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new DeleteUserCommand(id), cancellationToken);

        if (!result)
            return NotFound();

        return Ok(result);
    }

    // QUERIES - Read operations
    [HttpGet]
    public async Task<ActionResult<GetUsersResponse>> GetAllUsers(CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new GetAllUsersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetUserResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new GetUserByIdQuery(id), cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<GetUserResponse>> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(new GetUserByEmailQuery(email), cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}