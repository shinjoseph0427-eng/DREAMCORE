using UnityEngine;
using DreamCore.Rendering;

namespace DreamCore.World
{
    /// <summary>
    /// Rows of pastel neon fixtures mounted high on the walls: a dark housing,
    /// an emissive slab tilted toward the water, and a sparse set of real point
    /// lights so the light count stays VR-friendly.
    /// </summary>
    public static class LightStripBuilder
    {
        /// <param name="wallSign">-1 = left wall, +1 = right wall.</param>
        /// <param name="materialKeys">Emissive material keys distributed near-to-far along the strip.</param>
        public static GameObject Build(
            Transform parent, string name, float wallX, float wallSign, float y,
            float zStart, float zEnd, string[] materialKeys,
            float segmentLength, float gap, int lightEverySegments, float lightIntensity)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);

            var housingMat = DreamMaterialLibrary.Get(DreamMaterialLibrary.DarkFixture);

            float stride = segmentLength + gap;
            int count = Mathf.Max(1, Mathf.FloorToInt((zEnd - zStart) / stride));
            float used = count * stride - gap;
            float z0 = zStart + ((zEnd - zStart) - used) * 0.5f + segmentLength * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float z = z0 + i * stride;
                float frac = count == 1 ? 0f : i / (float)(count - 1);
                string key = materialKeys[Mathf.Min(materialKeys.Length - 1,
                    Mathf.FloorToInt(frac * materialKeys.Length))];
                var neonMat = DreamMaterialLibrary.Get(key);

                var seg = new GameObject($"Strip_{i:00}");
                seg.transform.SetParent(root.transform, false);
                seg.transform.localPosition = new Vector3(wallX, y, z);
                // Tilt the fixture slightly down toward the water.
                seg.transform.localRotation = Quaternion.Euler(0f, 0f, wallSign * 18f);

                ModularTileWall.Box("Housing", seg.transform,
                    Vector3.zero, new Vector3(0.16f, 0.20f, segmentLength + 0.10f),
                    housingMat, 1f, withCollider: false);

                ModularTileWall.Box("Tube", seg.transform,
                    new Vector3(-wallSign * 0.10f, -0.02f, 0f),
                    new Vector3(0.07f, 0.14f, segmentLength),
                    neonMat, 1f, withCollider: false);

                // Sparse real lights carry the color onto walls and water.
                if (lightEverySegments > 0 && i % lightEverySegments == 1 && lightIntensity > 0f)
                {
                    var lightGo = new GameObject("StripLight");
                    lightGo.transform.SetParent(seg.transform, false);
                    lightGo.transform.localPosition = new Vector3(-wallSign * 0.35f, -0.15f, 0f);
                    var light = lightGo.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = ApproxEmissionColor(neonMat);
                    light.intensity = lightIntensity;
                    light.range = 6.5f;
                    light.shadows = LightShadows.None;
                    lightGo.AddComponent<LightFlicker>().Configure(0.06f, 0.9f);
                }
            }

            return root;
        }

        private static Color ApproxEmissionColor(Material m)
        {
            Color c = m.HasProperty("_EmissionColor") ? m.GetColor("_EmissionColor") : Color.white;
            float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            return max > 0f ? new Color(c.r / max, c.g / max, c.b / max, 1f) : Color.white;
        }
    }
}
