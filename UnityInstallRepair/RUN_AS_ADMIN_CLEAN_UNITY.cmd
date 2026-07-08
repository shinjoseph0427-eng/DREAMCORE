@echo off
setlocal

echo ============================================================
echo  RUN FORCE UNITY CLEANUP AS ADMINISTRATOR
echo ============================================================
echo.
echo This launcher does not clean anything itself.
echo It will ask Windows to run FORCE_CLEAN_UNITY_INSTALL.ps1
echo with administrator rights.
echo.
echo You must approve the Windows UAC prompt.
echo.

set "SCRIPT_DIR=%~dp0"
set "PS_SCRIPT=%SCRIPT_DIR%FORCE_CLEAN_UNITY_INSTALL.ps1"

if not exist "%PS_SCRIPT%" (
  echo ERROR: Cannot find "%PS_SCRIPT%"
  pause
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process PowerShell -Verb RunAs -ArgumentList '-NoProfile -ExecutionPolicy Bypass -NoExit -File ""%PS_SCRIPT%""'"

echo.
echo If you approved the UAC prompt, the elevated cleanup window should now be open.
echo This launcher can be closed after the elevated window appears.
pause
