using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DreamCore.UI
{
    /// <summary>
    /// Small lower-center prompt ("E — Approach") that only appears near
    /// interactive objects, plus a Flash() for one-shot messages
    /// ("This is not the exit."). Never permanently visible.
    /// </summary>
    public class InteractionPrompt : MonoBehaviour
    {
        public static InteractionPrompt Instance { get; private set; }

        private Text label;
        private bool promptVisible;
        private string promptText = "";
        private Coroutine flashRoutine;

        private void Awake()
        {
            Instance = this;

            var rt = gameObject.GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 150f);
            rt.sizeDelta = new Vector2(900f, 44f);

            label = UiBuilder.Label(transform, "Text", "", 26, new Color(0.97f, 0.95f, 0.94f), Vector2.zero);
            UiBuilder.Stretch(label.gameObject);
            label.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void ShowPrompt(string text)
        {
            promptVisible = true;
            promptText = text;
            if (flashRoutine == null) Apply(text, true);
        }

        public void HidePrompt()
        {
            promptVisible = false;
            if (flashRoutine == null) Apply("", false);
        }

        /// <summary>Show a one-shot message for a few seconds, then restore the prompt state.</summary>
        public void Flash(string message, float duration)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(message, duration));
        }

        private IEnumerator FlashRoutine(string message, float duration)
        {
            Apply(message, true);
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            flashRoutine = null;
            Apply(promptText, promptVisible);
        }

        private void Apply(string text, bool visible)
        {
            if (label == null) return;
            label.text = text;
            label.gameObject.SetActive(visible && !string.IsNullOrEmpty(text));
        }
    }
}
