using UnityEngine;
using DreamCore.Rendering;

namespace DreamCore.World
{
    /// <summary>
    /// Assembles the entire dreamcore water corridor from modular primitive
    /// geometry, faithful to the reference images: aqua tiled walls, central
    /// shallow water channel the player wades through, raised beige walkways,
    /// chrome railings, wall clouds, pastel neon strips, fluorescent ceiling
    /// panels and the glowing pink CLOUD 09 door behind a stepped tile frame.
    ///
    /// Everything is parameterized; use the "Rebuild World" context menu after
    /// changing values.
    /// </summary>
    public class DreamcoreWorldBuilder : MonoBehaviour
    {
        [Header("Corridor Shell")]
        [SerializeField, Tooltip("Length of the water section in meters.")]
        private float corridorLength = 26f;
        [SerializeField, Tooltip("Width of the central water channel.")]
        private float waterWidth = 3.6f;
        [SerializeField, Tooltip("Width of each raised side walkway.")]
        private float walkwayWidth = 1.6f;
        [SerializeField] private float wallHeight = 4.5f;
        [SerializeField] private float wallThickness = 0.3f;
        [SerializeField, Tooltip("Physical size of one ceramic tile in meters.")]
        private float tileScale = 0.1f;

        [Header("Water")]
        [SerializeField, Tooltip("Water surface height above the channel floor.")]
        private float waterDepth = 0.42f;
        [SerializeField, Tooltip("Walkway/landing height above the channel floor.")]
        private float walkwayHeight = 0.55f;
        [SerializeField, Range(1, 8)] private int waterSegments = 4;

        [Header("Entry / Exit")]
        [SerializeField] private float entryLandingDepth = 2.4f;
        [SerializeField, Range(2, 5)] private int entryStepCount = 3;
        [SerializeField, Range(2, 5)] private int exitStepCount = 3;
        [SerializeField, Tooltip("Depth of the pink alcove landing at the far end.")]
        private float alcoveDepth = 3.0f;
        [SerializeField] private float doorWidth = 1.7f;
        [SerializeField] private float doorHeight = 2.5f;
        [SerializeField] private string doorLabel = "CLOUD 09";

        [Header("Railings")]
        [SerializeField] private float railingPostSpacing = 1.8f;

        [Header("Clouds")]
        [SerializeField, Range(0, 12)] private int cloudCountPerWall = 5;
        [SerializeField] private int cloudSeed = 7;

        [Header("Lighting")]
        [SerializeField, Range(1, 8)] private int ceilingPanelCount = 4;
        [SerializeField] private float ceilingLightIntensity = 1.1f;
        [SerializeField, Tooltip("Height of the neon strips on the walls.")]
        private float lightStripY = 3.7f;
        [SerializeField] private float stripSegmentLength = 1.6f;
        [SerializeField] private float stripGap = 0.55f;
        [SerializeField] private float stripLightIntensity = 0.7f;
        [SerializeField, Tooltip("A real point light every N strip segments.")]
        private int stripLightEverySegments = 4;
        [SerializeField] private float doorLightIntensity = 2.2f;

        [Header("Atmosphere")]
        [SerializeField] private Color fogColor = new Color(0.87f, 0.80f, 0.80f);
        [SerializeField] private float fogDensity = 0.045f;
        [SerializeField] private Color ambientColor = new Color(0.42f, 0.55f, 0.54f);

        private const float StepDepth = 0.34f;
        private const int TilesInTexture = 8;

        // ---- derived, valid after Build() ----
        private float HalfWater => waterWidth * 0.5f;
        private float HalfInner => HalfWater + walkwayWidth;
        private float WallX => HalfInner + wallThickness * 0.5f;
        private float ZWaterStart => entryLandingDepth;
        private float ZWaterEnd => entryLandingDepth + corridorLength;
        private float TotalLength => ZWaterEnd + alcoveDepth;
        private float UvPerMeter => 1f / (Mathf.Max(0.01f, tileScale) * TilesInTexture);

