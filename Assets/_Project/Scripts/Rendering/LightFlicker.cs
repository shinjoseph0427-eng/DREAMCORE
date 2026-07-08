using UnityEngine;

namespace DreamCore.Rendering
{
    /// <summary>
    /// Subtle fluorescent-fixture life: Perlin-noise intensity drift with a rare
    /// tiny dip. Deliberately gentle — this is dreamcore, not horror.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField, Range(0f, 0.5f), Tooltip("Max intensity deviation (fraction of base).")]
        private float amount = 0.06f;
        [SerializeField, Tooltip("Noise speed.")]
        private float speed = 0.9f;

        private Light targetLight;
        private float baseIntensity;
        private float seed;

        public void Configure(float flickerAmount, float flickerSpeed)
        {
            amount = flickerAmount;
            speed = flickerSpeed;
        }

        private void Start()
        {
            targetLight = GetComponent<Light>();
            baseIntensity = targetLight.intensity;
            seed = Random.value * 100f;
        }

        private void Update()
        {
            if (!Application.isPlaying || targetLight == null) return;

            float n = Mathf.PerlinNoise(Time.time * speed, seed); // 0..1
            float k = 1f + (n - 0.5f) * 2f * amount;

            // Rare, very small extra dip — a tired tube, not a haunted one.
            if (n > 0.965f) k -= amount * 1.5f;

            targetLight.intensity = baseIntensity * k;
        }
    }
}
