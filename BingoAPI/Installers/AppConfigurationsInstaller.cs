using BingoAPI.Options;
using BingoAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        }
    }
}
