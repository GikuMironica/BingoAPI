using Hopaut.Modules.Payments.Application;
using Hopaut.Modules.Payments.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Payments.Api;

public static class PaymentsModuleInstaller
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaymentsServiceOptions>(configuration.GetSection("PaymentsService"));
        services.AddHttpClient<IPaymentsServiceClient, PaymentsServiceClient>();
        return services;
    }

    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new PaymentsModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
