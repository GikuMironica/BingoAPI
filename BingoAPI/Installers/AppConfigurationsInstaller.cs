using BingoAPI.CustomMapper;
using BingoAPI.CustomValidation;
using BingoAPI.Domain;
using BingoAPI.Options;
using BingoAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Installers
{
    public class AppConfigurationsInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApplicationEmailSettings>(configuration.GetSection("ApplicationEmail"));
            services.AddSingleton<IEmailService, EmailService>();

            // in order to use HttpContext, IUriHelper in services classes
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x => {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });

            // custom mapper services
            services.AddSingleton<ICreatePostRequestMapper, CreatePostRequestMapper>();
            services.AddSingleton<IUpdatePostToDomain, UpdatePostToDomain>();
            services.AddSingleton<IDomainToResponseMapper, DomainToResponseMapper>();
            services.AddSingleton<IRequestToDomainMapper, RequestToDomainMapper>();

            // custom validation
            services.AddSingleton<IUpdatedPostDetailsWatcher, UpdatedPostDetailsWatcher>();

            // options
            services.Configure<EventTypes>(configuration.GetSection("Types"));
            services.Configure<AwsBucketSettings>(configuration.GetSection("AWS-ImageBucket"));
            services.Configure<OneSignalNotificationSettigs>(configuration.GetSection("OneSignalNotification"));
            services.Configure<NotificationTemplates>(configuration.GetSection("Message"));

            // services
            services.AddSingleton<IImageLoader, ImageLoader>();
            services.AddSingleton<IAwsBucketManager, AwsBucketManager>();
            services.AddSingleton<INotificationService, NotificationService>();
        }
    }
}
