$ErrorActionPreference = "Continue"

$repairRoot = "D:\DREAM CORE\UnityInstallRepair"
$projectRoot = "D:\DREAM CORE"
$targetFolder = "C:\Program Files\Unity\Hub\Editor\6000.5.2f1"
$licensingClient = Join-Path $targetFolder "Editor\Data\Resources\Licensing\Client\Unity.Licensing.Client.exe"
$logPath = Join-Path $repairRoot "force_cleanup_log.txt"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

New-Item -ItemType Directory -Force -Path $repairRoot | Out-Null

function Write-Log {
    param([string]$Message)
    $line = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') $Message"
    Write-Host $Message
    Add-Content -LiteralPath $logPath -Value $line
}

function Test-IsAdmin {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

"=== Force Unity cleanup log started $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') ===" | Set-Content -LiteralPath $logPath

$isAdmin = Test-IsAdmin
Write-Log "Admin status: $isAdmin"

if (-not $isAdmin) {
    Write-Log "ERROR: This script must be run as administrator. Exiting without changes."
    Write-Host ""
    Write-Host "Run D:\DREAM CORE\UnityInstallRepair\RUN_AS_ADMIN_CLEAN_UNITY.cmd and approve the UAC prompt."
    exit 1
}

$cacheTargets = @(
    @{ Path = Join-Path $env:APPDATA "UnityHub"; BackupName = "UnityHub_backup_$timestamp" },
    @{ Path = Join-Path $env:APPDATA "Unity"; BackupName = "Unity_roaming_backup_$timestamp" },
    @{ Path = Join-Path $env:LOCALAPPDATA "Unity"; BackupName = "Unity_local_backup_$timestamp" }
)

Write-Host "============================================================"
Write-Host " FORCE CLEAN BROKEN UNITY INSTALL"
Write-Host "============================================================"
Write-Host ""
Write-Host "Target folder to delete:"
Write-Host "  $targetFolder"
Write-Host ""
Write-Host "Cache folders to rename:"
foreach ($item in $cacheTargets) {
    Write-Host "  $($item.Path)"
}
Write-Host ""
Write-Host "Project that must NOT be touched:"
Write-Host "  $projectRoot"
Write-Host ""

$confirmation = Read-Host "Type FORCE CLEAN UNITY to proceed"
if ($confirmation -ne "FORCE CLEAN UNITY") {
    Write-Log "Cancelled by user. No cleanup performed."
    exit 0
}

Write-Log "User confirmed force cleanup."
if (Test-Path -LiteralPath $projectRoot) {
    Write-Log "DREAM CORE exists and will not be touched: $projectRoot"
} else {
    Write-Log "WARNING: DREAM CORE path not found during pre-check: $projectRoot"
}

Write-Log "Stopping Unity-related processes."
$processNames = @(
    "Unity Hub",
    "Unity",
    "UnityCrashHandler64",
    "UnityCrashHandler32",
    "Unity.Licensing.Client"
)

foreach ($name in $processNames) {
    $processes = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($processes) {
        foreach ($process in $processes) {
            try {
                Write-Log "Stopping process $($process.ProcessName) PID $($process.Id)"
                Stop-Process -Id $process.Id -Force -ErrorAction Stop
            } catch {
                Write-Log "WARNING: Failed to stop $($process.ProcessName) PID $($process.Id): $($_.Exception.Message)"
            }
        }
    } else {
        Write-Log "Process not running: $name"
    }
}

Write-Log "Inspecting node.exe processes for Unity Hub command lines."
try {
    $nodeProcesses = Get-CimInstance Win32_Process -Filter "Name='node.exe'" -ErrorAction Stop |
        Where-Object { $_.CommandLine -match "UnityHub|Unity Hub|unityhub" }

    if ($nodeProcesses) {
        foreach ($node in $nodeProcesses) {
            try {
                Write-Log "Stopping Unity Hub-related node.exe PID $($node.ProcessId): $($node.CommandLine)"
                Stop-Process -Id $node.ProcessId -Force -ErrorAction Stop
            } catch {
                Write-Log "WARNING: Failed to stop node.exe PID $($node.ProcessId): $($_.Exception.Message)"
            }
        }
    } else {
        Write-Log "No Unity Hub-related node.exe processes found."
    }
} catch {
    Write-Log "WARNING: Could not inspect node.exe processes: $($_.Exception.Message)"
}

if (Test-Path -LiteralPath $targetFolder) {
    Write-Log "Repairing ownership and permissions for: $targetFolder"

    Write-Log "Running takeown."
    $takeownOutput = & takeown.exe /F "$targetFolder" /R /D Y 2>&1
    $takeownExit = $LASTEXITCODE
    Add-Content -LiteralPath $logPath -Value $takeownOutput
    Write-Log "takeown exit code: $takeownExit"

    Write-Log "Running icacls grant Administrators:F."
    $icaclsOutput = & icacls.exe "$targetFolder" /grant "Administrators:F" /T 2>&1
    $icaclsExit = $LASTEXITCODE
    Add-Content -LiteralPath $logPath -Value $icaclsOutput
    Write-Log "icacls exit code: $icaclsExit"

    Write-Log "Clearing read-only/system/hidden attributes."
    $attribOutput = & attrib.exe -R -S -H "$targetFolder\*" /S /D 2>&1
    $attribExit = $LASTEXITCODE
    Add-Content -LiteralPath $logPath -Value $attribOutput
    Write-Log "attrib exit code: $attribExit"

    Write-Log "Attempting Remove-Item deletion."
    try {
        Remove-Item -LiteralPath $targetFolder -Recurse -Force -ErrorAction Stop
        Write-Log "Remove-Item completed."
    } catch {
        Write-Log "WARNING: Remove-Item failed: $($_.Exception.Message)"
        Write-Log "Attempting cmd rmdir fallback."
        $rmdirOutput = & cmd.exe /c rmdir /s /q "$targetFolder" 2>&1
        $rmdirExit = $LASTEXITCODE
        Add-Content -LiteralPath $logPath -Value $rmdirOutput
        Write-Log "rmdir exit code: $rmdirExit"
    }

    if (Test-Path -LiteralPath $targetFolder) {
        Write-Log "BROKEN UNITY FOLDER STILL EXISTS after deletion attempts."
        Write-Log "Remaining files/folders, first 200:"
        Get-ChildItem -LiteralPath $targetFolder -Recurse -Force -ErrorAction SilentlyContinue |
            Select-Object -First 200 FullName, Length, LastWriteTime |
            ForEach-Object { Add-Content -LiteralPath $logPath -Value ("REMAINING: " + $_.FullName) }
        Write-Log "A reboot is required. Rerun this script immediately after reboot before opening Unity Hub."
    } else {
        Write-Log "BROKEN UNITY FOLDER REMOVED."
    }
} else {
    Write-Log "Target folder not present before deletion: $targetFolder"
}

Write-Log "Renaming Unity cache folders if present."
foreach ($item in $cacheTargets) {
    $source = $item.Path
    if (-not (Test-Path -LiteralPath $source)) {
        Write-Log "Cache folder not present: $source"
        continue
    }

    $parent = Split-Path -Parent $source
    $destination = Join-Path $parent $item.BackupName
    $counter = 1
    while (Test-Path -LiteralPath $destination) {
        $destination = Join-Path $parent "$($item.BackupName)_$counter"
        $counter++
    }

    try {
        Write-Log "Renaming cache folder: $source -> $destination"
        Rename-Item -LiteralPath $source -NewName (Split-Path -Leaf $destination) -ErrorAction Stop
    } catch {
        Write-Log "WARNING: Failed to rename cache folder ${source}: $($_.Exception.Message)"
    }
}

$folderExists = Test-Path -LiteralPath $targetFolder
$clientExists = Test-Path -LiteralPath $licensingClient
$projectExists = Test-Path -LiteralPath $projectRoot

Write-Log "Verification: target folder exists = $folderExists"
Write-Log "Verification: licensing client exists = $clientExists"
Write-Log "Verification: DREAM CORE exists = $projectExists"

Write-Host ""
if ($folderExists) {
    Write-Host "BROKEN UNITY FOLDER STILL EXISTS"
} else {
    Write-Host "BROKEN UNITY FOLDER REMOVED"
}

if ($projectExists) {
    Write-Host "DREAM CORE SAFE"
}

Write-Host ""
if (-not $folderExists -and $projectExists) {
    Write-Log "Final recommendation: cleanup succeeded. Reboot, then install Unity 6.3 LTS with minimal modules."
    Write-Host "CLEANUP COMPLETE."
    Write-Host "REBOOT WINDOWS NOW."
    Write-Host "After reboot:"
    Write-Host "1. Open Unity Hub as administrator."
    Write-Host "2. Install Unity 6.3 LTS with minimal modules:"
    Write-Host "   - Unity Editor"
    Write-Host "   - Microsoft Visual Studio Community"
    Write-Host "   - Windows Build Support"
    Write-Host "3. First test a blank Universal 3D project."
    Write-Host "4. Only after the blank project opens, open D:\DREAM CORE."
} else {
    Write-Log "Final recommendation: cleanup incomplete. Reboot and rerun immediately before opening Unity Hub."
    Write-Host "CLEANUP INCOMPLETE."
    Write-Host "REBOOT WINDOWS."
    Write-Host "DO NOT OPEN UNITY HUB."
    Write-Host "RUN THIS SCRIPT AGAIN AS ADMIN IMMEDIATELY AFTER REBOOT."
}
