using Microsoft.Extensions.DependencyInjection;
using Turbo.API.DTOs;
using Turbo.API.Handlers.Queries;
using Turbo.API.Mediation;
using Turbo.API.Queries;
using Turbo.API.Repositories;

namespace Turbo.API.Tests.Mediation;

public class ReactiveMediationRegistrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddReactiveMediation(typeof(ReactiveMediator).Assembly);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddReactiveMediation_DiscoversRequestHandlersInAssembly()
    {
        using var provider = BuildProvider();

        var handler = provider.GetService<IReactiveRequestHandler<GetUserByIdQuery, GetUserResponse?>>();

        Assert.IsType<GetUserByIdQueryHandler>(handler);
    }

    [Fact]
    public void AddReactiveMediation_RegistersTheMediator()
    {
        using var provider = BuildProvider();

        Assert.IsType<ReactiveMediator>(provider.GetService<IReactiveMediator>());
    }

    [Fact]
    public void AddReactiveMediation_DiscoversEveryHandlerSoNoneNeedManualRegistration()
    {
        using var provider = BuildProvider();

        // Every handler the API ships must resolve; forgetting one used to fail only at runtime.
        Assert.NotNull(provider.GetService<IReactiveRequestHandler<GetAllUsersQuery, GetUsersResponse>>());
        Assert.NotNull(provider.GetService<IReactiveRequestHandler<GetUserByEmailQuery, GetUserResponse?>>());
    }

    [Fact]
    public void AddPipelineBehavior_AppliesTheBehaviorToResolvedRequests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddReactiveMediation(typeof(ReactiveMediator).Assembly);
        services.AddPipelineBehavior(typeof(LoggingPipelineBehavior<,>));
        using var provider = services.BuildServiceProvider();

        var behaviors = provider
            .GetServices<IReactivePipelineBehavior<GetUserByIdQuery, GetUserResponse?>>()
            .ToList();

        Assert.Single(behaviors);
        Assert.IsType<LoggingPipelineBehavior<GetUserByIdQuery, GetUserResponse?>>(behaviors[0]);
    }
}
