using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using DreamCore.Core;
using DreamCore.Rendering;
using DreamCore.World;

namespace DreamCore.EditorTools
{
    /// <summary>
    /// One-shot project setup that runs automatically the first time the project
    /// is opened (and can be re-run from the "Dream Core" menu):
    ///   1. Linear color space + product settings
    ///   2. Creates and assigns a URP pipeline asset (HDR on)
    ///   3. Persists all procedural materials/textures as assets
    ///   4. Saves the post-processing volume profile as an asset
    ///   5. Builds and saves Main.unity (full corridor) and LightingTest.unity
    ///   6. Registers both scenes in Build Settings
    /// </summary>
    public static class DreamCoreProjectSetup
    {
        private const string SettingsFolder = "Assets/_Project/Settings";
        private const string MarkerPath = SettingsFolder + "/setup_complete.marker";
        private const string UrpAssetPath = SettingsFolder + "/DreamCore_URP.asset";
        private const string UrpRendererPath = SettingsFolder + "/DreamCore_URP_Renderer.asset";
        private const string VolumeProfilePath = "Assets/_Project/Materials/PostProcess/DreamCoreVolumeProfile.asset";
        private const string LightingSettingsPath = SettingsFolder + "/DreamCore_Lighting.lighting";
        private const string TexturesFolder = "Assets/_Project/Art/Textures";
        private const string MainScenePath = "Assets/_Project/Scenes/Main.unity";
        private const string LightingTestScenePath = "Assets/_Project/Scenes/LightingTest.unity";

        [InitializeOnLoadMethod]
        private static void AutoSetupOnFirstOpen()
        {
            if (File.Exists(MarkerPath)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            // Delay one tick so the asset database is fully ready.
            EditorApplication.delayCall += () =>
            {
                if (File.Exists(MarkerPath)) return;
                Debug.Log("[DreamCore] First open detected — running full project setup…");
                RunFullSetup();
            };
        }

        [MenuItem("Tools/DREAM CORE/Setup Project", priority = 0)]
        public static void RunFullSetup()
        {
            try
            {
                EnsureFolders();
                ConfigurePlayerSettings();
                var pipeline = EnsureUrpAssigned();
                SaveMaterialAssets();
                var profile = EnsureVolumeProfileAsset();
                BuildMainScene(profile);
                BuildLightingTestScene();
                RegisterScenes();

                File.WriteAllText(MarkerPath, "DREAM CORE setup completed. Delete this file and reopen the project (or use the Dream Core menu) to re-run setup.");
                AssetDatabase.ImportAsset(MarkerPath);
                AssetDatabase.SaveAssets();

                EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
                Debug.Log("[DreamCore] Setup complete — Main.unity is open. Press Play.  " +
                          (pipeline != null ? "(URP active)" : "(URP could not be assigned — check logs)"));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DreamCore] Setup failed: {e}");
            }
        }

        // ------------------------------------------------------------------ steps

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(SettingsFolder);
            Directory.CreateDirectory(TexturesFolder);
            Directory.CreateDirectory("Assets/_Project/Scenes");
            Directory.CreateDirectory("Assets/_Project/Materials/PostProcess");
            AssetDatabase.Refresh();
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.companyName = "DreamCore";
            PlayerSettings.productName = "DREAM CORE";
        }

        private static UniversalRenderPipelineAsset EnsureUrpAssigned()
        {
            var rp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (rp == null)
            {
                var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, UrpRendererPath);
                TryAssignDefaultPostProcessData(rendererData);

                rp = UniversalRenderPipelineAsset.Create(rendererData);
                rp.supportsHDR = true;
                rp.msaaSampleCount = 4;
                rp.shadowDistance = 40f;
                rp.supportsCameraDepthTexture = true;
                AssetDatabase.CreateAsset(rp, UrpAssetPath);
                EditorUtility.SetDirty(rendererData);
                EditorUtility.SetDirty(rp);
                AssetDatabase.SaveAssets();
            }

            GraphicsSettings.defaultRenderPipeline = rp;

