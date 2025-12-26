namespace Turbo.API.DTOs;

public record GetUsersResponse(IEnumerable<GetUserResponse> Users);