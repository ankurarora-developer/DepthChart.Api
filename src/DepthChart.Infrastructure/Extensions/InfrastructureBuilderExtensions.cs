using DepthChart.Domain.Repositories;
using DepthChart.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DepthChart.Infrastructure.Extensions;

public static class InfrastructureBuilderExtensions
{
    public static void AddInfraServices(this IServiceCollection serviceCollection, ConfigurationManager builderConfiguration)
    {
        // DbContext (Postgres)
        serviceCollection.AddDbContext<DepthChartDbContext>(o =>
            o.UseNpgsql(builderConfiguration.GetConnectionString("Postgres")));
        serviceCollection.AddScoped<IDepthChartRepository, DepthChartRepository>();
    }
}
