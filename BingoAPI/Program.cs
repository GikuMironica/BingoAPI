using System;
using System.IO;
using System.Threading.Tasks;
using BingoAPI.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
namespace BingoAPI
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope())
            {
		        var dbContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();

                 await dbContext.Database.MigrateAsync();

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

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder
                     .ConfigureAppConfiguration((hostingContext, config) =>
                     {
                         config.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "wwwroot", "Configurations", "EventTypes.json"), optional: false, reloadOnChange: true);
                         config.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "wwwroot", "NotificationTemplates", "NotificationLangTemplates.json"), optional: false, reloadOnChange: true);
                         config.AddJsonFile(Path.Combine(Environment.CurrentDirectory, "wwwroot", "EmailTemplates", "Emails.json"), optional: false, reloadOnChange: true);
                         config.AddEnvironmentVariables();
                     })
                     .ConfigureLogging((context, logging) =>
                     {
                         logging.ClearProviders();
                     })
                     .UseStartup<Startup>();
             })
             .UseSerilog((context, configuration) =>
             {
                 configuration
                     .ReadFrom.Configuration(context.Configuration)
                     .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                     .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                     .Enrich.FromLogContext()
                     .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
             });
    }
}
