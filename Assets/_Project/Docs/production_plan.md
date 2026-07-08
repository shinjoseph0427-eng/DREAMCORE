# DREAM CORE — Production Plan

## Phase 1 — PC First-Person Exploration (this prototype)

Scope: one polished vertical slice — the dreamcore water corridor from the references.

- Unity 2022.3 LTS + URP, Linear color space, HDR, post-processing Volume.
- Procedural world: `DreamcoreWorldBuilder` assembles the corridor from primitives; every key
  dimension exposed in the inspector (`corridorLength`, `waterWidth`, `walkwayWidth`,
  `wallHeight`, `cloudCount`, `railingPostSpacing`, light colors, fog, bloom…).
- Player: `CharacterController`-based first-person controller (walk 3.2 m/s, slow-walk 1.7 m/s on
  Shift/Ctrl, subtle head bob, no sprint, no jump by default).
- One interactive element: the pink CLOUD 09 door → prompt "E — Approach" → "This is not the exit."
- Placeholder audio system (logs missing clips, never throws).
- Start screen → click to play; ESC pause menu.

Definition of done: project compiles clean, Main.unity plays, the corridor is immediately
recognizable against the reference images.

## Phase 2 — VR Support (XR Interaction Toolkit)

The player stack is deliberately layered so VR replaces only the input/camera layer:

- Add packages: `com.unity.xr.interaction.toolkit`, `com.unity.xr.management`, OpenXR plugin.
- Replace `Player` prefab with an **XR Origin (XR Rig)**: the world, interaction and game-state
  layers stay untouched. `FirstPersonController` / `MouseLook` / `PlayerHeadBob` are PC-only and
  are simply not present on the XR rig.
- Locomotion: Continuous Move (slow, comfort-first) + optional Teleportation anchors on the
  walkways; Snap Turn 45°.
- Comfort: tunnel vignette during locomotion (XRI Tunneling Vignette sample).
- Interaction: `PlayerInteraction`'s raycast is replaced by XRI Ray Interactor pointing at the
  door's `GlowingDoor` (which already exposes a plain `Interact()` method for this reason).
- UI: world-space canvas variants of the prompt / pause menu.
- Hook points are marked with `// VR:` comments in Core/ and Player/ scripts.

## Phase 3 — Optimized Quest / PCVR Build

- Bake lighting (currently mixed/real-time) → lightmaps + light probes; strip real-time lights to
  1–2 mixed lights.
- Collapse the procedural world into a saved static mesh set; enable static batching; atlas the
  procedural textures.
- Replace transparent water with a cheaper single-pass shader (no scene color); clamp bloom.
- Target: Quest 3 72–90 fps — corridor is one small interior, well within budget.
- Optional stretch: additional rooms behind the CLOUD 09 door (each room = one new
  `DreamcoreWorldBuilder` variant), save-slot-free session flow, photo mode.

## Milestone order actually used in Phase 1

1. Docs + reference analysis
2. Project skeleton (packages, folders)
3. Core/Player/UI scripts
4. World builder + materials (procedural textures)
5. Lighting + post + water animation + reflection probe
6. Door interaction, audio placeholders
7. Editor auto-setup (URP assignment, scene build, material assets), README
