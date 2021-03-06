using System.Net;
using AspNetCoreRateLimit;
using AutoMapper;
using BingoAPI.Extensions;
using BingoAPI.Middleware;
using BingoAPI.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BingoAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // extension method
            services.InstallServicesInAssembly(Configuration);
            services.AddAutoMapper(typeof(Startup));
            services.AddOptions();

            services.AddCors(o =>
            {
                o.AddPolicy(name: MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200", "http://localhost:3000");
                    });
            });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // disable for testing
            app.UseIpRateLimiting();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware(typeof(ErrorHandlingMiddleware));
                //app.UseExceptionHandler("/Error");
                //app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            // Enable/Disable swagger from appsettings.json
            var environmentOptions = new Options.EnvironmentOptions();
            Configuration.GetSection(nameof(EnvironmentOptions)).Bind(environmentOptions);
            if (environmentOptions.Swagger!=0)
            {
                var swaggerOptions = new Options.SwaggerOptions();
                Configuration.GetSection(nameof(swaggerOptions)).Bind(swaggerOptions);

                app.UseSwagger(option =>
                {
                    option.RouteTemplate = swaggerOptions.JsonRoute;
                });

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(swaggerOptions.UiEndpoint, swaggerOptions.Description);
                });
            } 
            
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

           //  app.Run(async (context) =>
           //  {
           //     await context.Response.WriteAsync(Configuration["Con:Conect"]);
           //  });
        }
    }
}
