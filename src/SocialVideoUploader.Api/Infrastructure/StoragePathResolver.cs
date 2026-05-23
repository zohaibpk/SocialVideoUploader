using SocialVideoUploader.Api.Configurations;

namespace SocialVideoUploader.Api.Infrastructure;

public static class StoragePathResolver
{
    public static string ResolveRootPath(IHostEnvironment environment, StorageConfiguration configuration)
    {
        return Path.IsPathRooted(configuration.RootPath)
            ? configuration.RootPath
            : Path.Combine(environment.ContentRootPath, configuration.RootPath);
    }
}
