$targetFolder = "C:\Program Files\Unity\Hub\Editor\6000.5.2f1"
$licensingClient = Join-Path $targetFolder "Editor\Data\Resources\Licensing\Client\Unity.Licensing.Client.exe"
$projectRoot = "D:\DREAM CORE"

$folderExists = Test-Path -LiteralPath $targetFolder
$clientExists = Test-Path -LiteralPath $licensingClient
$projectExists = Test-Path -LiteralPath $projectRoot

Write-Host "============================================================"
Write-Host " VERIFY AFTER FORCE CLEAN"
Write-Host "============================================================"
Write-Host ""

Write-Host "Broken Unity folder:"
Write-Host "  $targetFolder"
Write-Host "  " -NoNewline
if ($folderExists) { Write-Host "EXISTS" } else { Write-Host "MISSING" }

Write-Host ""
Write-Host "Unity.Licensing.Client.exe:"
Write-Host "  $licensingClient"
Write-Host "  " -NoNewline
if ($clientExists) { Write-Host "EXISTS" } else { Write-Host "MISSING" }

Write-Host ""
Write-Host "DREAM CORE:"
Write-Host "  $projectRoot"
Write-Host "  " -NoNewline
if ($projectExists) { Write-Host "EXISTS" } else { Write-Host "MISSING" }

Write-Host ""
Write-Host "Unity backup folders under APPDATA:"
Get-ChildItem -LiteralPath $env:APPDATA -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^(UnityHub|Unity)(_roaming)?_backup' } |
    Select-Object FullName, LastWriteTime |
    Format-Table -AutoSize

Write-Host ""
Write-Host "Unity backup folders under LOCALAPPDATA:"
Get-ChildItem -LiteralPath $env:LOCALAPPDATA -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^Unity(_local)?_backup' } |
    Select-Object FullName, LastWriteTime |
    Format-Table -AutoSize

Write-Host ""
if (-not $folderExists -and -not $clientExists -and $projectExists) {
    Write-Host "READY FOR UNITY REINSTALL"
} else {
    Write-Host "CLEANUP STILL INCOMPLETE"
}