        /// <summary>Where the player rig should spawn (on the entry landing, facing the door).</summary>
        public Vector3 PlayerSpawnPosition =>
            transform.TransformPoint(new Vector3(0f, walkwayHeight + 0.1f, 1.2f));

        [ContextMenu("Rebuild World")]
        public void Build()
        {
            DestroyChildren(transform);

            var aqua = DreamMaterialLibrary.Get(DreamMaterialLibrary.AquaGlossTile);
            var beige = DreamMaterialLibrary.Get(DreamMaterialLibrary.BeigeWetTile);
            var ceilingMat = DreamMaterialLibrary.Get(DreamMaterialLibrary.CeilingTile);

            var structure = Child("Structure");
            BuildFloors(structure, beige);
            BuildSteps(structure, beige);
            BuildWalls(structure, aqua, ceilingMat);
            BuildDoorAlcove(Child("DoorAlcove"), beige);

            WaterChannelBuilder.Build(Child("Water").transform,
                waterWidth + 0.35f, ZWaterStart + 0.4f, ZWaterEnd - 0.5f,
                waterDepth, waterSegments);

            var railings = Child("Railings");
            RailingBuilder.Build(railings.transform, "Railing_L", -(HalfWater + 0.12f),
                walkwayHeight, ZWaterStart + 0.1f, ZWaterEnd - 0.1f, railingPostSpacing);
            RailingBuilder.Build(railings.transform, "Railing_R", HalfWater + 0.12f,
                walkwayHeight, ZWaterStart + 0.1f, ZWaterEnd - 0.1f, railingPostSpacing);

            BuildClouds(Child("Clouds"));
            BuildLighting(Child("Lighting"));

            ReflectionProbeSetup.Create(transform,
                new Vector3(0f, wallHeight * 0.4f, TotalLength * 0.5f),
                new Vector3(WallX * 2f + wallThickness, wallHeight + 0.6f, TotalLength + 1f));

            ApplyAtmosphere();
        }

        // ------------------------------------------------------------------ floors & steps

        private void BuildFloors(GameObject parent, Material beige)
        {
            var t = parent.transform;

            ModularTileWall.Box("ChannelFloor", t,
                new Vector3(0f, -0.1f, (ZWaterStart + ZWaterEnd) * 0.5f),
                new Vector3(waterWidth + 0.4f, 0.2f, corridorLength),
                beige, UvPerMeter);

            float wx = HalfWater + walkwayWidth * 0.5f;
            ModularTileWall.Box("Walkway_L", t,
                new Vector3(-wx, walkwayHeight * 0.5f, TotalLength * 0.5f),
                new Vector3(walkwayWidth, walkwayHeight, TotalLength),
                beige, UvPerMeter);
            ModularTileWall.Box("Walkway_R", t,
                new Vector3(wx, walkwayHeight * 0.5f, TotalLength * 0.5f),
                new Vector3(walkwayWidth, walkwayHeight, TotalLength),
                beige, UvPerMeter);

            ModularTileWall.Box("EntryLanding", t,
                new Vector3(0f, walkwayHeight * 0.5f, entryLandingDepth * 0.5f),
                new Vector3(waterWidth, walkwayHeight, entryLandingDepth),
                beige, UvPerMeter);

            ModularTileWall.Box("AlcoveLanding", t,
                new Vector3(0f, walkwayHeight * 0.5f, ZWaterEnd + alcoveDepth * 0.5f),
                new Vector3(waterWidth, walkwayHeight, alcoveDepth),
                beige, UvPerMeter);
        }

