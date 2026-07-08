using UnityEngine;

namespace DreamCore.Rendering
{
    /// <summary>
    /// Code-generated placeholder textures so the prototype needs zero external
    /// art: square ceramic tile grids and a soft ripple normal map.
    /// The editor setup can persist these as assets; at runtime they are built
    /// in memory on demand.
    /// </summary>
    public static class ProceduralTextures
    {
        /// <summary>
        /// Small square tiles with grout lines and subtle per-tile hue/value
        /// variation (reads as hand-set ceramic rather than a flat color).
        /// </summary>
        public static Texture2D TileGrid(
            string name, int size, int tilesAcross,
            Color baseColor, Color groutColor,
            float variation = 0.06f, int seed = 1234)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true, false)
            {
                name = name,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 4
            };

            int tilePx = size / tilesAcross;
            int groutPx = Mathf.Max(1, tilePx / 10);
            var rng = new System.Random(seed);

            // Precompute one tint per tile.
            var tints = new Color[tilesAcross * tilesAcross];
            for (int i = 0; i < tints.Length; i++)
            {
                float v = 1f + ((float)rng.NextDouble() * 2f - 1f) * variation;
                float hueShift = ((float)rng.NextDouble() * 2f - 1f) * variation * 0.5f;
                Color c = baseColor * v;
                c.r = Mathf.Clamp01(c.r - hueShift);
                c.b = Mathf.Clamp01(c.b + hueShift);
                c.a = 1f;
                tints[i] = c;
            }

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                int ty = Mathf.Min(y / tilePx, tilesAcross - 1);
                int inY = y % tilePx;
                for (int x = 0; x < size; x++)
                {
                    int tx = Mathf.Min(x / tilePx, tilesAcross - 1);
                    int inX = x % tilePx;

                    bool grout = inX < groutPx || inY < groutPx;
                    Color c;
                    if (grout)
                    {
                        c = groutColor;
                    }
                    else
                    {
                        c = tints[ty * tilesAcross + tx];
                        // Soft edge darkening so each tile reads slightly beveled.
                        float ex = Mathf.Min(inX - groutPx, tilePx - 1 - inX) / (float)tilePx;
                        float ey = Mathf.Min(inY - groutPx, tilePx - 1 - inY) / (float)tilePx;
                        float edge = Mathf.Clamp01(Mathf.Min(ex, ey) * 10f);
                        c = Color.Lerp(c * 0.92f, c, edge);
                    }
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);
            return tex;
        }

        /// <summary>
        /// Tileable soft ripple normal map built from a few sine octaves.
        /// Encoded as a standard RGB(A=1) normal texture, which URP's
        /// UnpackNormal handles for runtime-created textures.
        /// </summary>
        public static Texture2D RippleNormal(string name, int size = 256, float strength = 1.0f)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true, true)
            {
                name = name,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear
            };

            var pixels = new Color[size * size];
            float tau = Mathf.PI * 2f;
            for (int y = 0; y < size; y++)
            {
                float v = (float)y / size;
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;

                    // Height field: overlapping tileable waves in several directions.
                    // Derivatives computed analytically for smooth normals.
                    float dhdx = 0f, dhdy = 0f;
                    AddWave(u, v, 3f, 2f, 0.50f, ref dhdx, ref dhdy);
                    AddWave(u, v, -2f, 4f, 0.35f, ref dhdx, ref dhdy);
                    AddWave(u, v, 6f, -3f, 0.22f, ref dhdx, ref dhdy);
                    AddWave(u, v, -5f, -6f, 0.15f, ref dhdx, ref dhdy);

                    var n = new Vector3(-dhdx * strength, -dhdy * strength, 1f).normalized;
                    pixels[y * size + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);

                    void AddWave(float uu, float vv, float fx, float fy, float amp,
                                 ref float ddx, ref float ddy)
                    {
                        float phase = tau * (uu * fx + vv * fy);
                        float d = Mathf.Cos(phase) * amp * tau;
                        ddx += d * fx / size * 40f;
                        ddy += d * fy / size * 40f;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(true);
            return tex;
        }
    }
}
