using Turbo.API.Commands;
using Turbo.API.DTOs;
using Turbo.API.Mediation;
using Turbo.API.Queries;
using Turbo.API.Repositories;

namespace Turbo.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();

        // Register MediatR
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        // Register ReactiveMediator
        builder.Services.AddSingleton<IReactiveMediator, ReactiveMediator>();

        // Register repositories
        builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        // Register handlers
        builder.Services
            .AddTransient<IReactiveRequestHandler<CreateUserCommand, GetUserResponse>, CreateUserCommandHandler>();
        builder.Services
            .AddTransient<IReactiveRequestHandler<UpdateUserCommand, GetUserResponse>, UpdateUserCommandHandler>();
        builder.Services.AddTransient<IReactiveRequestHandler<DeleteUserCommand, bool>, DeleteUserCommandHandler>();
        builder.Services
            .AddTransient<IReactiveRequestHandler<GetUserByIdQuery, GetUserResponse?>, GetUserByIdQueryHandler>();
        builder.Services
            .AddTransient<IReactiveRequestHandler<GetAllUsersQuery, GetUsersResponse>, GetAllUsersQueryHandler>();
        builder.Services
            .AddTransient<IReactiveRequestHandler<GetUserByEmailQuery, GetUserResponse?>, GetUserByEmailQueryHandler>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) app.MapOpenApi();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}