        private void BuildSteps(GameObject parent, Material beige)
        {
            var t = parent.transform;

            // Entry: descend from the landing into the water (lowest steps submerged).
            float rise = walkwayHeight / (entryStepCount + 1);
            for (int i = 0; i < entryStepCount; i++)
            {
                float top = walkwayHeight - rise * (i + 1);
                ModularTileWall.Box($"EntryStep_{i}", t,
                    new Vector3(0f, top * 0.5f, entryLandingDepth + (i + 0.5f) * StepDepth),
                    new Vector3(waterWidth, top, StepDepth),
                    beige, UvPerMeter);
            }

            // Exit: rise out of the water toward the pink alcove.
            rise = walkwayHeight / (exitStepCount + 1);
            float run = exitStepCount * StepDepth;
            for (int i = 0; i < exitStepCount; i++)
            {
                float top = rise * (i + 1);
                ModularTileWall.Box($"ExitStep_{i}", t,
                    new Vector3(0f, top * 0.5f, ZWaterEnd - run + (i + 0.5f) * StepDepth),
                    new Vector3(waterWidth, top, StepDepth),
                    beige, UvPerMeter);
            }
        }

        // ------------------------------------------------------------------ walls & ceiling

        private void BuildWalls(GameObject parent, Material aqua, Material ceilingMat)
        {
            var t = parent.transform;

            // Side walls and ceiling in ~6 m segments (keeps URP's per-object
            // light limit from starving long meshes).
            int segments = Mathf.Max(1, Mathf.CeilToInt(TotalLength / 6f));
            float segLen = TotalLength / segments;
            for (int i = 0; i < segments; i++)
            {
                float z = (i + 0.5f) * segLen;
                ModularTileWall.Box($"Wall_L_{i:00}", t,
                    new Vector3(-WallX, wallHeight * 0.5f, z),
                    new Vector3(wallThickness, wallHeight, segLen), aqua, UvPerMeter);
                ModularTileWall.Box($"Wall_R_{i:00}", t,
                    new Vector3(WallX, wallHeight * 0.5f, z),
                    new Vector3(wallThickness, wallHeight, segLen), aqua, UvPerMeter);
                ModularTileWall.Box($"Ceiling_{i:00}", t,
                    new Vector3(0f, wallHeight + 0.15f, z),
                    new Vector3((WallX + wallThickness * 0.5f) * 2f, 0.3f, segLen),
                    ceilingMat, UvPerMeter);
            }

            ModularTileWall.Box("BackWall", t,
                new Vector3(0f, wallHeight * 0.5f, -wallThickness * 0.5f),
                new Vector3((WallX + wallThickness * 0.5f) * 2f, wallHeight, wallThickness),
                aqua, UvPerMeter);

            // Front wall around the door opening.
            float openTop = walkwayHeight + doorHeight;
            float zFront = TotalLength + wallThickness * 0.5f;
            float sideWidth = WallX + wallThickness * 0.5f - doorWidth * 0.5f;
            ModularTileWall.Box("FrontWall_L", t,
                new Vector3(-(doorWidth * 0.5f + sideWidth * 0.5f), wallHeight * 0.5f, zFront),
                new Vector3(sideWidth, wallHeight, wallThickness), aqua, UvPerMeter);
            ModularTileWall.Box("FrontWall_R", t,
                new Vector3(doorWidth * 0.5f + sideWidth * 0.5f, wallHeight * 0.5f, zFront),
                new Vector3(sideWidth, wallHeight, wallThickness), aqua, UvPerMeter);
            ModularTileWall.Box("FrontWall_Top", t,
                new Vector3(0f, openTop + (wallHeight - openTop) * 0.5f, zFront),
                new Vector3(doorWidth, wallHeight - openTop, wallThickness), aqua, UvPerMeter);
            ModularTileWall.Box("FrontWall_Bottom", t,
                new Vector3(0f, walkwayHeight * 0.5f, zFront),
                new Vector3(doorWidth, walkwayHeight, wallThickness), aqua, UvPerMeter);
        }

        // ------------------------------------------------------------------ door alcove

