using Hopaut.Modules.Notifications.Application;
using Hopaut.Modules.Notifications.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Notifications.Api;

public static class NotificationsModuleInstaller
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Constraints arrive with the adapter hardening (F-031 → plan Step 8.11).
        services.AddOptions<OneSignalOptions>().BindConfiguration("OneSignal").ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<SmtpOptions>().BindConfiguration("Smtp").ValidateDataAnnotations().ValidateOnStart();

        services.AddHttpClient<IPushNotificationSender, OneSignalPushSender>();
        services.AddScoped<IEmailSender, MailKitEmailSender>();

        return services;
    }
}
