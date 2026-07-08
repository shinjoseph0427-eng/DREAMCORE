# DREAM CORE Static Check Report

Generated: 2026-07-07

## Scope

Unity is not installed, so no Unity Editor automation, Play Mode, asset import,
or Unity compilation was run. This report covers file-system and text-only checks.

## Commands And Results

### C# script inventory

Result:

```text
Files: 30
Types: 33
Issues: none found by static text checks
```

Checks performed:

- Counted all `.cs` files under `Assets/_Project/Scripts/`.
- Extracted declared `class`, `interface`, `struct`, and `enum` names.
- Checked simple `{` / `}` brace balance.
- Checked that each C# file ends with a closing brace after trailing whitespace.
- Checked for merge markers.
- Checked for `throw new NotImplementedException` and `FIXME`.
- Checked for duplicate declared type names.

### Required scripts

All required scripts listed in the continuation request are present. The only
missing file found during this pass was `Assets/_Project/Scripts/World/FloatingObject.cs`;
it has been added.

### Local type references

The expected local systems are represented by corresponding files and types:
core bootstrapping, player movement/look/interaction, world builders, procedural
materials/textures, post-processing, UI, and audio.

### WebPreview

Static and server checks:

```text
node --check WebPreview\main.js
```

No syntax errors were reported.

```text
python -m http.server 8777
Invoke-WebRequest http://localhost:8777
HTTP 200
Length 3792
```

The preview server was stopped after the smoke test.

### Launcher

`PLAY_WEB_PREVIEW.cmd` contains:

```bat
@echo off
cd /d "D:\DREAM CORE\WebPreview"
python -m http.server 8777
```

## Findings

- No obvious truncated C# files were found.
- No simple brace-balance failures were found.
- No duplicate type declarations were found.
- No merge markers were found in the checked project scripts.
- WebPreview has a runnable `index.html` and local vendored Three.js files.
- `Assets/_Project/Scenes/` is empty until Unity runs the setup script and saves
  generated scenes.

## Limitations

These checks cannot guarantee Unity compile success. Final validation must happen
inside Unity after package restore/import because the project depends on Unity
and URP assemblies.
