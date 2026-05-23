using SocialVideoUploader.Api.Contracts;
using SocialVideoUploader.Api.Platforms;

namespace SocialVideoUploader.Api.Services;

public sealed class UploadPlanningService(IEnumerable<IPlatformPublisher> platformPublishers)
{
    private readonly IReadOnlyDictionary<PlatformId, IPlatformPublisher> _platforms = platformPublishers
        .ToDictionary(platform => platform.PlatformId);

    public UploadPlanningResponse GetForm()
    {
        return new UploadPlanningResponse
        {
            Summary =
            [
                "This workspace models the v1 upload experience before file-storage and publish adapters are connected.",
                "Editing options are applied per-platform during variant generation: trim first, then resize/crop, then watermark.",
                "Per-platform overrides let users adapt copy for YouTube, LinkedIn, Instagram, Threads, Tumblr, Telegram, and other destinations."
            ],
            Sections =
            [
                new UploadFormSection
                {
                    Key = "basic",
                    Title = "Basic",
                    Description = "Cross-platform content fields shared by most destinations.",
                    Fields = ["Title", "Description", "Tags", "Platform selection", "Privacy", "Thumbnail"]
                },
                new UploadFormSection
                {
                    Key = "editing",
                    Title = "Editing",
                    Description = "Applied before platform-specific encoding profiles are generated.",
                    Fields = ["Trim start", "Trim end", "Watermark text", "Watermark position", "Watermark opacity"]
                },
                new UploadFormSection
                {
                    Key = "advanced",
                    Title = "Advanced",
                    Description = "Platform-sensitive settings and publishing preferences.",
                    Fields = ["Schedule", "Language", "YouTube category", "Instagram media type", "Threads reply control", "Telegram supports streaming"]
                },
                new UploadFormSection
                {
                    Key = "overrides",
                    Title = "Per-platform overrides",
                    Description = "Platform-specific copy overrides for selected destinations.",
                    Fields = ["Override title", "Override description", "Override tags", "Override thumbnail"]
                }
            ],
            Platforms = _platforms.Values
                .OrderBy(platform => platform.GetDefinition().DisplayName)
                .Select(platform => platform.GetDefinition())
                .ToArray(),
            Draft = BuildDraft()
        };
    }

    public UploadValidationResponse Validate(UploadDraftRequest draft)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var suggestedActions = new List<string>();

        if (string.IsNullOrWhiteSpace(draft.Basic.Title))
        {
            errors.Add("Title is required in the shared basic section.");
        }

        if (draft.Basic.Platforms.Count == 0)
        {
            errors.Add("Select at least one destination platform.");
        }

        if (draft.Editing.TrimStartSeconds < 0)
        {
            errors.Add("Trim start cannot be negative.");
        }

        if (draft.Editing.TrimEndSeconds <= draft.Editing.TrimStartSeconds)
        {
            errors.Add("Trim end must be greater than trim start.");
        }

        if (draft.Editing.TrimEndSeconds > draft.Asset.SourceDurationSeconds)
        {
            errors.Add("Trim end cannot exceed the inspected source duration.");
        }

        if (draft.Editing.WatermarkText.Length > 60)
        {
            errors.Add("Watermark text must stay within 60 characters.");
        }

        if (draft.Asset.SourceDurationSeconds <= 0)
        {
            errors.Add("Source duration must be greater than zero.");
        }

        if (draft.Asset.EstimatedFileSizeMb <= 0)
        {
            warnings.Add("Estimated source file size is missing or zero, so file-size validation may be incomplete.");
        }

        if (draft.Asset.FrameRate > 60)
        {
            warnings.Add("High-frame-rate sources should be normalized before generating social variants.");
        }

        var trimmedDuration = draft.Editing.TrimEndSeconds - draft.Editing.TrimStartSeconds;
        var platforms = new List<PlatformValidationResult>();

        foreach (var platformId in draft.Basic.Platforms.Distinct())
        {
            if (!_platforms.TryGetValue(platformId, out var platform))
            {
                errors.Add($"Platform {platformId} is not registered.");
                continue;
            }

            var definition = platform.GetDefinition();
            var context = new PlatformSubmissionContext
            {
                Definition = definition,
                Draft = draft,
                EffectiveTitle = GetEffectiveTitle(draft, platformId),
                EffectiveDescription = GetEffectiveDescription(draft, platformId),
                EffectiveThumbnailUrl = GetEffectiveThumbnailUrl(draft, platformId),
                EffectiveTags = SplitTags(GetEffectiveTagsText(draft, platformId)),
                TrimmedDurationSeconds = trimmedDuration,
                EstimatedFileSizeMb = draft.Asset.EstimatedFileSizeMb
            };

            platforms.Add(platform.Validate(context));
        }

        if (draft.Basic.Platforms.Contains(PlatformId.Telegram))
        {
            suggestedActions.Add("Keep a Telegram-specific compact profile under 50 MB and around three minutes when possible.");
        }

