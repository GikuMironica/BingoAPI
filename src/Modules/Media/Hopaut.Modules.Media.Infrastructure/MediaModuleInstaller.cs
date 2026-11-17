using Amazon;
using Amazon.S3;
using FluentValidation;
using Hopaut.Modules.Media.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hopaut.Modules.Media.Infrastructure;

/// <summary>Registers the Media module's services; handlers are registered by the Host's AddMediator.</summary>
public static class MediaModuleInstaller
{
    public static IServiceCollection AddMediaModule(this IServiceCollection services, IConfiguration configuration)
    {
        // S3 options (constraints arrive with the Media adapter hardening, F-031 → plan Step 8.10)
        services.AddOptions<S3MediaStorageOptions>()
            .BindConfiguration("AwsBucketSettings")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // S3 client as singleton, built from the bound options when first resolved
        services.AddSingleton<IAmazonS3>(provider =>
        {
            var s3Options = provider.GetRequiredService<IOptions<S3MediaStorageOptions>>().Value;
            return new AmazonS3Client(s3Options.AccessKeyId, s3Options.SecretAccessKey, RegionEndpoint.GetBySystemName(s3Options.Region));
        });

        // Services
        services.AddScoped<IMediaStorage, S3MediaStorage>();
        services.AddSingleton<IImageProcessor, ImageSharpProcessor>();

        // Validators; the handler is registered by the Host's AddMediator.
        services.AddValidatorsFromAssemblyContaining<IMediaCommand>(includeInternalTypes: true);

        return services;
    }
}
