using Hopaut.Modules.Payments.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Payments.Infrastructure;

/// <summary>Registers the Payments module's services; handlers are registered by the Host's AddMediator.</summary>
public static class PaymentsModuleInstaller
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Constraints arrive with the adapter hardening (F-031 → plan Step 8.11).
        services.AddOptions<PaymentsServiceOptions>().BindConfiguration("PaymentsService").ValidateDataAnnotations().ValidateOnStart();
        services.AddHttpClient<IPaymentsServiceClient, PaymentsServiceClient>();
        return services;
    }
}
