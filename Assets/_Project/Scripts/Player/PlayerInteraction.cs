using UnityEngine;
using DreamCore.Core;
using DreamCore.UI;

namespace DreamCore.Player
{
    /// <summary>
    /// Anything the player can use implements this. Kept deliberately minimal so
    /// an XRI Ray Interactor can call <see cref="Interact"/> in Phase 2 as well.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Prompt shown when the player looks at the object, e.g. "E — Approach".</summary>
        string PromptText { get; }
        void Interact();
    }

    /// <summary>
    /// Camera raycast interaction: shows a prompt when aiming at an
    /// <see cref="IInteractable"/> within range, triggers it on E.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField, Tooltip("Maximum interaction distance in meters.")]
        private float range = 2.8f;
        [SerializeField] private LayerMask mask = ~0;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private Camera cam;
        private IInteractable current;

        public void SetCamera(Camera camera) => cam = camera;

        private void Start()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
        }

        private void Update()
        {
            if (cam == null) return;
            bool playing = GameManager.Instance == null || GameManager.Instance.IsPlaying;
            if (!playing)
            {
                SetCurrent(null);
                return;
            }

            // VR: in Phase 2 this becomes an XRRayInteractor hover/select on the same
            // IInteractable components — only the input source changes.
            IInteractable hit = null;
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit info, range, mask, QueryTriggerInteraction.Ignore))
            {
                hit = info.collider.GetComponentInParent<IInteractable>();
            }
            SetCurrent(hit);

            if (current != null && Input.GetKeyDown(interactKey))
            {
                current.Interact();
            }
        }

        private void SetCurrent(IInteractable next)
        {
            if (ReferenceEquals(current, next)) return;
            current = next;

            var prompt = InteractionPrompt.Instance;
            if (prompt == null) return;
            if (current != null) prompt.ShowPrompt(current.PromptText);
            else prompt.HidePrompt();
        }
    }
}
