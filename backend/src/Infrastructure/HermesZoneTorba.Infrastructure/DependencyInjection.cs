using HermesZoneTorba.Application.Common.Interfaces;
using HermesZoneTorba.Application.HermesManagement;
using HermesZoneTorba.Domain.HermesManagement;
using HermesZoneTorba.Infrastructure.Common;
using HermesZoneTorba.Infrastructure.ExternalServices.Hermes;
using HermesZoneTorba.Infrastructure.Persistence;
using HermesZoneTorba.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HermesZoneTorba.Infrastructure;

/// <summary>
/// Composition entry point for the Infrastructure layer — every Application interface gets its concrete
/// implementation registered here. Called once from HermesZoneTorba.Api's Program.cs. See
/// docs/01-system-architecture.md#layering.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IHermesInstanceRepository, HermesInstanceRepository>();

        services.AddScoped<IHermesInstallerService, HermesInstallerService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
