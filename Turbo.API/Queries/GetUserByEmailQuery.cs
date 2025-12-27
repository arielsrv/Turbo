namespace Turbo.API.Queries;

public record GetUserByEmailQuery
{
    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }

    public string Email { get; } = string.Empty;
}