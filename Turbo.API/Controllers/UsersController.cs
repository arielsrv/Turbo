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
            var command = new CreateUserCommand(request);
            var result = await mediator.SendAsync(command, cancellationToken);
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
            var command = new UpdateUserCommand(id, request);
            var result = await mediator.SendAsync(command, cancellationToken);
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
        var command = new DeleteUserCommand(id);
        var result = await mediator.SendAsync(command, cancellationToken);

        if (!result)
            return NotFound();

        return Ok(result);
    }

    // QUERIES - Read operations
    [HttpGet]
    public async Task<ActionResult<GetUsersResponse>> GetAllUsers(CancellationToken cancellationToken)
    {
        var query = new GetAllUsersQuery();
        var result = await mediator.SendAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetUserResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await mediator.SendAsync(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<GetUserResponse>> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await mediator.SendAsync(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}