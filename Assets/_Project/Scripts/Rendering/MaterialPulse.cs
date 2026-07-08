using UnityEngine;

namespace DreamCore.Rendering
{
    /// <summary>
    /// Gently pulses a renderer's emission via MaterialPropertyBlock (no material
    /// instantiation, safe on shared/saved materials). Used for the door glow
    /// and the pink tube so the far end feels alive.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class MaterialPulse : MonoBehaviour
    {
        [SerializeField, Tooltip("Pulse amount relative to base emission (0.1 = ±10%).")]
        private float amplitude = 0.12f;
        [SerializeField, Tooltip("Pulses per second.")]
        private float frequency = 0.22f;
        [SerializeField, Tooltip("Phase offset so multiple pulses don't sync.")]
        private float phase = 0f;

        private Renderer targetRenderer;
        private MaterialPropertyBlock block;
        private Color baseEmission;
        private bool hasEmission;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        public void Configure(float amp, float freq, float ph = 0f)
        {
            amplitude = amp;
            frequency = freq;
            phase = ph;
        }

        private void Start()
        {
            targetRenderer = GetComponent<Renderer>();
            block = new MaterialPropertyBlock();
            var mat = targetRenderer.sharedMaterial;
            hasEmission = mat != null && mat.HasProperty(EmissionColorId);
            if (hasEmission) baseEmission = mat.GetColor(EmissionColorId);
        }

        private void Update()
        {
            if (!hasEmission || !Application.isPlaying) return;
            float k = 1f + Mathf.Sin((Time.time + phase) * frequency * Mathf.PI * 2f) * amplitude;
            targetRenderer.GetPropertyBlock(block);
            block.SetColor(EmissionColorId, baseEmission * k);
            targetRenderer.SetPropertyBlock(block);
        }
    }
}
