using UnityEngine;
using DreamCore.Player;
using DreamCore.UI;

namespace DreamCore.World
{
    /// <summary>
    /// The pink CLOUD 09 door at the end of the corridor. Non-functional by
    /// design in Phase 1: approaching shows a prompt, pressing E only answers
    /// "This is not the exit."
    /// </summary>
    public class GlowingDoor : MonoBehaviour, IInteractable
    {
        [Header("Interaction")]
        [SerializeField] private string promptText = "E — Approach";
        [SerializeField] private string deniedMessage = "This is not the exit.";
        [SerializeField, Tooltip("Seconds the denial message stays on screen.")]
        private float messageDuration = 2.6f;
        [SerializeField, Tooltip("Optional soft sound when the door refuses.")]
        private AudioClip denialClip;

        private float lastInteractTime = -10f;

        public string PromptText => promptText;

        // VR: an XRRayInteractor SelectEntered event should simply call Interact().
        public void Interact()
        {
            // Small debounce so hammering E doesn't restart the message every frame.
            if (Time.unscaledTime - lastInteractTime < 0.75f) return;
            lastInteractTime = Time.unscaledTime;

            if (InteractionPrompt.Instance != null)
            {
                InteractionPrompt.Instance.Flash(deniedMessage, messageDuration);
            }
            else
            {
                Debug.Log($"[DreamCore] {deniedMessage}");
            }

            if (denialClip != null)
            {
                AudioSource.PlayClipAtPoint(denialClip, transform.position, 0.5f);
            }
        }
    }
}
