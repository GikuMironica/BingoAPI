using Amazon;
using Amazon.S3;
using Hopaut.Modules.Media.Application;
using Hopaut.Modules.Media.Application.Commands.UploadImages;
using Hopaut.Modules.Media.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hopaut.Modules.Media.Api;

public static class MediaModuleInstaller
{
    public static IServiceCollection AddMediaModule(this IServiceCollection services, IConfiguration configuration)
    {
        // S3 options
        var s3Options = new S3MediaStorageOptions();
        configuration.GetSection("AwsBucketSettings").Bind(s3Options);
        services.Configure<S3MediaStorageOptions>(configuration.GetSection("AwsBucketSettings"));

        // S3 client as singleton
        services.AddSingleton<IAmazonS3>(_ =>
            new AmazonS3Client(s3Options.AccessKeyId, s3Options.SecretAccessKey, RegionEndpoint.GetBySystemName(s3Options.Region)));

        // Services
        services.AddScoped<IMediaStorage, S3MediaStorage>();
        services.AddSingleton<IImageProcessor, ImageSharpProcessor>();

        // MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<UploadImagesCommand>());

        return services;
    }

    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        new MediaModule().MapEndpoints(endpoints);
        return endpoints;
    }
}
