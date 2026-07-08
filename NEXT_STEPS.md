# DREAM CORE Next Steps

## Install Unity

1. Install Unity Hub.
2. Install Unity 6.5 `6000.5.2f1`.
3. Unity 6.5 `6000.5.2f1` is the current project target in
   `ProjectSettings/ProjectVersion.txt`.
4. Unity 6.5 may upgrade URP and package metadata on first open. Allow the import
   to finish before judging errors.

## Open The Project

1. In Unity Hub, choose Add project from disk.
2. Select `D:\DREAM CORE`.
3. Wait for package import and script compilation.
4. If the automatic setup does not run, use `Dream Core > Run Full Setup`.
5. Open `Assets/_Project/Scenes/Main.unity`.
6. Press Play.

## Controls

- WASD: move
- Mouse: look
- Shift or Ctrl: slow walk
- ESC: pause / unlock cursor
- E: interact near the CLOUD 09 door

## If Errors Appear

Copy the exact Unity Console errors back into Claude Code. Include the first error
in the Console, not only later follow-on errors.

## Optional WebPreview

Before Unity is installed, you can run the visual preview:

```bat
cd /d "D:\DREAM CORE\WebPreview"
python -m http.server 8777
```

Open:

```text
http://localhost:8777
```
