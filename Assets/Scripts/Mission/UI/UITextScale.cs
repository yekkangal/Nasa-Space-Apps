// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MissionArchitect
{
    /// <summary>
    /// Central readable-text scale. Every runtime TMP text registers here; F2 cycles
    /// Small / Normal / Large and the choice is stored in PlayerPrefs.
    /// Base sizes are tuned for the 1920 x 1080 reference canvas and rounded to multiples of 4.
    /// </summary>
    public static class UITextScale
    {
        public const int Small = 0;
        public const int Normal = 1;
        public const int Large = 2;

        public const string PrefsKey = "MA_TextSize";

        static readonly List<TMP_Text> _texts = new List<TMP_Text>();
        static readonly List<float> _requested = new List<float>();
        static int _setting = -1;
        static TextSizeHotkey _hotkey;

        public static int Setting
        {
            get
            {
                if (_setting < 0)
                    _setting = Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, Large), Small, Large);
                return _setting;
            }
        }

        public static string SettingName
        {
            get
            {
                return Setting == Small ? "Small" : (Setting == Normal ? "Normal" : "Large");
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void CreateHotkey()
        {
            if (_hotkey != null) return;
            var go = new GameObject("TextSizeHotkey");
            Object.DontDestroyOnLoad(go);
            _hotkey = go.AddComponent<TextSizeHotkey>();
        }

        /// <summary>Registers a runtime text and immediately applies the readable size.</summary>
        public static void Register(TMP_Text text, float requestedSize)
        {
            if (text == null) return;
            if (!_texts.Contains(text))
            {
                _texts.Add(text);
                _requested.Add(requestedSize);
            }
            Apply(text, requestedSize);
        }

        public static void Cycle()
        {
            _setting = (Setting + 1) % 3;
            PlayerPrefs.SetInt(PrefsKey, _setting);
            PlayerPrefs.Save();
            RefreshAll();
        }

        public static void SetSetting(int setting)
        {
            _setting = Mathf.Clamp(setting, Small, Large);
            PlayerPrefs.SetInt(PrefsKey, _setting);
            PlayerPrefs.Save();
            RefreshAll();
        }

        public static void RefreshAll()
        {
            for (int i = _texts.Count - 1; i >= 0; i--)
            {
                if (_texts[i] == null)
                {
                    _texts.RemoveAt(i);
                    _requested.RemoveAt(i);
                    continue;
                }
                Apply(_texts[i], _requested[i]);
            }
        }

        static void Apply(TMP_Text text, float requested)
        {
            int baseSize = MapSize(requested);
            int scaled = ScaleForSetting(baseSize);
            text.enableAutoSizing = true;
            text.fontSizeMax = scaled;
            // Safety floor: if a box really cannot hold the larger text it shrinks, but never
            // below 75% of the requested readable size.
            text.fontSizeMin = Mathf.Max(20f, Round4(scaled * 0.75f));
            text.fontSize = scaled;
            text.lineSpacing = 10f;              // about +10% leading for the 90 pt atlas
            text.SetAllDirty();
        }

        /// <summary>Maps the old hand-picked sizes to readable ones (multiples of 4).</summary>
        static int MapSize(float requested)
        {
            if (requested <= 10.5f) return 26;   // log / small captions
            if (requested <= 12.5f) return 28;   // tables
            if (requested <= 15.5f) return 30;   // body
            if (requested <= 20f) return 36;     // large body / section headings
            if (requested <= 28f) return 40;     // page headings
            return Round4(requested * 1.35f);
        }

        static int ScaleForSetting(int size)
        {
            float factor = Setting == Small ? 0.85f : (Setting == Normal ? 1.0f : 1.15f);
            return Round4(size * factor);
        }

        static int Round4(float value)
        {
            return Mathf.Max(16, Mathf.RoundToInt(value / 4f) * 4);
        }
    }

    /// <summary>Hidden runtime object that listens for F2.</summary>
    public sealed class TextSizeHotkey : MonoBehaviour
    {
        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f2Key.wasPressedThisFrame)
                UITextScale.Cycle();
        }
    }
}
