# DREAM CORE Prototype

DREAM CORE is a first-person dreamcore / liminal-space exploration prototype:
a narrow indoor pool corridor with aqua tiled walls, a shallow turquoise water
channel, chrome railings, cotton clouds, pastel neon, and a glowing pink CLOUD
09 door at the far end.

The Unity scene is generated from project scripts and primitives. No paid assets
or external models are required.

## Unity Version

Installed editor target: Unity 6.5 `6000.5.2f1`, matching
`ProjectSettings/ProjectVersion.txt`.

Opening the project in Unity 6.5 may prompt Unity to upgrade URP and package
metadata on first import. Let Unity complete the upgrade, then review the
Console. The code avoids Unity 6.5-only APIs where practical so it remains easy
to repair if a package-level compatibility issue appears.

## Open The Project

1. Install Unity Hub.
2. Install Unity 6.5 `6000.5.2f1`.
3. In Unity Hub, choose Add project from disk.
4. Select `D:\DREAM CORE`.
5. Wait for Unity to import packages and compile scripts.
6. If the setup script does not run automatically, choose `Dream Core > Run Full Setup`.
7. Open `Assets/_Project/Scenes/Main.unity`.
8. Press Play.

On first open, `Assets/_Project/Scripts/Editor/DreamCoreProjectSetup.cs` creates
project settings, URP assets, generated materials/textures, post-processing, and
the `Main.unity` and `LightingTest.unity` scenes. These scene files may not exist
until Unity imports the project and runs the editor setup.

## Controls

| Input | Action |
| --- | --- |
| WASD | Move |
| Mouse | Look |
| Shift / Ctrl | Slow walk |
| ESC | Pause / unlock cursor |
| E | Interact near the CLOUD 09 door |

## Prototype Contents

- Procedural dreamcore corridor assembled by `DreamcoreWorldBuilder`.
- Aqua glossy tile walls, beige wet tile walkways, central animated water.
- Chrome railings, floating cloud props, pastel neon strips, fluorescent panels.
- Pink CLOUD 09 door with interaction text: "This is not the exit."
- URP post-processing controller for bloom, fog, vignette, film grain, color grade,
  chromatic aberration, and subtle VHS-style wobble.
- Code-built start screen, pause menu, and interaction prompt.
- Placeholder ambient audio controller ready for later clips.
- Reference images stored in `Assets/_Project/Art/References/`.

## WebPreview

`WebPreview/` is an optional Three.js look-dev preview that approximates the
Unity corridor without needing Unity installed. It is not the main project; the
Unity project remains the source of truth.

Run it from a command prompt:

```bat
cd /d "D:\DREAM CORE\WebPreview"
python -m http.server 8777
```

Then open:

```text
http://localhost:8777
```

You can also run `PLAY_WEB_PREVIEW.cmd` from the project root.

## If Unity Reports Errors

Do not delete or recreate the project. Copy the exact Unity Console errors and
send them back for repair. The current pass used static file checks only because
Unity is not installed, so final compile/import validation must happen inside
Unity after installation.

## Project Docs

- `Assets/_Project/Docs/reference_visual_analysis.md` explains the visual target.
- `Assets/_Project/Docs/production_plan.md` outlines PC prototype, later VR work,
  and optimization phases.
