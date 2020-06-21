using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
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
            var host = CreateWebHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // create Admin role if doesnt exist, create
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    var adminRole = new IdentityRole("Admin");
                    await roleManager.CreateAsync(adminRole);
                }

                // create User role if doesnt exist, create
                if (!await roleManager.RoleExistsAsync("User"))
                {
                    var posterRole = new IdentityRole("User");
                    await roleManager.CreateAsync(posterRole);
                }

                // create SuperAdmin role if doesnt exist, create
                if (!await roleManager.RoleExistsAsync("SuperAdmin"))
                {
                    var posterRole = new IdentityRole("SuperAdmin");
                    await roleManager.CreateAsync(posterRole);
                }
            }

            await host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
         WebHost.CreateDefaultBuilder(args)
             //attach additional JSON files
             .ConfigureAppConfiguration((hostingContext, config) =>
             {
                 config.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "wwwroot", "Configurations", "EventTypes.json"), optional: false, reloadOnChange: true);
                 config.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "wwwroot", "Notifications", "NotificationLangTemplates.json"), optional: false, reloadOnChange: true);
             })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddDebug();
                logging.AddConsole();  // eventSourec , eventlog , trace source, azureAppServiceFile, azureAppServiceBlob, Insight
            })
            .UseStartup<Startup>();
    }
}
