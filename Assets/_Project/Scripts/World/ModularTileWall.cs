using UnityEngine;

namespace DreamCore.World
{
    /// <summary>
    /// Factory for tiled box geometry. Unlike scaled cube primitives, these boxes
    /// get UVs proportional to their world size, so one tile material keeps a
    /// consistent physical tile size (e.g. 10 cm) on every wall, floor and step.
    /// </summary>
    public static class ModularTileWall
    {
        /// <summary>
        /// Create a box with world-scaled UVs.
        /// </summary>
        /// <param name="uvPerMeter">UV units per world meter (1 / (tileScale * tilesInTexture)).</param>
        public static GameObject Box(
            string name, Transform parent, Vector3 center, Vector3 size,
            Material material, float uvPerMeter, bool withCollider = true, bool isStatic = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.isStatic = isStatic;

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = BuildBoxMesh(size, uvPerMeter);

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;

            if (withCollider)
            {
                var box = go.AddComponent<BoxCollider>();
                box.size = size;
            }
            return go;
        }

        /// <summary>Invisible collision-only box (used as railing barrier etc.).</summary>
        public static GameObject Barrier(string name, Transform parent, Vector3 center, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.isStatic = true;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            return go;
        }

        /// <summary>24-vertex box mesh; each face's UVs span (faceWidth, faceHeight) * uvPerMeter.</summary>
        public static Mesh BuildBoxMesh(Vector3 size, float uvPerMeter)
        {
            Vector3 h = size * 0.5f;
            var verts = new Vector3[24];
            var norms = new Vector3[24];
            var uvs = new Vector2[24];
            var tris = new int[36];

            int v = 0, t = 0;

            // uDir/vDir are chosen so cross(uDir, vDir) == -normal, which makes the
            // shared (0,3,2)/(0,2,1) triangle pattern clockwise (front-facing) from
            // outside in Unity's left-handed convention.
            // right (+X): face plane ZY
            AddFace(new Vector3(h.x, 0, 0), Vector3.right, new Vector3(0, 0, 1), Vector3.up, size.z, size.y);
            // left (-X)
            AddFace(new Vector3(-h.x, 0, 0), Vector3.left, new Vector3(0, 0, -1), Vector3.up, size.z, size.y);
            // top (+Y): face plane XZ
            AddFace(new Vector3(0, h.y, 0), Vector3.up, Vector3.right, Vector3.forward, size.x, size.z);
            // bottom (-Y)
            AddFace(new Vector3(0, -h.y, 0), Vector3.down, Vector3.right, new Vector3(0, 0, -1), size.x, size.z);
            // front (+Z)
            AddFace(new Vector3(0, 0, h.z), Vector3.forward, Vector3.left, Vector3.up, size.x, size.y);
            // back (-Z)
            AddFace(new Vector3(0, 0, -h.z), Vector3.back, Vector3.right, Vector3.up, size.x, size.y);

            var mesh = new Mesh { name = $"Box_{size.x:0.##}x{size.y:0.##}x{size.z:0.##}" };
            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;

            void AddFace(Vector3 center, Vector3 normal, Vector3 uDir, Vector3 vDir, float uSize, float vSize)
            {
                Vector3 uh = uDir * (uSize * 0.5f);
                Vector3 vh = vDir * (vSize * 0.5f);
                float uu = uSize * uvPerMeter;
                float vv = vSize * uvPerMeter;

                verts[v + 0] = center - uh - vh;
                verts[v + 1] = center + uh - vh;
                verts[v + 2] = center + uh + vh;
                verts[v + 3] = center - uh + vh;
                for (int i = 0; i < 4; i++) norms[v + i] = normal;
                uvs[v + 0] = new Vector2(0, 0);
                uvs[v + 1] = new Vector2(uu, 0);
                uvs[v + 2] = new Vector2(uu, vv);
                uvs[v + 3] = new Vector2(0, vv);

                tris[t + 0] = v + 0; tris[t + 1] = v + 3; tris[t + 2] = v + 2;
                tris[t + 3] = v + 0; tris[t + 4] = v + 2; tris[t + 5] = v + 1;
                v += 4; t += 6;
            }
        }

        /// <summary>Flat horizontal grid mesh (used for the water surface), UVs world-scaled.</summary>
        public static Mesh BuildGridMesh(float width, float length, int xDivs, int zDivs, float uvPerMeter)
        {
            int vx = xDivs + 1, vz = zDivs + 1;
            var verts = new Vector3[vx * vz];
            var uvs = new Vector2[vx * vz];
            var norms = new Vector3[vx * vz];
            var tris = new int[xDivs * zDivs * 6];

            for (int z = 0; z < vz; z++)
            {
                for (int x = 0; x < vx; x++)
                {
                    int i = z * vx + x;
                    float px = (x / (float)xDivs - 0.5f) * width;
                    float pz = (z / (float)zDivs - 0.5f) * length;
                    verts[i] = new Vector3(px, 0f, pz);
                    uvs[i] = new Vector2((px + width * 0.5f) * uvPerMeter, (pz + length * 0.5f) * uvPerMeter);
                    norms[i] = Vector3.up;
                }
            }

            int t = 0;
            for (int z = 0; z < zDivs; z++)
            {
                for (int x = 0; x < xDivs; x++)
                {
                    int i = z * vx + x;
                    tris[t++] = i; tris[t++] = i + vx; tris[t++] = i + vx + 1;
                    tris[t++] = i; tris[t++] = i + vx + 1; tris[t++] = i + 1;
                }
            }

            var mesh = new Mesh { name = $"Grid_{width:0.#}x{length:0.#}" };
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.normals = norms;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}
