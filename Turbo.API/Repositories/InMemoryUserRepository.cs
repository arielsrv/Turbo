using System.Reactive.Linq;
using Turbo.API.Models;

namespace Turbo.API.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private readonly object _lock = new();

    public IObservable<User> AddAsync(User user)
    {
        return Observable.Create<User>(observer =>
        {
            try
            {
                lock (_lock)
                {
                    // Simular validación de email único
                    if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        observer.OnError(new InvalidOperationException($"User with email {user.Email} already exists"));
                        return () => { };
                    }

                    _users.Add(user);
                    observer.OnNext(user);
                    observer.OnCompleted();
                }
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return () => { };
        });
    }

    public IObservable<User?> GetByIdAsync(Guid id)
    {
        return Observable.Create<User?>(observer =>
        {
            try
            {
                lock (_lock)
                {
                    var user = _users.FirstOrDefault(u => u.Id == id);
                    observer.OnNext(user);
                    observer.OnCompleted();
                }
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return () => { };
        });
    }

    public IObservable<IEnumerable<User>> GetAllAsync()
    {
        return Observable.Create<IEnumerable<User>>(observer =>
        {
            try
            {
                lock (_lock)
                {
                    var users = _users.ToList();
                    observer.OnNext(users);
                    observer.OnCompleted();
                }
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return () => { };
        });
    }

    public IObservable<User> UpdateAsync(User user)
    {
        return Observable.Create<User>(observer =>
        {
            try
            {
                lock (_lock)
                {
                    var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
                    if (existingUser == null)
                    {
                        observer.OnError(new InvalidOperationException($"User with id {user.Id} not found"));
                        return () => { };
                    }

                    // Verificar que el email no esté duplicado (excluyendo el usuario actual)
                    if (_users.Any(u => u.Id != user.Id && u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        observer.OnError(new InvalidOperationException($"User with email {user.Email} already exists"));
                        return () => { };
                    }

                    var index = _users.IndexOf(existingUser);
                    _users[index] = user;
                    observer.OnNext(user);
                    observer.OnCompleted();
                }
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return () => { };
        });
    }

    public IObservable<bool> DeleteAsync(Guid id)
    {
        return Observable.Create<bool>(observer =>
        {
            try
            {
                lock (_lock)
                {
                    var user = _users.FirstOrDefault(u => u.Id == id);
                    if (user == null)
                    {
                        observer.OnNext(false);
                        observer.OnCompleted();
                        return () => { };
                    }

                    var removed = _users.Remove(user);
                    observer.OnNext(removed);
                    observer.OnCompleted();
                }
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return () => { };
        });
    }

    public IObservable<User?> GetByEmailAsync(string email)
    {
        return Observable.Create<User?>(observer =>
        {
            try
            {
                lock (_lock)
                {
                    var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                    observer.OnNext(user);
                    observer.OnCompleted();
                }
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }

            return () => { };
        });
    }
} 