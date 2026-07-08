using UnityEngine;
using DreamCore.UI;

namespace DreamCore.Core
{
    /// <summary>
    /// Central game-state machine: StartScreen -> Playing <-> Paused.
    /// Owns cursor lock state and time scale. Everything else reads
    /// <see cref="IsPlaying"/> instead of tracking input focus itself.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState { StartScreen, Playing, Paused }

        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField, Tooltip("Current state (read-only at runtime).")]
        private GameState state = GameState.StartScreen;

        [Header("Options")]
        [SerializeField, Tooltip("Skip the start screen (useful while iterating in the editor).")]
        private bool skipStartScreen = false;

        public GameState State => state;

        /// <summary>True while the player should be able to move and look around.</summary>
        public bool IsPlaying => state == GameState.Playing;

        private StartScreen startScreen;
        private PauseMenu pauseMenu;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            startScreen = FindFirstObjectByType<StartScreen>(FindObjectsInactive.Include);
            pauseMenu = FindFirstObjectByType<PauseMenu>(FindObjectsInactive.Include);

            if (skipStartScreen || startScreen == null)
            {
                StartGame();
            }
            else
            {
                state = GameState.StartScreen;
                CursorManager.Unlock();
                startScreen.Show();
                if (pauseMenu != null) pauseMenu.Hide();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (state == GameState.Playing) Pause();
                else if (state == GameState.Paused) Resume();
            }

            // Clicking back into the game re-locks the cursor if it was freed by the OS.
            if (state == GameState.Playing && !CursorManager.IsLocked && Input.GetMouseButtonDown(0))
            {
                CursorManager.Lock();
            }
        }

        public void StartGame()
        {
            state = GameState.Playing;
            Time.timeScale = 1f;
            CursorManager.Lock();
            if (startScreen != null) startScreen.HideAnimated();
            if (pauseMenu != null) pauseMenu.Hide();
        }

        public void Pause()
        {
            if (state != GameState.Playing) return;
            state = GameState.Paused;
            Time.timeScale = 0f;
            CursorManager.Unlock();
            if (pauseMenu != null) pauseMenu.Show();
        }

        public void Resume()
        {
            if (state != GameState.Paused) return;
            state = GameState.Playing;
            Time.timeScale = 1f;
            CursorManager.Lock();
            if (pauseMenu != null) pauseMenu.Hide();
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // VR: in Phase 2 the start/pause flow moves to a world-space canvas in front of the
        // XR camera, and cursor locking becomes a no-op. Keep all state changes routed
        // through this class so the XR layer only swaps the presentation.
    }
}