            // Assign to every quality level so switching quality never drops URP.
            int active = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = rp;
            }
            QualitySettings.SetQualityLevel(active, false);

            TryEnsureUrpGlobalSettings();
            DreamMaterialLibrary.ClearCache(); // materials must rebuild against URP/Lit
            return rp;
        }

        private static void TryAssignDefaultPostProcessData(UniversalRendererData rendererData)
        {
            if (rendererData.postProcessData != null) return;
            rendererData.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            if (rendererData.postProcessData == null)
                Debug.LogWarning("[DreamCore] Could not load default PostProcessData — post-processing may not render.");
        }

        private static void TryEnsureUrpGlobalSettings()
        {
            Debug.LogWarning("[DreamCore] Skipping optional URP global settings setup because the API is not public in this Unity/URP version.");
        }

        [MenuItem("Dream Core/Save Material + Texture Assets", priority = 20)]
        public static void SaveMaterialAssets()
        {
            foreach (var (key, _) in DreamMaterialLibrary.All)
            {
                string path = DreamMaterialLibrary.AssetPath(key);
                if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) continue;

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var mat = DreamMaterialLibrary.Get(key);
                if (AssetDatabase.Contains(mat)) continue;

                PersistTexture(mat, "_BaseMap");
                PersistTexture(mat, "_MainTex");
                PersistTexture(mat, "_BumpMap");
                AssetDatabase.CreateAsset(mat, path);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[DreamCore] Material assets saved to Assets/_Project/Materials/");
        }

        private static void PersistTexture(Material mat, string property)
        {
            if (!mat.HasProperty(property)) return;
            var tex = mat.GetTexture(property);
            if (tex == null || AssetDatabase.Contains(tex)) return;
            string path = $"{TexturesFolder}/{tex.name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (existing != null) mat.SetTexture(property, existing);
            else AssetDatabase.CreateAsset(tex, path);
        }

        private static VolumeProfile EnsureVolumeProfileAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (existing != null) return existing;

            // Build the profile with the controller's defaults via a temp instance.
            var temp = new GameObject("~TempVolume");
            try
            {
                var controller = temp.AddComponent<VHSPostProcessController>();
                var profile = controller.BuildProfile();
                AssetDatabase.CreateAsset(profile, VolumeProfilePath);
                foreach (var component in profile.components)
                {
                    AssetDatabase.AddObjectToAsset(component, profile);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(VolumeProfilePath);
                return profile;
            }
            finally
            {
                Object.DestroyImmediate(temp);
            }
        }

        private static LightingSettings EnsureLightingSettingsAsset()
        {
            var ls = AssetDatabase.LoadAssetAtPath<LightingSettings>(LightingSettingsPath);
            if (ls == null)
            {
                ls = new LightingSettings
                {
                    name = "DreamCore_Lighting",
                    bakedGI = false,
                    realtimeGI = false
                };
                AssetDatabase.CreateAsset(ls, LightingSettingsPath);
            }
            return ls;
        }

        private static void BuildMainScene(VolumeProfile profile)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrapGo = new GameObject("SceneBootstrapper");
            var bootstrap = bootstrapGo.AddComponent<SceneBootstrapper>();
            bootstrap.EnsureAll(runtime: false); // world + post-processing + player rig

            // Point the volume at the saved profile so the grade is editable as an asset.
            var controller = Object.FindFirstObjectByType<VHSPostProcessController>();
            if (controller != null && profile != null)
            {
                controller.GetComponent<Volume>().sharedProfile = profile;
            }

            var world = Object.FindFirstObjectByType<DreamcoreWorldBuilder>();
            if (world != null) world.ApplyAtmosphere();

            Lightmapping.lightingSettings = EnsureLightingSettingsAsset();

            EditorSceneManager.SaveScene(scene, MainScenePath);
        }

        private static void BuildLightingTestScene()
        {
            if (File.Exists(LightingTestScenePath)) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Small material gallery: every library material on a sphere over a tiled floor.
            var root = new GameObject("MaterialGallery");
            ModularTileWall.Box("Floor", root.transform, new Vector3(0f, -0.1f, 0f),
                new Vector3(14f, 0.2f, 6f),
                DreamMaterialLibrary.Get(DreamMaterialLibrary.BeigeWetTile), 1.25f);

            int i = 0;
            foreach (var (key, _) in DreamMaterialLibrary.All)
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = $"Mat_{key}";
                sphere.transform.SetParent(root.transform, false);
                sphere.transform.localPosition = new Vector3(-6.5f + (i % 8) * 1.8f, 0.75f, i < 8 ? -1.2f : 1.2f);
                sphere.GetComponent<MeshRenderer>().sharedMaterial = DreamMaterialLibrary.Get(key);
                i++;
            }

            CreateLight("Key_White", new Vector3(0f, 3.5f, -2f), new Color(1f, 0.96f, 0.9f), 1.3f);
            CreateLight("Fill_Pink", new Vector3(4f, 2.5f, 2f), new Color(1f, 0.55f, 0.75f), 1.0f);
            CreateLight("Fill_Cyan", new Vector3(-4f, 2.5f, 2f), new Color(0.5f, 0.9f, 0.85f), 1.0f);

            var camGo = new GameObject("Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.transform.SetPositionAndRotation(new Vector3(0f, 2.2f, -6f), Quaternion.Euler(12f, 0f, 0f));
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.13f, 0.15f, 0.16f);

            Lightmapping.lightingSettings = EnsureLightingSettingsAsset();
            EditorSceneManager.SaveScene(scene, LightingTestScenePath);

            void CreateLight(string name, Vector3 pos, Color color, float intensity)
            {
                var go = new GameObject(name);
                go.transform.position = pos;
                var l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = color;
                l.intensity = intensity;
                l.range = 12f;
            }
        }

        private static void RegisterScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScenePath, true),
                new EditorBuildSettingsScene(LightingTestScenePath, true),
            };
        }

        [MenuItem("Dream Core/Rebuild World In Open Scene", priority = 10)]
        public static void RebuildWorldInOpenScene()
        {
            var world = Object.FindFirstObjectByType<DreamcoreWorldBuilder>();
            if (world == null)
            {
                world = new GameObject("DreamcoreWorld").AddComponent<DreamcoreWorldBuilder>();
            }
            world.Build();
            world.ApplyAtmosphere();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[DreamCore] World rebuilt.");
        }
    }
}
