namespace SocialVideoUploader.Api.Configurations;

public sealed class StorageConfiguration
{
    public const string SectionName = "Storage";

    public string RootPath { get; set; } = "App_Data\\storage";

    public string RequestPath { get; set; } = "/media";

    public string VideosFolder { get; set; } = "videos";

    public string ThumbnailsFolder { get; set; } = "thumbnails";

    public string VariantsFolder { get; set; } = "variants";

    public long MaxVideoBytes { get; set; } = 536_870_912;

    public long MaxThumbnailBytes { get; set; } = 10_485_760;
}
