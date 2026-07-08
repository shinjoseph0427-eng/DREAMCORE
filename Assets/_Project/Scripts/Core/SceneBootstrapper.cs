using UnityEngine;
using UnityEngine.SceneManagement;
using DreamCore.Player;
using DreamCore.Rendering;
using DreamCore.UI;
using DreamCore.World;

namespace DreamCore.Core
{
    /// <summary>
    /// Guarantees the scene contains everything DREAM CORE needs, creating any
    /// missing piece on demand. The editor setup uses it to assemble Main.unity
    /// at edit time; at runtime it fills in the runtime-only parts (UI, audio)
    /// and acts as a safety net if the scene is empty.
    /// </summary>
    public class SceneBootstrapper : MonoBehaviour
    {
        [SerializeField, Tooltip("Log each piece the bootstrapper creates.")]
        private bool verbose = false;

        private bool ensured;

        private void Awake()
        {
            if (Application.isPlaying && !ensured)
            {
                EnsureAll(true);
            }
        }

        /// <summary>
        /// Runtime safety net: pressing Play in an empty (or freshly created) scene
        /// still produces a playable game.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RuntimeBootstrap()
        {
            if (FindFirstObjectByType<SceneBootstrapper>() != null) return;

            string sceneName = SceneManager.GetActiveScene().name;
            bool known = sceneName == "Main" || sceneName == "LightingTest" ||
                         string.IsNullOrEmpty(sceneName) || sceneName == "Untitled" ||
                         sceneName == "SampleScene";
            if (!known) return;

            var go = new GameObject("SceneBootstrapper");
            go.AddComponent<SceneBootstrapper>(); // Awake -> EnsureAll(true)
        }

        /// <summary>
        /// Create every missing scene piece. With <paramref name="runtime"/> false
        /// (edit mode), UI/audio/game-state objects are skipped — they are cheap
        /// and self-construct on Play.
        /// </summary>
        public void EnsureAll(bool runtime)
        {
            ensured = true;

            EnsureWorld();
            EnsurePostProcessing();
            EnsurePlayer();

            if (runtime)
            {
                EnsureGameManager();
                EnsureUI();
                EnsureAudio();
            }
        }

        private void EnsureWorld()
        {
            var world = FindFirstObjectByType<DreamcoreWorldBuilder>();
            if (world == null)
            {
                var go = new GameObject("DreamcoreWorld");
                world = go.AddComponent<DreamcoreWorldBuilder>();
                Log("Created DreamcoreWorld");
            }
            if (world.transform.childCount == 0)
            {
                world.Build();
                Log("Built dreamcore corridor");
            }
        }

        private void EnsurePostProcessing()
        {
            VHSPostProcessController.EnsureSceneVolume();
        }

        private void EnsurePlayer()
        {
            if (FindFirstObjectByType<FirstPersonController>() != null) return;

            var world = FindFirstObjectByType<DreamcoreWorldBuilder>();
            Vector3 spawn = world != null ? world.PlayerSpawnPosition : new Vector3(0f, 0.6f, 1.2f);

            // --- PC first-person rig ---------------------------------------------------
            // VR: in Phase 2 this whole rig is replaced by an XR Origin:
            //   XR Origin (XRRig)
            //     Camera Offset
            //       Main Camera (TrackedPoseDriver)
            //       LeftHand / RightHand Controller (XRRayInteractor for door interaction)
            // Locomotion: Continuous Move (slow) + Teleportation + 45° Snap Turn,
            // with a comfort tunneling vignette during motion.
            // The world/interaction/game-state layers below this rig stay untouched.
            var player = new GameObject("Player");
            player.transform.SetPositionAndRotation(spawn, Quaternion.identity);

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.stepOffset = 0.4f;
            cc.slopeLimit = 50f;

            player.AddComponent<FirstPersonController>();
            player.AddComponent<PlayerFootsteps>();

            var pivot = new GameObject("CameraPivot");
            pivot.transform.SetParent(player.transform, false);
            pivot.transform.localPosition = new Vector3(0f, 1.65f, 0f);

            var bob = new GameObject("BobPivot");
            bob.transform.SetParent(pivot.transform, false);
            bob.AddComponent<PlayerHeadBob>();

            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(bob.transform, false);
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 62f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 80f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = RenderSettings.fogColor;
            cam.allowHDR = true;
            camGo.AddComponent<AudioListener>();
            VHSPostProcessController.ConfigureCamera(cam);

            var look = player.AddComponent<MouseLook>();
            look.SetRig(player.transform, pivot.transform);

            var interaction = camGo.AddComponent<PlayerInteraction>();
            interaction.SetCamera(cam);

            // Remove any stray cameras/listeners the scene may contain.
            foreach (var other in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (other != cam && other.GetComponentInParent<FirstPersonController>() == null)
                {
                    Log($"Disabling extra camera '{other.name}'");
                    other.gameObject.SetActive(false);
                }
            }

            Log("Created Player rig");
        }

        private void EnsureGameManager()
        {
            if (FindFirstObjectByType<GameManager>() == null)
            {
                new GameObject("GameManager").AddComponent<GameManager>();
                Log("Created GameManager");
            }
        }

        private void EnsureUI()
        {
            if (FindFirstObjectByType<StartScreen>(FindObjectsInactive.Include) == null)
            {
                UiBuilder.BuildGameUI();
                Log("Created UI canvas");
            }
        }

        private void EnsureAudio()
        {
            if (FindFirstObjectByType<DreamCore.Audio.AmbientAudioController>() == null)
            {
                var audio = new GameObject("Audio");
                audio.AddComponent<DreamCore.Audio.AmbientAudioController>();
                Log("Created Audio root");
            }
        }

        private void Log(string msg)
        {
            if (verbose) Debug.Log($"[DreamCore] {msg}");
        }
    }
}
