using System.Reactive.Linq;
using Turbo.API.Models;

namespace Turbo.API.Repositories;

public interface IUserRepository
{
    IObservable<User> AddAsync(User user);
    IObservable<User?> GetByIdAsync(Guid id);
    IObservable<IEnumerable<User>> GetAllAsync();
    IObservable<User> UpdateAsync(User user);
    IObservable<bool> DeleteAsync(Guid id);
    IObservable<User?> GetByEmailAsync(string email);
} 