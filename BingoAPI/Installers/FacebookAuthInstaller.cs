using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BingoAPI.Options;
using BingoAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BingoAPI.Installers
{
    public class FacebookAuthInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var facebookAuthSettings = new FacebookAuthSettings();

            configuration.Bind(nameof(facebookAuthSettings), facebookAuthSettings);

            services.AddSingleton(facebookAuthSettings);
            services.AddHttpClient();
            services.AddScoped<IFacebookAuthService, FacebookAuthService>();
        }
    }
}
