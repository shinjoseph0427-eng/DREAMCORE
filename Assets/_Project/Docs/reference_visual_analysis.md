# DREAM CORE — Reference Visual Analysis

Source material: 5 uploaded reference stills + `referencedreamcore-video.mp4` (frames extracted to
`Assets/_Project/Art/References/`). All numbers below are estimated from the references and drive
the values in `DreamcoreWorldBuilder`.

---

## 1. Visual Style

Dreamcore / liminal-space indoor pool corridor. Related to "poolrooms" imagery but softer, cleaner
and more pastel: a narrow bathhouse-like water corridor that reads as artificial, sacred, empty and
carefully composed. Not horror, not backrooms-dark, not sci-fi. Everything is wet, glossy, hazy and
slightly overexposed, like a VHS memory of a place that never existed.

Key identity elements (must all be recognizable in-game):
- narrow symmetric corridor with a central shallow water channel the viewer moves through
- small aqua ceramic tiles on the walls
- cotton-cloud props mounted directly on the tile walls
- chrome handrails flanking the water
- pastel neon strips high on both walls (warm side / cool-pink side)
- white fluorescent ceiling panels
- glowing pink stepped doorway ("CLOUD 09") at the far end
- colored light reflecting and smearing across the rippling water

## 2. Color Palette

| Element | Approx. color | Hex (sRGB) |
|---|---|---|
| Wall tile aqua | saturated sky-cyan | `#5FC6D8` – `#7ED4E0` |
| Wall tile grout | warm off-white | `#D8D4C0` |
| Walkway/ceiling tile | warm beige/cream | `#E8E2CE` |
| Water body | milky turquoise-green | `#4FA98F` – `#66C2A8` |
| Door + alcove glow | pastel pink | `#FFB6C9`, glow `#FF9EC0` |
| Neon left (near→far) | yellow → green → cyan | `#FFE066`, `#7DFF8E`, `#66F0E0` |
| Neon right (near→far) | magenta → pink → violet | `#FF6EC7`, `#FF9EDD`, `#C77DFF` |
| Fluorescent panels | warm white | `#FFF6E8` |
| Haze/fog tint | pale warm pink-cream | `#E8D8D4` |

Shadows drift cyan/mint; highlights drift pink/yellow. Nothing is fully dark; nothing is pure white.

## 3. Architectural Layout

- Single straight corridor, ~28–32 m long, ~8 m total width, ~4.5 m ceiling.
- Central sunken water channel (~3.5 m wide, ~0.5 m deep water) — **this is the walking path**.
- Raised tiled walkways on both sides (~1.5 m wide, ~0.55 m above the channel floor), edged with
  a low tiled curb and chrome railings on the water side.
- Near end: tiled steps descending into the water (player start, seen in stills 1/4/5).
- Far end: short tiled steps rising out of the water to a pink-lit landing, then a recessed
  doorway with a **stepped / ziggurat tile frame** (art-deco steps visible in stills 1, 2, 4).
- Perfectly symmetric left/right except for lighting color.

## 4. Camera Perspective

Eye height slightly above the railings while standing in the channel (~1.6–1.7 m above the channel
floor). One-point perspective straight down the corridor; the pink door is always the vanishing
point and focal anchor. Vertical 9:16 crops in the refs emphasize the corridor's narrowness —
in-game FOV ~60–65° vertical keeps that compressed, intimate feel.

## 5. Wall Material

Small square ceramic tiles (~10 cm), saturated aqua with visible lighter grout, high gloss —
strong smeared reflections of the neon strips. Slight per-tile hue/value variation (some tiles
greener, some bluer). Implementation: procedurally generated albedo texture (grid + per-tile
variation) + high smoothness (~0.75) URP/Lit material, tiled ~10 tiles/m.

## 6. Ceiling Material

Same small square tile but warm off-white/beige, glossy, with recessed rectangular fluorescent
panels (~1.8 × 0.9 m) spaced evenly (3–4 down the corridor). Ceiling picks up pink/green neon
bounce near the walls.

## 7. Floor Material

- Walkways + steps: beige/cream small tile, wet-glossy (smoothness ~0.7), warm reflections.
- Underwater channel floor: same beige tile, seen through turquoise water.
- Low tiled curb walls separate walkway from channel.

## 8. Water Material

