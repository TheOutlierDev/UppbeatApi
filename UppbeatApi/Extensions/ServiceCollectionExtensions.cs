using UppbeatApi.Data;
using UppbeatApi.Data.Repositories;
using UppbeatApi.Interfaces;
using UppbeatApi.Services;

namespace UppbeatApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScopedServices(this IServiceCollection services)
    {
        services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    public static IServiceCollection AddSingletonServices(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionFactory, ConnectionFactory>();

        return services;
    }
}
