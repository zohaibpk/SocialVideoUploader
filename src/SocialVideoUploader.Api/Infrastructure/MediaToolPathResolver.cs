namespace SocialVideoUploader.Api.Infrastructure;

public static class MediaToolPathResolver
{
    public static string ResolveExecutablePath(IHostEnvironment environment, string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return configuredPath;
        }

        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        var candidate = Path.GetFullPath(Path.Combine(environment.ContentRootPath, configuredPath));
        return File.Exists(candidate) ? candidate : configuredPath;
    }

    public static string ResolveOptionalFilePath(IHostEnvironment environment, string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath) || Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        var candidate = Path.GetFullPath(Path.Combine(environment.ContentRootPath, configuredPath));
        return File.Exists(candidate) ? candidate : configuredPath;
    }
}
