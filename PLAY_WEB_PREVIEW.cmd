@echo off
title DREAM CORE Web Preview
setlocal

set "PREVIEW_DIR=D:\DREAM CORE\WebPreview"
set "PREVIEW_URL=http://localhost:8777"

if not exist "%PREVIEW_DIR%\index.html" (
  echo WebPreview was not found or index.html is missing.
  echo Expected folder:
  echo %PREVIEW_DIR%
  echo.
  pause
  exit /b 1
)

where python >nul 2>nul
if errorlevel 1 (
  echo Python is required to run the local preview.
  echo Install Python, then run this file again.
  echo.
  pause
  exit /b 1
)

cd /d "%PREVIEW_DIR%"
echo Open %PREVIEW_URL% in your browser.
echo.
python -m http.server 8777
echo.
pause
