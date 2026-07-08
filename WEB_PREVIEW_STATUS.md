# Web Preview Status

## Files Checked

- `WebPreview/index.html`
- `WebPreview/main.js`
- `WebPreview/vendor/three.module.js`
- `WebPreview/vendor/addons/controls/PointerLockControls.js`
- `WebPreview/vendor/addons/objects/Water.js`
- `WebPreview/vendor/addons/postprocessing/EffectComposer.js`
- `WebPreview/vendor/addons/postprocessing/RenderPass.js`
- `WebPreview/vendor/addons/postprocessing/UnrealBloomPass.js`
- `WebPreview/vendor/addons/postprocessing/OutputPass.js`
- `PLAY_WEB_PREVIEW.cmd`

## Files Created Or Updated

- `WebPreview/index.html`
- `WebPreview/main.js`
- `WebPreview/style.css`
- `WebPreview/assets/audio/Ambience.mp3`
- `PLAY_WEB_PREVIEW.cmd`
- `OPEN_WEB_PREVIEW.url`
- `WEB_AUDIO_README.md`
- `WEB_PREVIEW_README.md`
- `WEB_PREVIEW_STATUS.md`

## Web Audio Update

Added the 20-minute ambience track to the localhost WebPreview.

```text
Source used:
D:\DREAM CORE\Ambience.mp3

Copied to:
D:\DREAM CORE\WebPreview\assets\audio\Ambience.mp3

Web path:
assets/audio/Ambience.mp3
```

Audio behavior:

- Audio starts only after the user clicks to enter the scene.
- `Ambience.mp3` loops.
- Volume fades from `0` to `0.45` over `4` seconds.
- `M` toggles mute/unmute after the audio has started.
- `R` restarts the track from the beginning and fades it back in.
- A subtle on-screen status shows `Music: On` or `Music: Muted`.

Audio checks:

```text
node --check WebPreview\main.js: passed
WebPreview\index.html exists: yes
WebPreview\assets\audio\Ambience.mp3 exists: yes
Audio file size: 34165165 bytes
http://localhost:8777: HTTP 200
http://localhost:8777/assets/audio/Ambience.mp3: HTTP 200
Existing localhost server was used: yes
Started/stopped temporary test server: no
```

Browser interaction check with system Chrome:

```text
After click:
audio element exists: yes
audio src: /assets/audio/Ambience.mp3
paused: false
muted: false
status text: Music: On
fade-in volume after ~1.4s: 0.1614

After pressing M:
paused: false
muted: true
status text: Music: Muted
console/page errors: none
```

## Second Area Update

Added a second explorable area to the localhost WebPreview.

- Existing first corridor remains.
- The far pink door now works as a portal.
- Near the pink door, the prompt changes to `E - Enter`.
- Pressing `E`, or moving very close to the door, triggers a dreamy pink/cyan fade transition.
- After the fade, the player is placed inside a dark glowing mushroom waterpark room.
- Second area includes:
  - giant red mushrooms with white spots
  - glossy pale mushroom stems
  - teal glowing shallow water / lazy-river-like curved pools
  - pink circular float objects
  - dark wet tiled walls/floor/ceiling
  - cyan fluorescent ceiling lights
  - humid retro dreamcore haze
- Existing retro lens, grain, chromatic softness, audio, and first-person controls remain.

Second area checks:

```text
node --check WebPreview\main.js: passed
WebPreview\index.html exists: yes
http://localhost:8777: HTTP 200
Browser render URL: http://localhost:8777/?shot&area=second&z=2.0
Canvas present: yes
Console/page errors: none
```

## Second Area Rework Update (2026-07-08)

Redesigned the second area into a giant closed indoor dream-waterpark hall
(reference-image driven). First corridor and pink-door portal unchanged.

- second area enlarged significantly: 11.5x17.5m box -> 34x58m hall
- curved ceiling added: flattened barrel vault, walls 6.2m, apex ~14.6m, fading into haze
- leftward entry route added: spawn on the SE deck, water/light/tubes all pull the player left
- deep winding glowing water channel added: closed ~100m lazy-river loop, water depth ~1.0m
  (vs 0.35m in room one), emissive mint water with scrolling ripple normals, pale curved coping
- pink floating tubes added: 10 glossy handled tubes drifting endlessly along the loop
  with spacing, time offsets, bobbing and slow spin
- grassy visibility-blocking berms added: 10 mounds (up to 2.6m) at the channel bends,
  walkable, planted with ~2,600 instanced dark grass blades and hedge fringes
- mushrooms varied in scale: 4 giant / 5 medium / 6 small amanitas, spotted-cap canvas
  textures, drooping caps, pale lathe stems, stem collision
- white flowers added: ~720 instanced softly-glowing flowers on hill rims and edges
- darker lighting with single sky-blue ceiling light emphasis: near-zero ambient,
  dark teal fog, one pale cyan ceiling panel + spot as the key light, mint water glow,
  distant red exit-door light, faint pink arrival door
