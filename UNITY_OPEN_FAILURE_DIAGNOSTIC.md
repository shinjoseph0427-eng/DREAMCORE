# Unity Open Failure Diagnostic

Generated: 2026-07-07

## What Was Checked

- Confirmed `ProjectSettings/ProjectVersion.txt` targets Unity `6000.5.2f1`.
- Inspected `Packages/manifest.json`.
- Checked whether generated/cache folders existed:
  - `Library`
  - `Temp`
  - `Obj`
  - `Logs`
  - `UserSettings`
- Created direct Unity launcher scripts that bypass Unity Hub and write logs.
- Ran static checks only. Unity was not launched.

## What Was Changed

`Packages/manifest.json` had URP pinned to:

```json
"com.unity.render-pipelines.universal": "14.0.11"
```

That is a Unity 2022-era URP package version. It was updated to:

```json
"com.unity.render-pipelines.universal": "17.5.0"
```

This keeps URP installed while moving the project onto the Unity 6000.5 package
line. The project scripts reference URP namespaces, so removing URP entirely
would likely create compile errors.

## ProjectVersion.txt Status

`ProjectSettings/ProjectVersion.txt` was already correct:

```text
m_EditorVersion: 6000.5.2f1
m_EditorVersionWithRevision: 6000.5.2f1
```

## Manifest Status

The manifest did contain an old package version:

- Old: `com.unity.render-pipelines.universal` `14.0.11`
- New: `com.unity.render-pipelines.universal` `17.5.0`

The rest of the manifest was left intact.

## Cache Folder Cleanup

No generated/cache folders were present, so none were removed.

```text
Removed: none
Not present: Library, Temp, Obj, Logs, UserSettings
```

No project source folders were deleted.

## Open Unity Directly

Run:

```bat
OPEN_UNITY_DIRECT.cmd
```

It executes:

```bat
"C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Unity.exe" -projectPath "D:\DREAM CORE" -logFile "D:\DREAM CORE\unity-open.log"
```

The log will appear at:

```text
D:\DREAM CORE\unity-open.log
```

## Diagnostic Import Launcher

Run:

```bat
OPEN_UNITY_SAFE_MODE.cmd
```

It executes Unity directly with a separate log file for import/open diagnostics:

```bat
"C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Unity.exe" -projectPath "D:\DREAM CORE" -logFile "D:\DREAM CORE\unity-open-safe-mode.log"
```

The log will appear at:

```text
D:\DREAM CORE\unity-open-safe-mode.log
```

## If Unity Still Fails

Copy back the first clear failure block from the log, especially lines containing:

- `Package Manager`
- `Failed to resolve packages`
- `error CS`
- `Exception`
- `Crash`
- `Aborting batchmode`
- `Licensing`
- `Project path`

Start with the earliest error in the log. Later errors are often follow-on noise.

## Notes

Unity was not run during this diagnostic fix. No scripts were changed, no gameplay
was added, and the project was not recreated.
