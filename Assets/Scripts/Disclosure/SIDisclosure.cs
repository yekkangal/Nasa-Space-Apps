// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MissionArchitect
{
    /// <summary>
    /// Persistent SI disclosure: a small bottom-left label in every scene and an F1 panel with
    /// the full disclosure. Created before the first scene loads, like SceneFader.
    /// </summary>
    public sealed class SIDisclosure : MonoBehaviour
    {
        static SIDisclosure _instance;
        GameObject _panel;
        Canvas _canvas;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create()
        {
            if (_instance != null) return;
            var go = new GameObject("SIDisclosure");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SIDisclosure>();
            _instance.Build();
        }

        void Build()
        {
            _canvas = MissionUIFactory.CreateCanvas("SIDisclosureCanvas", 5000, transform);
            var root = MissionUIFactory.Stretch(_canvas.transform, "Root", 0, 0, 0, 0);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            var label = MissionUIFactory.PanelBox(root, "Label", 16, 1018, 900, 44,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, null);
            MissionUIFactory.AddText(label.rectTransform, "Text", 10, 4, 880, 36,
                SIDisclosureText.Label, 12, MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, null);

            _panel = MissionUIFactory.Stretch(root, "Panel", 0, 0, 0, 0).gameObject;
            MissionUIFactory.AddImage(_panel.GetComponent<RectTransform>(), new Color(0f, 0f, 0f, 0.72f));
            var box = MissionUIFactory.PanelBox(_panel.GetComponent<RectTransform>(), "Box",
                300, 130, 1320, 820, MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, null);
            MissionUIFactory.AddText(box.rectTransform, "Title", 20, 12, 1280, 28,
                "SI DISCLOSURE - F1 TO CLOSE", 18, MissionUIFactory.Green,
                TextAlignmentOptions.TopLeft, null);
            MissionUIFactory.AddText(box.rectTransform, "Body", 20, 48, 1280, 700,
                SIDisclosureText.FullText + "\n\n" + MusicCredit.Line, 13, MissionUIFactory.TextMain,
                TextAlignmentOptions.TopLeft, null);
            _panel.SetActive(false);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || _panel == null) return;
            if (kb.f1Key.wasPressedThisFrame)
                _panel.SetActive(!_panel.activeSelf);
        }
    }
}
