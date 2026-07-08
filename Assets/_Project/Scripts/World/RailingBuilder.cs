using UnityEngine;
using DreamCore.Rendering;

namespace DreamCore.World
{
    /// <summary>
    /// Chrome handrails along the water channel: two horizontal rails on evenly
    /// spaced posts, plus one invisible barrier box per side so the player
    /// collision stays simple and gap-free.
    /// </summary>
    public static class RailingBuilder
    {
        private const float RailRadius = 0.025f;
        private const float PostRadius = 0.021f;
        private const float TopRailHeight = 0.92f;
        private const float MidRailHeight = 0.52f;

        public static GameObject Build(
            Transform parent, string name, float x, float baseY,
            float zStart, float zEnd, float postSpacing)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);

            var chrome = DreamMaterialLibrary.Get(DreamMaterialLibrary.ChromeRailing);
            float length = zEnd - zStart;
            float zMid = (zStart + zEnd) * 0.5f;

            // Horizontal rails (cylinder primitive axis = Y, rotated onto Z).
            CreateRail("Rail_Top", new Vector3(x, baseY + TopRailHeight, zMid), length);
            CreateRail("Rail_Mid", new Vector3(x, baseY + MidRailHeight, zMid), length);

            // Posts.
            int postCount = Mathf.Max(2, Mathf.RoundToInt(length / Mathf.Max(0.5f, postSpacing)) + 1);
            for (int i = 0; i < postCount; i++)
            {
                float z = Mathf.Lerp(zStart, zEnd, i / (float)(postCount - 1));
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"Post_{i:00}";
                post.transform.SetParent(root.transform, false);
                float h = TopRailHeight;
                post.transform.localPosition = new Vector3(x, baseY + h * 0.5f, z);
                post.transform.localScale = new Vector3(PostRadius * 2f, h * 0.5f, PostRadius * 2f);
                Strip(post);
                post.GetComponent<MeshRenderer>().sharedMaterial = chrome;
            }

            // One clean invisible barrier instead of dozens of small colliders.
            ModularTileWall.Barrier("Barrier", root.transform,
                new Vector3(x, baseY + 0.85f, zMid),
                new Vector3(0.08f, 1.7f, length));

            return root;

            void CreateRail(string railName, Vector3 center, float railLength)
            {
                var rail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rail.name = railName;
                rail.transform.SetParent(root.transform, false);
                rail.transform.localPosition = center;
                rail.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                rail.transform.localScale = new Vector3(RailRadius * 2f, railLength * 0.5f, RailRadius * 2f);
                Strip(rail);
                rail.GetComponent<MeshRenderer>().sharedMaterial = chrome;
            }
        }

        /// <summary>Remove the primitive's collider (the barrier handles collision).</summary>
        private static void Strip(GameObject primitive)
        {
            var col = primitive.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Object.Destroy(col);
                else Object.DestroyImmediate(col);
            }
            primitive.isStatic = true;
        }
    }
}
