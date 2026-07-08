using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DreamCore.Core;

namespace DreamCore.UI
{
    /// <summary>
    /// Minimal title screen: DREAM POOL over black, click anywhere to start.
    /// Builds its own widgets on first Show().
    /// </summary>
    public class StartScreen : MonoBehaviour
    {
        [SerializeField] private string title = "DREAM POOL";
        [SerializeField] private string subtitle = "A dreamcore exploration prototype";
        [SerializeField] private string hint = "Click to Start";
        [SerializeField] private float fadeDuration = 1.1f;

        private CanvasGroup group;
        private Text hintText;
        private bool built;

        private void EnsureBuilt()
        {
            if (built) return;
            built = true;

            UiBuilder.Stretch(gameObject);
            group = gameObject.AddComponent<CanvasGroup>();

            UiBuilder.Panel(transform, "Backdrop", new Color(0.02f, 0.02f, 0.03f, 0.97f));

            UiBuilder.Label(transform, "Title", title, 92,
                new Color(0.93f, 0.96f, 0.97f), new Vector2(0, 90), FontStyle.Bold);
            UiBuilder.Label(transform, "Subtitle", subtitle, 26,
                new Color(0.66f, 0.82f, 0.82f), new Vector2(0, 10));
            hintText = UiBuilder.Label(transform, "Hint", hint, 24,
                new Color(1.00f, 0.78f, 0.88f), new Vector2(0, -150));

            // Fullscreen invisible button: click anywhere to start.
            var clickGo = new GameObject("ClickCatcher");
            clickGo.transform.SetParent(transform, false);
            UiBuilder.Stretch(clickGo);
            var img = clickGo.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);
            var button = clickGo.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null) GameManager.Instance.StartGame();
            });
        }

        private void Update()
        {
            if (hintText != null && gameObject.activeInHierarchy)
            {
                var c = hintText.color;
                c.a = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 2.2f);
                hintText.color = c;
            }
        }

        public void Show()
        {
            EnsureBuilt();
            gameObject.SetActive(true);
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        public void HideAnimated()
        {
            if (!built || !gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                return;
            }
            group.blocksRaycasts = false;
            group.interactable = false;
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = 1f - Mathf.SmoothStep(0f, 1f, t / fadeDuration);
                yield return null;
            }
            gameObject.SetActive(false);
        }
    }
}
