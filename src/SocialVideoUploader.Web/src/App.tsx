import { useEffect, useMemo, useState } from 'react'
import './App.css'

type PlatformId =
  | 'YouTube'
  | 'Facebook'
  | 'Instagram'
  | 'LinkedIn'
  | 'Vimeo'
  | 'Dailymotion'
  | 'Threads'
  | 'Tumblr'
  | 'Telegram'

type PrivacyOption = 'Public' | 'Private' | 'Unlisted'
type WatermarkPosition = 'TopLeft' | 'TopRight' | 'Center' | 'BottomLeft' | 'BottomRight'
type InstagramMediaType = 'Reels' | 'Video' | 'Story'
type ThreadsReplyControl =
  | 'Everyone'
  | 'AccountsYouFollow'
  | 'MentionedOnly'
  | 'FollowersOnly'
  | 'ParentPostAuthorOnly'
type LinkedInVisibility = 'Public' | 'LoggedIn'
type VimeoPrivacyView = 'Anybody' | 'Nobody' | 'Password' | 'Contacts'
type TumblrPostState = 'Published' | 'Draft' | 'Queue' | 'Private'
type MediaInspectionStatus = 'Complete' | 'Partial' | 'Unavailable' | 'Failed'

type UploadFormSection = {
  key: string
  title: string
  description: string
  fields: string[]
}

type PlatformDefinition = {
  id: PlatformId
  displayName: string
  destinationType: string
  description: string
  capability: {
    supportsTitle: boolean
    supportsTags: boolean
    supportsDescription: boolean
    supportsThumbnail: boolean
    supportsScheduling: boolean
    supportsPrivacy: boolean
    supportsPlatformOverrides: boolean
    supportsWatermark: boolean
    supportsTrim: boolean
  }
  limits: {
    maxTitleLength?: number
    maxDescriptionLength?: number
    maxTags?: number
    maxFileSizeMb?: number
    maxDurationSeconds?: number
    recommendedAspectRatio: string
    recommendedResolution: string
    uploadStyle: string
    supportedPrivacyValues: string[]
  }
  coreFields: string[]
  advancedFields: string[]
  notes: string[]
  connection: {
    apiBaseUrl: string
    authBaseUrl: string
    requiresPublicUrlStaging: boolean
  }
}

type UploadPlanningResponse = {
  applicationName: string
  summary: string[]
  sections: UploadFormSection[]
  platforms: PlatformDefinition[]
  draft: UploadDraftRequest
}

type UploadDraftRequest = {
  asset: {
    videoAssetId: string
    sourceFileName: string
    sourceUrl: string
    sourceDurationSeconds: number
    estimatedFileSizeMb: number
    width: number
    height: number
    frameRate: number
    inspectionStatus: string
    inspectionWarnings: string[]
  }
  basic: {
    title: string
    description: string
    tagsText: string
    platforms: PlatformId[]
    privacy: PrivacyOption
    thumbnailUrl: string
  }
  editing: {
    trimStartSeconds: number
    trimEndSeconds: number
    watermarkText: string
    watermarkPosition: WatermarkPosition
    watermarkOpacityPercent: number
  }
  advanced: {
    scheduledPublishAt?: string | null
    language: string
    youTubeCategoryId: string
    youTubeMadeForKids?: boolean | null
    instagramCoverImageUrl: string
    instagramShareToFeed: boolean
    instagramMediaType: InstagramMediaType
    linkedInVisibility: LinkedInVisibility
    vimeoPrivacyView: VimeoPrivacyView
    dailymotionChannel: string
    dailymotionExplicitContent: boolean
    threadsReplyControl: ThreadsReplyControl
    threadsTopicTag: string
    tumblrPostState: TumblrPostState
    telegramCaption: string
    telegramProtectContent: boolean
    telegramSupportsStreaming: boolean
    commentsAllowed: boolean
  }
  platformOverrides: PlatformOverride[]
}

