namespace Turbo.API.Models;

public class User
{
    public User(string name, string email)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; init; }
    public virtual string Name { get; private set; }
    public string Email { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    public void Update(string name, string email)
    {
        Name = name;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
}