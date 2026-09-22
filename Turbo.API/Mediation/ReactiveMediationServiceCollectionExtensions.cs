using System.Reflection;

namespace Turbo.API.Mediation;

public static class ReactiveMediationServiceCollectionExtensions
{
    private static readonly Type[] HandlerInterfaces =
        [typeof(IReactiveRequestHandler<,>), typeof(IReactiveNotificationHandler<>)];

    /// <summary>
    ///     Registers the mediator and every request and notification handler declared in
    ///     <paramref name="assembly" />, so adding a handler no longer means remembering to register it.
    /// </summary>
    public static IServiceCollection AddReactiveMediation(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton<IReactiveMediator, ReactiveMediator>();

        var registrations =
            from type in assembly.GetTypes()
            where type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false }
            from contract in type.GetInterfaces()
            where contract.IsGenericType && HandlerInterfaces.Contains(contract.GetGenericTypeDefinition())
            select (contract, type);

        foreach (var (contract, implementation) in registrations)
            services.AddTransient(contract, implementation);

        return services;
    }

    /// <summary>
    ///     Adds a cross-cutting behaviour to every request. Order of calls is the order of wrapping:
    ///     the first behaviour added is the outermost.
    /// </summary>
    /// <param name="openGenericBehavior">An open generic such as <c>typeof(LoggingPipelineBehavior&lt;,&gt;)</c>.</param>
    public static IServiceCollection AddPipelineBehavior(this IServiceCollection services, Type openGenericBehavior)
    {
        if (!openGenericBehavior.IsGenericTypeDefinition)
            throw new ArgumentException(
                $"{openGenericBehavior.Name} must be an open generic type definition, e.g. typeof(MyBehavior<,>).",
                nameof(openGenericBehavior));

        services.AddTransient(typeof(IReactivePipelineBehavior<,>), openGenericBehavior);
        return services;
    }
}
