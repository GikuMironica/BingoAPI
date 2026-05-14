using System.Net;
using BingoAPI.Extensions;
using BingoAPI.Middleware;
using BingoAPI.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

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
            services.AddProblemDetails();
            services.AddExceptionHandler<Middleware.GlobalExceptionHandler>();
            services.AddHealthChecks()
                .AddDbContextCheck<Data.DataContext>("database");

            services.AddOpenTelemetry()
                .ConfigureResource(r => r.AddService("Hopaut.Api"))
                .WithTracing(b => b
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter())
                .WithMetrics(b => b
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter());

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = 429;
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = 7;
                    opt.Window = System.TimeSpan.FromSeconds(1);
                    opt.QueueLimit = 0;
                });
            });

            var corsSettings = new CorsSettings();
            Configuration.GetSection(nameof(CorsSettings)).Bind(corsSettings);

            services.AddCors(o =>
            {
                o.AddPolicy(name: MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins(corsSettings.AllowedOrigins);
                    });
            });
            var proxySettings = new ProxySettings();
            Configuration.GetSection(nameof(ProxySettings)).Bind(proxySettings);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                foreach (var ip in proxySettings.KnownProxies)
                    options.KnownProxies.Add(IPAddress.Parse(ip));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();

            var environmentOptions = new Options.EnvironmentOptions();
            Configuration.GetSection(nameof(EnvironmentOptions)).Bind(environmentOptions);

            // Rate limiting (built-in fixed window)
            if (environmentOptions.Environment > 0)
            {
                app.UseRateLimiter();
            }
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
                app.UseExceptionHandler();
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            // Enable/Disable swagger from appsettings.json
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
                endpoints.MapHealthChecks("/health");
            });

           //  app.Run(async (context) =>
           //  {
           //     await context.Response.WriteAsync(Configuration["Con:Conect"]);
           //  });
        }
    }
}
