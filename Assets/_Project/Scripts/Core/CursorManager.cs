using UnityEngine;

namespace DreamCore.Core
{
    /// <summary>
    /// Tiny static wrapper around cursor lock state so the rest of the code
    /// never touches UnityEngine.Cursor directly.
    /// </summary>
    public static class CursorManager
    {
        public static bool IsLocked => Cursor.lockState == CursorLockMode.Locked;

        public static void Lock()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public static void Unlock()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // VR: cursor lock is meaningless on an HMD. In Phase 2 these become no-ops
        // when an XR display is active (XRSettings.isDeviceActive).
    }
}
