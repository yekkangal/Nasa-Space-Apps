// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MissionArchitect
{
    /// <summary>
    /// Asteroid terminal shell: status bar, five tabs, event console, CRT open/close.
    /// Pages live in the partial files (Pages / Ops); Moon and Mars shells are untouched.
    /// </summary>
    public sealed partial class AsteroidTerminalUI : MonoBehaviour, IMissionTerminalUI
    {
        public static AsteroidTerminalUI Instance { get; private set; }
        public bool IsOpen { get; private set; }

        const float PageW = 1582f;
        const float PageH = 824f;

        TMP_FontAsset _font;
        Sprite _panelSprite;
        Sprite _buttonSprite;

        Canvas _canvas;
        RectTransform _frame;
        CanvasGroup _group;
        TMP_Text _bootText;
        RectTransform _content;
        TMP_Text _console;

        TMP_Text _budgetValue, _massValue, _dvValue, _sampleValue, _capsuleValue, _successValue;
        readonly TMP_Text[] _tabLabels = new TMP_Text[5];
        readonly Button[] _tabButtons = new Button[5];
        readonly GameObject[] _pages = new GameObject[5];
        readonly Action[] _pageRefresh = new Action[5];

        int _tab;
        int _mcVersion = -1;
        Coroutine _anim;
        bool _refreshing;
        bool _updating;

        sealed class Choice
        {
            public Button Button;
            public string Group;
            public string Value;
            public int Index;
        }

        readonly List<Choice> _choices = new List<Choice>();

        // ======================================================================
        // creation
        // ======================================================================

        public static AsteroidTerminalUI Create(Transform parent, TMP_FontAsset font, Sprite panelSprite, Sprite buttonSprite)
        {
            if (Instance != null) return Instance;

            var go = new GameObject("AsteroidTerminalUI");
            if (parent != null) go.transform.SetParent(parent, false);

            var ui = go.AddComponent<AsteroidTerminalUI>();
            ui._font = font;
            ui._panelSprite = panelSprite;
            ui._buttonSprite = buttonSprite;
            ui.Build();
            MissionUIState.TerminalTabs = 5;
            Instance = ui;
            return ui;
        }

        void Build()
        {
            MissionUIFactory.EnsureEventSystem();

            _canvas = MissionUIFactory.CreateCanvas("AsteroidTerminalCanvas", 100, transform);
            var root = MissionUIFactory.Stretch(_canvas.transform, "Root", 0, 0, 0, 0);
            _group = root.gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;

            _frame = MissionUIFactory.Stretch(root, "Frame", 0, 0, 0, 0);
            MissionUIFactory.AddImage(_frame, MissionUIFactory.Background);

            _bootText = MissionUIFactory.AddText(_frame, "BootText", 60, 80, 1400, 44, "", 22,
                MissionUIFactory.Green, TextAlignmentOptions.TopLeft, _font);
            _bootText.gameObject.SetActive(false);

            BuildStatusBar();
            BuildNav();
            BuildContent();
            BuildPages();
            BuildLaunchOverlay();

            _canvas.gameObject.SetActive(false);
        }

        void BuildStatusBar()
        {
            var bar = MissionUIFactory.PanelBox(_frame, "StatusBar", 12, 12, 1896, 52,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);

            MissionUIFactory.AddText(bar.rectTransform, "Mission", 20, 6, 160, 16, "MISSION ASTEROID", 10,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);
            MissionUIFactory.AddText(bar.rectTransform, "MissionValue", 20, 21, 170, 26, "SAMPLE RETURN", 14,
                MissionUIFactory.Amber, TextAlignmentOptions.MidlineLeft, _font);

            _budgetValue = Stat(bar.rectTransform, 200, 200, AsteroidStrings.StatBudget);
            _massValue = Stat(bar.rectTransform, 410, 240, AsteroidStrings.StatMass);
            _dvValue = Stat(bar.rectTransform, 660, 160, AsteroidStrings.StatDeltaV);
            _sampleValue = Stat(bar.rectTransform, 830, 190, AsteroidStrings.StatSample);
            _capsuleValue = Stat(bar.rectTransform, 1030, 210, AsteroidStrings.StatCapsule);
            _successValue = Stat(bar.rectTransform, 1250, 180, AsteroidStrings.StatSuccess);

            var close = MissionUIFactory.AddButton(bar.rectTransform, "Close", 1844, 8, 44, 36,
                AsteroidStrings.CloseGlyph, 18, _font, Close, TextAlignmentOptions.Center, _buttonSprite);
            MissionUIFactory.SetButtonVisual(close, MissionUIFactory.PanelAlt, MissionUIFactory.Red);
        }

        TMP_Text Stat(RectTransform parent, float x, float w, string label)
        {
            MissionUIFactory.AddText(parent, label + "_label", x, 6, w, 16, label, 9.5f,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);
            return MissionUIFactory.AddText(parent, label + "_value", x, 20, w, 27, "-", 15,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
        }

        void BuildNav()
        {
            var nav = MissionUIFactory.PanelBox(_frame, "Nav", 12, 76, 270, 856,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            var navRt = nav.rectTransform;

            MissionUIFactory.AddText(navRt, "NavTitle", 16, 14, 238, 44, "ASTEROID\nSAMPLE RETURN", 18,
                MissionUIFactory.Amber, TextAlignmentOptions.TopLeft, _font);
            MissionUIFactory.AddText(navRt, "NavSub", 16, 62, 238, 18, "CONTROL TERMINAL", 10,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            string[] tabs =
            {
                AsteroidStrings.TabTarget,
                AsteroidStrings.TabSpacecraft,
                AsteroidStrings.TabTrajectory,
                AsteroidStrings.TabProx,
                AsteroidStrings.TabReview
            };

            for (int i = 0; i < 5; i++)
            {
                int index = i;
                var btn = MissionUIFactory.AddButton(navRt, "Tab" + i, 14, 96 + i * 56, 242, 46,
                    tabs[i], 13, _font, () => SwitchTab(index), TextAlignmentOptions.MidlineLeft, _buttonSprite);
                _tabButtons[i] = btn;
                _tabLabels[i] = MissionUIFactory.ButtonLabel(btn);
            }

            MissionUIFactory.AddText(navRt, "NavHint", 16, 790, 238, 50,
                "ESC CLOSE\n1-5 TABS", 11, MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            var line = MissionUIFactory.Rect(navRt, "Rule", 14, 776, 242, 2);
            MissionUIFactory.AddImage(line, MissionUIFactory.BorderColor);
        }

        void BuildContent()
        {
            var panel = MissionUIFactory.PanelBox(_frame, "Content", 294, 76, 1614, 856,
                MissionUIFactory.Background, MissionUIFactory.BorderColor, _panelSprite);
            _content = panel.rectTransform;

            var console = MissionUIFactory.PanelBox(_frame, "Console", 294, 944, 1614, 124,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(console.rectTransform, "LogLabel", 12, 6, 200, 16, "EVENT LOG", 10,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            MissionUIFactory.AddText(console.rectTransform, "LogHint", 1100, 6, 496, 16,
                "DSN // ASTEROID LINK", 10, MissionUIFactory.TextDim, TextAlignmentOptions.TopRight, _font);
            _console = MissionUIFactory.AddText(console.rectTransform, "LogText", 12, 24, 1590, 94, "",
                11.5f, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _console.lineSpacing = 2f;
        }

        void BuildPages()
        {
            _pages[0] = BuildTargetPage();
            _pageRefresh[0] = RefreshTarget;
            _pages[1] = BuildSpacecraftPage();
            _pageRefresh[1] = RefreshSpacecraft;
            _pages[2] = BuildTrajectoryPage();
            _pageRefresh[2] = RefreshTrajectory;
            _pages[3] = BuildProxPage();
            _pageRefresh[3] = RefreshProx;
            _pages[4] = BuildReviewPage();
            _pageRefresh[4] = RefreshReview;

            SwitchTab(0);
        }

        GameObject NewPage(string name, out RectTransform rt)
        {
            rt = MissionUIFactory.Stretch(_content, name, 16, 16, 16, 16);
            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }

        TMP_Text PageTitle(RectTransform page, string text)
        {
            return MissionUIFactory.AddText(page, "Title", 0, 0, PageW, 34, text, 25,
                MissionUIFactory.Green, TextAlignmentOptions.TopLeft, _font);
        }

        void AddSectionHeader(RectTransform page, float x, float y, float w, string title)
        {
            MissionUIFactory.AddText(page, title + "_label", x, y, w, 18, title, 11.5f,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            var line = MissionUIFactory.Rect(page, title + "_line", x, y + 18, w, 1);
            MissionUIFactory.AddImage(line, MissionUIFactory.BorderColor);
        }

        void AddChoice(RectTransform page, float x, float y, float w, string group, string value, int index,
            string label)
        {
            var btn = MissionUIFactory.AddButton(page, group + "_" + value, x, y, w, 28, label, 11.5f,
                _font, null, TextAlignmentOptions.MidlineLeft, _buttonSprite);
            var choice = new Choice { Button = btn, Group = group, Value = value, Index = index };
            btn.onClick.AddListener(() => OnChoice(choice));
            _choices.Add(choice);
        }

        void OnChoice(Choice choice)
        {
            var d = AsteroidResultStore.Design;
            switch (choice.Group)
            {
                case "target":
                    d.TargetId = choice.Value;
                    d.OrbitRadiusM = AsteroidCatalog.Target(choice.Value).DefaultOrbitRadiusM;
                    break;
                case "bus": d.BusId = choice.Value; break;
                case "prop": d.PropulsionId = choice.Value; break;
                case "array": d.ArrayAreaM2 = AsteroidCatalog.Arrays[choice.Index].AreaM2; break;
                case "battery": d.BatteryWh = AsteroidCatalog.Batteries[choice.Index].CapacityWh; break;
                case "antenna": d.AntennaId = choice.Value; break;
                case "rf": d.RfPowerW = MissionCatalog.RfPowers[choice.Index].Watts; break;
                case "sampler": d.SamplerId = choice.Value; break;
                case "capsule": d.CapsuleId = choice.Value; break;
                case "instrument": d.ToggleInstrument(choice.Index); break;
                case "traj": d.TrajectoryType = choice.Value; break;
                case "vehicle": d.VehicleId = choice.Value; break;
                case "site": d.SiteId = choice.Value; break;
                case "nav": d.Navigation = !d.Navigation; break;
                case "attempts": d.Attempts = choice.Index; break;
            }

            d.Touch();
            AsteroidResultStore.Invalidate();
            MissionLog.Add("ASTEROID DESIGN UPDATE // " + choice.Group.ToUpperInvariant());
            RefreshAll();
        }

        void OnOrbitSlider(float value)
        {
            if (_updating) return;
            var d = AsteroidResultStore.Design;
            d.OrbitRadiusM = value;
            d.Touch();
            AsteroidResultStore.Invalidate();
            RefreshAll();
        }

        void OnCampaignSlider(float value)
        {
            if (_updating) return;
            var d = AsteroidResultStore.Design;
            d.CampaignYears = Math.Round(value * 4.0) / 4.0;
            d.Touch();
            AsteroidResultStore.Invalidate();
            RefreshAll();
        }

        void OnContactSlider(float value)
        {
            if (_updating) return;
            var d = AsteroidResultStore.Design;
            d.ContactSpeedCmS = value;
            d.Touch();
            AsteroidResultStore.Invalidate();
            RefreshAll();
        }

        // ======================================================================
        // open / close / navigation
        // ======================================================================

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            _tab = 0;
            MissionUIState.TerminalOpen = true;
            MissionUIState.SyncInputLock();
            MissionUIFactory.EnsureEventSystem();

            _canvas.gameObject.SetActive(true);
            _group.alpha = 1f;
            _group.blocksRaycasts = true;
            _group.interactable = true;

            _launching = false;
            _skipLaunch = false;
            if (_launchOverlay != null) _launchOverlay.SetActive(false);

            SwitchTab(0);
            RefreshAll();

            UIAnimations.Stop(ref _anim);
            UIAnimations.ResetTransform(_frame, Vector2.zero, _group, 1f);
            _anim = UIAnimations.CRTOn(_frame, _group, _bootText, AsteroidStrings.TerminalBoot, null);

            MissionLog.Add("ASTEROID TERMINAL ONLINE // " + AsteroidResultStore.Report.Target.Name);
            RefreshConsole();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            MissionUIState.TerminalOpen = false;
            MissionUIState.SyncInputLock();

            if (_bootText != null) _bootText.gameObject.SetActive(false);

            _launching = false;
            _skipLaunch = true;
            if (_launchOverlay != null) _launchOverlay.SetActive(false);

            UIAnimations.Stop(ref _anim);
            UIAnimations.ResetTransform(_frame, Vector2.zero, _group, 1f);
            _anim = UIAnimations.CRTOff(_frame, _group, () =>
            {
                if (!IsOpen) _canvas.gameObject.SetActive(false);
            });

            MissionLog.Add("ASTEROID TERMINAL OFFLINE");
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        void SwitchTab(int index)
        {
            if (_launching) return;
            if (index < 0 || index >= 5) return;
            _tab = index;
            for (int i = 0; i < 5; i++)
                if (_pages[i] != null) _pages[i].SetActive(i == index);
            RefreshTabs();
            if (_pageRefresh[index] != null && _pages[index] != null && _pages[index].activeInHierarchy)
                _pageRefresh[index]();
        }

        void Update()
        {
            if (!IsOpen) return;
            var kb = Keyboard.current;
            if (kb == null) return;

            if (_launching)
            {
                if (kb.spaceKey.wasPressedThisFrame) _skipLaunch = true;
                return;
            }

            if (kb.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }

            if (kb.digit1Key.wasPressedThisFrame) SwitchTab(0);
            if (kb.digit2Key.wasPressedThisFrame) SwitchTab(1);
            if (kb.digit3Key.wasPressedThisFrame) SwitchTab(2);
            if (kb.digit4Key.wasPressedThisFrame) SwitchTab(3);
            if (kb.digit5Key.wasPressedThisFrame) SwitchTab(4);
        }

        // ======================================================================
        // refresh
        // ======================================================================

        void RefreshAll()
        {
            if (!IsOpen || _refreshing) return;
            _refreshing = true;

            var design = AsteroidResultStore.Design;
            if (_mcVersion != design.Version)
            {
                AsteroidResultStore.RunMonteCarlo(1000);
                _mcVersion = design.Version;
            }

            var report = AsteroidResultStore.Report;
            RefreshStatus(report);
            RefreshTabs();

            if (_pageRefresh[_tab] != null) _pageRefresh[_tab]();

            RefreshConsole();
            _refreshing = false;
        }

        void RefreshStatus(AsteroidReport report)
        {
            double cap = report.Cost.CapM;
            double cost = report.Cost.TotalM;
            _budgetValue.text = "$" + F(cost, "0") + " / " + F(cap, "0") + "M";
            _budgetValue.color = cost <= cap ? MissionUIFactory.Green
                : (cost <= cap * 1.1 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            double capacity = report.Mass.CapacityKg;
            _massValue.text = capacity > 0
                ? F(report.Mass.WetKg, "0") + " / " + F(capacity, "0") + " kg"
                : "NO CAPACITY";
            _massValue.color = capacity <= 0 ? MissionUIFactory.Red
                : (report.Mass.Pass ? MissionUIFactory.Green
                    : (report.Mass.WetKg <= capacity ? MissionUIFactory.Amber : MissionUIFactory.Red));

            _dvValue.text = F(report.Trajectory.TotalKmS, "0.00") + " km/s";
            _dvValue.color = report.Mass.Pass && capacity > 0 ? MissionUIFactory.Green : MissionUIFactory.Red;

            _sampleValue.text = report.ExpectedGrams + " g";
            _sampleValue.color = report.ExpectedGrams >= AsteroidConstants.SampleRequirementG
                ? MissionUIFactory.Green
                : (report.ExpectedGrams >= 20 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            _capsuleValue.text = Pct(report.Capsule.SurvivalProbability, "0") + " / " +
                                 (report.Capsule.Margin >= 0 ? "+" : "") + Pct(report.Capsule.Margin, "0");
            _capsuleValue.color = report.Capsule.Pass ? MissionUIFactory.Green
                : (report.Capsule.SurvivalProbability >= 0.3 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            var mc = AsteroidResultStore.MonteCarloResult;
            _successValue.text = Pct(mc.SuccessRate, "0.0");
            _successValue.color = mc.SuccessRate >= 0.60f ? MissionUIFactory.Green
                : (mc.SuccessRate >= 0.35f ? MissionUIFactory.Amber : MissionUIFactory.Red);
        }

        void RefreshTabs()
        {
            for (int i = 0; i < 5; i++)
            {
                if (_tabButtons[i] == null) continue;
                bool active = i == _tab;
                MissionUIFactory.SetButtonVisual(_tabButtons[i],
                    active ? MissionUIFactory.Selected : MissionUIFactory.PanelAlt,
                    active ? MissionUIFactory.Green : MissionUIFactory.TextDim);
            }
        }

        void RefreshConsole()
        {
            if (_console != null) _console.text = MissionLog.Join("\n");
        }

        // ---- helpers ----------------------------------------------------------

        static string F(double value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        static string Pct(double value, string format)
        {
            return (value * 100.0).ToString(format, CultureInfo.InvariantCulture) + "%";
        }

        static string Sci(double value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        static string Wrap(string text, int width)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var words = text.Split(' ');
            var sb = new System.Text.StringBuilder();
            int line = 0;
            for (int i = 0; i < words.Length; i++)
            {
                int add = words[i].Length + (line > 0 ? 1 : 0);
                if (line > 0 && line + add > width)
                {
                    sb.Append('\n');
                    line = 0;
                }
                if (line > 0) { sb.Append(' '); line++; }
                sb.Append(words[i]);
                line += words[i].Length;
            }
            return sb.ToString();
        }
    }
}
