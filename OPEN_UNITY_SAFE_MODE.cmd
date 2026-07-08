@echo off
rem Diagnostic launcher for capturing Unity import/open errors.
rem It writes a project-open log without using Unity Hub, so failures are easier to inspect.
"C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Unity.exe" -projectPath "D:\DREAM CORE" -logFile "D:\DREAM CORE\unity-open-safe-mode.log"
