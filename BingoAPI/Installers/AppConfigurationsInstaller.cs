using BingoAPI.CustomMapper;
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

            services.AddSingleton<ICreatePostRequestMapper, CreatePostRequestMapper>();
            services.Configure<EventTypes>(configuration.GetSection("Types"));
            services.AddSingleton<IImageToWebpProcessor, ImageToWebpProcessor>();
            services.Configure<AwsBucketSettings>(configuration.GetSection("AWS-ImageBucket"));
            services.AddSingleton<IAwsImageUploader, AwsImageUploader>();
        }
    }
}
