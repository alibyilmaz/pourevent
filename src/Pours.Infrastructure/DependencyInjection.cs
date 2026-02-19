using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pours.Domain.Abstractions;
using Pours.Infrastructure.Persistence;
using Pours.Infrastructure.Repositories;

namespace Pours.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<PourDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<PourDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPourEventRepository, PourEventRepository>();
        services.AddScoped<IPourSummaryRepository, PourSummaryRepository>();

        return services;
    }
}
