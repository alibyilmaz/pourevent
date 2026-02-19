using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pours.Application.Services;

namespace Pours.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Validators.RecordPourCommandValidator>(ServiceLifetime.Scoped);
        services.AddScoped<IPourService, PourService>();
        services.AddScoped<ISummaryService, SummaryService>();

        return services;
    }
}
