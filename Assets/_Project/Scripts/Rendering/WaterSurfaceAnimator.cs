using System.Collections.Generic;
using UnityEngine;

namespace DreamCore.Rendering
{
    /// <summary>
    /// Dreamlike pool water motion: slowly scrolling ripple normals (two
    /// opposing pseudo-layers) plus a barely perceptible vertical bob.
    /// Instantiates one runtime material shared by all water segments so the
    /// saved material asset is never mutated.
    /// </summary>
    public class WaterSurfaceAnimator : MonoBehaviour
    {
        [Header("Ripples")]
        [SerializeField, Tooltip("UV scroll speed of the ripple normal map (uv/sec).")]
        private float rippleSpeed = 0.022f;
        [SerializeField, Tooltip("Normal map strength.")]
        private float rippleStrength = 0.55f;
        [SerializeField, Tooltip("Slow breathing of ripple strength (0 = constant).")]
        private float strengthWobble = 0.15f;

        [Header("Surface Bob")]
        [SerializeField, Tooltip("Vertical bob amplitude in meters. Keep tiny.")]
        private float bobAmplitude = 0.012f;
        [SerializeField] private float bobSpeed = 0.35f;

        private Material runtimeMaterial;
        private float baseY;
        private static readonly int BumpMapId = Shader.PropertyToID("_BumpMap");
        private static readonly int BumpScaleId = Shader.PropertyToID("_BumpScale");
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

        private void Start()
        {
            baseY = transform.localPosition.y;
            if (!Application.isPlaying) return;

            // One instanced material for every segment.
            var renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
            if (renderers.Count == 0) return;
            var source = renderers[0].sharedMaterial;
            if (source == null) return;
            runtimeMaterial = new Material(source) { name = source.name + " (Instance)" };
            foreach (var r in renderers) r.sharedMaterial = runtimeMaterial;
        }

        private void Update()
        {
            if (!Application.isPlaying || runtimeMaterial == null) return;

            float t = Time.time;

            // Two drift directions combined into one offset path (cheap two-layer feel).
            var offset = new Vector2(
                t * rippleSpeed + Mathf.Sin(t * 0.13f) * 0.05f,
                t * rippleSpeed * 0.7f + Mathf.Cos(t * 0.09f) * 0.05f);
            if (runtimeMaterial.HasProperty(BumpMapId))
                runtimeMaterial.SetTextureOffset(BumpMapId, offset);
            if (runtimeMaterial.HasProperty(BaseMapId))
                runtimeMaterial.SetTextureOffset(BaseMapId, offset * -0.35f);

            if (runtimeMaterial.HasProperty(BumpScaleId))
            {
                float wobble = 1f + Mathf.Sin(t * 0.21f) * strengthWobble;
                runtimeMaterial.SetFloat(BumpScaleId, rippleStrength * wobble);
            }

            var p = transform.localPosition;
            p.y = baseY + Mathf.Sin(t * bobSpeed * Mathf.PI * 2f) * bobAmplitude;
            transform.localPosition = p;
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null && Application.isPlaying) Destroy(runtimeMaterial);
        }
    }
}