        private void BuildDoorAlcove(GameObject parent, Material beige)
        {
            var t = parent.transform;
            float baseY = walkwayHeight;
            float openTop = baseY + doorHeight;

            // Stepped ziggurat frame: largest outline nearest the viewer, stepping
            // down to the door plane (matches the art-deco jambs in the refs).
            const int frames = 3;
            const float frameDepth = 0.13f;
            const float frameStep = 0.16f;
            for (int k = 0; k < frames; k++)
            {
                int fromDoor = frames - 1 - k; // 0 = closest to door
                float grow = frameStep * (k + 1) * 2f;
                float w = doorWidth + grow;
                float hTop = openTop + frameStep * (k + 1) * 0.8f;
                float z = TotalLength - frameDepth * (k + 0.5f);
                float jamb = 0.22f;

                ModularTileWall.Box($"Frame{fromDoor}_L", t,
                    new Vector3(-(w * 0.5f - jamb * 0.5f), baseY + (hTop - baseY) * 0.5f, z),
                    new Vector3(jamb, hTop - baseY, frameDepth), beige, UvPerMeter);
                ModularTileWall.Box($"Frame{fromDoor}_R", t,
                    new Vector3(w * 0.5f - jamb * 0.5f, baseY + (hTop - baseY) * 0.5f, z),
                    new Vector3(jamb, hTop - baseY, frameDepth), beige, UvPerMeter);
                ModularTileWall.Box($"Frame{fromDoor}_Top", t,
                    new Vector3(0f, hTop - jamb * 0.5f, z),
                    new Vector3(w, jamb, frameDepth), beige, UvPerMeter);
            }

            // Pink door slab.
            var doorMat = DreamMaterialLibrary.Get(DreamMaterialLibrary.PinkPortalDoor);
            float slabW = doorWidth - 0.14f;
            float slabH = doorHeight - 0.10f;
            var door = ModularTileWall.Box("Door_Cloud09", t,
                new Vector3(0f, baseY + slabH * 0.5f, TotalLength - 0.05f),
                new Vector3(slabW, slabH, 0.09f), doorMat, 0.5f);
            door.isStatic = false;
            door.AddComponent<GlowingDoor>();
            door.AddComponent<MaterialPulse>().Configure(0.10f, 0.20f);

            // Door label.
            var textGo = new GameObject("DoorLabel");
            textGo.transform.SetParent(t, false);
            textGo.transform.localPosition = new Vector3(0f, baseY + slabH * 0.62f, TotalLength - 0.11f);
            textGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var text = textGo.AddComponent<TextMesh>();
            text.text = doorLabel;
            text.font = DreamCore.UI.UiBuilder.GetBuiltinFont();
            if (text.font != null)
                textGo.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            text.fontSize = 72;
            text.characterSize = 0.055f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(1.00f, 0.60f, 0.25f);

            // Pink tube above the door.
            var tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tube.name = "DoorTube_Pink";
            tube.transform.SetParent(t, false);
            tube.transform.localPosition = new Vector3(0f, openTop + 0.22f, TotalLength - 0.28f);
            tube.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            tube.transform.localScale = new Vector3(0.05f, doorWidth * 0.55f, 0.05f);
            var tubeCol = tube.GetComponent<Collider>();
            if (tubeCol != null) { if (Application.isPlaying) Destroy(tubeCol); else DestroyImmediate(tubeCol); }
            tube.GetComponent<MeshRenderer>().sharedMaterial =
                DreamMaterialLibrary.Get(DreamMaterialLibrary.NeonPink);
            tube.AddComponent<MaterialPulse>().Configure(0.15f, 0.26f, 0.7f);
        }

        // ------------------------------------------------------------------ clouds

        private void BuildClouds(GameObject parent)
        {
            var rng = new System.Random(cloudSeed);
            for (int side = 0; side < 2; side++)
            {
                float sign = side == 0 ? -1f : 1f;
                for (int i = 0; i < cloudCountPerWall; i++)
                {
                    float f = (i + 0.5f) / cloudCountPerWall;
                    float z = Mathf.Lerp(ZWaterStart + 1.2f, ZWaterEnd - 1.2f, f)
                              + ((float)rng.NextDouble() - 0.5f) * 2.2f;
                    float y = 2.35f + (float)rng.NextDouble() * 0.75f;
                    float scale = 1.0f + (float)rng.NextDouble() * 0.7f;

                    CloudPropBuilder.Build(parent.transform,
                        $"Cloud_{(side == 0 ? "L" : "R")}_{i:00}",
                        new Vector3(sign * (HalfInner - 0.28f), y, z),
                        scale, cloudSeed * 100 + side * 50 + i, sign);
                }
            }
        }

