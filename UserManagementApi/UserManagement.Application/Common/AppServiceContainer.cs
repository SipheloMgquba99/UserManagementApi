using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Contracts;
using UserManagement.Application.Dtos;
using UserManagement.Application.Services;

namespace UserManagement.Application.Common;

public static class AppServiceContainer
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services
        ,IConfiguration configuration)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
        services.AddScoped<IValidator<LoginUserDto>, LoginUserDtoValidator>();
        services.AddScoped<IValidator<string>, PasswordValidator>();
        services.AddMemoryCache(); 
        return services;
    }
}