using SocialVideoUploader.Api.Contracts;

namespace SocialVideoUploader.Api.Services;

public sealed class VideoProcessingProfileRegistry
{
    private static readonly VideoProcessingProfile[] Profiles =
    [
        new()
        {
            Id = "short-form-vertical",
            DisplayName = "Short-form vertical",
            Description = "For Instagram Reels, Facebook Reels, and Threads video posts.",
            Platforms = [PlatformId.Facebook, PlatformId.Instagram, PlatformId.Threads],
            Width = 1080,
            Height = 1920,
            TargetFrameRate = 30,
            MaxDurationSeconds = 90,
            TargetMaxFileSizeMb = 200,
            AspectRatio = "9:16",
            Notes = "Applies trim, pad/crop, and watermark after resizing."
        },
        new()
        {
            Id = "story-vertical",
            DisplayName = "Story vertical",
            Description = "For Instagram Story uploads with a hard 60-second limit.",
            Platforms = [PlatformId.Instagram],
            Width = 1080,
            Height = 1920,
            TargetFrameRate = 30,
            MaxDurationSeconds = 60,
            TargetMaxFileSizeMb = 95,
            AspectRatio = "9:16",
            Notes = "Only used when Instagram media type is Story."
        },
        new()
        {
            Id = "long-form-horizontal",
            DisplayName = "Long-form horizontal",
            Description = "For YouTube, LinkedIn, Vimeo, and Dailymotion uploads.",
            Platforms = [PlatformId.YouTube, PlatformId.LinkedIn, PlatformId.Vimeo, PlatformId.Dailymotion],
            Width = 1920,
            Height = 1080,
            TargetFrameRate = 30,
            MaxDurationSeconds = 1800,
            TargetMaxFileSizeMb = 450,
            AspectRatio = "16:9",
            Notes = "General long-form profile with faststart MP4 output."
        },
        new()
        {
            Id = "square-social",
            DisplayName = "Square social",
            Description = "For Tumblr-style square publishing and later square social surfaces.",
            Platforms = [PlatformId.Tumblr],
            Width = 1080,
            Height = 1080,
            TargetFrameRate = 30,
            MaxDurationSeconds = 600,
            TargetMaxFileSizeMb = 300,
            AspectRatio = "1:1",
            Notes = "Uses padded square output for broad compatibility."
        },
        new()
        {
            Id = "telegram-compact",
            DisplayName = "Telegram compact",
            Description = "Compact MP4 output designed to fit the Bot API file-size ceiling.",
            Platforms = [PlatformId.Telegram],
            Width = 854,
            Height = 480,
            TargetFrameRate = 30,
            MaxDurationSeconds = 180,
            TargetMaxFileSizeMb = 45,
            AspectRatio = "16:9",
            Notes = "Uses lower bitrate and lower resolution to target <50 MB."
        }
    ];

    public IReadOnlyList<VideoProcessingProfile> GetProfiles()
    {
        return Profiles;
    }

    public IReadOnlyList<VideoProcessingProfile> ResolveProfiles(UploadDraftRequest draft)
    {
        var selected = new List<VideoProcessingProfile>();

        foreach (var platform in draft.Basic.Platforms.Distinct())
        {
            if (platform == PlatformId.Instagram && draft.Advanced.InstagramMediaType == InstagramMediaType.Story)
            {
                AddIfMissing(selected, "story-vertical");
                continue;
            }

            if (platform == PlatformId.Instagram || platform == PlatformId.Facebook || platform == PlatformId.Threads)
            {
                AddIfMissing(selected, "short-form-vertical");
                continue;
            }

            if (platform == PlatformId.YouTube
                || platform == PlatformId.LinkedIn
                || platform == PlatformId.Vimeo
                || platform == PlatformId.Dailymotion)
            {
                AddIfMissing(selected, "long-form-horizontal");
                continue;
            }

            if (platform == PlatformId.Tumblr)
            {
                AddIfMissing(selected, "square-social");
                continue;
            }

            if (platform == PlatformId.Telegram)
            {
                AddIfMissing(selected, "telegram-compact");
            }
        }

        return selected;
    }

    private static void AddIfMissing(ICollection<VideoProcessingProfile> profiles, string profileId)
    {
        var profile = Profiles.First(item => item.Id == profileId);

        if (profiles.All(item => item.Id != profileId))
        {
            profiles.Add(profile);
        }
    }
}
