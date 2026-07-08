$ErrorActionPreference = "Stop"

$previewDir = "D:\DREAM CORE\WebPreview"
$previewUrl = "http://localhost:8777"

Write-Host "DREAM CORE Web Preview"
Write-Host ""

if (-not (Test-Path (Join-Path $previewDir "index.html"))) {
    Write-Host "WebPreview was not found or index.html is missing."
    Write-Host "Expected folder:"
    Write-Host $previewDir
    Write-Host ""
    Read-Host "Press Enter to close"
    exit 1
}

$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    Write-Host "Python is required to run the local preview."
    Write-Host "Install Python, then run this script again."
    Write-Host ""
    Read-Host "Press Enter to close"
    exit 1
}

Set-Location $previewDir
Write-Host "Open $previewUrl in your browser."
Write-Host ""

try {
    python -m http.server 8777
}
finally {
    Write-Host ""
    Read-Host "Press Enter to close"
}
