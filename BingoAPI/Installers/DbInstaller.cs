using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BingoAPI.Data;
using BingoAPI.Models;
using BingoAPI.Models.SqlRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BingoAPI.Installers
{
    public class DbInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options =>
                //options.UseMySql(configuration.GetConnectionString("DefaultConnection")));
                options.UseNpgsql(configuration.GetConnectionString("PostgreConnection")));

            // configure custom Identity User
            services.AddDefaultIdentity<AppUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 2;
                options.SignIn.RequireConfirmedEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            }).AddRoles<IdentityRole>()
              .AddEntityFrameworkStores<DataContext>()
              .AddDefaultUI()
              .AddDefaultTokenProviders();

            services.AddScoped<IPostsRepository, PostRepository>();
        }
    }
}
