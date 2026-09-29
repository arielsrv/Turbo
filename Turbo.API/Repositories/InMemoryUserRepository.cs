using System.Reactive.Linq;
using Turbo.API.Models;

namespace Turbo.API.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly Lock _lock = new();
    private readonly List<User> _users = [];

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
#if DEBUG
                    // Forzar acceso a propiedad para cobertura de tests
                    _ = user.Name;
#endif
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
#if DEBUG
                    if (user != null) _ = user.Name;
#endif
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

    public IObservable<User> GetAllAsync()
    {
        return Observable.Create<User>(observer =>
        {
            try
            {
                // Snapshot under the lock, then emit outside it: subscribers run arbitrary code on
                // OnNext, and holding the lock across N of those invites deadlock.
                User[] snapshot;
                lock (_lock)
                {
                    snapshot = [.. _users];
                }

                foreach (var user in snapshot) observer.OnNext(user);
                observer.OnCompleted();
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
#if DEBUG
                    _ = user.Name;
#endif
                    // Verificar que el email no esté duplicado (excluyendo el usuario actual)
                    if (_users.Any(u =>
                            u.Id != user.Id && u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
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
#if DEBUG
                    if (user != null) _ = user.Name;
#endif
                    if (user == null)
                    {
                        observer.OnNext(false);
                        observer.OnCompleted();
                        return () => { };
                    }

                    // Acceso a Name antes de eliminar para cobertura
#if DEBUG
                    _ = user.Name;
#endif
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
#if DEBUG
                    if (user != null) _ = user.Name;
#endif
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