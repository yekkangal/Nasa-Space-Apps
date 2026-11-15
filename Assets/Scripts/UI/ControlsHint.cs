// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MissionArchitect
{
    /// <summary>
    /// Persistent Roblox-style key-cap hint row at the bottom centre. Context aware:
    /// world, Mission Lab, terminal and handbook each show their own chips.
    /// H hides/show hints (saved); the B chip pulses in Mission Lab until the handbook is opened.
    /// </summary>
    public sealed class ControlsHint : MonoBehaviour
    {
        public const int SortingOrder = 9000;      // below SceneFader (9999)
        const string HintsKey = "MA_HintsOn";
        const string HandbookSeenKey = "MA_HandbookSeen";
        const float ChipHeight = 56f;
        const float Gap = 12f;

        struct Hint
        {
            public string Key;
            public string Label;

            public Hint(string key, string label)
            {
                Key = key;
                Label = label;
            }
        }

        static ControlsHint _instance;

        Canvas _canvas;
        CanvasGroup _group;
        RectTransform _row;
        Image _bHandbookCap;
        Coroutine _anim;
        string _signature = "";
        bool _built;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create()
        {
            if (_instance != null) return;
            var go = new GameObject("ControlsHint");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ControlsHint>();
            _instance.Build();
        }

        void Build()
        {
            _canvas = MissionUIFactory.CreateCanvas("ControlsHintCanvas", SortingOrder, transform);
            var root = MissionUIFactory.Stretch(_canvas.transform, "Root", 0, 0, 0, 0);
            _group = root.gameObject.AddComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _group.alpha = 0f;

            _row = new GameObject("Row", typeof(RectTransform)).GetComponent<RectTransform>();
            _row.SetParent(root, false);
            _row.anchorMin = new Vector2(0.5f, 0f);
            _row.anchorMax = new Vector2(0.5f, 0f);
            _row.pivot = new Vector2(0.5f, 0f);
            _row.anchoredPosition = Vector2.zero;
            _row.sizeDelta = new Vector2(100f, ChipHeight);
        }

        void Update()
        {
            if (_group == null) return;

            var kb = Keyboard.current;
            bool hintsOn = PlayerPrefs.GetInt(HintsKey, 1) == 1;

            if (kb != null && kb.hKey.wasPressedThisFrame)
            {
                hintsOn = !hintsOn;
                PlayerPrefs.SetInt(HintsKey, hintsOn ? 1 : 0);
                PlayerPrefs.Save();
                _signature = "";
            }

            if (MissionUIState.HandbookOpen && PlayerPrefs.GetInt(HandbookSeenKey, 0) == 0)
            {
                PlayerPrefs.SetInt(HandbookSeenKey, 1);
                PlayerPrefs.Save();
                _signature = "";
            }

            string signature = BuildSignature(hintsOn);
            if (!_built || signature != _signature)
            {
                _signature = signature;
                Rebuild(hintsOn);
            }

            AnimateBHandbook();
        }

        bool InMissionLab()
        {
            return SceneManager.GetActiveScene().name == "MissionLab";
        }

        string BuildSignature(bool hintsOn)
        {
            if (!hintsOn) return "hidden";
            if (MissionUIState.TerminalOpen)
                return "terminal:" + MissionUIState.TerminalTabs + ":" + UITextScale.Setting;
            if (MissionUIState.HandbookOpen) return "handbook:" + UITextScale.Setting;
            if (InMissionLab())
                return "lab:" + PlayerPrefs.GetInt(HandbookSeenKey, 0) + ":" + UITextScale.Setting;
            return "world:" + UITextScale.Setting;
        }

        List<Hint> CurrentHints(bool hintsOn)
        {
            var list = new List<Hint>();
            if (!hintsOn)
            {
                list.Add(new Hint("H", "Show hints"));
                return list;
            }

            if (MissionUIState.TerminalOpen)
            {
                int n = Mathf.Max(2, MissionUIState.TerminalTabs);
                list.Add(new Hint("1-" + n, "Tabs"));
                list.Add(new Hint("Esc", "Close"));
                return list;
            }

            if (MissionUIState.HandbookOpen)
            {
                list.Add(new Hint("< >", "Page"));
                list.Add(new Hint("B/Esc", "Close"));
                return list;
            }

            list.Add(new Hint("WASD", "Move"));
            list.Add(new Hint("E", "Interact"));
            list.Add(new Hint("M", "Music"));
            list.Add(new Hint("F2", "Text size"));
            list.Add(new Hint("H", "Hide hints"));
            if (InMissionLab()) list.Add(new Hint("B", "Handbook"));
            return list;
        }

        static float KeyCapWidth(string key)
        {
            return Mathf.Clamp(key.Length * 18f + 16f, 48f, 112f);
        }

        static float ChipWidth(Hint hint)
        {
            return KeyCapWidth(hint.Key) + 10f + LabelWidth(hint.Label);
        }

        static float LabelWidth(string label)
        {
            return Mathf.Clamp(label.Length * 15f + 8f, 48f, 210f);
        }

        void Rebuild(bool hintsOn)
        {
            if (_row == null) return;
            UIAnimations.Stop(ref _anim);
            _bHandbookCap = null;

            for (int i = _row.childCount - 1; i >= 0; i--)
                Destroy(_row.GetChild(i).gameObject);

            var hints = CurrentHints(hintsOn);
            float width = 0f;
            for (int i = 0; i < hints.Count; i++)
                width += ChipWidth(hints[i]) + (i > 0 ? Gap : 0f);
            _row.sizeDelta = new Vector2(width, ChipHeight);

            // Bottom-centre and above the bottom-left SI label. Two-chip rows are narrow, so
            // they clear the label horizontally as well.
            float offset = 78f;
            Vector2 home = new Vector2(0f, offset);
            _row.anchoredPosition = home;

            float x = 0f;
            for (int i = 0; i < hints.Count; i++)
            {
                var chip = BuildChip(hints[i], x);
                if (InMissionLab() && hintsOn && hints[i].Key == "B")
                    _bHandbookCap = chip;
                x += ChipWidth(hints[i]) + Gap;
            }

            _group.alpha = 0f;
            StartIntro(home);
        }

        Image BuildChip(Hint hint, float x)
        {
            float keyW = KeyCapWidth(hint.Key);
            float chipW = ChipWidth(hint);
            var chip = new GameObject("Chip_" + hint.Key, typeof(RectTransform)).GetComponent<RectTransform>();
            chip.SetParent(_row, false);
            chip.anchorMin = new Vector2(0f, 0f);
            chip.anchorMax = new Vector2(0f, 0f);
            chip.pivot = new Vector2(0f, 0f);
            chip.anchoredPosition = new Vector2(x, 0f);
            chip.sizeDelta = new Vector2(chipW, ChipHeight);

            var cap = MissionUIFactory.PanelBox(chip, "Cap", 0, 6, keyW, 44,
                MissionUIFactory.Selected, MissionUIFactory.BorderBright, null);
            MissionUIFactory.AddText(cap.rectTransform, "Key", 0, 4, keyW, 36, hint.Key, 11,
                MissionUIFactory.Green, TextAlignmentOptions.Center, null);

            MissionUIFactory.AddText(chip, "Label", keyW + 10f, 10f, ChipWidth(hint) - keyW - 10f, 40f,
                hint.Label, 12, MissionUIFactory.TextMain, TextAlignmentOptions.MidlineLeft, null);

            return cap;
        }

        void StartIntro(Vector2 home)
        {
            _anim = UIAnimations.Run(IntroRoutine(home));
        }

        IEnumerator IntroRoutine(Vector2 home)
        {
            const int steps = 4;
            Vector2 start = home + new Vector2(0f, -40f);
            for (int i = 1; i <= steps; i++)
            {
                float k = i / (float)steps;
                _group.alpha = k;
                _row.anchoredPosition = Vector2.Lerp(start, home, k);
                yield return null;
            }
            _group.alpha = 1f;
            _row.anchoredPosition = home;
            _anim = null;
        }

        void AnimateBHandbook()
        {
            if (_bHandbookCap == null) return;
            bool seen = PlayerPrefs.GetInt(HandbookSeenKey, 0) == 1;
            if (seen)
            {
                _bHandbookCap.color = MissionUIFactory.BorderBright;
                _bHandbookCap = null;
                return;
            }

            float p = Mathf.PingPong(Time.unscaledTime * 1.6f, 1f);
            _bHandbookCap.color = Color.Lerp(MissionUIFactory.BorderBright, MissionUIFactory.Amber, p);
        }
    }
}
