using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using SocialVideoUploader.Api.Configurations;
using SocialVideoUploader.Api.Infrastructure;
using SocialVideoUploader.Api.Platforms;
using SocialVideoUploader.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<PlatformsConfiguration>(
    builder.Configuration.GetSection(PlatformsConfiguration.SectionName));
builder.Services.Configure<StorageConfiguration>(
    builder.Configuration.GetSection(StorageConfiguration.SectionName));
builder.Services.Configure<MediaToolsConfiguration>(
    builder.Configuration.GetSection(MediaToolsConfiguration.SectionName));
builder.Services.PostConfigure<MediaToolsConfiguration>(configuration =>
{
    configuration.FfmpegPath = MediaToolPathResolver.ResolveExecutablePath(builder.Environment, configuration.FfmpegPath);
    configuration.FfprobePath = MediaToolPathResolver.ResolveExecutablePath(builder.Environment, configuration.FfprobePath);
    configuration.DefaultFontFile = MediaToolPathResolver.ResolveOptionalFilePath(builder.Environment, configuration.DefaultFontFile);
});
builder.Services.AddSingleton<AssetStorageService>();
builder.Services.AddSingleton<MediaInspectionService>();
builder.Services.AddSingleton<VideoProcessingProfileRegistry>();
builder.Services.AddSingleton<VideoProcessingService>();
builder.Services.AddSingleton<UploadPlanningService>();
builder.Services.AddSingleton<IPlatformPublisher, YouTubePlatformPublisher>();
builder.Services.AddSingleton<IPlatformPublisher, FacebookPlatformPublisher>();
builder.Services.AddSingleton<IPlatformPublisher, InstagramPlatformPublisher>();
builder.Services.AddSingleton<IPlatformPublisher, LinkedInPlatformPublisher>();
builder.Services.AddSingleton<IPlatformPublisher, VimeoPlatformPublisher>();
builder.Services.AddSingleton<IPlatformPublisher, DailymotionPlatformPublisher>();
builder.Services.AddSingleton<IPlatformPublisher, ThreadsPlatformPublisher>();
builder.Services.AddSingleton<IPlatformPublisher, TumblrPlatformPublisher>();
builder.Services.AddSingleton<IPlatformPublisher, TelegramPlatformPublisher>();

var app = builder.Build();
var storageConfiguration = app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageConfiguration>>()
    .Value;
var storageRootPath = StoragePathResolver.ResolveRootPath(app.Environment, storageConfiguration);
Directory.CreateDirectory(storageRootPath);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storageRootPath),
    RequestPath = storageConfiguration.RequestPath
});

app.UseAuthorization();

app.MapControllers();

app.Run();
