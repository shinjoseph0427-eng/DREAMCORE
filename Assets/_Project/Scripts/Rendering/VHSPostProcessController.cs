using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DreamCore.Rendering
{
    /// <summary>
    /// Owns the global URP post-processing volume: soft halation bloom, pastel
    /// grade (mint shadows / pink-yellow highlights via split toning), gentle
    /// vignette, chromatic aberration and film grain — plus a very subtle
    /// animated wobble on the aberration/grain for VHS softness.
    /// Builds its profile in code if none is assigned, so it works with zero assets.
    /// </summary>
    [RequireComponent(typeof(Volume))]
    public class VHSPostProcessController : MonoBehaviour
    {
        [Header("Bloom")]
        [SerializeField] private float bloomIntensity = 0.85f;
        [SerializeField] private float bloomThreshold = 0.82f;
        [SerializeField, Range(0f, 1f)] private float bloomScatter = 0.72f;

        [Header("Grade")]
        [SerializeField] private float postExposure = 0.15f;
        [SerializeField] private float saturation = 6f;
        [SerializeField] private float contrast = -4f;
        [SerializeField] private Color shadowTint = new Color(0.62f, 0.85f, 0.78f);
        [SerializeField] private Color highlightTint = new Color(1.00f, 0.81f, 0.88f);

        [Header("VHS Layer")]
        [SerializeField] private float vignetteIntensity = 0.26f;
        [SerializeField] private float chromaticAberration = 0.09f;
        [SerializeField] private float filmGrain = 0.22f;
        [SerializeField, Tooltip("Animated wobble on aberration/grain (fraction of base).")]
        private float wobbleAmount = 0.3f;
        [SerializeField] private float wobbleSpeed = 0.55f;

        [Header("Optional")]
        [SerializeField, Tooltip("Very subtle Gaussian DoF. Off by default (costs clarity).")]
        private bool enableDepthOfField = false;

        private Volume volume;
        private Bloom bloom;
        private ChromaticAberration aberration;
        private FilmGrain grain;

        /// <summary>Find or create the scene's global post-processing volume.</summary>
        public static VHSPostProcessController EnsureSceneVolume()
        {
            var existing = FindFirstObjectByType<VHSPostProcessController>();
            if (existing != null) return existing;

            var go = new GameObject("PostProcessing");
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 10f;
            return go.AddComponent<VHSPostProcessController>();
        }

        /// <summary>Enable post-processing + SMAA on a camera (URP only).</summary>
        public static void ConfigureCamera(Camera cam)
        {
            if (GraphicsSettings.currentRenderPipeline == null) return;
            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.dithering = true;
        }

        private void Awake()
        {
            volume = GetComponent<Volume>();
            volume.isGlobal = true;
            if (volume.sharedProfile == null)
            {
                volume.profile = BuildProfile(); // runtime in-memory profile
            }
            CacheOverrides();
        }

        private void CacheOverrides()
        {
            var profile = Application.isPlaying ? volume.profile : volume.sharedProfile;
            if (profile == null) return;
            profile.TryGet(out bloom);
            profile.TryGet(out aberration);
            profile.TryGet(out grain);
        }

        private void Update()
        {
            if (!Application.isPlaying || wobbleAmount <= 0f) return;

            float t = Time.unscaledTime * wobbleSpeed;
            if (aberration != null)
            {
                float n = Mathf.PerlinNoise(t, 3.7f) - 0.5f;
                aberration.intensity.value =
                    Mathf.Max(0f, chromaticAberration * (1f + n * 2f * wobbleAmount));
            }
            if (grain != null)
            {
                float n = Mathf.PerlinNoise(t * 1.7f, 9.1f) - 0.5f;
                grain.intensity.value =
                    Mathf.Clamp01(filmGrain * (1f + n * 2f * wobbleAmount));
            }
        }

        /// <summary>Create the full profile from the serialized settings.</summary>
        public VolumeProfile BuildProfile()
        {
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "DreamCoreVolumeProfile";

            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.Neutral);

            var b = profile.Add<Bloom>(true);
            b.intensity.Override(bloomIntensity);
            b.threshold.Override(bloomThreshold);
            b.scatter.Override(bloomScatter);

            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(postExposure);
            color.saturation.Override(saturation);
            color.contrast.Override(contrast);
            color.colorFilter.Override(new Color(1.00f, 0.985f, 0.965f));

            var split = profile.Add<SplitToning>(true);
            split.shadows.Override(shadowTint);
            split.highlights.Override(highlightTint);
            split.balance.Override(-12f);

            var vig = profile.Add<Vignette>(true);
            vig.intensity.Override(vignetteIntensity);
            vig.smoothness.Override(0.45f);

            var ca = profile.Add<ChromaticAberration>(true);
            ca.intensity.Override(chromaticAberration);

            var fg = profile.Add<FilmGrain>(true);
            fg.type.Override(FilmGrainLookup.Thin1);
            fg.intensity.Override(filmGrain);
            fg.response.Override(0.7f);

            if (enableDepthOfField)
            {
                var dof = profile.Add<DepthOfField>(true);
                dof.mode.Override(DepthOfFieldMode.Gaussian);
                dof.gaussianStart.Override(14f);
                dof.gaussianEnd.Override(40f);
                dof.gaussianMaxRadius.Override(0.6f);
            }

            return profile;
        }
    }
}
