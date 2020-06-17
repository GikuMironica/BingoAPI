using BingoAPI.Data;
using BingoAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Installers
{
    public class LoggingInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ErrorDataContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("ErrorPGConnection")));

            services.AddScoped<IErrorService, ErrorService>();
        }
    }
}
