using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BingoAPI.Data;
using BingoAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql;

namespace BingoAPI.Installers
{
    public class DbInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options =>
                options.UseMySql(configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<AppUser>()
                .AddEntityFrameworkStores<DataContext>();
        }
    }
}
