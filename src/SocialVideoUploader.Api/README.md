# SocialVideoUploader API

This project is the ASP.NET Core backend for SocialVideoUploader. It owns platform definitions, draft validation, media ingestion, inspection, and FFmpeg-based variant generation.

## Responsibilities

- Expose the upload workspace contract used by the frontend
- Validate upload drafts against platform capabilities and limits
- Accept video and thumbnail uploads
- Inspect uploaded media with `ffprobe`
- Generate output variants with FFmpeg
- Serve stored media from `/media`

## Main folders

| Folder | Purpose |
| --- | --- |
| `Configurations` | Strongly typed app settings for platforms, storage, and media tools |
| `Contracts` | Shared request/response models |
| `Controllers` | HTTP endpoints |
| `Infrastructure` | Path resolution and supporting helpers |
| `Platforms` | One platform publisher per file |
| `Services` | Validation, storage, inspection, profiles, and processing logic |

## Important runtime configuration

`appsettings.json` contains three important sections:

### `Storage`

Controls where uploaded and generated media is stored and how it is exposed through `/media`.

### `MediaTools`

Controls:

- `FfmpegPath`
- `FfprobePath`
- `DefaultFontFile`
- processing timeout

By default this project points to the repository-local FFmpeg binaries installed under:

```text
tools\ffmpeg\current\bin
```

### `Platforms`

Contains platform-specific limits and metadata, including title length, duration ceilings, file size guidance, and upload style.

## Endpoints

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/system/status` | Returns a basic API status payload |
| `GET` | `/api/uploads/form` | Returns the workspace form, default draft, and platform definitions |
| `POST` | `/api/uploads/validate` | Validates a draft against selected platform rules |
| `POST` | `/api/uploads/assets/video` | Uploads a source video into local storage |
| `POST` | `/api/uploads/assets/thumbnail` | Uploads a thumbnail image into local storage |
| `GET` | `/api/uploads/process/profiles` | Returns the processing profile catalog |
| `POST` | `/api/uploads/process/variants` | Generates output variants for the uploaded source video |

## Media pipeline

Current processing flow:

1. Save uploaded video to local storage
2. Inspect with `ffprobe`
3. Resolve output profiles from selected platforms
4. Generate variants with FFmpeg
5. Inspect generated outputs
6. Return metadata and output URLs to the frontend

Current implemented editing operations:

- trim start / trim end
- text watermark with configurable position
- resize + pad per profile
- target FPS normalization
- Telegram compact bitrate targeting

## Development

Run from the repository root:

```powershell
dotnet run --project .\src\SocialVideoUploader.Api\SocialVideoUploader.Api.csproj --launch-profile http
```

Default local URL:

- `http://localhost:5002`

## Notes

- Static files are exposed through `/media`
- OpenAPI is enabled in development
- The API currently supports preparation and processing, not live publishing to social APIs yet
