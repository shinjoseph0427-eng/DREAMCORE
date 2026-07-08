$ErrorActionPreference = "Stop"

$repairRoot = "D:\DREAM CORE\UnityInstallRepair"
$projectRoot = "D:\DREAM CORE"
$logPath = Join-Path $repairRoot "cleanup_log.txt"
$editorDir = "C:\Program Files\Unity\Hub\Editor\6000.5.2f1"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

function Write-Log {
    param([string]$Message)
    $line = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') $Message"
    Write-Host $Message
    Add-Content -LiteralPath $logPath -Value $line
}

New-Item -ItemType Directory -Force -Path $repairRoot | Out-Null
"=== Unity cleanup log started $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') ===" | Set-Content -LiteralPath $logPath

$cacheFolders = @(
    @{ Path = Join-Path $env:APPDATA "UnityHub"; Base = "UnityHub_backup_$timestamp" },
    @{ Path = Join-Path $env:APPDATA "Unity"; Base = "Unity_backup_$timestamp" },
    @{ Path = Join-Path $env:LOCALAPPDATA "Unity"; Base = "Unity_backup_$timestamp" }
)

Write-Host "============================================================"
Write-Host " CLEAN BROKEN UNITY INSTALL"
Write-Host "============================================================"
Write-Host ""
Write-Host "This script will:"
Write-Host "  - Stop Unity Hub / Unity / Unity crash handler / licensing processes."
Write-Host "  - Stop node.exe only when its command line includes UnityHub or Unity Hub."
Write-Host "  - Remove: $editorDir"
Write-Host "  - Rename Unity Hub / Unity cache folders with timestamped backup names."
Write-Host "  - Write a log to: $logPath"
Write-Host ""
if (Test-Path -LiteralPath $projectRoot) {
    Write-Host "Confirmed project exists and will NOT be touched: $projectRoot"
} else {
    Write-Host "WARNING: $projectRoot was not found, but this script still will not touch any DREAM CORE project files."
}
Write-Host ""
Write-Host "Folders that may be renamed:"
foreach ($folder in $cacheFolders) {
    Write-Host "  - $($folder.Path)"
}
Write-Host ""

$confirmation = Read-Host "Type CLEAN UNITY to proceed"
if ($confirmation -ne "CLEAN UNITY") {
    Write-Log "Cancelled by user. No cleanup performed."
    exit 0
}

Write-Log "Cleanup confirmed by user."

$processNames = @(
    "Unity Hub",
    "Unity",
    "UnityCrashHandler64",
    "UnityCrashHandler32",
    "Unity.Licensing.Client"
)

foreach ($name in $processNames) {
    $procs = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($procs) {
        foreach ($proc in $procs) {
            Write-Log "Stopping process $($proc.ProcessName) PID $($proc.Id)"
            Stop-Process -Id $proc.Id -Force
        }
    } else {
        Write-Log "Process not running: $name"
    }
}

Write-Log "Checking for Unity Hub-related node.exe processes."
$nodeProcesses = Get-CimInstance Win32_Process -Filter "Name='node.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -match "UnityHub|Unity Hub" }

if ($nodeProcesses) {
    foreach ($node in $nodeProcesses) {
        Write-Log "Stopping Unity Hub-related node.exe PID $($node.ProcessId): $($node.CommandLine)"
        Stop-Process -Id $node.ProcessId -Force
    }
} else {
    Write-Log "No Unity Hub-related node.exe processes found."
}

if (Test-Path -LiteralPath $editorDir) {
    Write-Log "Removing broken Unity editor folder: $editorDir"
    Remove-Item -LiteralPath $editorDir -Recurse -Force
    if (Test-Path -LiteralPath $editorDir) {
        Write-Log "WARNING: Editor folder still exists after removal attempt. Run PowerShell as administrator."
    } else {
        Write-Log "Removed editor folder."
    }
} else {
    Write-Log "Editor folder not present: $editorDir"
}

foreach ($folder in $cacheFolders) {
    $source = $folder.Path
    if (-not (Test-Path -LiteralPath $source)) {
        Write-Log "Cache folder not present: $source"
        continue
    }

    $parent = Split-Path -Parent $source
    $destination = Join-Path $parent $folder.Base
    $suffix = 1
    while (Test-Path -LiteralPath $destination) {
        $destination = Join-Path $parent "$($folder.Base)_$suffix"
        $suffix++
    }

    Write-Log "Renaming cache folder: $source -> $destination"
    Rename-Item -LiteralPath $source -NewName (Split-Path -Leaf $destination)
}

Write-Log "CLEANUP COMPLETE"
Write-Log "REBOOT WINDOWS NOW"
Write-Host ""
Write-Host "CLEANUP COMPLETE"
Write-Host "REBOOT WINDOWS NOW"
