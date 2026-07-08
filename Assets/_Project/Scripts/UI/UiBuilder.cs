using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DreamCore.UI
{
    /// <summary>
    /// Programmatic uGUI construction so the prototype needs no UI prefabs or
    /// font assets. Builds the whole game UI (start screen, pause menu,
    /// interaction prompt) into one canvas.
    /// </summary>
    public static class UiBuilder
    {
        private static Font cachedFont;

        public static Font GetBuiltinFont()
        {
            if (cachedFont != null) return cachedFont;
            try { cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { /* older Unity */ }
            if (cachedFont == null)
            {
                try { cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch { /* give up gracefully */ }
            }
            return cachedFont;
        }

        public static GameObject BuildGameUI()
        {
            EnsureEventSystem();

            var canvasGo = new GameObject("UI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Order matters: prompt below the menus.
            var promptGo = new GameObject("InteractionPrompt");
            promptGo.transform.SetParent(canvasGo.transform, false);
            promptGo.AddComponent<InteractionPrompt>();

            var pauseGo = new GameObject("PauseMenu");
            pauseGo.transform.SetParent(canvasGo.transform, false);
            pauseGo.AddComponent<PauseMenu>();

            var startGo = new GameObject("StartScreen");
            startGo.transform.SetParent(canvasGo.transform, false);
            startGo.AddComponent<StartScreen>();

            // VR: in Phase 2 menus move to a world-space canvas on the XR rig
            // (or a simple gaze/laser UI); this overlay canvas is PC-only.
            return canvasGo;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ------------------------------------------------------------------ widgets

        public static RectTransform Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Stretch(go);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Text Label(Transform parent, string name, string content, int size,
            Color color, Vector2 anchoredPos, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1400, size * 1.6f);
            rt.anchoredPosition = anchoredPos;

            var text = go.AddComponent<Text>();
            text.font = GetBuiltinFont();
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return text;
        }

        public static Button TextButton(Transform parent, string name, string label,
            Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.07f);

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.85f, 0.92f, 0.35f);
            colors.pressedColor = new Color(1f, 0.7f, 0.85f, 0.5f);
            colors.fadeDuration = 0.12f;
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(onClick);

            var text = Label(go.transform, "Label", label, 30, new Color(0.96f, 0.94f, 0.94f), Vector2.zero);
            Stretch(text.gameObject);
            text.raycastTarget = false;
            return button;
        }
    }
}
