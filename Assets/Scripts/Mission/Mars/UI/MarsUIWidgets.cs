// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MissionArchitect
{
    /// <summary>Runtime widget helpers used by the Mars terminal (sliders, bars).</summary>
    public static class MarsUIWidgets
    {
        public static Slider AddSlider(RectTransform parent, string name, float x, float y, float w, float h,
            float min, float max, bool wholeNumbers, UnityAction<float> onChanged)
        {
            var rt = MissionUIFactory.Rect(parent, name, x, y, w, h);
            var hit = MissionUIFactory.Stretch(rt, "Hit", 0, 0, 0, 0);
            MissionUIFactory.AddImage(hit, new Color(0f, 0f, 0f, 0f), null, false, true);
            hit.SetAsFirstSibling();

            var slider = rt.gameObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.transition = Selectable.Transition.None;

            var background = MissionUIFactory.Rect(rt, "Background", 0, h * 0.5f - 5f, w, 10f);
            MissionUIFactory.AddImage(background, MissionUIFactory.PanelAlt);

            var fillArea = MissionUIFactory.Rect(rt, "Fill Area", 10f, 4f, w - 20f, h - 8f);
            var fill = new GameObject("Fill", typeof(RectTransform)).GetComponent<RectTransform>();
            fill.SetParent(fillArea, false);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImage = MissionUIFactory.AddImage(fill, MissionUIFactory.Selected);

            var handleArea = MissionUIFactory.Rect(rt, "Handle Slide Area", 10f, 0f, w - 20f, h);
            var handle = new GameObject("Handle", typeof(RectTransform)).GetComponent<RectTransform>();
            handle.SetParent(handleArea, false);
            handle.anchorMin = new Vector2(0.5f, 0f);
            handle.anchorMax = new Vector2(0.5f, 1f);
            handle.sizeDelta = new Vector2(18f, 0f);
            handle.anchoredPosition = Vector2.zero;
            var handleImage = MissionUIFactory.AddImage(handle, MissionUIFactory.Green);
            handleImage.raycastTarget = true;

            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            if (onChanged != null) slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        public static void SetSlider(Slider slider, float value)
        {
            if (slider != null) slider.SetValueWithoutNotify(value);
        }

        /// <summary>Segmented horizontal bar (used for the mini day).</summary>
        public static RectTransform[] AddSegmentedBar(RectTransform parent, string name, float x, float y,
            float w, float h, int segments, Color[] colors)
        {
            var root = MissionUIFactory.Rect(parent, name, x, y, w, h);
            MissionUIFactory.AddImage(root, MissionUIFactory.Black);
            var result = new RectTransform[segments];
            for (int i = 0; i < segments; i++)
            {
                var seg = MissionUIFactory.Rect(root, "Seg" + i, 0, 0, 1, h);
                seg.anchorMin = new Vector2(0f, 0f);
                seg.anchorMax = new Vector2(0f, 1f);
                seg.pivot = new Vector2(0f, 0.5f);
                seg.anchoredPosition = Vector2.zero;
                seg.sizeDelta = new Vector2(0f, 0f);
                MissionUIFactory.AddImage(seg, colors[i % colors.Length]);
                result[i] = seg;
            }
            return result;
        }

        public static void SetSegments(RectTransform[] segments, float[] weights, float totalWidth)
        {
            if (segments == null || weights == null) return;
            float cumulative = 0f;
            for (int i = 0; i < segments.Length && i < weights.Length; i++)
            {
                if (segments[i] == null) continue;
                float w = Mathf.Max(0f, weights[i]) * totalWidth;
                segments[i].sizeDelta = new Vector2(w, 0f);
                segments[i].anchoredPosition = new Vector2(cumulative, 0f);
                cumulative += w;
            }
        }
    }
}
