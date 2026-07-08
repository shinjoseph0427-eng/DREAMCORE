using UnityEngine;
using DreamCore.Core;

namespace DreamCore.Player
{
    /// <summary>
    /// Slow, deliberate first-person movement on a CharacterController.
    /// Velocity is smoothed so starting/stopping feels cinematic rather than digital.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Speeds")]
        [SerializeField, Tooltip("Default walking speed (m/s).")]
        private float walkSpeed = 3.2f;
        [SerializeField, Tooltip("Slow contemplative walk while holding Shift or Ctrl (m/s).")]
        private float slowWalkSpeed = 1.7f;
        [SerializeField, Tooltip("Seconds to reach target speed. Higher = floatier.")]
        private float accelerationTime = 0.22f;

        [Header("Jumping")]
        [SerializeField, Tooltip("Dreamcore walking sims usually keep this off.")]
        private bool allowJump = false;
        [SerializeField] private float jumpHeight = 0.9f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -14f;

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private Vector3 velocitySmoothing;
        private float verticalVelocity;

        /// <summary>Current horizontal speed in m/s — used by head bob and footsteps.</summary>
        public float CurrentSpeed => horizontalVelocity.magnitude;
        public bool IsGrounded => controller != null && controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            bool canMove = GameManager.Instance == null || GameManager.Instance.IsPlaying;

            // VR: in Phase 2 this input block is replaced by the XRI Continuous Move
            // provider on the XR Origin; this component simply isn't added to the rig.
            Vector2 input = canMove
                ? new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
                : Vector2.zero;
            input = Vector2.ClampMagnitude(input, 1f);

            bool slow = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||
                        Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            float targetSpeed = slow ? slowWalkSpeed : walkSpeed;

            Vector3 targetVelocity = (transform.right * input.x + transform.forward * input.y) * targetSpeed;
            horizontalVelocity = Vector3.SmoothDamp(
                horizontalVelocity, targetVelocity, ref velocitySmoothing, accelerationTime);

            if (controller.isGrounded)
            {
                verticalVelocity = -2f; // keep grounded on steps/slopes
                if (allowJump && canMove && Input.GetButtonDown("Jump"))
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 motion = horizontalVelocity;
            motion.y = verticalVelocity;
            controller.Move(motion * Time.deltaTime);
        }
    }
}