        // ------------------------------------------------------------------ lighting

        private void BuildLighting(GameObject parent)
        {
            var t = parent.transform;
            var panelMat = DreamMaterialLibrary.Get(DreamMaterialLibrary.CeilingFluorescent);

            // Fluorescent ceiling panels + their lights.
            for (int i = 0; i < ceilingPanelCount; i++)
            {
                float z = Mathf.Lerp(1.6f, ZWaterEnd - 1.5f, (i + 0.5f) / ceilingPanelCount);
                ModularTileWall.Box($"CeilingPanel_{i:00}", t,
                    new Vector3(0f, wallHeight - 0.03f, z),
                    new Vector3(1.7f, 0.06f, 1.0f), panelMat, 1f, withCollider: false);

                var lightGo = new GameObject($"CeilingLight_{i:00}");
                lightGo.transform.SetParent(t, false);
                lightGo.transform.localPosition = new Vector3(0f, wallHeight - 0.55f, z);
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.96f, 0.90f);
                light.intensity = ceilingLightIntensity;
                light.range = 7.5f;
                light.shadows = i == ceilingPanelCount / 2 ? LightShadows.Soft : LightShadows.None;
                if (light.shadows != LightShadows.None) light.shadowStrength = 0.55f;
                lightGo.AddComponent<LightFlicker>().Configure(0.04f, 1.1f);
            }

            // Pastel neon strips: warm colors left, pink/violet right (as in the refs).
            LightStripBuilder.Build(t, "NeonStrips_L",
                -(HalfInner - 0.10f), -1f, lightStripY, 1.0f, ZWaterEnd,
                new[] { DreamMaterialLibrary.NeonYellow, DreamMaterialLibrary.NeonGreen, DreamMaterialLibrary.NeonCyan },
                stripSegmentLength, stripGap, stripLightEverySegments, stripLightIntensity);
            LightStripBuilder.Build(t, "NeonStrips_R",
                HalfInner - 0.10f, 1f, lightStripY, 1.0f, ZWaterEnd,
                new[] { DreamMaterialLibrary.NeonPink, DreamMaterialLibrary.NeonPink, DreamMaterialLibrary.NeonViolet },
                stripSegmentLength, stripGap, stripLightEverySegments, stripLightIntensity);

            // Pink wash flooding out of the alcove.
            var doorLight = new GameObject("DoorLight_Pink");
            doorLight.transform.SetParent(t, false);
            doorLight.transform.localPosition = new Vector3(0f, 2.1f, TotalLength - 1.1f);
            var dl = doorLight.AddComponent<Light>();
            dl.type = LightType.Point;
            dl.color = new Color(1.00f, 0.52f, 0.72f);
            dl.intensity = doorLightIntensity;
            dl.range = 9f;
            dl.shadows = LightShadows.None;
            doorLight.AddComponent<LightFlicker>().Configure(0.05f, 0.7f);

            var alcoveFill = new GameObject("AlcoveFill_Pink");
            alcoveFill.transform.SetParent(t, false);
            alcoveFill.transform.localPosition = new Vector3(0f, 1.0f, ZWaterEnd + alcoveDepth * 0.6f);
            var af = alcoveFill.AddComponent<Light>();
            af.type = LightType.Point;
            af.color = new Color(1.00f, 0.62f, 0.78f);
            af.intensity = 1.0f;
            af.range = 5f;
            af.shadows = LightShadows.None;
        }

        // ------------------------------------------------------------------ atmosphere

        /// <summary>Scene-wide haze and mint ambient — also safe to call from the editor.</summary>
        public void ApplyAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambientColor;
            RenderSettings.skybox = null;
        }

        // ------------------------------------------------------------------ helpers

        private GameObject Child(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go;
        }

        private static void DestroyChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var child = t.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }
    }
}
