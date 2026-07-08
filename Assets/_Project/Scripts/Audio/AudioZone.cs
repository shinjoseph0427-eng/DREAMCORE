using UnityEngine;

namespace DreamCore.Audio
{
    /// <summary>
    /// Trigger volume that shifts the ambient mix while the player is inside
    /// (e.g. boost the reverb pad near the pink door). Requires a trigger
    /// collider on the same object.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AudioZone : MonoBehaviour
    {
        [SerializeField, Tooltip("Reverb pad volume multiplier while inside the zone.")]
        private float padBoost = 1.6f;

        private AmbientAudioController controller;

        private void Start()
        {
            GetComponent<Collider>().isTrigger = true;
            controller = FindFirstObjectByType<AmbientAudioController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (controller != null && other.GetComponent<CharacterController>() != null)
            {
                controller.SetPadBoost(padBoost);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (controller != null && other.GetComponent<CharacterController>() != null)
            {
                controller.SetPadBoost(1f);
            }
        }
    }
}
