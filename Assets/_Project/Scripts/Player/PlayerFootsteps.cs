using UnityEngine;

namespace DreamCore.Player
{
    /// <summary>
    /// Stride-based footstep sounds. Fully optional: with no clips assigned it
    /// stays silent and never errors. In this world the player wades through
    /// shallow water, so the intended clips are soft water slosh sounds.
    /// </summary>
    public class PlayerFootsteps : MonoBehaviour
    {
        [Header("Clips (optional)")]
        [SerializeField, Tooltip("Random pick per step. Water slosh / wet tile steps.")]
        private AudioClip[] stepClips;

        [Header("Tuning")]
        [SerializeField, Tooltip("Meters travelled per footstep.")]
        private float strideLength = 1.9f;
        [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
        [SerializeField, Tooltip("Random pitch range around 1.")]
        private float pitchJitter = 0.06f;

        private FirstPersonController controller;
        private AudioSource source;
        private float distanceAccumulator;
        private bool warned;

        private void Awake()
        {
            controller = GetComponent<FirstPersonController>();
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        private void Update()
        {
            if (controller == null || !controller.IsGrounded) return;

            distanceAccumulator += controller.CurrentSpeed * Time.deltaTime;
            if (distanceAccumulator < strideLength) return;
            distanceAccumulator = 0f;

            if (stepClips == null || stepClips.Length == 0)
            {
                if (!warned)
                {
                    Debug.Log("[DreamCore] PlayerFootsteps: no step clips assigned (optional) — staying silent.");
                    warned = true;
                }
                return;
            }

            var clip = stepClips[Random.Range(0, stepClips.Length)];
            if (clip == null) return;
            source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            source.PlayOneShot(clip, volume);
        }
    }
}
