# Safe Mode Fix Report

## Errors Found

Unity Console reported these two compile errors in `Assets/_Project/Scripts/Editor/DreamCoreProjectSetup.cs`:

```text
Assets\_Project\Scripts\Editor\DreamCoreProjectSetup.cs(140,17): error CS0234:
The type or namespace name 'ResourceReloader' does not exist in the namespace 'UnityEditor.Rendering' (are you missing an assembly reference?)

Assets\_Project\Scripts\Editor\DreamCoreProjectSetup.cs(162,32): error CS0122:
'UniversalRenderPipelineGlobalSettings' is inaccessible due to its protection level
```

The CS0618 `FindFirstObjectByType<T>()` messages are warnings only and were not changed.

## Code Changed

Changed only:

```text
Assets/_Project/Scripts/Editor/DreamCoreProjectSetup.cs
```

Changes made:

- Removed the optional `ResourceReloader` calls from URP asset setup.
- Removed the helper method that depended on `ResourceReloader`.
- Removed direct/reflected access to `UniversalRenderPipelineGlobalSettings`.
- Kept URP asset creation through public APIs:
  - `ScriptableObject.CreateInstance<UniversalRendererData>()`
  - `UniversalRenderPipelineAsset.Create(rendererData)`
  - `GraphicsSettings.defaultRenderPipeline = rp`
  - `QualitySettings.renderPipeline = rp`
- Kept a clear warning when optional URP global settings setup is skipped.
- Added the required setup menu item:

```text
Tools/DREAM CORE/Setup Project
```

## Why This Fix Is Safe

- The change is limited to the editor setup script.
- No gameplay scripts were changed.
- No scene, material, lighting, camera, player, or visual design behavior was rebuilt.
- The setup system remains in place and still creates/assigns a URP asset through public Unity 6.5 compatible APIs.
- The removed calls were optional compatibility helpers that caused compilation failure in Unity `6000.5.3f1`.
- Skipping inaccessible URP global settings setup is safer than depending on internal/protected APIs.

## Static Checks

Unity was not run.

Static source scan confirmed `DreamCoreProjectSetup.cs` no longer contains:

```text
UnityEditor.Rendering.ResourceReloader
ResourceReloader
UniversalRenderPipelineGlobalSettings
```

Ran Unity's generated editor assembly compiler response file through the bundled compiler without opening Unity:

```text
C:\Program Files\Unity\Hub\Editor\6000.5.3f1\Editor\Data\MonoBleedingEdge\bin\mono.exe
C:\Program Files\Unity\Hub\Editor\6000.5.3f1\Editor\Data\MonoBleedingEdge\lib\mono\msbuild\Current\bin\Roslyn\csc.exe
@Library\Bee\artifacts\1900b0aE.dag\Assembly-CSharp-Editor.rsp
```

Result:

```text
Exit code 0. No C# compile errors.
```

Warnings observed:

- Existing CS0618 warnings for `Object.FindFirstObjectByType<T>()`.
- CS8032 analyzer/source-generator loading warnings caused by invoking Roslyn manually outside Unity's normal compiler host.

## What To Do Next In Unity

1. Return to Unity Safe Mode.
2. Let Unity recompile scripts.
3. Use `Tools > DREAM CORE > Setup Project` if the setup needs to be run manually.
4. Ignore the CS0618 warnings for now unless you want a separate warning-only cleanup pass later.
