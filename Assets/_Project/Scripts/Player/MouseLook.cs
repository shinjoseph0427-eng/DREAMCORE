using UnityEngine;
using DreamCore.Core;

namespace DreamCore.Player
{
    /// <summary>
    /// Smoothed mouse look. Yaw rotates the player body, pitch rotates the
    /// camera pivot. Kept separate from movement so a future XR rig can drop it.
    /// </summary>
    public class MouseLook : MonoBehaviour
    {
        [Header("Sensitivity")]
        [SerializeField, Tooltip("Degrees per mouse unit.")]
        private float sensitivity = 2.2f;
        [SerializeField, Range(0f, 0.2f), Tooltip("Seconds of look smoothing. 0 = raw.")]
        private float smoothingTime = 0.045f;

        [Header("Limits")]
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        [Header("Rig (auto-assigned by SceneBootstrapper)")]
        [SerializeField, Tooltip("Transform receiving yaw (the player body).")]
        private Transform body;
        [SerializeField, Tooltip("Transform receiving pitch (the camera pivot).")]
        private Transform pitchPivot;

        private float yaw;
        private float pitch;
        private float smoothYaw;
        private float smoothPitch;
        private float yawVelocity;
        private float pitchVelocity;

        public void SetRig(Transform bodyTransform, Transform pivotTransform)
        {
            body = bodyTransform;
            pitchPivot = pivotTransform;
            yaw = smoothYaw = body != null ? body.eulerAngles.y : 0f;
        }

        private void Start()
        {
            if (body == null) body = transform;
            yaw = smoothYaw = body.eulerAngles.y;
        }

        private void Update()
        {
            // VR: in Phase 2 head orientation comes from the HMD's TrackedPoseDriver
            // and (optionally) XRI Snap Turn — this component is not used at all.
            bool canLook = (GameManager.Instance == null || GameManager.Instance.IsPlaying)
                           && CursorManager.IsLocked;
            if (canLook)
            {
                yaw += Input.GetAxisRaw("Mouse X") * sensitivity;
                pitch -= Input.GetAxisRaw("Mouse Y") * sensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            if (smoothingTime > 0f)
            {
                smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawVelocity, smoothingTime);
                smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchVelocity, smoothingTime);
            }
            else
            {
                smoothYaw = yaw;
                smoothPitch = pitch;
            }

            if (body != null) body.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
            if (pitchPivot != null) pitchPivot.localRotation = Quaternion.Euler(smoothPitch, 0f, 0f);
        }
    }
}
