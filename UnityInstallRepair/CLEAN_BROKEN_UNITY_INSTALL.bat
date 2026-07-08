@echo off
setlocal EnableExtensions EnableDelayedExpansion

echo ============================================================
echo  CLEAN BROKEN UNITY INSTALL
echo ============================================================
echo.
echo This script will close Unity-related processes and clean the
echo broken Unity Editor / Unity Hub cache state.
echo.
echo It WILL remove:
echo   C:\Program Files\Unity\Hub\Editor\6000.5.2f1
echo.
echo It WILL rename, not permanently delete, these cache folders:
echo   %%APPDATA%%\UnityHub
echo   %%APPDATA%%\Unity
echo   %%LOCALAPPDATA%%\Unity
echo.
echo It WILL NOT delete or modify:
echo   D:\DREAM CORE
echo.
echo NOTE: This batch script cannot safely inspect every node.exe
echo command line. Use CLEAN_BROKEN_UNITY_INSTALL.ps1 for safer
echo Unity Hub-related node process detection.
echo.
set /p CONFIRM=Press Y to continue, or anything else to cancel: 
if /I not "%CONFIRM%"=="Y" (
  echo Cancelled. No changes made.
  exit /b 0
)

echo.
echo Stopping Unity-related processes if running...
call :KillProcess "Unity Hub.exe"
call :KillProcess "Unity.exe"
call :KillProcess "UnityCrashHandler64.exe"
call :KillProcess "UnityCrashHandler32.exe"
call :KillProcess "Unity.Licensing.Client.exe"

echo.
echo Checking Unity Hub-related node processes...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-CimInstance Win32_Process -Filter \"Name='node.exe'\" | Where-Object { $_.CommandLine -match 'UnityHub|Unity Hub' } | ForEach-Object { Write-Host ('Stopping Unity Hub node process PID ' + $_.ProcessId); Stop-Process -Id $_.ProcessId -Force }"
if errorlevel 1 (
  echo Could not safely inspect node command lines from batch.
  echo If Unity Hub remains stuck, run the PowerShell cleanup script instead.
)

echo.
echo Removing broken Unity editor folder if it exists...
set "EDITOR_DIR=C:\Program Files\Unity\Hub\Editor\6000.5.2f1"
if exist "%EDITOR_DIR%" (
  echo Removing "%EDITOR_DIR%"
  rmdir /s /q "%EDITOR_DIR%"
  if exist "%EDITOR_DIR%" (
    echo WARNING: Folder still exists. Run this script as administrator.
  ) else (
    echo Removed broken editor folder.
  )
) else (
  echo Not present: "%EDITOR_DIR%"
)

echo.
echo Renaming Unity Hub / Unity cache folders...
call :RenameFolder "%APPDATA%\UnityHub" "UnityHub_backup_before_reinstall"
call :RenameFolder "%APPDATA%\Unity" "Unity_backup_before_reinstall"
call :RenameFolder "%LOCALAPPDATA%\Unity" "Unity_backup_before_reinstall"

echo.
echo ============================================================
echo  CLEANUP SCRIPT FINISHED
echo ============================================================
echo.
echo Final instructions:
echo   1. Reboot Windows.
echo   2. Open Unity Hub as administrator.
echo   3. Install Unity 6.3 LTS or Unity 6.5 with minimal modules.
echo   4. Test a blank Universal 3D project before opening DREAM CORE.
echo.
echo Reminder: D:\DREAM CORE was not deleted by this script.
pause
exit /b 0

:KillProcess
set "PROC=%~1"
tasklist /fi "imagename eq %PROC%" | find /i "%PROC%" >nul
if errorlevel 1 (
  echo Not running: %PROC%
) else (
  echo Stopping: %PROC%
  taskkill /f /im "%PROC%" >nul 2>&1
)
exit /b 0

:RenameFolder
set "SRC=%~1"
set "BASE=%~2"
if not exist "%SRC%" (
  echo Not present: "%SRC%"
  exit /b 0
)
for %%I in ("%SRC%") do set "PARENT=%%~dpI"
set "DEST=!PARENT!!BASE!"
if exist "!DEST!" (
  for /f "tokens=1-6 delims=/:. " %%a in ("%date% %time%") do (
    set "STAMP=%%c%%a%%b_%%d%%e%%f"
  )
  set "STAMP=!STAMP: =0!"
  set "DEST=!PARENT!!BASE!_!STAMP!"
)
echo Renaming "%SRC%" to "!DEST!"
ren "%SRC%" "!DEST:%PARENT%=!"
exit /b 0
