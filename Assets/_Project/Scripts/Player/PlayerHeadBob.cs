using UnityEngine;

namespace DreamCore.Player
{
    /// <summary>
    /// Very subtle walking head bob applied to a dedicated pivot between the
    /// camera pivot and the camera, so it never fights mouse look.
    /// </summary>
    public class PlayerHeadBob : MonoBehaviour
    {
        [Header("Bob")]
        [SerializeField, Tooltip("Vertical bob amplitude in meters. Keep tiny.")]
        private float verticalAmplitude = 0.030f;
        [SerializeField, Tooltip("Sideways sway amplitude in meters.")]
        private float lateralAmplitude = 0.016f;
        [SerializeField, Tooltip("Steps per second at full walk speed.")]
        private float stepFrequency = 1.7f;
        [SerializeField, Tooltip("How fast the bob fades in/out with movement.")]
        private float blendSpeed = 4f;

        private FirstPersonController controller;
        private float cycle;
        private float weight;

        // VR: head bob must be DISABLED in VR (instant motion sickness).
        // The XR rig simply omits this component.

        private void Awake()
        {
            controller = GetComponentInParent<FirstPersonController>();
        }

        private void Update()
        {
            if (controller == null) return;

            float speed01 = Mathf.Clamp01(controller.CurrentSpeed / 3.2f);
            bool moving = controller.CurrentSpeed > 0.25f && controller.IsGrounded;
            weight = Mathf.MoveTowards(weight, moving ? speed01 : 0f, blendSpeed * Time.deltaTime);

            cycle += Time.deltaTime * stepFrequency * Mathf.Max(speed01, 0.2f) * Mathf.PI * 2f;

            float y = Mathf.Sin(cycle * 2f) * verticalAmplitude; // two bounces per stride
            float x = Mathf.Sin(cycle) * lateralAmplitude;
            transform.localPosition = new Vector3(x * weight, y * weight, 0f);
        }
    }
}
