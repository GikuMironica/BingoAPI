using Hopaut.Modules.Notifications.Application;
using Hopaut.Modules.Notifications.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Notifications.Api;

public static class NotificationsModuleInstaller
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OneSignalOptions>(configuration.GetSection("OneSignal"));
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));

        services.AddHttpClient<IPushNotificationSender, OneSignalPushSender>();
        services.AddScoped<IEmailSender, MailKitEmailSender>();

        return services;
    }
}