Shallow, calm, milky-turquoise pool water. Semi-transparent (channel floor visible near the
camera), strongly reflective at grazing angles (Fresnel), carrying long vertical smears of pink /
yellow / green light. Gentle overlapping ripples, no waves. Implementation: URP/Lit transparent
material, scrolling procedural ripple normal map (two layers, opposing directions, ~0.1 m/s),
smoothness ~0.9, plus a reflection probe close to the water surface. `WaterSurfaceAnimator`
scrolls UVs and adds ~1 cm vertical bob.

## 9. Railings

Polished stainless/chrome, warm-tinted reflections. Two horizontal rails (~0.9 m and ~0.55 m above
walkway) on cylindrical posts every ~1.8 m, running the full corridor on both sides of the channel;
short return rails near the entry steps. Rail diameter ~5 cm, posts ~4.5 cm. Metallic 1.0,
smoothness 0.9.

## 10. Cloud Wall Props

Fluffy white cotton-stuffing clouds bolted flat against the tile walls (not floating free),
4–6 per wall, sizes ~1–2.5 m, mounted at ~2.2–3.2 m height, irregular spacing. Soft rounded
silhouettes, no hard edges. Implementation: clusters of 8–16 overlapping spheres, shared soft-white
high-roughness material with a faint self-illumination so they never go grey; optional very slow
scale "breathing" via `MaterialPulse`.

## 11. Lighting Placement

- 3–4 recessed white fluorescent ceiling panels down the center (area-like glow, soft shadows).
- Continuous rows of neon strip fixtures mounted high on both walls (~3.6 m), tilted slightly
  downward; left wall warm (yellow/green/cyan), right wall cool-pink (magenta/pink/violet).
- Pink glow flooding the far alcove: horizontal pink tube above the door + pink area light.
- Cyan-tinted ambient fill so shadows stay mint, never black.
- Reflection probe centered over the water; light haze/fog throughout.

## 12. Glow Colors

Bloom sources ranked by intensity: pink door tube > ceiling panels > wall neon strips > water
specular highlights. Bloom is soft and wide (VHS-like halation), never sharp lens-flare-like.

## 13. Door / Portal Design

Recessed alcove at the top of 3–4 steps; **stepped tile frame** (three inset jambs, ziggurat
profile); interior walls washed pink; pastel-pink door slab (~1.6 × 2.4 m) with rounded feel;
orange-yellow text on the door reading **CLOUD 09** (the video shows a garbled "C100 ㅕ" — we use
CLOUD 09); one bare pink fluorescent tube glowing above the door. The door never opens in Phase 1.

## 14. Mood

Quiet, nostalgic, artificial, gently uncanny. Empty but not threatening — like a memory of a public
bath from a dream. Slow movement, soft hum, water lapping. The environment is the main character;
UI and mechanics stay out of the way.

## 15. Unity Implementation Plan

- URP (Linear color space, HDR) + global Volume: Bloom (soft, ~0.35 threshold ~0.85), Color
  Adjustments (slight warm-pink highlights / mint shadows via color filter + saturation),
  Vignette (~0.25), Chromatic Aberration (~0.08), Film Grain (~0.25), optional very subtle DoF.
- `DreamcoreWorldBuilder` procedurally assembles the corridor from primitives at edit time (via the
  editor setup) and/or at runtime (SceneBootstrapper fallback), fully parameterized.
- Procedural textures generated in code (tile grids, ripple normal map) so the project needs zero
  external assets; saved as PNG assets by the editor setup for tweakability.
- Mixed lighting: a few real-time point/area-approx lights (ceiling, door) + emissive materials
  carrying most of the perceived light through bloom; fog via `RenderSettings` + Volume.
- Player: CharacterController walking inside the water channel, clamped by colliders.

## 16. Buildable Now With Primitives

- Entire corridor shell (cubes), steps (cubes), curbs, ceiling, door alcove and stepped frame.
- Railings (cylinders + capsule-free simple colliders).
- Clouds (sphere clusters).
- Neon strips + ceiling panels (thin emissive cubes).
- Water (subdivided plane + scrolling normals).
- Door slab + CLOUD 09 text (3D Text / TextMesh or generated texture decal).

## 17. Replace Later With Custom Assets

- True tile geometry with beveled edges + trim sheets (replace procedural tile textures).
- Sculpted volumetric clouds (VDB-like shells or baked meshes) instead of sphere clusters.
- Real water shader (Shader Graph: depth fade, refraction, caustics) instead of scrolling normals.
- Modeled door with proper frame trim, printed decal texture.
- Baked lightmaps + light probes pass for final quality; audio recorded ambiences.
