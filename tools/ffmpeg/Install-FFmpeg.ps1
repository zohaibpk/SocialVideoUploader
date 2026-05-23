param(
    [string]$DownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$downloadsPath = Join-Path $scriptRoot "downloads"
$extractPath = Join-Path $scriptRoot "extract"
$currentPath = Join-Path $scriptRoot "current"
$archivePath = Join-Path $downloadsPath "ffmpeg-release-essentials.zip"

New-Item -ItemType Directory -Path $downloadsPath -Force | Out-Null
if (Test-Path $extractPath) {
    Remove-Item $extractPath -Recurse -Force
}

Write-Host "Downloading FFmpeg from $DownloadUrl"
Invoke-WebRequest -Uri $DownloadUrl -OutFile $archivePath

Write-Host "Extracting archive"
Expand-Archive -Path $archivePath -DestinationPath $extractPath -Force

$extractedRoot = Get-ChildItem -Path $extractPath -Directory | Select-Object -First 1
if ($null -eq $extractedRoot) {
    throw "The FFmpeg archive did not contain an extracted root directory."
}

if (Test-Path $currentPath) {
    Remove-Item $currentPath -Recurse -Force
}

Move-Item -Path $extractedRoot.FullName -Destination $currentPath

$ffmpegExe = Join-Path $currentPath "bin\\ffmpeg.exe"
$ffprobeExe = Join-Path $currentPath "bin\\ffprobe.exe"

if (-not (Test-Path $ffmpegExe) -or -not (Test-Path $ffprobeExe)) {
    throw "The expected FFmpeg binaries were not found after extraction."
}

if (Test-Path $extractPath) {
    Remove-Item $extractPath -Recurse -Force
}

Write-Host "FFmpeg installed successfully."
Write-Host "ffmpeg.exe: $ffmpegExe"
Write-Host "ffprobe.exe: $ffprobeExe"
