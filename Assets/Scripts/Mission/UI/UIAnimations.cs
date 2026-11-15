// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MissionArchitect
{
    /// <summary>
    /// One reusable animation helper for every Mission Architect UI. Always uses unscaled
    /// time, stepped frames for a pixel-art feel, and is re-entrancy safe: each routine sets
    /// exact start/end transforms so an interrupted animation can never leave a panel scaled
    /// or offset.
    /// </summary>
    public static class UIAnimations
    {
        public const float TerminalOpenSeconds = 0.35f;
        public const float TerminalCloseSeconds = 0.20f;
        public const float HandbookOpenSeconds = 0.35f;
        public const float HandbookCloseSeconds = 0.20f;
        public const float PageFlipSeconds = 0.15f;

        // ---- runner -----------------------------------------------------------

        sealed class Runner : MonoBehaviour
        {
            static Runner _instance;

            public static Runner Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        var go = new GameObject("MissionArchitect.UIAnimations");
                        go.hideFlags = HideFlags.HideAndDontSave;
                        _instance = go.AddComponent<Runner>();
                        UnityEngine.Object.DontDestroyOnLoad(go);
                    }
                    return _instance;
                }
            }
        }

        public static Coroutine Run(IEnumerator routine)
        {
            return Runner.Instance.StartCoroutine(routine);
        }

        public static void Stop(ref Coroutine routine)
        {
            if (routine == null) return;
            if (Runner.Instance != null) Runner.Instance.StopCoroutine(routine);
            routine = null;
        }

        public static void ResetScale(RectTransform target)
        {
            if (target != null) target.localScale = Vector3.one;
        }

        /// <summary>Hard reset used before every open/close so interrupts cannot leak state.</summary>
        public static void ResetTransform(RectTransform target, Vector2 basePosition, CanvasGroup group = null, float alpha = 1f)
        {
            if (target == null) return;
            target.localScale = Vector3.one;
            target.anchoredPosition = basePosition;
            if (group != null) group.alpha = alpha;
        }

        static float Steps(float t, int steps)
        {
            if (steps <= 0) return t;
            return Mathf.Floor(t * steps) / steps;
        }

        static IEnumerator WaitUnscaled(float seconds)
        {
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // ---- terminal CRT -----------------------------------------------------

        /// <summary>Thin line -> horizontal expansion -> vertical expansion -> typed boot text.</summary>
        public static Coroutine CRTOn(RectTransform panel, CanvasGroup group, TMP_Text boot,
            string bootString, Action onDone)
        {
            return Run(CRTOnRoutine(panel, group, boot, bootString, onDone));
        }

        static IEnumerator CRTOnRoutine(RectTransform panel, CanvasGroup group, TMP_Text boot,
            string bootString, Action onDone)
        {
            if (boot != null)
            {
                boot.text = string.Empty;
                boot.transform.SetAsLastSibling();
                boot.gameObject.SetActive(true);
            }

            if (panel != null)
            {
                panel.localScale = new Vector3(0.02f, 0.02f, 1f);
                panel.anchoredPosition = Vector2.zero;
            }
            if (group != null) group.alpha = 1f;

            // horizontal line, 3 stepped frames
            float t = 0f;
            const float hTime = 0.10f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / hTime;
                float k = Steps(Mathf.Clamp01(t), 3);
                if (panel != null)
                    panel.localScale = new Vector3(Mathf.Lerp(0.02f, 1f, k), 0.02f, 1f);
                yield return null;
            }
            if (panel != null) panel.localScale = new Vector3(1f, 0.02f, 1f);

            // vertical opening, 5 stepped frames
            t = 0f;
            const float vTime = 0.25f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / vTime;
                float k = Steps(Mathf.Clamp01(t), 5);
                if (panel != null)
                    panel.localScale = new Vector3(1f, Mathf.Lerp(0.02f, 1f, k), 1f);
                yield return null;
            }
            if (panel != null) panel.localScale = Vector3.one;

            if (boot != null && !string.IsNullOrEmpty(bootString))
            {
                yield return TypeTextRoutine(boot, bootString, 0.40f, true, null);
                boot.text = string.Empty;
                boot.gameObject.SetActive(false);
            }

            if (onDone != null) onDone();
        }

        /// <summary>Reverse CRT: vertical collapse then horizontal collapse.</summary>
        public static Coroutine CRTOff(RectTransform panel, CanvasGroup group, Action onDone)
        {
            return Run(CRTOffRoutine(panel, group, onDone));
        }

        static IEnumerator CRTOffRoutine(RectTransform panel, CanvasGroup group, Action onDone)
        {
            float t = 0f;
            const float vTime = 0.12f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / vTime;
                float k = Steps(Mathf.Clamp01(t), 4);
                if (panel != null)
                    panel.localScale = new Vector3(1f, Mathf.Lerp(1f, 0.02f, k), 1f);
                yield return null;
            }

            t = 0f;
            const float hTime = 0.08f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / hTime;
                float k = Steps(Mathf.Clamp01(t), 3);
                if (panel != null)
                    panel.localScale = new Vector3(Mathf.Lerp(1f, 0.02f, k), 0.02f, 1f);
                if (group != null) group.alpha = 1f - k;
                yield return null;
            }

            if (panel != null)
            {
                panel.localScale = Vector3.one;
                panel.anchoredPosition = Vector2.zero;
            }
            if (group != null) group.alpha = 0f;

            if (onDone != null) onDone();
        }

        // ---- handbook ---------------------------------------------------------

        /// <summary>Slides up from the bottom while the cover opens (scaleX 0.1 -> 1).</summary>
        public static Coroutine HandbookOpen(RectTransform book, Vector2 basePosition, CanvasGroup group, Action onDone)
        {
            return Run(HandbookOpenRoutine(book, basePosition, group, onDone));
        }

        static IEnumerator HandbookOpenRoutine(RectTransform book, Vector2 basePosition, CanvasGroup group, Action onDone)
        {
            Vector2 start = basePosition + new Vector2(0f, -200f);
            if (book != null)
            {
                book.anchoredPosition = start;
                book.localScale = new Vector3(0.1f, 1f, 1f);
            }
            if (group != null) group.alpha = 1f;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / HandbookOpenSeconds;
                float k = Steps(Mathf.Clamp01(t), 6);
                if (book != null)
                {
                    book.anchoredPosition = Vector2.Lerp(start, basePosition, k);
                    book.localScale = new Vector3(Mathf.Lerp(0.1f, 1f, k), 1f, 1f);
                }
                yield return null;
            }

            if (book != null)
            {
                book.anchoredPosition = basePosition;
                book.localScale = Vector3.one;
            }
            if (onDone != null) onDone();
        }

        public static Coroutine HandbookClose(RectTransform book, Vector2 basePosition, CanvasGroup group, Action onDone)
        {
            return Run(HandbookCloseRoutine(book, basePosition, group, onDone));
        }

        static IEnumerator HandbookCloseRoutine(RectTransform book, Vector2 basePosition, CanvasGroup group, Action onDone)
        {
            Vector2 end = basePosition + new Vector2(0f, -200f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / HandbookCloseSeconds;
                float k = Steps(Mathf.Clamp01(t), 4);
                if (book != null)
                {
                    book.anchoredPosition = Vector2.Lerp(basePosition, end, k);
                    book.localScale = new Vector3(Mathf.Lerp(1f, 0.1f, k), 1f, 1f);
                }
                if (group != null) group.alpha = 1f - k;
                yield return null;
            }

            if (book != null)
            {
                book.anchoredPosition = basePosition;
                book.localScale = Vector3.one;
            }
            if (group != null) group.alpha = 0f;
            if (onDone != null) onDone();
        }

        /// <summary>Quick page flip: X scale 1 -> 0 -> 1 while content is swapped at the midpoint.</summary>
        public static Coroutine PageFlip(RectTransform left, RectTransform right, Action swapContent, Action onDone)
        {
            return Run(PageFlipRoutine(left, right, swapContent, onDone));
        }

        static IEnumerator PageFlipRoutine(RectTransform left, RectTransform right, Action swapContent, Action onDone)
        {
            float half = PageFlipSeconds * 0.5f;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / half;
                float k = Steps(Mathf.Clamp01(t), 3);
                float sx = Mathf.Lerp(1f, 0.05f, k);
                if (left != null) left.localScale = new Vector3(sx, 1f, 1f);
                if (right != null) right.localScale = new Vector3(sx, 1f, 1f);
                yield return null;
            }

            if (swapContent != null) swapContent();

            t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / half;
                float k = Steps(Mathf.Clamp01(t), 3);
                float sx = Mathf.Lerp(0.05f, 1f, k);
                if (left != null) left.localScale = new Vector3(sx, 1f, 1f);
                if (right != null) right.localScale = new Vector3(sx, 1f, 1f);
                yield return null;
            }

            if (left != null) left.localScale = Vector3.one;
            if (right != null) right.localScale = Vector3.one;
            if (onDone != null) onDone();
        }

        // ---- boot text --------------------------------------------------------

        public static Coroutine TypeText(TMP_Text text, string content, float seconds, Action onDone)
        {
            return Run(TypeTextRoutine(text, content, seconds, true, onDone));
        }

        static IEnumerator TypeTextRoutine(TMP_Text text, string content, float seconds, bool skippable, Action onDone)
        {
            if (text == null)
            {
                if (onDone != null) onDone();
                yield break;
            }

            content = content ?? string.Empty;
            text.text = string.Empty;
            float t = 0f;
            int shown = -1;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.001f, seconds);

                bool skip = skippable && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
                if (skip) t = 1f;

                int count = Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(t) * content.Length + 0.5f), 0, content.Length);
                if (count != shown)
                {
                    shown = count;
                    text.text = content.Substring(0, count);
                }
                yield return null;
            }

            text.text = content;
            if (onDone != null) onDone();
        }
    }
}