- retro filmed look preserved (same lens shader, grain, vignette, chromatic softness)
- performance safeguards for low-end CPUs: second area fully hidden until the portal is
  used, corridor water reflection pass disabled after the one-way transition, instanced
  foliage, texture-based cap spots, second-area animation updates gated by area

Second area rework checks:

```text
node --check WebPreview\main.js: passed
WebPreview\index.html exists: yes
http://localhost:8777: HTTP 200 (temporary python http.server started for the test)
http://localhost:8777/main.js: HTTP 200
Temporary test server stopped after check: yes
Headless-browser render check: skipped by user request (software WebGL was too CPU-heavy);
verify visually at http://localhost:8777 and http://localhost:8777/?shot&area=second
```

## Visual Quality Update

Updated `WebPreview/main.js` and `WebPreview/style.css` to make the corridor closer to the reference image while keeping the same layout and first-person preview:

- Reduced fog density from `0.048` to `0.030`, keeping soft haze without washing out the corridor.
- Final fog density is `0.022`, keeping soft haze without washing out the corridor.
- Reduced bloom strength from `0.55` to `0.22` and lowered tone mapping exposure from `1.12` to `0.88`.
- Increased tile readability with stronger grout contrast, beveled tile highlights, and subtle bump mapping on wall, floor, and ceiling tile materials.
- Shifted water toward clearer turquoise and reduced distortion while keeping reflective water rendering.
- Strengthened the far pink door as the focal point with a controlled pink halo and stronger local pink light.
- Reduced ceiling/neon emissive intensity so pastel lights remain colorful but less blown out.
- Added subtle underside shadow puffs to cloud props and reduced their flat white emissive look.
- Slightly softened the CSS vignette so the image stays calm and premium rather than dark.
- Added a tiny inline favicon to avoid browser 404 noise during local render checks.

## Mood Rework Update

Updated `WebPreview/main.js` and `WebPreview/style.css` again to push the scene away from a clean 3D prototype and closer to a retro dreamcore/poolcore photograph:

- Compressed corridor proportions:
  - water corridor length `26` -> `19.6`
  - half water channel width `1.8` -> `1.25`
  - walkway width `1.6` -> `0.88`
  - wall height `4.5` -> `3.25`
  - door size `1.7 x 2.5` -> `1.42 x 2.05`
- Changed camera feel:
  - FOV `64` -> `53`
  - far clip `90` -> `70`
  - eye height `1.65` -> `1.48`
  - render pixel ratio reduced to soften the output
- Added a custom retro lens `ShaderPass`:
  - soft sampling
  - mild barrel distortion
  - chromatic aberration
  - highlight color bleed
  - film grain/sensor noise
  - subtle scan texture
  - heavier vignette
- Reworked atmosphere:
  - denser humid haze with a cooler grey-green fog color
  - distance is more compressed and the pink door is more veiled
- Reworked cloud props:
  - more lobes per cloud
  - flatter wall-mounted forms
  - soft alpha cloud backing texture
  - underside shadow puffs
  - less plastic emissive material
- Reduced clean CG material feel:
  - rougher rail metal
  - rougher/worn tile and ceiling materials
  - murkier turquoise water with lower distortion
  - lower environment reflections
  - warmer, less sterile lights

## Localhost Smoke Test

Passed.

```text
Existing server on port 8777 before test: no
Started temporary server: python -m http.server 8777
Checked URL: http://localhost:8777
HTTP status: 200
Stopped temporary server: yes
Stopped PID: 4632
```

Latest visual update smoke test:

```text
Existing server on port 8777 before test: yes
Listening address: 127.0.0.1
Listening PID: 14948
Checked URL: http://localhost:8777
HTTP status: 200
JavaScript syntax check: node --check WebPreview\main.js passed
Server stopped after test: no, existing preview server was left running
```

Final browser render check:

```text
Browser: system Chrome via Playwright
URL: http://localhost:8777/?shot&z=4.4
Canvas present: yes
Canvas size: 960 x 1280
Console/page errors: none
Visual result: clearer pastel corridor, readable aqua raised tiles, clearer turquoise water, softer bloom/fog, visible pink destination door, and clouds with subtle depth
```

Mood rework browser render check:

```text
Browser: system Chrome via Playwright
URL: http://localhost:8777/?shot&z=3.6
Canvas present: yes
Canvas CSS size: 720 x 1280
Canvas render size: 518 x 921
Console/page errors: none
HTTP status: 200
JavaScript syntax check: node --check WebPreview\main.js passed
Visual result: tighter/lower corridor, heavier humid haze, retro film grain and chromatic softness, murkier reflective water, softer wall-cloud silhouettes, and veiled pink door focal point
```

## Exact Command To Run

```text
D:\DREAM CORE\PLAY_WEB_PREVIEW.cmd
```

Manual server command:

```text
cd /d "D:\DREAM CORE\WebPreview"
python -m http.server 8777
```

## Exact URL To Open

```text
http://localhost:8777
```

## Known Limitations

- This is a lightweight Three.js browser preview, not the final Unity version.
- It approximates the current corridor mood and layout but does not run Unity gameplay systems.
- Final gameplay behavior still needs to be tested in Unity Play Mode.
- Pointer lock requires clicking inside the browser page.
