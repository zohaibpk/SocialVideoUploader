# SocialVideoUploader

SocialVideoUploader is an ASP.NET 10 + React application for **upload-once, publish-many** video workflows. The current build focuses on the preparation pipeline: ingesting source media, inspecting it, validating platform constraints, generating platform-specific variants, and preparing the application for future OAuth and publish-job orchestration.

## Current status

This repository already includes:

- A **.NET 10 Web API** backend for platform metadata, validation, uploads, inspection, and variant generation
- A **React + TypeScript + Vite** frontend for the planning and processing workspace
- A **project-local FFmpeg/ffprobe setup** for local variant generation
- Platform modeling for:
  - YouTube
  - Facebook Pages
  - Instagram Professional
  - LinkedIn
  - Vimeo
  - Dailymotion
  - Threads
  - Tumblr
  - Telegram Channels

This repository does **not** yet include real OAuth connection flows or real publishing to platform APIs. The current focus is preparing and generating platform-safe media outputs.

## What the app does today

1. Load platform rules, limits, and metadata for supported destinations
2. Build and edit an upload draft with shared fields and per-platform overrides
3. Upload a source video and optional thumbnail
4. Inspect uploaded media with `ffprobe`
5. Validate the draft against platform limits
6. Generate output variants with FFmpeg, including:
   - trim start/end
   - text watermark overlay
   - profile-based resize and pad
   - FPS normalization
   - Telegram compact output

## Solution structure

```text
SocialVideoUploader.sln
src/
  SocialVideoUploader.Api/    ASP.NET Core Web API
  SocialVideoUploader.Web/    React + Vite frontend
tools/
  ffmpeg/                     Project-local FFmpeg installer and binaries
```

## Quick start

### Prerequisites

- Windows
- .NET SDK 10
- Node.js + npm

### 1. Restore/install dependencies

```powershell
Set-Location C:\Users\zohai\.projects\SocialVideoUploader
npm --prefix .\src\SocialVideoUploader.Web install
dotnet restore .\SocialVideoUploader.sln
```

### 2. Install FFmpeg locally

This project is configured to use FFmpeg from the repository instead of relying on a machine-wide install.

```powershell
Set-Location C:\Users\zohai\.projects\SocialVideoUploader
powershell -ExecutionPolicy Bypass -File .\tools\ffmpeg\Install-FFmpeg.ps1
```

### 3. Run the API

```powershell
Set-Location C:\Users\zohai\.projects\SocialVideoUploader
dotnet run --project .\src\SocialVideoUploader.Api\SocialVideoUploader.Api.csproj --launch-profile http
```

API base URL:

- `http://localhost:5002`

### 4. Run the frontend

```powershell
Set-Location C:\Users\zohai\.projects\SocialVideoUploader\src\SocialVideoUploader.Web
npm run dev
```

Frontend URL:

- `http://127.0.0.1:5173`

## Key API endpoints

| Endpoint | Purpose |
| --- | --- |
| `GET /api/system/status` | Health/status check |
| `GET /api/uploads/form` | Load the upload workspace model |
| `POST /api/uploads/validate` | Validate the current upload draft |
| `POST /api/uploads/assets/video` | Upload a source video |
| `POST /api/uploads/assets/thumbnail` | Upload a thumbnail/cover image |
| `GET /api/uploads/process/profiles` | List output profiles |
| `POST /api/uploads/process/variants` | Generate FFmpeg variants |
| `/media/...` | Static access to stored videos, thumbnails, and generated variants |

## Architecture notes

- `Platforms/` contains **one file per platform publisher**
- Platform API settings are grouped inside a root `PlatformsConfiguration`
- Storage and media tools are configured through `appsettings.json`
- Variant generation is profile-driven through `VideoProcessingProfileRegistry`
- The frontend talks to the API through Vite proxies for `/api` and `/media`

## Local storage

Uploaded and generated media is stored under:

```text
src\SocialVideoUploader.Api\App_Data\storage
```

This includes:

- `videos/`
- `thumbnails/`
- `variants/`

## Roadmap

Planned next steps include:

- OAuth and connected-platform account storage
- publish-job orchestration and retry tracking
- persistent draft/history storage
- real platform publishing adapters
- authentication and upload management

## Project-specific docs

- [API README](src/SocialVideoUploader.Api/README.md)
- [Web README](src/SocialVideoUploader.Web/README.md)
