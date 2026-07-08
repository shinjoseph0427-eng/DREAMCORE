$editorDir = "C:\Program Files\Unity\Hub\Editor\6000.5.2f1"
$licensingClient = "C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Data\Resources\Licensing\Client\Unity.Licensing.Client.exe"
$projectRoot = "D:\DREAM CORE"

$editorExists = Test-Path -LiteralPath $editorDir
$clientExists = Test-Path -LiteralPath $licensingClient
$projectExists = Test-Path -LiteralPath $projectRoot

Write-Host "============================================================"
Write-Host " UNITY CLEANUP VERIFICATION"
Write-Host "============================================================"
Write-Host ""

Write-Host "Broken editor folder:"
Write-Host "  $editorDir"
Write-Host "  " -NoNewline
if ($editorExists) { Write-Host "EXISTS" } else { Write-Host "MISSING" }

Write-Host ""
Write-Host "Licensing client:"
Write-Host "  $licensingClient"
Write-Host "  " -NoNewline
if ($clientExists) { Write-Host "EXISTS" } else { Write-Host "MISSING" }

Write-Host ""
Write-Host "Backup folders under APPDATA:"
Get-ChildItem -LiteralPath $env:APPDATA -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^(UnityHub|Unity)_backup' } |
    Select-Object FullName, LastWriteTime |
    Format-Table -AutoSize

Write-Host ""
Write-Host "Backup folders under LOCALAPPDATA:"
Get-ChildItem -LiteralPath $env:LOCALAPPDATA -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^Unity_backup' } |
    Select-Object FullName, LastWriteTime |
    Format-Table -AutoSize

Write-Host ""
Write-Host "DREAM CORE project folder:"
Write-Host "  $projectRoot"
Write-Host "  " -NoNewline
if ($projectExists) { Write-Host "EXISTS" } else { Write-Host "MISSING" }

Write-Host ""
if (-not $editorExists -and -not $clientExists -and $projectExists) {
    Write-Host "READY FOR REINSTALL"
} else {
    Write-Host "CLEANUP INCOMPLETE"
}
