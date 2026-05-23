# SocialVideoUploader Web

This project is the React + TypeScript frontend for SocialVideoUploader. It provides the workspace for editing upload drafts, uploading media, validating platform readiness, and generating output variants through the API.

## What the frontend currently supports

- Load platform definitions and the default upload draft
- Edit shared metadata:
  - title
  - description
  - tags
  - privacy
- Select destination platforms
- Adjust editing inputs:
  - trim start/end
  - watermark text
  - watermark position
  - watermark opacity
- Upload source video and thumbnail
- Trigger validation
- Trigger FFmpeg variant generation
- Display resolved profiles and generated outputs

## Development

From this folder:

```powershell
npm install
npm run dev
```

Default local URL:

- `http://127.0.0.1:5173`

## Build

```powershell
npm run build
```

## Lint

```powershell
npm run lint
```

## API integration

The Vite dev server proxies the following paths to the API running on `http://localhost:5002`:

- `/api`
- `/media`

That means the frontend expects the API to be running during local development.

## Key files

| File | Purpose |
| --- | --- |
| `src/App.tsx` | Main workspace UI |
| `src/App.css` | Workspace styling |
| `src/main.tsx` | React entry point |
| `vite.config.ts` | Vite config and API/media proxy setup |
| `index.html` | App host page |

## Current UX focus

The current UI is a **processing workspace**, not a final end-user product surface yet. It is intended to help develop and verify:

- upload draft modeling
- platform-aware validation
- local media storage
- FFmpeg-driven variant generation

Future iterations will add authentication, saved drafts, connected accounts, and real publish-job workflows.