        if (draft.Basic.Platforms.Contains(PlatformId.Facebook))
        {
            suggestedActions.Add("Trim reels to 90 seconds or less for Facebook Page publishing.");
        }

        if (draft.Basic.Platforms.Contains(PlatformId.Instagram)
            || draft.Basic.Platforms.Contains(PlatformId.Threads))
        {
            suggestedActions.Add("Plan for staging storage because Meta and Threads flows commonly pull video from a public URL.");
        }

        if (draft.Basic.Platforms.Count > 1)
        {
            suggestedActions.Add("Use per-platform overrides so captions match each destination's tone and length limits.");
        }

        return new UploadValidationResponse
        {
            IsValid = errors.Count == 0 && platforms.All(platform => platform.IsValid),
            Errors = errors.ToArray(),
            Warnings = warnings.ToArray(),
            SuggestedActions = suggestedActions.Distinct().ToArray(),
            Platforms = platforms.ToArray()
        };
    }

    private static UploadDraftRequest BuildDraft()
    {
        return new UploadDraftRequest
        {
            Asset = new AssetSourceInput
            {
                VideoAssetId = string.Empty,
                SourceFileName = string.Empty,
                SourceUrl = string.Empty,
                SourceDurationSeconds = 145,
                EstimatedFileSizeMb = 180,
                Width = 3840,
                Height = 2160,
                FrameRate = 60,
                InspectionStatus = "Planning",
                InspectionWarnings = []
            },
            Basic = new BasicUploadInput
            {
                Title = "How we plan one upload for every social destination",
                Description = "Walk through the creator workflow, editing controls, and platform-safe variants that SocialVideoUploader will generate before publishing.",
                TagsText = "workflow,video,creator,social media,automation",
                Platforms = [PlatformId.YouTube, PlatformId.Instagram, PlatformId.Threads, PlatformId.Telegram],
                Privacy = PrivacyOption.Private,
                ThumbnailUrl = "https://images.example.com/uploads/social-video-cover.jpg"
            },
            Editing = new EditingInput
            {
                TrimStartSeconds = 5,
                TrimEndSeconds = 75,
                WatermarkText = "@zohaibpk-work",
                WatermarkPosition = WatermarkPosition.BottomRight,
                WatermarkOpacityPercent = 80
            },
            Advanced = new AdvancedUploadInput
            {
                Language = "en",
                YouTubeCategoryId = "22",
                InstagramMediaType = InstagramMediaType.Reels,
                InstagramShareToFeed = true,
                LinkedInVisibility = LinkedInVisibility.Public,
                VimeoPrivacyView = VimeoPrivacyView.Anybody,
                DailymotionChannel = "tech",
                ThreadsReplyControl = ThreadsReplyControl.Everyone,
                TumblrPostState = TumblrPostState.Published,
                TelegramCaption = "Planning SocialVideoUploader's cross-platform publishing flow.",
                TelegramSupportsStreaming = true
            },
            PlatformOverrides =
            [
                new PlatformOverrideInput
                {
                    PlatformId = PlatformId.Threads,
                    Description = "One upload. Platform-safe variants. Smarter publishing."
                },
                new PlatformOverrideInput
                {
                    PlatformId = PlatformId.Telegram,
                    Description = "Compact cut for Telegram subscribers.",
                    ThumbnailUrl = "https://images.example.com/uploads/social-video-cover-telegram.jpg"
                }
            ]
        };
    }

    private static string GetEffectiveTitle(UploadDraftRequest draft, PlatformId platformId)
    {
        var overrideValue = draft.PlatformOverrides.FirstOrDefault(item => item.PlatformId == platformId)?.Title;
        return string.IsNullOrWhiteSpace(overrideValue) ? draft.Basic.Title : overrideValue;
    }

    private static string GetEffectiveDescription(UploadDraftRequest draft, PlatformId platformId)
    {
        if (platformId == PlatformId.Telegram && !string.IsNullOrWhiteSpace(draft.Advanced.TelegramCaption))
        {
            return draft.Advanced.TelegramCaption;
        }

        var overrideValue = draft.PlatformOverrides.FirstOrDefault(item => item.PlatformId == platformId)?.Description;
        return string.IsNullOrWhiteSpace(overrideValue) ? draft.Basic.Description : overrideValue;
    }

    private static string GetEffectiveTagsText(UploadDraftRequest draft, PlatformId platformId)
    {
        var overrideValue = draft.PlatformOverrides.FirstOrDefault(item => item.PlatformId == platformId)?.TagsText;
        return string.IsNullOrWhiteSpace(overrideValue) ? draft.Basic.TagsText : overrideValue;
    }

    private static string GetEffectiveThumbnailUrl(UploadDraftRequest draft, PlatformId platformId)
    {
        var overrideValue = draft.PlatformOverrides.FirstOrDefault(item => item.PlatformId == platformId)?.ThumbnailUrl;
        return string.IsNullOrWhiteSpace(overrideValue) ? draft.Basic.ThumbnailUrl : overrideValue;
    }

    private static IReadOnlyList<string> SplitTags(string value)
    {
        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
