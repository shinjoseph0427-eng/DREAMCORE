using UnityEngine;
using DreamCore.Rendering;

namespace DreamCore.World
{
    /// <summary>
    /// Fluffy wall-mounted cotton clouds built from clustered spheres —
    /// deterministic per seed so rebuilds don't reshuffle the world.
    /// No colliders: they are purely decorative.
    /// </summary>
    public static class CloudPropBuilder
    {
        /// <param name="wallSign">-1 = left wall, +1 = right wall (flattens the cluster toward the wall).</param>
        public static GameObject Build(
            Transform parent, string name, Vector3 position, float scale, int seed, float wallSign)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;

            var mat = DreamMaterialLibrary.Get(DreamMaterialLibrary.CloudSoftWhite);
            var rng = new System.Random(seed);

            int mainLobes = 4 + rng.Next(3);          // big lobes along the wall
            float halfSpan = scale * (0.55f + mainLobes * 0.12f);

            for (int i = 0; i < mainLobes; i++)
            {
                float t = mainLobes == 1 ? 0.5f : i / (float)(mainLobes - 1);
                // Larger in the middle, smaller at the ends.
                float profile = 0.62f + 0.38f * Mathf.Sin(t * Mathf.PI);
                float radius = scale * profile * (0.42f + (float)rng.NextDouble() * 0.12f);

                AddPuff(
                    z: Mathf.Lerp(-halfSpan, halfSpan, t) * 0.8f,
                    y: ((float)rng.NextDouble() - 0.5f) * scale * 0.16f,
                    radius: radius);

                // A couple of smaller top puffs per big lobe for the cauliflower silhouette.
                int tops = 1 + rng.Next(2);
                for (int k = 0; k < tops; k++)
                {
                    AddPuff(
                        z: Mathf.Lerp(-halfSpan, halfSpan, t) * 0.8f + ((float)rng.NextDouble() - 0.5f) * scale * 0.4f,
                        y: radius * (0.45f + (float)rng.NextDouble() * 0.35f),
                        radius: radius * (0.45f + (float)rng.NextDouble() * 0.25f));
                }
            }

            return root;

            void AddPuff(float z, float y, float radius)
            {
                var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                puff.name = "Puff";
                puff.transform.SetParent(root.transform, false);

                // Flatten toward the wall and press slightly into it (wall-mounted look).
                float depth = radius * 0.62f;
                puff.transform.localPosition = new Vector3(wallSign * depth * 0.35f, y, z);
                puff.transform.localScale = new Vector3(depth * 2f, radius * 1.75f, radius * 2f);

                var col = puff.GetComponent<Collider>();
                if (col != null)
                {
                    if (Application.isPlaying) Object.Destroy(col);
                    else Object.DestroyImmediate(col);
                }
                puff.GetComponent<MeshRenderer>().sharedMaterial = mat;
                puff.isStatic = true;
            }
        }
    }
}
