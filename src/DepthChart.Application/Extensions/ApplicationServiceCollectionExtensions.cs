using Microsoft.Extensions.DependencyInjection;

namespace DepthChart.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDepthChartService, DepthChartService>();
    }
}