type PlatformOverride = {
  platformId: PlatformId
  title: string
  description: string
  tagsText: string
  thumbnailUrl: string
}

type UploadValidationResponse = {
  isValid: boolean
  errors: string[]
  warnings: string[]
  suggestedActions: string[]
  platforms: {
    platformId: PlatformId
    displayName: string
    isValid: boolean
    errors: string[]
    warnings: string[]
    notes: string[]
  }[]
}

type AssetUploadResponse = {
  assetId: string
  assetType: string
  fileName: string
  contentType: string
  sizeBytes: number
  sizeMb: number
  publicUrl: string
  inspection: {
    status: MediaInspectionStatus
    source: string
    durationSeconds?: number
    width?: number
    height?: number
    frameRate?: number
    container: string
    videoCodec: string
    audioCodec: string
    warnings: string[]
  }
}

type VideoProcessingProfile = {
  id: string
  displayName: string
  description: string
  platforms: PlatformId[]
  width: number
  height: number
  targetFrameRate: number
  maxDurationSeconds?: number
  targetMaxFileSizeMb?: number
  aspectRatio: string
  notes: string
}

type GenerateVariantsResponse = {
  ffmpegAvailable: boolean
  warnings: string[]
  profiles: VideoProcessingProfile[]
  variants: {
    profileId: string
    displayName: string
    platforms: PlatformId[]
    succeeded: boolean
    outputUrl: string
    sizeBytes: number
    sizeMb: number
    commandPreview: string
    inspection: {
      status: MediaInspectionStatus
      source: string
      durationSeconds?: number
      width?: number
      height?: number
      frameRate?: number
      container: string
      videoCodec: string
      audioCodec: string
      warnings: string[]
    }
    warnings: string[]
    error: string
  }[]
}

const numberFormatter = new Intl.NumberFormat()

