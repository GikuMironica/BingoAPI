using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BingoAPI.Installers
{
    public class McInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();

            services.AddSwaggerGen(x =>
            {
                // register Swagger document generator, defining >1 Swagger documents
                x.SwaggerDoc("v1", new OpenApiInfo { Title = "Bingo REST API", Version = "v1" });
            });
        }
    }
}
