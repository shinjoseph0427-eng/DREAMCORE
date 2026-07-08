using UnityEngine;
using DreamCore.Core;

namespace DreamCore.UI
{
    /// <summary>
    /// ESC pause overlay: Resume / Quit. Quit stops play mode in the editor
    /// and quits the application in a build.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        private bool built;

        private void EnsureBuilt()
        {
            if (built) return;
            built = true;

            UiBuilder.Stretch(gameObject);
            UiBuilder.Panel(transform, "Dim", new Color(0.01f, 0.02f, 0.03f, 0.62f));

            UiBuilder.Label(transform, "Title", "PAUSED", 54,
                new Color(0.92f, 0.95f, 0.96f), new Vector2(0, 120), FontStyle.Bold);

            UiBuilder.TextButton(transform, "ResumeButton", "Resume",
                new Vector2(0, 10), new Vector2(320, 62),
                () => { if (GameManager.Instance != null) GameManager.Instance.Resume(); });

            UiBuilder.TextButton(transform, "QuitButton", "Quit",
                new Vector2(0, -75), new Vector2(320, 62),
                () => { if (GameManager.Instance != null) GameManager.Instance.QuitGame(); });

            UiBuilder.Label(transform, "Controls",
                "WASD — move      Mouse — look      Shift — slow walk      E — interact",
                20, new Color(0.62f, 0.72f, 0.74f), new Vector2(0, -190));
        }

        public void Show()
        {
            EnsureBuilt();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
