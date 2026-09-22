using Scalar.AspNetCore;
using Turbo.API.Mediation;
using Turbo.API.Middleware;
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

        // Configure RFC 7807 Problem Details
        builder.Services.AddProblemDetails();

        // Register repositories
        builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        // Mediator plus every handler in this assembly, discovered by scanning
        builder.Services.AddReactiveMediation(typeof(Program).Assembly);
        builder.Services.AddPipelineBehavior(typeof(LoggingPipelineBehavior<,>));

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(); // UI interactiva en /scalar/v1
        }

        app.UseExceptionHandling(); // RFC 7807 Problem Details error handling
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}