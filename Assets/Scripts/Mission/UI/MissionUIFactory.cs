// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MissionArchitect
{
    /// <summary>Runtime-only UI construction helpers: no prefabs, no Inspector wiring.</summary>
    public static class MissionUIFactory
    {
        // ---- palette (retro CRT) ---------------------------------------------
        public static readonly Color Background = Hex("#0B1410");
        public static readonly Color PanelFill = Hex("#0F1D16");
        public static readonly Color PanelAlt = Hex("#13251C");
        public static readonly Color Selected = Hex("#1E4230");
        public static readonly Color BorderColor = Hex("#2E5A42");
        public static readonly Color BorderBright = Hex("#4E8A66");
        public static readonly Color Green = Hex("#7DFFB2");
        public static readonly Color GreenDim = Hex("#4E8A66");
        public static readonly Color Amber = Hex("#FFC857");
        public static readonly Color Red = Hex("#FF6B6B");
        public static readonly Color TextMain = Hex("#EAF6FF");
        public static readonly Color TextDim = Hex("#A9C4D6");
        public static readonly Color Black = Hex("#050907");

        // ---- handbook palette ------------------------------------------------
        public static readonly Color Paper = Hex("#E9DCC0");
        public static readonly Color PaperLine = Hex("#D6C6A4");
        public static readonly Color Cover = Hex("#3B2A1E");
        public static readonly Color CoverEdge = Hex("#8A6A45");
        public static readonly Color Ink = Hex("#2B2118");
        public static readonly Color InkDim = Hex("#6B5A45");

        public static Color Hex(string hex)
        {
            Color c;
            return ColorUtility.TryParseHtmlString(hex, out c) ? c : Color.magenta;
        }

        public static Color StatusColor(bool pass, bool warn = false)
        {
            if (pass) return Green;
            return warn ? Amber : Red;
        }

        // ---- font -------------------------------------------------------------

        static TMP_FontAsset _pixelFont;

        public static TMP_FontAsset ResolveFont(TMP_FontAsset preferred)
        {
            // The game font must be Pixelify Sans everywhere, including UI built before any scene
            // loads. Assets/Resources/Fonts keeps a runtime-loadable copy.
            if (_pixelFont == null)
                _pixelFont = Resources.Load<TMP_FontAsset>("Fonts/PixelifySans-Regular");
            if (_pixelFont != null) return _pixelFont;
            if (preferred != null) return preferred;
            if (TMP_Settings.defaultFontAsset != null) return TMP_Settings.defaultFontAsset;
            return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        // ---- canvas / event system -------------------------------------------

        public static Canvas CreateCanvas(string name, int sortingOrder, Transform parent)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            if (parent != null) go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 16f;
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            var existing = EventSystem.current;
            if (existing == null)
            {
                var found = Object.FindFirstObjectByType<EventSystem>();
                if (found != null) existing = found;
            }

            if (existing == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                existing = go.GetComponent<EventSystem>();
            }
            else
            {
                // never keep the legacy StandaloneInputModule
                var legacy = existing.GetComponent<StandaloneInputModule>();
                if (legacy != null) Object.Destroy(legacy);
                if (existing.GetComponent<BaseInputModule>() == null)
                    existing.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        // ---- rect helpers (top-left origin) ----------------------------------

        public static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform Stretch(Transform parent, string name, float left, float top, float right, float bottom)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        // ---- images, panels, text, buttons -----------------------------------

        public static Image AddImage(RectTransform rt, Color color, Sprite sprite = null, bool sliced = false, bool raycast = false)
        {
            var img = rt.GetComponent<Image>();
            if (img == null) img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
                if (sliced) img.pixelsPerUnitMultiplier = 1f;
            }
            return img;
        }

        /// <summary>Panel with a 2 px border drawn as an outer image and an inset fill.</summary>
        public static Image PanelBox(RectTransform parent, string name, float x, float y, float w, float h,
            Color fill, Color border, Sprite sprite = null)
        {
            var rt = Rect(parent, name, x, y, w, h);
            AddImage(rt, border, sprite, true);
            var inner = Stretch(rt, "Fill", 2f, 2f, 2f, 2f);
            AddImage(inner, fill, sprite, true);
            return rt.GetComponent<Image>();
        }

        public static TMP_Text AddText(RectTransform parent, string name, float x, float y, float w, float h,
            string content, float size, Color color, TextAlignmentOptions align, TMP_FontAsset font)
        {
            var rt = Rect(parent, name, x, y, w, h);
            return AddTextTo(rt, content, size, color, align, font);
        }

        public static TMP_Text AddTextTo(RectTransform rt, string content, float size, Color color,
            TextAlignmentOptions align, TMP_FontAsset font)
        {
            var tmp = rt.gameObject.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = ResolveFont(font);
            tmp.text = content;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
            tmp.margin = Vector4.zero;
            tmp.richText = true;
            UITextScale.Register(tmp, size);
            return tmp;
        }

        public static Button AddButton(RectTransform parent, string name, float x, float y, float w, float h,
            string label, float fontSize, TMP_FontAsset font, UnityAction onClick,
            TextAlignmentOptions align = TextAlignmentOptions.MidlineLeft, Sprite sprite = null)
        {
            var rt = Rect(parent, name, x, y, w, h);
            var bg = AddImage(rt, PanelAlt, sprite, sprite != null, true);

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = Hex("#D9FFE8");
            cb.pressedColor = Hex("#8FE6B4");
            cb.selectedColor = Color.white;
            cb.disabledColor = Hex("#39473E");
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.05f;
            btn.colors = cb;
            if (onClick != null) btn.onClick.AddListener(onClick);

            AddText(rt, "Label", 10f, 1f, w - 20f, h - 2f, label, fontSize, TextMain, align, font);
            return btn;
        }

        public static void SetButtonVisual(Button button, Color fill, Color labelColor)
        {
            if (button == null) return;
            var bg = button.targetGraphic as Image;
            if (bg != null) bg.color = fill;
            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = labelColor;
        }

        public static TMP_Text ButtonLabel(Button button)
        {
            return button != null ? button.GetComponentInChildren<TMP_Text>() : null;
        }

        // ---- progress bar -----------------------------------------------------

        public static Image Bar(RectTransform parent, string name, float x, float y, float w, float h)
        {
            var root = Rect(parent, name, x, y, w, h);
            AddImage(root, Hex("#0A1510"));

            var fillRt = Stretch(root, "Fill", 2f, 2f, 2f, 2f);
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.pivot = new Vector2(0f, 0.5f);
            fillRt.offsetMin = new Vector2(2f, 2f);
            fillRt.offsetMax = new Vector2(2f, -2f);
            fillRt.sizeDelta = new Vector2(0f, -4f);
            var img = AddImage(fillRt, Green);
            img.raycastTarget = false;
            return img;
        }

        public static void SetBar(Image fill, float pct, Color color)
        {
            if (fill == null) return;
            var parent = fill.transform.parent as RectTransform;
            float width = parent != null ? parent.rect.width : 100f;
            var rt = fill.rectTransform;
            rt.sizeDelta = new Vector2(Mathf.Max(0f, (width - 4f) * Mathf.Clamp01(pct)), -4f);
            fill.color = color;
        }

        public static void SetActive(Component c, bool on)
        {
            if (c != null && c.gameObject.activeSelf != on) c.gameObject.SetActive(on);
        }

        public static void SetActive(GameObject go, bool on)
        {
            if (go != null && go.activeSelf != on) go.SetActive(on);
        }
    }
}
