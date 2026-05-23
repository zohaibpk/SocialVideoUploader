namespace SocialVideoUploader.Api.Configurations;

public sealed class MediaToolsConfiguration
{
    public const string SectionName = "MediaTools";

    public string FfmpegPath { get; set; } = "ffmpeg";

    public string FfprobePath { get; set; } = "ffprobe";

    public string DefaultFontFile { get; set; } = "C:\\Windows\\Fonts\\arial.ttf";

    public int ProcessTimeoutSeconds { get; set; } = 15;
}
