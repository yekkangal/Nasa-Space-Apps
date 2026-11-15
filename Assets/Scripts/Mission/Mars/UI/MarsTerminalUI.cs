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
    /// Mars terminal shell: status bar, six tabs, event console, CRT open/close. Page content
    /// lives in the partial files (Pages / Surface / Review) so the Moon terminal stays untouched.
    /// </summary>
    public sealed partial class MarsTerminalUI : MonoBehaviour, IMissionTerminalUI
    {
        public static MarsTerminalUI Instance { get; private set; }
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

        TMP_Text _budgetValue, _massValue, _c3Value, _edlValue, _powerValue, _dataValue, _successValue;
        readonly TMP_Text[] _tabLabels = new TMP_Text[6];
        readonly Button[] _tabButtons = new Button[6];
        readonly GameObject[] _pages = new GameObject[6];
        readonly Action[] _pageRefresh = new Action[6];

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

        // ---- page widgets ----------------------------------------------------
        Slider _windowSlider;
        TMP_Text _windowLabel, _windowNote, _capacityText;
        Image _capacityBar;

        TMP_Text _roverSummary, _roverStats;

        TMP_Text _edlBig, _edlDetail, _edlFactors, _landedText;
        Image _landedBar;

        readonly Slider[] _policySliders = new Slider[5];
        readonly TMP_Text[] _policyValues = new TMP_Text[5];
        RectTransform[] _daySegments;
        TMP_Text _dayLegend, _plannerPower, _plannerPerSol, _plannerSeason;

        TMP_Text _commsBig, _commsBreakdown, _commsNote;
        Image _commsBar;

        TMP_Text _reviewBig, _reviewSub, _reviewTips, _reviewPhases;
        readonly TMP_Text[] _readiness = new TMP_Text[6];
        GameObject _beforeLaunch, _debriefGroup;
        Button _launchButton;
        TMP_Text _debriefRating, _debriefBody;
        bool _debrief;

        GameObject _launchOverlay;
        TMP_Text _launchLog, _launchPhase, _launchMonth, _launchStatus;
        Image _launchBar;
        bool _launching, _skipLaunch;
        readonly List<string> _launchLines = new List<string>();

        // ======================================================================
        // creation
        // ======================================================================

        public static MarsTerminalUI Create(Transform parent, TMP_FontAsset font, Sprite panelSprite, Sprite buttonSprite)
        {
            if (Instance != null) return Instance;

            var go = new GameObject("MarsTerminalUI");
            if (parent != null) go.transform.SetParent(parent, false);

            var ui = go.AddComponent<MarsTerminalUI>();
            ui._font = font;
            ui._panelSprite = panelSprite;
            ui._buttonSprite = buttonSprite;
            ui.Build();
            MissionUIState.TerminalTabs = 6;
            Instance = ui;
            return ui;
        }

        void Build()
        {
            MissionUIFactory.EnsureEventSystem();

            _canvas = MissionUIFactory.CreateCanvas("MarsTerminalCanvas", 100, transform);
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

            MissionUIFactory.AddText(bar.rectTransform, "Mission", 20, 6, 150, 16, "MARS ROVER", 11,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);
            MissionUIFactory.AddText(bar.rectTransform, "MissionValue", 20, 21, 150, 26, "MISSION MARS", 16,
                MissionUIFactory.Amber, TextAlignmentOptions.MidlineLeft, _font);

            _budgetValue = Stat(bar.rectTransform, 180, 190, MarsStrings.StatBudget);
            _massValue = Stat(bar.rectTransform, 380, 230, MarsStrings.StatMass);
            _c3Value = Stat(bar.rectTransform, 620, 150, MarsStrings.StatC3);
            _edlValue = Stat(bar.rectTransform, 780, 140, MarsStrings.StatEdl);
            _powerValue = Stat(bar.rectTransform, 930, 260, MarsStrings.StatPower);
            _dataValue = Stat(bar.rectTransform, 1200, 200, MarsStrings.StatData);
            _successValue = Stat(bar.rectTransform, 1410, 180, MarsStrings.StatSuccess);

            var close = MissionUIFactory.AddButton(bar.rectTransform, "Close", 1844, 8, 44, 36,
                MarsStrings.CloseGlyph, 18, _font, Close, TextAlignmentOptions.Center, _buttonSprite);
            MissionUIFactory.SetButtonVisual(close, MissionUIFactory.PanelAlt, MissionUIFactory.Red);
        }

        TMP_Text Stat(RectTransform parent, float x, float w, string label)
        {
            MissionUIFactory.AddText(parent, label + "_label", x, 6, w, 16, label, 10,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);
            return MissionUIFactory.AddText(parent, label + "_value", x, 20, w, 27, "-", 16,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
        }

        void BuildNav()
        {
            var nav = MissionUIFactory.PanelBox(_frame, "Nav", 12, 76, 270, 856,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            var navRt = nav.rectTransform;

            MissionUIFactory.AddText(navRt, "NavTitle", 16, 14, 238, 44, "MARS\nMISSION", 20,
                MissionUIFactory.Amber, TextAlignmentOptions.TopLeft, _font);
            MissionUIFactory.AddText(navRt, "NavSub", 16, 62, 238, 18, "CONTROL TERMINAL", 10,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            string[] tabs =
            {
                MarsStrings.TabWindow,
                MarsStrings.TabRover,
                MarsStrings.TabEdl,
                MarsStrings.TabSurface,
                MarsStrings.TabComms,
                MarsStrings.TabReview
            };

            for (int i = 0; i < 6; i++)
            {
                int index = i;
                var btn = MissionUIFactory.AddButton(navRt, "Tab" + i, 14, 96 + i * 52, 242, 44,
                    tabs[i], 13, _font, () => SwitchTab(index), TextAlignmentOptions.MidlineLeft, _buttonSprite);
                _tabButtons[i] = btn;
                _tabLabels[i] = MissionUIFactory.ButtonLabel(btn);
            }

            MissionUIFactory.AddText(navRt, "NavHint", 16, 786, 238, 54,
                "ESC CLOSE\n1-6 TABS", 11, MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            var line = MissionUIFactory.Rect(navRt, "Rule", 14, 772, 242, 2);
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
            MissionUIFactory.AddText(console.rectTransform, "LogHint", 1140, 6, 456, 16,
                "MARS RELAY // DSN NOMINAL", 10, MissionUIFactory.TextDim, TextAlignmentOptions.TopRight, _font);
            _console = MissionUIFactory.AddText(console.rectTransform, "LogText", 12, 24, 1590, 94, "",
                11.5f, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _console.lineSpacing = 2f;
        }

        void BuildPages()
        {
            _pages[0] = BuildWindowPage();
            _pageRefresh[0] = RefreshWindow;
            _pages[1] = BuildRoverPage();
            _pageRefresh[1] = RefreshRover;
            _pages[2] = BuildEdlPage();
            _pageRefresh[2] = RefreshEdl;
            _pages[3] = BuildSurfacePage();
            _pageRefresh[3] = RefreshSurface;
            _pages[4] = BuildCommsPage();
            _pageRefresh[4] = RefreshComms;
            _pages[5] = BuildReviewPage();
            _pageRefresh[5] = RefreshReview;

            SwitchTab(0);
        }

        void BuildLaunchOverlay()
        {
            var rt = MissionUIFactory.Stretch(_content, "LaunchOverlay", 16, 16, 16, 16);
            _launchOverlay = rt.gameObject;
            MissionUIFactory.AddImage(rt, MissionUIFactory.Background, null, false, true);

            MissionUIFactory.AddText(rt, "Title", 0, 0, PageW, 32, MarsStrings.LaunchSequence, 26,
                MissionUIFactory.Green, TextAlignmentOptions.TopLeft, _font);
            _launchPhase = MissionUIFactory.AddText(rt, "Phase", 0, 36, PageW, 22, "PRE-LAUNCH", 12,
                MissionUIFactory.Amber, TextAlignmentOptions.TopLeft, _font);
            MissionUIFactory.AddText(rt, "SkipHint", PageW - 360, 4, 360, 22, MarsStrings.SkipHint, 11,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopRight, _font);

            _launchLog = MissionUIFactory.AddText(rt, "Log", 0, 70, PageW, 600, "", 12.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _launchLog.lineSpacing = 4f;

            _launchMonth = MissionUIFactory.AddText(rt, "MonthLabel", 0, 686, 900, 24, "CRUISE", 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.MidlineLeft, _font);
            _launchBar = MissionUIFactory.Bar(rt, "ProgressBar", 0, 714, PageW, 24);
            _launchStatus = MissionUIFactory.AddText(rt, "Status", 0, 752, PageW, 24, "NOMINAL", 12,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
            MissionUIFactory.AddText(rt, "Footer", 0, 784, PageW, 20,
                "TIME ACCELERATED // CRUISE, EDL AND 668 SOLS OF SURFACE OPERATIONS", 10,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            _launchOverlay.SetActive(false);
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
            var d = MarsResultStore.Design;
            switch (choice.Group)
            {
                case "vehicle": d.VehicleId = choice.Value; break;
                case "bus": d.BusId = choice.Value; break;
                case "mobility": d.MobilityId = choice.Value; break;
                case "arm": d.RoboticArm = !d.RoboticArm; break;
                case "thermal": d.ThermalId = choice.Value; break;
                case "power": d.PowerType = choice.Value; break;
                case "array": d.SolarAreaM2 = MarsCatalog.Arrays[choice.Index].AreaM2; break;
                case "battery": d.BatteryWh = MarsCatalog.Batteries[choice.Index].CapacityWh; break;
                case "instrument": d.ToggleInstrument(choice.Index); break;
                case "edl": d.EdlSystemId = choice.Value; break;
                case "shield": d.HeatShieldId = choice.Value; break;
                case "parachute": d.Parachute = !d.Parachute; break;
                case "trn": d.Trn = !d.Trn; break;
                case "site": d.SiteId = choice.Value; break;
                case "relay": d.RelayEnabled = !d.RelayEnabled; break;
                case "passes": d.RelayPasses = choice.Index; break;
                case "dte": d.DirectToEarth = !d.DirectToEarth; break;
            }

            d.Touch();
            MarsResultStore.Invalidate();
            MissionLog.Add("MARS DESIGN UPDATE // " + choice.Group.ToUpperInvariant());
            RefreshAll();
        }

        void OnWindowSlider(float value)
        {
            if (_updating) return;
            var d = MarsResultStore.Design;
            d.LaunchOffsetDays = Mathf.RoundToInt(value);
            d.Touch();
            MarsResultStore.Invalidate();
            RefreshAll();
        }

        void OnPolicySlider(float value)
        {
            if (_updating) return;
            var d = MarsResultStore.Design;
            d.DrivePct = Mathf.RoundToInt(_policySliders[0] != null ? _policySliders[0].value : 0);
            d.SciencePct = Mathf.RoundToInt(_policySliders[1] != null ? _policySliders[1].value : 0);
            d.DrillPct = Mathf.RoundToInt(_policySliders[2] != null ? _policySliders[2].value : 0);
            d.CommsPct = Mathf.RoundToInt(_policySliders[3] != null ? _policySliders[3].value : 0);
            d.RestPct = Mathf.RoundToInt(_policySliders[4] != null ? _policySliders[4].value : 0);
            d.Touch();
            MarsResultStore.Invalidate();
            RefreshAll();
        }

        void OnNormalisePolicy()
        {
            var d = MarsResultStore.Design;
            d.NormalizePolicy();
            d.Touch();
            MarsResultStore.Invalidate();
            MissionLog.Add("DAILY POLICY NORMALISED TO 100%");
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
            _anim = UIAnimations.CRTOn(_frame, _group, _bootText, MarsStrings.TerminalBoot, null);

            MissionLog.Add("MARS TERMINAL ONLINE // " + MarsConstants.PrimeMissionSols + " SOLS");
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

            MissionLog.Add("MARS TERMINAL OFFLINE");
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        void SwitchTab(int index)
        {
            if (_launching) return;
            if (index < 0 || index >= 6) return;
            _tab = index;
            for (int i = 0; i < 6; i++)
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
            if (kb.digit6Key.wasPressedThisFrame) SwitchTab(5);
        }

        // ======================================================================
        // refresh
        // ======================================================================

        void RefreshAll()
        {
            if (!IsOpen || _refreshing) return;
            _refreshing = true;

            var design = MarsResultStore.Design;
            if (_mcVersion != design.Version)
            {
                MarsResultStore.RunMonteCarlo(1000);
                _mcVersion = design.Version;
            }

            var report = MarsResultStore.Report;
            RefreshStatus(report);
            RefreshTabs();

            if (_pageRefresh[_tab] != null) _pageRefresh[_tab]();

            RefreshConsole();
            _refreshing = false;
        }

        void RefreshStatus(MarsReport report)
        {
            double cap = report.Cost.CapM;
            double cost = report.Cost.TotalM;
            _budgetValue.text = "$" + F(cost, "0") + " / " + F(cap, "0") + "M";
            _budgetValue.color = cost <= cap ? MissionUIFactory.Green
                : (cost <= cap * 1.1 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            double capacity = report.Mass.CapacityKg;
            _massValue.text = capacity > 0
                ? F(report.Mass.LaunchKg, "0") + " / " + F(capacity, "0") + " kg"
                : "NO CAPACITY";
            _massValue.color = capacity <= 0 ? MissionUIFactory.Red
                : (report.Mass.Pass ? MissionUIFactory.Green
                    : (report.Mass.LaunchKg <= capacity ? MissionUIFactory.Amber : MissionUIFactory.Red));

            _c3Value.text = F(report.Transfer.C3Km2S2, "0.0") + " km2/s2";
            _c3Value.color = report.Transfer.C3Km2S2 <= 10.0 ? MissionUIFactory.Green
                : (report.Transfer.C3Km2S2 <= 14.0 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            _edlValue.text = F(report.Edl.SuccessProbability * 100.0, "0") + "%";
            _edlValue.color = report.Edl.SuccessProbability >= 0.80 ? MissionUIFactory.Green
                : (report.Edl.SuccessProbability >= 0.60 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            _powerValue.text = F(report.Power.WorstGenerationWhSol, "0") + " Wh / "
                               + F(report.Power.LoadWhSol, "0") + " Wh";
            _powerValue.color = report.Power.Pass ? MissionUIFactory.Green
                : (report.Power.WorstMargin >= 0.0 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            _dataValue.text = F(report.Comms.TotalMbSol, "0") + " Mb/sol";
            _dataValue.color = report.Data.Pass ? MissionUIFactory.Green
                : (report.Data.Fraction >= 0.3 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            var mc = MarsResultStore.MonteCarloResult;
            _successValue.text = F(mc.SuccessRate * 100.0, "0.0") + "%";
            _successValue.color = mc.SuccessRate >= 0.65f ? MissionUIFactory.Green
                : (mc.SuccessRate >= 0.40f ? MissionUIFactory.Amber : MissionUIFactory.Red);
        }

        void RefreshTabs()
        {
            for (int i = 0; i < 6; i++)
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

        static string StatusOf(bool pass, bool warn)
        {
            return pass ? MarsStrings.StatusPass : (warn ? MarsStrings.StatusWarn : MarsStrings.StatusFail);
        }
    }
}
