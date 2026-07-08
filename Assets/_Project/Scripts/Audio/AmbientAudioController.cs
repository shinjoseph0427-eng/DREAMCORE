using System.Collections;
using UnityEngine;

namespace DreamCore.Audio
{
    /// <summary>
    /// Placeholder-friendly ambience system: indoor pool loop, water movement,
    /// fluorescent hum, a distant reverb pad and random 3D drips. Any clip left
    /// unassigned is logged once and skipped — the system never errors without
    /// audio files.
    /// </summary>
    public class AmbientAudioController : MonoBehaviour
    {
        [Header("Loops (all optional)")]
        [SerializeField] private AudioClip poolAmbience;
        [SerializeField, Range(0f, 1f)] private float poolVolume = 0.5f;
        [SerializeField] private AudioClip waterMovement;
        [SerializeField, Range(0f, 1f)] private float waterVolume = 0.4f;
        [SerializeField] private AudioClip fluorescentHum;
        [SerializeField, Range(0f, 1f)] private float humVolume = 0.18f;
        [SerializeField] private AudioClip reverbPad;
        [SerializeField, Range(0f, 1f)] private float padVolume = 0.3f;

        [Header("Random Drips (optional)")]
        [SerializeField] private AudioClip[] dripClips;
        [SerializeField] private Vector2 dripIntervalRange = new Vector2(5f, 14f);
        [SerializeField, Range(0f, 1f)] private float dripVolume = 0.35f;
        [SerializeField, Tooltip("Local-space box drips spawn inside (roughly the corridor).")]
        private Bounds dripArea = new Bounds(new Vector3(0f, 0.6f, 15f), new Vector3(3f, 0.5f, 24f));

        [Header("Zone Mixing")]
        [SerializeField, Tooltip("How fast volumes follow AudioZone changes.")]
        private float mixLerpSpeed = 1.5f;

        private AudioSource padSource;
        private AudioSource dripSource;
        private float padBoost = 1f;

        public void SetPadBoost(float multiplier) => padBoost = Mathf.Max(0f, multiplier);

        private void Start()
        {
            CreateLoop("Loop_PoolAmbience", poolAmbience, poolVolume);
            CreateLoop("Loop_WaterMovement", waterMovement, waterVolume);
            CreateLoop("Loop_FluorescentHum", fluorescentHum, humVolume);
            padSource = CreateLoop("Loop_ReverbPad", reverbPad, padVolume);

            var dripGo = new GameObject("Drips");
            dripGo.transform.SetParent(transform, false);
            dripSource = dripGo.AddComponent<AudioSource>();
            dripSource.playOnAwake = false;
            dripSource.spatialBlend = 1f;
            dripSource.rolloffMode = AudioRolloffMode.Linear;
            dripSource.maxDistance = 18f;

            StartCoroutine(DripRoutine());
        }

        private void Update()
        {
            if (padSource != null)
            {
                float target = padVolume * padBoost;
                padSource.volume = Mathf.MoveTowards(
                    padSource.volume, target, mixLerpSpeed * Time.deltaTime);
            }
        }

        private AudioSource CreateLoop(string name, AudioClip clip, float volume)
        {
            if (clip == null)
            {
                Debug.Log($"[DreamCore] AmbientAudioController: '{name}' has no clip assigned (optional) — skipping.");
                return null;
            }
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.volume = volume;
            source.spatialBlend = 0f;
            source.Play();
            return source;
        }

        private IEnumerator DripRoutine()
        {
            if (dripClips == null || dripClips.Length == 0)
            {
                Debug.Log("[DreamCore] AmbientAudioController: no drip clips assigned (optional) — drips disabled.");
                yield break;
            }

            while (true)
            {
                yield return new WaitForSeconds(Random.Range(dripIntervalRange.x, dripIntervalRange.y));
                var clip = dripClips[Random.Range(0, dripClips.Length)];
                if (clip == null || dripSource == null) continue;

                var local = new Vector3(
                    Random.Range(dripArea.min.x, dripArea.max.x),
                    Random.Range(dripArea.min.y, dripArea.max.y),
                    Random.Range(dripArea.min.z, dripArea.max.z));
                dripSource.transform.position = transform.TransformPoint(local);
                dripSource.pitch = Random.Range(0.92f, 1.08f);
                dripSource.PlayOneShot(clip, dripVolume);
            }
        }
    }
}
