# DREAM CORE Mood Rework Report

## 1. Proportion Changes

Changed only the local `WebPreview` scene proportions. Unity scene and C# scripts were not modified.

- Narrowed the corridor by reducing the water channel and walkway widths.
- Lowered the ceiling from a tall clean game-space feel to a tighter enclosed pool corridor.
- Shortened the corridor so the pink door feels closer and more oppressive.
- Reduced the door size to fit the lower ceiling and make the end wall feel more cramped.
- Lowered the player eye height slightly for a more filmed/photo viewpoint.

Key values changed in `WebPreview/main.js`:

```text
CORRIDOR: 26 -> 19.6
HALF_WATER: 1.8 -> 1.25
WALKWAY: 1.6 -> 0.88
WALL_H: 4.5 -> 3.25
DOOR_W / DOOR_H: 1.7 / 2.5 -> 1.42 / 2.05
EYE: 1.65 -> 1.48
```

## 2. Camera And Post-Processing Changes

- Lowered camera FOV from `64` to `53` for a less default-game-camera feel.
- Reduced render pixel ratio to soften the clinically sharp WebGL image.
- Added a custom retro lens shader pass with:
  - mild barrel distortion
  - chromatic aberration
  - soft sampling
  - film grain / sensor noise
  - subtle scan texture
  - highlight color bleed
  - heavier vignette
- Kept soft bloom/halation, but made it part of the hazy lens feel rather than a clean premium render effect.

## 3. Cloud Improvements

- Reworked clouds from obvious grouped spheres into flatter wall-mounted installation clouds.
- Increased lobe count and made silhouettes more uneven.
- Added soft alpha cloud backing textures to blend cloud masses into the tiled walls.
- Added underside shadow puffs for depth.
- Reduced plastic-white emissive material so the clouds feel more like photographed fabric/foam/cloud props.

## 4. Fog And Haze Changes

- Changed haze from clean white washout to heavier humid spatial atmosphere.
- Fog color is cooler grey-green to make the air feel damp and enclosed.
- Fog density was increased for the mood rework so the end door is partially veiled, closer to the reference photo.
- The result keeps pastel luminosity but compresses distance and makes the corridor feel more suffocating.

## 5. Less Cheap Game, More Retro Filmed Dreamcore

- Lowered ceiling and narrowed the path to reduce the open showroom feeling.
- Made water murkier and more photographic while keeping reflections.
- Roughened rail metal so it no longer reads like toy chrome.
- Roughened tile/ceiling materials and reduced clean environment reflections.
- Added retro lens artifacts: softness, grain, chromatic offset, color bleed, vignette, and slight barrel distortion.
- Warmed and softened lighting so it feels like a strange old digital camera photo of a real indoor pool corridor.

## 6. What To Check Next

Localhost preview:

```text
http://localhost:8777
```

Check:

- Does the corridor feel tighter and lower?
- Does the image feel like a retro/old-camera poolcore photo instead of a clean 3D demo?
- Are the clouds more believable on the walls?
- Is the pink door still a visible focal point through haze?
- Is the movement still comfortable in the narrower corridor?

Unity:

- This pass did not modify the Unity scene or C# scripts.
- Final Unity Play Mode should be checked separately if you want the same mood applied inside Unity.
