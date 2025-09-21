using DepthChart.Api.Middleware;
using DepthChart.Application.Extensions;
using DepthChart.Common;
using DepthChart.Common.Interfaces;
using DepthChart.Infrastructure.Extensions;

namespace DepthChart.Api.Extensions;
public static class AppBuilderExtensions
{
    public static void RegisterBuilderServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddTransient<ExceptionHandlingMiddleware>();
        builder.Services.AddTransient(typeof(ICorrelationLogger<>), typeof(CorrelationLogger<>));
        builder.Services.AddInfraServices(builder.Configuration);
        builder.Services.AddApplicationServices();
    }
}
