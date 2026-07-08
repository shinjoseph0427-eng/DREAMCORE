using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DreamCore.Rendering
{
    /// <summary>
    /// Single source for every material in the prototype. Materials are created
    /// procedurally (URP/Lit when URP is active, Standard as a safety fallback)
    /// and cached by key. In the editor, previously saved material assets in
    /// Assets/_Project/Materials/ are used instead, so tweaks persist.
    /// </summary>
    public static class DreamMaterialLibrary
    {
        // Material keys.
        public const string AquaGlossTile = "AquaGlossTile";
        public const string BeigeWetTile = "BeigeWetTile";
        public const string CeilingTile = "CeilingTile";
        public const string WaterTurquoiseDream = "Water_Turquoise_Dream";
        public const string ChromeRailing = "Chrome_Railing";
        public const string CloudSoftWhite = "CloudSoftWhite";
        public const string PinkPortalDoor = "PinkPortalDoor";
        public const string PinkGlowWall = "PinkGlowWall";
        public const string NeonPink = "NeonPink";
        public const string NeonYellow = "NeonYellow";
        public const string NeonCyan = "NeonCyan";
        public const string NeonGreen = "NeonGreen";
        public const string NeonViolet = "NeonViolet";
        public const string CeilingFluorescent = "CeilingFluorescent";
        public const string DarkFixture = "DarkFixture";

        /// <summary>Keys in build order, with the Materials/ subfolder each asset lives in.</summary>
        public static readonly (string key, string subfolder)[] All =
        {
            (AquaGlossTile, "Tiles"), (BeigeWetTile, "Tiles"), (CeilingTile, "Tiles"),
            (WaterTurquoiseDream, "Water"), (ChromeRailing, "Metal"), (CloudSoftWhite, "Clouds"),
            (PinkPortalDoor, "Door"), (PinkGlowWall, "Door"),
            (NeonPink, "Lights"), (NeonYellow, "Lights"), (NeonCyan, "Lights"),
            (NeonGreen, "Lights"), (NeonViolet, "Lights"),
            (CeilingFluorescent, "Lights"), (DarkFixture, "Metal"),
        };

        public static string AssetPath(string key)
        {
            foreach (var (k, sub) in All)
                if (k == key) return $"Assets/_Project/Materials/{sub}/{key}.mat";
            return $"Assets/_Project/Materials/{key}.mat";
        }

        private static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

        public static bool UsingURP => GraphicsSettings.currentRenderPipeline != null;

        public static void ClearCache() => cache.Clear();

        public static Material Get(string key)
        {
            if (cache.TryGetValue(key, out var cached) && cached != null) return cached;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(AssetPath(key));
                if (asset != null)
                {
                    cache[key] = asset;
                    return asset;
                }
            }
#endif
            var mat = Build(key);
            mat.name = key;
            cache[key] = mat;
            return mat;
        }

        // ------------------------------------------------------------------ builders

        private static Material Build(string key)
        {
            switch (key)
            {
                case AquaGlossTile:
                    return Lit(new Color(0.42f, 0.80f, 0.86f), 0.78f, 0f,
                        ProceduralTextures.TileGrid("Tex_AquaTile", 512, 8,
                            new Color(0.40f, 0.78f, 0.85f), new Color(0.85f, 0.83f, 0.75f), 0.10f, 101));

                case BeigeWetTile:
                    return Lit(new Color(0.91f, 0.89f, 0.81f), 0.70f, 0f,
                        ProceduralTextures.TileGrid("Tex_BeigeTile", 512, 8,
                            new Color(0.91f, 0.88f, 0.79f), new Color(0.78f, 0.75f, 0.66f), 0.05f, 202));

                case CeilingTile:
                    return Lit(new Color(0.95f, 0.93f, 0.88f), 0.55f, 0f,
                        ProceduralTextures.TileGrid("Tex_CeilingTile", 512, 8,
                            new Color(0.95f, 0.93f, 0.87f), new Color(0.84f, 0.82f, 0.75f), 0.03f, 303));

                case WaterTurquoiseDream:
                {
                    var m = Lit(new Color(0.24f, 0.62f, 0.50f, 0.62f), 0.93f, 0f, null);
                    MakeTransparent(m);
                    SetNormalMap(m, ProceduralTextures.RippleNormal("Tex_WaterRippleN", 256, 1.0f), 0.55f);
                    return m;
                }

                case ChromeRailing:
                    return Lit(new Color(0.92f, 0.90f, 0.86f), 0.92f, 1f, null);

                case CloudSoftWhite:
                {
                    var m = Lit(new Color(0.985f, 0.98f, 0.975f), 0.18f, 0f, null);
                    // Faint self-glow keeps the cotton bright inside the haze.
                    SetEmission(m, new Color(1f, 0.99f, 0.985f) * 0.22f);
                    return m;
                }

                case PinkPortalDoor:
                {
                    var m = Lit(new Color(1.00f, 0.79f, 0.85f), 0.62f, 0f, null);
                    SetEmission(m, new Color(1.00f, 0.62f, 0.76f) * 0.45f);
                    return m;
                }

                case PinkGlowWall:
                {
                    var m = Lit(new Color(1.00f, 0.72f, 0.80f), 0.60f, 0f, null);
                    SetEmission(m, new Color(1.00f, 0.55f, 0.72f) * 0.9f);
                    return m;
                }

                case NeonPink: return Emissive(new Color(1.00f, 0.43f, 0.78f), 4.2f);
                case NeonYellow: return Emissive(new Color(1.00f, 0.88f, 0.40f), 3.6f);
                case NeonCyan: return Emissive(new Color(0.40f, 0.94f, 0.88f), 3.6f);
                case NeonGreen: return Emissive(new Color(0.49f, 1.00f, 0.56f), 3.6f);
                case NeonViolet: return Emissive(new Color(0.78f, 0.49f, 1.00f), 3.8f);
                case CeilingFluorescent: return Emissive(new Color(1.00f, 0.96f, 0.91f), 3.2f);

                case DarkFixture: return Lit(new Color(0.16f, 0.16f, 0.17f), 0.45f, 0.4f, null);

                default:
                    Debug.LogWarning($"[DreamCore] Unknown material key '{key}', using magenta fallback.");
                    return Lit(Color.magenta, 0.5f, 0f, null);
            }
        }

        // ------------------------------------------------------------------ helpers

        private static Shader LitShader()
        {
            Shader s = UsingURP ? Shader.Find("Universal Render Pipeline/Lit") : null;
            if (s == null) s = Shader.Find("Standard");
            return s;
        }

        private static Material Lit(Color color, float smoothness, float metallic, Texture2D albedo)
        {
            var m = new Material(LitShader());
            bool urp = m.shader.name.Contains("Universal");

            m.SetColor(urp ? "_BaseColor" : "_Color", color);
            m.SetFloat(urp ? "_Smoothness" : "_Glossiness", smoothness);
            m.SetFloat("_Metallic", metallic);
            if (albedo != null) m.SetTexture(urp ? "_BaseMap" : "_MainTex", albedo);
            m.enableInstancing = true;
            return m;
        }

        private static void SetNormalMap(Material m, Texture2D normal, float strength)
        {
            m.SetTexture("_BumpMap", normal);
            m.SetFloat("_BumpScale", strength);
            m.EnableKeyword("_NORMALMAP");
        }

        private static void SetEmission(Material m, Color hdrColor)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", hdrColor);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        private static Material Emissive(Color color, float intensity)
        {
            var m = Lit(color * 0.6f, 0.6f, 0f, null);
            SetEmission(m, color * intensity);
            return m;
        }

        /// <summary>Configure a URP/Lit (or Standard) material for alpha blending.</summary>
        public static void MakeTransparent(Material m)
        {
            bool urp = m.shader.name.Contains("Universal");
            if (urp)
            {
                m.SetFloat("_Surface", 1f); // 0 = opaque, 1 = transparent
                m.SetFloat("_Blend", 0f);   // alpha
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                // Standard shader "Fade" mode.
                m.SetFloat("_Mode", 2f);
                m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.EnableKeyword("_ALPHABLEND_ON");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
            m.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}
