using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BingoAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {          
            var host = CreateHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // create Admin, User, Super Admin role if doesnt exist, create
                if (!await roleManager.RoleExistsAsync("User"))
                {
                     var userRole = new IdentityRole("User");
                     await roleManager.CreateAsync(userRole);
                }
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    var adminRole = new IdentityRole("Admin");
                    await roleManager.CreateAsync(adminRole);
                }
                if (!await roleManager.RoleExistsAsync("SuperAdmin"))
                {
                    var superadminRole = new IdentityRole("SuperAdmin");
                    await roleManager.CreateAsync(superadminRole);
                }

            }
            await host.RunAsync();
           
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
