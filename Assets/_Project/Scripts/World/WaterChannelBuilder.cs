using UnityEngine;
using DreamCore.Rendering;

namespace DreamCore.World
{
    /// <summary>
    /// Builds the central shallow water surface: a few flat grid segments
    /// (segmented so URP's per-object light limit never starves them) driven
    /// by one shared <see cref="WaterSurfaceAnimator"/>.
    /// </summary>
    public static class WaterChannelBuilder
    {
        public static GameObject Build(
            Transform parent, float width, float zStart, float zEnd,
            float surfaceY, int segments)
        {
            var root = new GameObject("WaterSurface");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0f, surfaceY, 0f);

            var material = DreamMaterialLibrary.Get(DreamMaterialLibrary.WaterTurquoiseDream);
            float totalLength = zEnd - zStart;
            segments = Mathf.Max(1, segments);
            float segLength = totalLength / segments;

            // ~0.4 UV per meter -> the ripple texture repeats every 2.5 m.
            const float uvPerMeter = 0.4f;

            for (int i = 0; i < segments; i++)
            {
                var seg = new GameObject($"Water_{i:00}");
                seg.transform.SetParent(root.transform, false);
                float zCenter = zStart + segLength * (i + 0.5f);
                seg.transform.localPosition = new Vector3(0f, 0f, zCenter);

                var mf = seg.AddComponent<MeshFilter>();
                mf.sharedMesh = ModularTileWall.BuildGridMesh(width, segLength, 6, 8, uvPerMeter);
                var mr = seg.AddComponent<MeshRenderer>();
                mr.sharedMaterial = material;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }

            root.AddComponent<WaterSurfaceAnimator>();
            return root;
        }
    }
}