function App() {
  const [workspace, setWorkspace] = useState<UploadPlanningResponse | null>(null)
  const [draft, setDraft] = useState<UploadDraftRequest | null>(null)
  const [validation, setValidation] = useState<UploadValidationResponse | null>(null)
  const [profiles, setProfiles] = useState<VideoProcessingProfile[]>([])
  const [processingResult, setProcessingResult] = useState<GenerateVariantsResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [uploadingVideo, setUploadingVideo] = useState(false)
  const [uploadingThumbnail, setUploadingThumbnail] = useState(false)
  const [generatingVariants, setGeneratingVariants] = useState(false)
  const [videoUpload, setVideoUpload] = useState<AssetUploadResponse | null>(null)
  const [thumbnailUpload, setThumbnailUpload] = useState<AssetUploadResponse | null>(null)

  useEffect(() => {
    const loadWorkspace = async () => {
      try {
        const [workspaceResponse, profilesResponse] = await Promise.all([
          fetch('/api/uploads/form'),
          fetch('/api/uploads/process/profiles'),
        ])

        if (!workspaceResponse.ok) {
          throw new Error(`Workspace API returned ${workspaceResponse.status}`)
        }

        if (!profilesResponse.ok) {
          throw new Error(`Profiles API returned ${profilesResponse.status}`)
        }

        const workspacePayload = (await workspaceResponse.json()) as UploadPlanningResponse
        const profilePayload = (await profilesResponse.json()) as { profiles: VideoProcessingProfile[] }

        setWorkspace(workspacePayload)
        setDraft(workspacePayload.draft)
        setProfiles(profilePayload.profiles)
      } catch (requestError) {
        const message =
          requestError instanceof Error ? requestError.message : 'Unable to load the upload workspace'

        setError(message)
      } finally {
        setLoading(false)
      }
    }

    void loadWorkspace()
  }, [])

  const selectedPlatforms = useMemo(() => draft?.basic.platforms ?? [], [draft])

  const trimmedDuration = useMemo(() => {
    if (!draft) {
      return 0
    }

    return Math.max(draft.editing.trimEndSeconds - draft.editing.trimStartSeconds, 0)
  }, [draft])

  const selectedPlatformDefinitions = useMemo(() => {
    if (!workspace) {
      return []
    }

    return workspace.platforms.filter((platform) => selectedPlatforms.includes(platform.id))
  }, [workspace, selectedPlatforms])

  const resolvedProfiles = useMemo(() => {
    if (!draft) {
      return []
    }

    return profiles.filter((profile) => {
      if (profile.id === 'story-vertical') {
        return (
          draft.basic.platforms.includes('Instagram') &&
          draft.advanced.instagramMediaType === 'Story'
        )
      }

      if (profile.id === 'short-form-vertical') {
        return draft.basic.platforms.some((platform) =>
          ['Facebook', 'Instagram', 'Threads'].includes(platform),
        )
      }

      if (profile.id === 'long-form-horizontal') {
        return draft.basic.platforms.some((platform) =>
          ['YouTube', 'LinkedIn', 'Vimeo', 'Dailymotion'].includes(platform),
        )
      }

      if (profile.id === 'square-social') {
        return draft.basic.platforms.includes('Tumblr')
      }

      if (profile.id === 'telegram-compact') {
        return draft.basic.platforms.includes('Telegram')
      }

      return false
    })
  }, [draft, profiles])

  const handleValidate = async () => {
    if (!draft) {
      return
    }

    setSubmitting(true)
    setError(null)

    try {
      const response = await fetch('/api/uploads/validate', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(draft),
      })

      if (!response.ok) {
        throw new Error(`Validation returned ${response.status}`)
      }

      const payload = (await response.json()) as UploadValidationResponse
      setValidation(payload)
    } catch (requestError) {
      const message =
        requestError instanceof Error ? requestError.message : 'Validation request failed'

      setError(message)
    } finally {
      setSubmitting(false)
    }
  }

  const handleGenerateVariants = async () => {
    if (!draft) {
      return
    }

    setGeneratingVariants(true)
    setError(null)

    try {
      const response = await fetch('/api/uploads/process/variants', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ draft }),
      })

      if (!response.ok) {
        const message = await response.text()
        throw new Error(message || `Variant generation returned ${response.status}`)
      }

      const payload = (await response.json()) as GenerateVariantsResponse
      setProcessingResult(payload)
    } catch (requestError) {
      const message =
        requestError instanceof Error ? requestError.message : 'Variant generation failed'

      setError(message)
    } finally {
      setGeneratingVariants(false)
    }
  }

  const updateDraft = (updater: (current: UploadDraftRequest) => UploadDraftRequest) => {
    setDraft((current) => (current ? updater(current) : current))
  }

  const togglePlatform = (platformId: PlatformId) => {
    updateDraft((current) => {
      const isSelected = current.basic.platforms.includes(platformId)
      const platforms = isSelected
        ? current.basic.platforms.filter((item) => item !== platformId)
        : [...current.basic.platforms, platformId]

      return {
        ...current,
        basic: {
          ...current.basic,
          platforms,
        },
        platformOverrides: isSelected
          ? current.platformOverrides.filter((item) => item.platformId !== platformId)
          : current.platformOverrides,
      }
    })
  }

  const updateOverride = (
    platformId: PlatformId,
    key: keyof PlatformOverride,
    value: string,
  ) => {
    updateDraft((current) => {
      const existing = current.platformOverrides.find((item) => item.platformId === platformId)
      const nextOverride: PlatformOverride = {
        platformId,
        title: existing?.title ?? '',
        description: existing?.description ?? '',
        tagsText: existing?.tagsText ?? '',
        thumbnailUrl: existing?.thumbnailUrl ?? '',
        [key]: value,
      }

      const others = current.platformOverrides.filter((item) => item.platformId !== platformId)

      return {
        ...current,
        platformOverrides: [...others, nextOverride],
      }
    })
  }

  const handleVideoUpload = async (file: File | null) => {
    if (!file) {
      return
    }

    setUploadingVideo(true)
    setError(null)

    try {
      const formData = new FormData()
      formData.append('file', file)

      const response = await fetch('/api/uploads/assets/video', {
        method: 'POST',
        body: formData,
      })

      if (!response.ok) {
        const message = await response.text()
        throw new Error(message || `Video upload failed with ${response.status}`)
      }

      const payload = (await response.json()) as AssetUploadResponse
      setVideoUpload(payload)

      updateDraft((current) => ({
        ...current,
        asset: {
          ...current.asset,
          videoAssetId: payload.assetId,
          sourceFileName: payload.fileName,
          sourceUrl: payload.publicUrl,
          estimatedFileSizeMb: Number(payload.sizeMb.toFixed(2)),
          sourceDurationSeconds: payload.inspection.durationSeconds ?? current.asset.sourceDurationSeconds,
          width: payload.inspection.width ?? current.asset.width,
          height: payload.inspection.height ?? current.asset.height,
          frameRate: payload.inspection.frameRate ?? current.asset.frameRate,
          inspectionStatus: payload.inspection.status,
          inspectionWarnings: payload.inspection.warnings,
        },
      }))
    } catch (requestError) {
      const message = requestError instanceof Error ? requestError.message : 'Video upload failed'
      setError(message)
    } finally {
      setUploadingVideo(false)
    }
  }

  const handleThumbnailUpload = async (file: File | null) => {
    if (!file) {
      return
    }

    setUploadingThumbnail(true)
    setError(null)

    try {
      const formData = new FormData()
      formData.append('file', file)

      const response = await fetch('/api/uploads/assets/thumbnail', {
        method: 'POST',
        body: formData,
      })

      if (!response.ok) {
        const message = await response.text()
        throw new Error(message || `Thumbnail upload failed with ${response.status}`)
      }

      const payload = (await response.json()) as AssetUploadResponse
      setThumbnailUpload(payload)

      updateDraft((current) => ({
        ...current,
        basic: {
          ...current.basic,
          thumbnailUrl: payload.publicUrl,
        },
        advanced: {
          ...current.advanced,
          instagramCoverImageUrl:
            current.advanced.instagramCoverImageUrl || payload.publicUrl,
        },
      }))
    } catch (requestError) {
      const message =
        requestError instanceof Error ? requestError.message : 'Thumbnail upload failed'
      setError(message)
    } finally {
      setUploadingThumbnail(false)
    }
  }

  if (loading) {
    return (
      <main className="app-shell">
        <section className="panel">
          <span className="eyebrow">SocialVideoUploader</span>
          <h1>Loading the upload workspace…</h1>
        </section>
      </main>
    )
  }

  if (error && !workspace) {
    return (
      <main className="app-shell">
        <section className="panel">
          <span className="eyebrow">SocialVideoUploader</span>
          <h1>Workspace unavailable</h1>
          <p>{error}</p>
        </section>
      </main>
    )
  }

  if (!workspace || !draft) {
    return null
  }

  return (
    <main className="app-shell">
      <section className="hero">
        <div>
          <span className="eyebrow">{workspace.applicationName}</span>
          <h1>Processing workspace</h1>
          <p className="hero-copy">
            Uploaded source media can now move into a real FFmpeg-backed variant step. If FFmpeg
            is configured, the API generates output variants; otherwise the UI shows the exact
            readiness warning and planned profile set.
          </p>
        </div>
        <div className="hero-meta">
          <div className="metric-card">
            <span className="metric-label">Selected platforms</span>
            <strong>{selectedPlatforms.length}</strong>
          </div>
          <div className="metric-card">
            <span className="metric-label">Trimmed duration</span>
            <strong>{trimmedDuration.toFixed(1)}s</strong>
          </div>
          <div className="metric-card">
            <span className="metric-label">Planned profiles</span>
            <strong>{resolvedProfiles.length}</strong>
          </div>
        </div>
      </section>

      {error && (
        <section className="banner banner-danger">
          <strong>Action needed</strong>
          <p>{error}</p>
        </section>
      )}

      <section className="summary-grid">
        {workspace.summary.map((item) => (
          <article key={item} className="summary-card">
            <p>{item}</p>
          </article>
        ))}
      </section>

      <section className="workspace-grid">
        <div className="editor-column">
          <section className="panel">
            <div className="section-header">
              <div>
                <h2>Ingest</h2>
                <p>Upload source media into local API storage before processing variants.</p>
              </div>
              <span className="pill">Real file uploads</span>
            </div>

            <div className="upload-grid">
              <article className="upload-card">
                <h3>Video source</h3>
                <p>Stored under the API media root and reused by the variant-generation endpoint.</p>
                <label className="upload-input">
                  <span>{uploadingVideo ? 'Uploading video…' : 'Choose video file'}</span>
                  <input
                    type="file"
                    accept="video/*"
                    disabled={uploadingVideo}
                    onChange={(event) => {
                      const file = event.target.files?.[0] ?? null
                      void handleVideoUpload(file)
                      event.currentTarget.value = ''
                    }}
                  />
                </label>

                <dl className="asset-stats">
                  <div>
                    <dt>Stored file</dt>
                    <dd>{draft.asset.sourceFileName || 'No video uploaded yet'}</dd>
                  </div>
                  <div>
                    <dt>Inspection</dt>
                    <dd>{draft.asset.inspectionStatus}</dd>
                  </div>
                  <div>
                    <dt>Public URL</dt>
                    <dd className="truncate">{draft.asset.sourceUrl || 'No stored URL yet'}</dd>
                  </div>
                </dl>

                {draft.asset.sourceUrl ? (
                  <video className="video-preview" controls src={draft.asset.sourceUrl} />
                ) : null}

                {videoUpload?.inspection.warnings.length ? (
                  <div className="warning-box">
                    <strong>Inspection notes</strong>
                    <ul>
                      {videoUpload.inspection.warnings.map((warning) => (
                        <li key={warning}>{warning}</li>
                      ))}
                    </ul>
                  </div>
                ) : null}
              </article>

              <article className="upload-card">
                <h3>Thumbnail / cover</h3>
                <p>Uploads become reusable local media URLs for general thumbnails and Instagram cover images.</p>
                <label className="upload-input">
                  <span>{uploadingThumbnail ? 'Uploading thumbnail…' : 'Choose image file'}</span>
                  <input
                    type="file"
                    accept="image/*"
                    disabled={uploadingThumbnail}
                    onChange={(event) => {
                      const file = event.target.files?.[0] ?? null
                      void handleThumbnailUpload(file)
                      event.currentTarget.value = ''
                    }}
                  />
                </label>

                {draft.basic.thumbnailUrl ? (
                  <img className="thumbnail-preview" src={draft.basic.thumbnailUrl} alt="Uploaded thumbnail preview" />
                ) : (
                  <p className="muted-copy">No thumbnail uploaded yet.</p>
                )}

                {thumbnailUpload ? (
                  <p className="muted-copy">Stored as {thumbnailUpload.fileName}</p>
                ) : null}
              </article>
            </div>
          </section>

          <section className="panel">
            <div className="section-header">
              <div>
                <h2>Basic upload fields</h2>
                <p>Shared content that most platforms can reuse before overrides.</p>
              </div>
              <span className="pill">{workspace.sections.find((item) => item.key === 'basic')?.fields.length ?? 0} fields</span>
            </div>

            <div className="form-grid">
              <label className="field full">
                <span>Title</span>
                <input
                  value={draft.basic.title}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      basic: { ...current.basic, title: event.target.value },
                    }))
                  }
                />
              </label>

              <label className="field full">
                <span>Description / Caption</span>
                <textarea
                  rows={5}
                  value={draft.basic.description}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      basic: { ...current.basic, description: event.target.value },
                    }))
                  }
                />
              </label>

              <label className="field">
                <span>Tags</span>
                <input
                  value={draft.basic.tagsText}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      basic: { ...current.basic, tagsText: event.target.value },
                    }))
                  }
                />
              </label>

              <label className="field">
                <span>Privacy</span>
                <select
                  value={draft.basic.privacy}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      basic: {
                        ...current.basic,
                        privacy: event.target.value as PrivacyOption,
                      },
                    }))
                  }
                >
                  <option value="Public">Public</option>
                  <option value="Private">Private</option>
                  <option value="Unlisted">Unlisted</option>
                </select>
              </label>
            </div>

            <div className="platform-picker">
              {workspace.platforms.map((platform) => {
                const checked = selectedPlatforms.includes(platform.id)

                return (
                  <label key={platform.id} className={`platform-chip ${checked ? 'selected' : ''}`}>
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={() => togglePlatform(platform.id)}
                    />
                    <span>{platform.displayName}</span>
                  </label>
                )
              })}
            </div>
          </section>

          <section className="panel">
            <div className="section-header">
              <div>
                <h2>Editing</h2>
                <p>Trim, watermark, and source metadata that flow into FFmpeg profile generation.</p>
              </div>
            </div>

            <div className="form-grid">
              <label className="field">
                <span>Source duration (seconds)</span>
                <input
                  type="number"
                  min={1}
                  value={draft.asset.sourceDurationSeconds}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      asset: { ...current.asset, sourceDurationSeconds: Number(event.target.value) },
                    }))
                  }
                />
              </label>

              <label className="field">
                <span>Estimated source size (MB)</span>
                <input
                  type="number"
                  min={1}
                  value={draft.asset.estimatedFileSizeMb}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      asset: { ...current.asset, estimatedFileSizeMb: Number(event.target.value) },
                    }))
                  }
                />
              </label>

              <label className="field">
                <span>Frame rate</span>
                <input
                  type="number"
                  min={1}
                  step={0.01}
                  value={draft.asset.frameRate}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      asset: { ...current.asset, frameRate: Number(event.target.value) },
                    }))
                  }
                />
              </label>

              <label className="field">
                <span>Trim start</span>
                <input
                  type="number"
                  min={0}
                  step={0.1}
                  value={draft.editing.trimStartSeconds}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      editing: { ...current.editing, trimStartSeconds: Number(event.target.value) },
                    }))
                  }
                />
              </label>

              <label className="field">
                <span>Trim end</span>
                <input
                  type="number"
                  min={0}
                  step={0.1}
                  value={draft.editing.trimEndSeconds}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      editing: { ...current.editing, trimEndSeconds: Number(event.target.value) },
                    }))
                  }
                />
              </label>

              <label className="field full">
                <span>Watermark text</span>
                <input
                  maxLength={60}
                  value={draft.editing.watermarkText}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      editing: { ...current.editing, watermarkText: event.target.value },
                    }))
                  }
                />
              </label>

              <label className="field">
                <span>Watermark position</span>
                <select
                  value={draft.editing.watermarkPosition}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      editing: {
                        ...current.editing,
                        watermarkPosition: event.target.value as WatermarkPosition,
                      },
                    }))
                  }
                >
                  <option value="TopLeft">Top left</option>
                  <option value="TopRight">Top right</option>
                  <option value="Center">Center</option>
                  <option value="BottomLeft">Bottom left</option>
                  <option value="BottomRight">Bottom right</option>
                </select>
              </label>

              <label className="field">
                <span>Watermark opacity (%)</span>
                <input
                  type="number"
                  min={10}
                  max={100}
                  value={draft.editing.watermarkOpacityPercent}
                  onChange={(event) =>
                    updateDraft((current) => ({
                      ...current,
                      editing: {
                        ...current.editing,
                        watermarkOpacityPercent: Number(event.target.value),
                      },
                    }))
                  }
                />
              </label>
            </div>
          </section>

          <section className="panel">
            <div className="section-header">
              <div>
                <h2>Per-platform overrides</h2>
                <p>Use different copy per selected destination without changing the shared draft.</p>
              </div>
            </div>

            <div className="override-stack">
              {selectedPlatformDefinitions.map((platform) => {
                const override =
                  draft.platformOverrides.find((item) => item.platformId === platform.id) ?? {
                    platformId: platform.id,
                    title: '',
                    description: '',
                    tagsText: '',
                    thumbnailUrl: '',
                  }

                return (
                  <article key={platform.id} className="override-card">
                    <header>
                      <h3>{platform.displayName}</h3>
                      <p>{platform.description}</p>
                    </header>

                    <div className="form-grid">
                      <label className="field">
                        <span>Override title</span>
                        <input
                          value={override.title}
                          onChange={(event) =>
                            updateOverride(platform.id, 'title', event.target.value)
                          }
                        />
                      </label>

                      <label className="field">
                        <span>Override tags</span>
                        <input
                          value={override.tagsText}
                          onChange={(event) =>
                            updateOverride(platform.id, 'tagsText', event.target.value)
                          }
                        />
                      </label>

                      <label className="field full">
                        <span>Override description</span>
                        <textarea
                          rows={3}
                          value={override.description}
                          onChange={(event) =>
                            updateOverride(platform.id, 'description', event.target.value)
                          }
                        />
                      </label>
                    </div>
                  </article>
                )
              })}
            </div>
          </section>
        </div>

        <aside className="inspector-column">
          <section className="panel sticky">
            <div className="section-header">
              <div>
                <h2>Draft insights</h2>
                <p>Readiness, validation, and processing controls for the current uploaded source.</p>
              </div>
            </div>

            <dl className="stats-list">
              <div>
                <dt>Trimmed runtime</dt>
                <dd>{trimmedDuration.toFixed(1)} seconds</dd>
              </div>
              <div>
                <dt>Inspection status</dt>
                <dd>{draft.asset.inspectionStatus}</dd>
              </div>
              <div>
                <dt>Source resolution</dt>
                <dd>
                  {numberFormatter.format(draft.asset.width)} × {numberFormatter.format(draft.asset.height)}
                </dd>
              </div>
              <div>
                <dt>Frame rate</dt>
                <dd>{draft.asset.frameRate} FPS</dd>
              </div>
            </dl>

            <div className="button-row">
              <button className="primary-action" type="button" onClick={handleValidate} disabled={submitting}>
                {submitting ? 'Validating…' : 'Validate draft'}
              </button>
              <button
                className="secondary-action"
                type="button"
                onClick={handleGenerateVariants}
                disabled={generatingVariants || !draft.asset.videoAssetId}
              >
                {generatingVariants ? 'Generating…' : 'Generate variants'}
              </button>
            </div>

            {draft.asset.inspectionWarnings.length > 0 && (
              <div className="message-group">
                <h3>Inspection warnings</h3>
                <ul>
                  {draft.asset.inspectionWarnings.map((warning) => (
                    <li key={warning}>{warning}</li>
                  ))}
                </ul>
              </div>
            )}

            {validation && (
              <div className={`validation-banner ${validation.isValid ? 'success' : 'danger'}`}>
                <strong>{validation.isValid ? 'Draft is valid' : 'Draft needs changes'}</strong>
                <p>
                  {validation.isValid
                    ? 'The selected platforms can accept the current draft.'
                    : 'Fix the listed blocking issues before moving into processing or publish jobs.'}
                </p>
              </div>
            )}

            {processingResult && (
              <div className={`validation-banner ${processingResult.ffmpegAvailable ? 'success' : 'danger'}`}>
                <strong>
                  {processingResult.ffmpegAvailable
                    ? 'Processing attempt finished'
                    : 'FFmpeg is not ready yet'}
                </strong>
                <p>
                  {processingResult.ffmpegAvailable
                    ? 'Generated variants and readiness notes are shown below.'
                    : 'Install or configure FFmpeg and then rerun generation.'}
                </p>
              </div>
            )}
          </section>

          <section className="panel">
            <div className="section-header">
              <div>
                <h2>Resolved profiles</h2>
                <p>Profiles selected from the registry based on the chosen platforms and Instagram mode.</p>
              </div>
            </div>

            <div className="profile-stack">
              {resolvedProfiles.map((profile) => (
                <article key={profile.id} className="profile-card">
                  <h3>{profile.displayName}</h3>
                  <p>{profile.description}</p>
                  <dl className="platform-limits">
                    <div>
                      <dt>Output</dt>
                      <dd>
                        {profile.width}×{profile.height} • {profile.aspectRatio}
                      </dd>
                    </div>
                    <div>
                      <dt>Frame rate</dt>
                      <dd>{profile.targetFrameRate} FPS</dd>
                    </div>
                    <div>
                      <dt>Max duration</dt>
                      <dd>{profile.maxDurationSeconds ? `${profile.maxDurationSeconds}s` : 'None'}</dd>
                    </div>
                    <div>
                      <dt>Target file size</dt>
                      <dd>{profile.targetMaxFileSizeMb ? `${profile.targetMaxFileSizeMb} MB` : 'None'}</dd>
                    </div>
                  </dl>
                </article>
              ))}
            </div>
          </section>

          {processingResult && (
            <section className="panel">
              <div className="section-header">
                <div>
                  <h2>Generated variants</h2>
                  <p>The API returns created outputs or readiness warnings from the FFmpeg execution layer.</p>
                </div>
              </div>

              {processingResult.warnings.length > 0 && (
                <div className="warning-box">
                  <strong>Processing warnings</strong>
                  <ul>
                    {processingResult.warnings.map((warning) => (
                      <li key={warning}>{warning}</li>
                    ))}
                  </ul>
                </div>
              )}

              <div className="variant-stack">
                {processingResult.variants.map((variant) => (
                  <article key={variant.profileId} className="variant-card">
                    <header className="platform-card-header">
                      <div>
                        <h3>{variant.displayName}</h3>
                        <p>{variant.platforms.join(', ')}</p>
                      </div>
                      <span className={`status-dot ${variant.succeeded ? 'ok' : 'bad'}`}>
                        {variant.succeeded ? 'Ready' : 'Failed'}
                      </span>
                    </header>

                    {variant.outputUrl ? (
                      <video className="video-preview" controls src={variant.outputUrl} />
                    ) : null}

                    <dl className="platform-limits">
                      <div>
                        <dt>Output URL</dt>
                        <dd className="truncate">{variant.outputUrl || 'No file generated'}</dd>
                      </div>
                      <div>
                        <dt>Size</dt>
                        <dd>{variant.sizeMb ? `${variant.sizeMb.toFixed(2)} MB` : '—'}</dd>
                      </div>
                      <div>
                        <dt>Inspection</dt>
                        <dd>{variant.inspection.status}</dd>
                      </div>
                    </dl>

                    {variant.error ? <p className="error-copy">{variant.error}</p> : null}

                    {variant.warnings.length > 0 && (
                      <div className="feedback-list">
                        <strong>Warnings</strong>
                        <ul>
                          {variant.warnings.map((warning) => (
                            <li key={warning}>{warning}</li>
                          ))}
                        </ul>
                      </div>
                    )}

                    <details className="command-preview">
                      <summary>Command preview</summary>
                      <code>{variant.commandPreview}</code>
                    </details>
                  </article>
                ))}
              </div>
            </section>
          )}
        </aside>
      </section>
    </main>
  )
}

export default App
