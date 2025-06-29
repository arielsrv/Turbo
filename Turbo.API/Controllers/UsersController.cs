using System.Reactive.Threading.Tasks;
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
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            CreateUserCommand command = new CreateUserCommand(request);
            UserResponse result = await mediator.Send(command).ToTask();
            return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var command = new UpdateUserCommand(id, request);
            var result = await mediator.Send<UserResponse>(command).ToTask();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<bool>> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);
        var result = await mediator.Send(command).ToTask();
        
        if (!result)
            return NotFound();
            
        return Ok(result);
    }

    // QUERIES - Read operations
    [HttpGet]
    public async Task<ActionResult<UsersResponse>> GetAllUsers()
    {
        var query = new GetAllUsersQuery();
        var result = await mediator.Send(query).ToTask();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await mediator.Send(query).ToTask();
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<UserResponse>> GetUserByEmail(string email)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await mediator.Send(query).ToTask();
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }
} 