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
    /// Full-screen CRT mission terminal, built entirely in code. Five tabs, live status bar,
    /// event console, and the LAUNCH CONTROL page. Input: 1-5 tabs, Esc / [X] close.
    /// </summary>
    public sealed class MissionTerminalUI : MonoBehaviour, IMissionTerminalUI
    {
        public static MissionTerminalUI Instance { get; private set; }
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

        TMP_Text _missionValue, _budgetValue, _massValue, _powerValue, _dvValue, _linkValue, _successValue;
        readonly TMP_Text[] _tabLabels = new TMP_Text[5];
        readonly Button[] _tabButtons = new Button[5];
        readonly GameObject[] _pages = new GameObject[5];
        readonly Action[] _pageRefresh = new Action[5];

        int _tab;
        int _mcVersion = -1;
        Coroutine _anim;
        bool _refreshing;

        // ---- page widgets ----------------------------------------------------

        sealed class Choice
        {
            public Button Button;
            public string Group;
            public string Value;
            public int Index;
        }

        sealed class AnalysisSection
        {
            public string Title;
            public TMP_Text[] Names;
            public TMP_Text[] Values;
            public TMP_Text[] Status;
            public TMP_Text Why;
        }

        readonly List<Choice> _choices = new List<Choice>();
        readonly List<AnalysisSection> _analysis = new List<AnalysisSection>();
        TMP_Text _spacecraftSummary;

        TMP_Text _briefTitle, _briefText, _briefOffline;
        TMP_Text[][] _briefCells;
        TMP_Text _facts1, _facts2, _facts3;

        Image _massBar;
        TMP_Text _massText, _launchInfo;

        TMP_Text _successBig, _successSub, _readinessWhy;
        readonly TMP_Text[] _readinessStatus = new TMP_Text[6];
        TMP_Text _debriefRating, _debriefBody, _debriefTips;
        GameObject _debriefGroup, _beforeLaunchGroup;
        Button _launchButton, _redesignButton, _closeTerminalButton;
        bool _debrief;

        GameObject _launchOverlay;
        TMP_Text _launchLog, _launchPhase, _launchMonth, _launchStatus;
        Image _launchBar;
        bool _launching;
        bool _skipLaunch;
        readonly List<string> _launchLines = new List<string>();

        TMP_Text _analysisHint;

        // ======================================================================
        // creation
        // ======================================================================

        public static MissionTerminalUI Create(Transform parent, TMP_FontAsset font, Sprite panelSprite, Sprite buttonSprite)
        {
            if (Instance != null) return Instance;

            var go = new GameObject("MissionTerminalUI");
            if (parent != null) go.transform.SetParent(parent, false);

            var ui = go.AddComponent<MissionTerminalUI>();
            ui._font = font;
            ui._panelSprite = panelSprite;
            ui._buttonSprite = buttonSprite;
            ui.Build();
            Instance = ui;
            return ui;
        }

        void Build()
        {
            MissionUIFactory.EnsureEventSystem();

            _canvas = MissionUIFactory.CreateCanvas("MissionTerminalCanvas", 100, transform);
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

        void BuildLaunchOverlay()
        {
            var rt = MissionUIFactory.Stretch(_content, "LaunchOverlay", 16, 16, 16, 16);
            _launchOverlay = rt.gameObject;
            MissionUIFactory.AddImage(rt, MissionUIFactory.Background, null, false, true);
            var border = MissionUIFactory.Stretch(rt, "Border", 0, 0, 0, 0);
            var borderImg = MissionUIFactory.AddImage(border, new Color(0f, 0f, 0f, 0f));
            borderImg.raycastTarget = false;

            MissionUIFactory.AddText(rt, "Title", 0, 0, PageW, 32, "LAUNCH SEQUENCE", 26,
                MissionUIFactory.Green, TextAlignmentOptions.TopLeft, _font);
            _launchPhase = MissionUIFactory.AddText(rt, "Phase", 0, 36, PageW, 22, "PRE-LAUNCH", 12,
                MissionUIFactory.Amber, TextAlignmentOptions.TopLeft, _font);
            MissionUIFactory.AddText(rt, "SkipHint", PageW - 360, 4, 360, 22,
                "PRESS SPACE TO SKIP", 11, MissionUIFactory.TextDim, TextAlignmentOptions.TopRight, _font);

            _launchLog = MissionUIFactory.AddText(rt, "Log", 0, 70, PageW, 600, "", 12.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _launchLog.lineSpacing = 4f;

            _launchMonth = MissionUIFactory.AddText(rt, "MonthLabel", 0, 686, 700, 24, "MONTH 00 / 12", 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.MidlineLeft, _font);
            _launchBar = MissionUIFactory.Bar(rt, "MonthBar", 0, 714, PageW, 24);
            _launchStatus = MissionUIFactory.AddText(rt, "Status", 0, 752, PageW, 24, "NOMINAL", 12,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
            MissionUIFactory.AddText(rt, "Footer", 0, 784, PageW, 20,
                "TIME ACCELERATED // RANDOM EVENTS ARE DRAWN FROM THE MONTE CARLO MODEL", 10,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            _launchOverlay.SetActive(false);
        }

        // ---- shell ------------------------------------------------------------

        void BuildStatusBar()
        {
            var bar = MissionUIFactory.PanelBox(_frame, "StatusBar", 12, 12, 1896, 52,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);

            _missionValue = Stat(bar.rectTransform, 20, 220, MissionStrings.StatMission);
            _budgetValue = Stat(bar.rectTransform, 250, 220, MissionStrings.StatBudget);
            _massValue = Stat(bar.rectTransform, 480, 250, MissionStrings.StatMass);
            _powerValue = Stat(bar.rectTransform, 740, 200, MissionStrings.StatPower);
            _dvValue = Stat(bar.rectTransform, 950, 190, MissionStrings.StatDeltaV);
            _linkValue = Stat(bar.rectTransform, 1150, 200, MissionStrings.StatLink);
            _successValue = Stat(bar.rectTransform, 1360, 240, MissionStrings.StatSuccess);

            var close = MissionUIFactory.AddButton(bar.rectTransform, "Close", 1844, 8, 44, 36,
                MissionStrings.CloseGlyph, 18, _font, Close, TextAlignmentOptions.Center, _buttonSprite);
            MissionUIFactory.SetButtonVisual(close, MissionUIFactory.PanelAlt, MissionUIFactory.Red);
        }

        TMP_Text Stat(RectTransform parent, float x, float w, string label)
        {
            MissionUIFactory.AddText(parent, label + "_label", x, 6, w, 16, label, 11,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);
            return MissionUIFactory.AddText(parent, label + "_value", x, 21, w, 26, "-", 17,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
        }

        void BuildNav()
        {
            var nav = MissionUIFactory.PanelBox(_frame, "Nav", 12, 76, 270, 856,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            var navRt = nav.rectTransform;

            MissionUIFactory.AddText(navRt, "NavTitle", 16, 14, 238, 44, "MISSION\nCONTROL", 20,
                MissionUIFactory.Green, TextAlignmentOptions.TopLeft, _font);
            MissionUIFactory.AddText(navRt, "NavSub", 16, 62, 238, 18, "TERMINAL v1.0", 10,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            string[] tabs =
            {
                MissionStrings.TabBriefing,
                MissionStrings.TabSpacecraft,
                MissionStrings.TabLaunch,
                MissionStrings.TabAnalysis,
                MissionStrings.TabLaunchControl
            };

            for (int i = 0; i < 5; i++)
            {
                int index = i;
                var btn = MissionUIFactory.AddButton(navRt, "Tab" + i, 14, 96 + i * 56, 242, 46,
                    tabs[i], 14, _font, () => SwitchTab(index), TextAlignmentOptions.MidlineLeft, _buttonSprite);
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
            MissionUIFactory.AddText(console.rectTransform, "LogHint", 1200, 6, 396, 16,
                "MISSION CONTROL // LINK NOMINAL", 10, MissionUIFactory.TextDim, TextAlignmentOptions.TopRight, _font);
            _console = MissionUIFactory.AddText(console.rectTransform, "LogText", 12, 24, 1590, 94, "",
                11.5f, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _console.lineSpacing = 2f;
        }

        // ======================================================================
        // pages
        // ======================================================================

        void BuildPages()
        {
            _pages[0] = BuildBriefing();
            _pageRefresh[0] = RefreshBriefing;
            _pages[1] = BuildSpacecraft();
            _pageRefresh[1] = RefreshSpacecraft;
            _pages[2] = BuildLaunch();
            _pageRefresh[2] = RefreshLaunch;
            _pages[3] = BuildAnalysis();
            _pageRefresh[3] = RefreshAnalysis;
            _pages[4] = BuildLaunchControl();
            _pageRefresh[4] = RefreshLaunchControl;

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

        // ---- 1 briefing -------------------------------------------------------

        GameObject BuildBriefing()
        {
            RectTransform p;
            var page = NewPage("PageBriefing", out p);
            PageTitle(p, MissionStrings.BriefingTitle);

            _briefTitle = MissionUIFactory.AddText(p, "MissionName", 0, 40, PageW, 28, "", 19,
                MissionUIFactory.Amber, TextAlignmentOptions.TopLeft, _font);
            _briefText = MissionUIFactory.AddText(p, "Brief", 0, 70, PageW, 52, "", 13.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _briefText.lineSpacing = 2f;

            _briefOffline = MissionUIFactory.AddText(p, "Offline", 0, 150, PageW, 80,
                MissionStrings.DataLinkOffline, 20, MissionUIFactory.Amber, TextAlignmentOptions.Center, _font);
            _briefOffline.characterSpacing = 4f;

            float[] colX = { 0f, 64f, 402f, 550f, 988f, 1326f, 1462f };
            float[] colW = { 56f, 330f, 142f, 428f, 330f, 130f, 98f };
            string[] headers =
            {
                "ID", MissionStrings.LabelObjective, MissionStrings.LabelMetric,
                MissionStrings.LabelRequirement, MissionStrings.LabelCurrent,
                MissionStrings.LabelStatus, MissionStrings.LabelPoints
            };

            for (int c = 0; c < headers.Length; c++)
            {
                MissionUIFactory.AddText(p, "H" + c, colX[c], 126, colW[c], 18, headers[c], 11,
                    MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            }
            var rule = MissionUIFactory.Rect(p, "HeadRule", 0, 144, PageW, 2);
            MissionUIFactory.AddImage(rule, MissionUIFactory.BorderColor);

            _briefCells = new TMP_Text[6][];
            for (int r = 0; r < 6; r++)
            {
                float y = 150f + r * 68f;
                var rowBg = MissionUIFactory.PanelBox(p, "Row" + r, 0, y, PageW, 64,
                    r % 2 == 0 ? MissionUIFactory.PanelFill : MissionUIFactory.PanelAlt,
                    MissionUIFactory.BorderColor, _panelSprite);

                _briefCells[r] = new TMP_Text[7];
                for (int c = 0; c < 7; c++)
                {
                    _briefCells[r][c] = MissionUIFactory.AddText(rowBg.rectTransform, "C" + c,
                        colX[c] + 6f, 4, colW[c] - 10f, 56f, "", c == 1 ? 12.5f : 11.5f,
                        MissionUIFactory.TextMain, TextAlignmentOptions.MidlineLeft, _font);
                }
            }

            var f1 = MissionUIFactory.PanelBox(p, "Facts1", 0, 566, 512, 214,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            _facts1 = MissionUIFactory.AddText(f1.rectTransform, "T", 14, 12, 484, 190, "", 12.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _facts1.lineSpacing = 10f;

            var f2 = MissionUIFactory.PanelBox(p, "Facts2", 524, 566, 512, 214,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            _facts2 = MissionUIFactory.AddText(f2.rectTransform, "T", 14, 12, 484, 190, "", 12.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _facts2.lineSpacing = 10f;

            var f3 = MissionUIFactory.PanelBox(p, "Facts3", 1048, 566, 514, 214,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            _facts3 = MissionUIFactory.AddText(f3.rectTransform, "T", 14, 12, 486, 190, "", 12.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _facts3.lineSpacing = 10f;

            MissionUIFactory.AddText(p, "Hint", 0, 796, PageW, 20, MissionStrings.HoverHint, 10,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            return page;
        }

        void RefreshBriefing()
        {
            var def = MissionDefinition.Get(MissionSelection.Key);
            var report = MissionResultStore.Report;
            var objectives = MissionObjectives.Evaluate(report, def);

            _briefTitle.text = def.Name + "   //   " + def.CodeName;
            _briefText.text = def.Brief;

            bool showTable = def.Playable;
            MissionUIFactory.SetActive(_briefOffline.gameObject, !showTable);

            for (int r = 0; r < 6; r++)
            {
                var rowGo = _briefCells[r][0].transform.parent.gameObject;
                bool visible = showTable && r < objectives.Length;
                MissionUIFactory.SetActive(rowGo, visible);
                if (!visible) continue;

                var o = objectives[r];
                var color = o.State == ObjectiveState.Met ? MissionUIFactory.Green
                    : (o.State == ObjectiveState.NotMet ? MissionUIFactory.Red : MissionUIFactory.TextDim);

                _briefCells[r][0].text = o.Id;
                _briefCells[r][1].text = o.Title;
                _briefCells[r][2].text = o.MetricLabel;
                _briefCells[r][3].text = o.Requirement;
                _briefCells[r][4].text = o.CurrentValue;
                _briefCells[r][5].text = MissionStrings.StateText(o.State);
                _briefCells[r][6].text = o.Id == "O2"
                    ? o.EarnedPoints.ToString("0", CultureInfo.InvariantCulture) + "/"
                      + report.SciencePoints.ToString("0", CultureInfo.InvariantCulture)
                    : o.Points.ToString("0", CultureInfo.InvariantCulture);

                _briefCells[r][0].color = color;
                _briefCells[r][5].color = color;
                _briefCells[r][6].color = color;
                _briefCells[r][1].color = MissionUIFactory.TextMain;
                _briefCells[r][2].color = MissionUIFactory.TextDim;
                _briefCells[r][3].color = MissionUIFactory.TextDim;
                _briefCells[r][4].color = MissionUIFactory.TextMain;
            }

            if (def.Playable)
            {
                _facts1.text =
                    "EARTH-MOON DISTANCE\n<color=#7DFFB2>" + F(def.EarthDistanceKm, "0,0") + " km</color>\n\n" +
                    "ONE-WAY LIGHT TIME\n<color=#7DFFB2>" + F(def.LightTimeOneWayS, "0.00") + " s</color>\n\n" +
                    "TRANSFER (CHEMICAL)\n<color=#7DFFB2>" + def.TransferWindow + "</color>";
                _facts2.text =
                    "ORBIT PERIOD (100 km)\n<color=#7DFFB2>" + F(def.OrbitPeriodMin, "0") + " min</color>\n\n" +
                    "LUNAR NIGHT\n<color=#7DFFB2>" + F(def.LunarNightDays, "0.00") + " Earth days</color>\n\n" +
                    "MISSION LIFE\n<color=#7DFFB2>" + def.MissionMonths + " months</color>";
                _facts3.text =
                    "DELTA-V BUDGET\n<color=#7DFFB2>" + F(def.DeltaVBudgetKmS, "0.00") + " km/s</color>\n\n" +
                    "BUDGET CAP\n<color=#7DFFB2>" + F(def.BudgetCapMillions, "0") + " $M</color>\n\n" +
                    "MASS LIMIT\n<color=#7DFFB2>95% of launch capacity</color>";
            }
            else
            {
                _facts1.text = ""; _facts2.text = ""; _facts3.text = "";
            }
        }

        // ---- 2 spacecraft -----------------------------------------------------

        GameObject BuildSpacecraft()
        {
            RectTransform p;
            var page = NewPage("PageSpacecraft", out p);
            PageTitle(p, MissionStrings.SpacecraftTitle);

            AddSectionHeader(p, 0, 44, 760, MissionStrings.SectionPropulsion);
            for (int i = 0; i < MissionCatalog.Propulsion.Length; i++)
                AddChoice(p, 0, 70 + i * 34, 760, "prop", MissionCatalog.Propulsion[i].Id, i, "PROPULSION " + i, MissionStrings.SectionPropulsion);

            AddSectionHeader(p, 0, 180, 760, MissionStrings.SectionArray);
            for (int i = 0; i < MissionCatalog.Arrays.Length; i++)
                AddChoice(p, 0, 206 + i * 34, 760, "array", "array" + i, i, MissionStrings.SectionArray + " " + i, MissionStrings.SectionArray);

            AddSectionHeader(p, 0, 346, 760, MissionStrings.SectionBattery);
            for (int i = 0; i < MissionCatalog.Batteries.Length; i++)
                AddChoice(p, 0, 372 + i * 34, 760, "battery", "battery" + i, i, MissionStrings.SectionBattery + " " + i, MissionStrings.SectionBattery);

            AddSectionHeader(p, 0, 546, 760, MissionStrings.SectionRadiation);
            for (int i = 0; i < MissionCatalog.RadiationClasses.Length; i++)
                AddChoice(p, 0, 572 + i * 34, 760, "rad", MissionCatalog.RadiationClasses[i].Id, i, MissionStrings.SectionRadiation + " " + i, MissionStrings.SectionRadiation);

            AddSectionHeader(p, 0, 682, 760, MissionStrings.SectionRedundancy);
            AddChoice(p, 0, 708, 760, "redundancy", "redundancy", 0, "REDUNDANCY", MissionStrings.SectionRedundancy);

            AddSectionHeader(p, 794, 44, 770, MissionStrings.SectionAntenna);
            for (int i = 0; i < MissionCatalog.Antennas.Length; i++)
                AddChoice(p, 794, 70 + i * 34, 770, "antenna", MissionCatalog.Antennas[i].Id, i, MissionStrings.SectionAntenna + " " + i, MissionStrings.SectionAntenna);

            AddSectionHeader(p, 794, 246, 770, MissionStrings.SectionRf);
            for (int i = 0; i < MissionCatalog.RfPowers.Length; i++)
                AddChoice(p, 794, 272 + i * 34, 770, "rf", "rf" + i, i, MissionStrings.SectionRf + " " + i, MissionStrings.SectionRf);

            AddSectionHeader(p, 794, 412, 770, MissionStrings.SectionGround);
            for (int i = 0; i < MissionCatalog.GroundStations.Length; i++)
                AddChoice(p, 794, 438 + i * 34, 770, "station", MissionCatalog.GroundStations[i].Id, i, MissionStrings.SectionGround + " " + i, MissionStrings.SectionGround);

            AddSectionHeader(p, 794, 512, 770, MissionStrings.SectionInstruments);
            for (int i = 0; i < MissionCatalog.Instruments.Length; i++)
                AddChoice(p, 794, 538 + i * 34, 770, "instrument", "inst" + i, i, MissionStrings.SectionInstruments + " " + i, MissionStrings.SectionInstruments);

            _spacecraftSummary = MissionUIFactory.AddText(p, "Summary", 0, 770, PageW, 40, "", 14,
                MissionUIFactory.Amber, TextAlignmentOptions.MidlineLeft, _font);

            return page;
        }

        void AddSectionHeader(RectTransform page, float x, float y, float w, string title)
        {
            MissionUIFactory.AddText(page, title + "_label", x, y, w, 18, title, 11.5f,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            var line = MissionUIFactory.Rect(page, title + "_line", x, y + 18, w, 1);
            MissionUIFactory.AddImage(line, MissionUIFactory.BorderColor);
        }

        void AddChoice(RectTransform page, float x, float y, float w, string group, string value, int index,
            string label, string section)
        {
            var btn = MissionUIFactory.AddButton(page, group + "_" + value, x, y, w, 30, label, 12,
                _font, null, TextAlignmentOptions.MidlineLeft, _buttonSprite);
            var choice = new Choice { Button = btn, Group = group, Value = value, Index = index };
            btn.onClick.AddListener(() => OnChoice(choice));
            _choices.Add(choice);
        }

        void OnChoice(Choice choice)
        {
            var d = MissionResultStore.Design;
            switch (choice.Group)
            {
                case "prop": d.PropulsionId = choice.Value; break;
                case "array": d.ArrayAreaM2 = MissionCatalog.Arrays[choice.Index].AreaM2; break;
                case "battery": d.BatteryWh = MissionCatalog.Batteries[choice.Index].CapacityWh; break;
                case "antenna": d.AntennaId = choice.Value; break;
                case "rf": d.RfPowerW = MissionCatalog.RfPowers[choice.Index].Watts; break;
                case "station": d.GroundStationId = choice.Value; break;
                case "rad": d.RadiationId = choice.Value; break;
                case "instrument": d.ToggleInstrument(choice.Index); break;
                case "redundancy": d.Redundancy = !d.Redundancy; break;
            }

            d.Touch();
            MissionResultStore.Invalidate();
            MissionResultStore.RunMonteCarlo(1000);
            _mcVersion = d.Version;

            MissionLog.Add("DESIGN UPDATE // " + choice.Group.ToUpperInvariant());
            RefreshAll();
        }

        void RefreshSpacecraft()
        {
            var d = MissionResultStore.Design;

            for (int i = 0; i < _choices.Count; i++)
            {
                var c = _choices[i];
                bool sel = false;
                string label = null;
                var labelText = MissionUIFactory.ButtonLabel(c.Button);

                switch (c.Group)
                {
                    case "prop":
                        {
                            var e = MissionCatalog.Propulsion[c.Index];
                            sel = d.PropulsionId == c.Value;
                            label = e.Name + "   Isp " + F(e.IspSeconds, "0") +
                                    " s   " + F(e.MassKg, "0") + " kg   $" + F(e.CostMillions, "0") + "M";
                            break;
                        }
                    case "array":
                        {
                            var a = MissionCatalog.Arrays[c.Index];
                            sel = Math.Abs(d.ArrayAreaM2 - a.AreaM2) < 0.01;
                            label = F(a.AreaM2, "0") + " m2 array   " +
                                    F(a.MassKg, "0") + " kg   $" + F(a.CostMillions, "0.0") + "M";
                            break;
                        }
                    case "battery":
                        {
                            var b = MissionCatalog.Batteries[c.Index];
                            sel = Math.Abs(d.BatteryWh - b.CapacityWh) < 0.01;
                            label = F(b.CapacityWh, "0") + " Wh   " +
                                    F(b.MassKg, "0.0") + " kg   $" + F(b.CostMillions, "0.00") + "M";
                            break;
                        }
                    case "antenna":
                        {
                            var a = MissionCatalog.Antenna(c.Value);
                            sel = d.AntennaId == c.Value;
                            label = a.Name + "   band " + a.Band + "   " +
                                    F(a.MassKg, "0") + " kg   $" + F(a.CostMillions, "0") + "M";
                            break;
                        }
                    case "rf":
                        {
                            var rf = MissionCatalog.RfPowers[c.Index];
                            sel = Math.Abs(d.RfPowerW - rf.Watts) < 0.01;
                            label = F(rf.Watts, "0") + " W RF   " +
                                    F(rf.MassKg, "0") + " kg   $" + F(rf.CostMillions, "0") + "M";
                            break;
                        }
                    case "station":
                        {
                            var s = MissionCatalog.Station(c.Value);
                            sel = d.GroundStationId == c.Value;
                            label = s.Name + "   " + F(s.VisibilityHoursPerDay, "0") +
                                    " h/day   $" + F(s.CostPerMonthMillions, "0.0") + "M/mo";
                            break;
                        }
                    case "rad":
                        {
                            var rad = MissionCatalog.Radiation(c.Value);
                            sel = d.RadiationId == c.Value;
                            label = rad.Name + "   " + F(rad.ToleranceKrad, "0") +
                                    " krad   cost x" + F(rad.CostMultiplier, "0.0") + "   mass x" + F(rad.MassMultiplier, "0.00");
                            break;
                        }
                    case "instrument":
                        {
                            var ins = MissionCatalog.Instruments[c.Index];
                            sel = d.InstrumentEnabled[c.Index];
                            label = (sel ? "ON   " : "OFF  ") + ins.Name + "   " + F(ins.MassKg, "0") + " kg   " +
                                    F(ins.PowerW, "0") + " W   +" + F(ins.Points, "0") + " pts";
                            break;
                        }
                    case "redundancy":
                        sel = d.Redundancy;
                        label = (sel ? "ON   " : "OFF  ") + "REDUNDANT BUS   +8% mass   +10% cost";
                        break;
                }

                if (label != null && labelText != null) labelText.text = label;
                MissionUIFactory.SetButtonVisual(c.Button,
                    sel ? MissionUIFactory.Selected : MissionUIFactory.PanelAlt,
                    sel ? MissionUIFactory.Green : MissionUIFactory.TextMain);
            }

            var r = MissionResultStore.Report;
            _spacecraftSummary.text =
                "ACTIVE INSTRUMENTS: " + d.ActiveInstrumentCount + "   //   " +
                F(r.Mass.InstrumentsKg, "0") + " kg   " + F(d.InstrumentPowerW, "0") + " W   +" +
                F(d.InstrumentPoints, "0") + " PTS   //   ARRAY " + F(d.ArrayAreaM2, "0") + " m2";
        }

        // ---- 3 launch ---------------------------------------------------------

        GameObject BuildLaunch()
        {
            RectTransform p;
            var page = NewPage("PageLaunch", out p);
            PageTitle(p, MissionStrings.LaunchTitle);

            for (int i = 0; i < MissionCatalog.LaunchVehicles.Length; i++)
            {
                var v = MissionCatalog.LaunchVehicles[i];
                var btn = MissionUIFactory.AddButton(p, "vehicle_" + v.Id, 0, 48 + i * 72, PageW, 60, v.Name, 15,
                    _font, null, TextAlignmentOptions.MidlineLeft, _buttonSprite);
                var choice = new Choice { Button = btn, Group = "vehicle", Value = v.Id, Index = i };
                btn.onClick.AddListener(() => OnChoice(choice));
                _choices.Add(choice);
            }

            var mass = MissionUIFactory.PanelBox(p, "MassPanel", 0, 350, PageW, 160,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(mass.rectTransform, "T", 14, 10, 600, 20,
                "WET MASS  vs  LAUNCH CAPACITY", 12, MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _massBar = MissionUIFactory.Bar(mass.rectTransform, "MassBar", 14, 44, 1200, 26);
            _massText = MissionUIFactory.AddText(mass.rectTransform, "MassText", 14, 80, 1530, 24, "", 14,
                MissionUIFactory.TextMain, TextAlignmentOptions.MidlineLeft, _font);
            MissionUIFactory.AddText(mass.rectTransform, "MassHint", 14, 110, 1530, 36,
                "The 5% reserve is mandatory: wet mass must stay below 95% of the capacity to the target orbit.",
                11, MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            var info = MissionUIFactory.PanelBox(p, "LaunchInfo", 0, 526, PageW, 200,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            _launchInfo = MissionUFInfo(info.rectTransform);

            return page;
        }

        TMP_Text MissionUFInfo(RectTransform parent)
        {
            var t = MissionUIFactory.AddText(parent, "Info", 14, 12, PageW - 28, 176, "", 13,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            t.lineSpacing = 8f;
            return t;
        }

        void RefreshLaunch()
        {
            var d = MissionResultStore.Design;
            var r = MissionResultStore.Report;

            for (int i = 0; i < _choices.Count; i++)
            {
                var c = _choices[i];
                if (c.Group != "vehicle") continue;
                var v = MissionCatalog.LaunchVehicles[c.Index];
                bool sel = d.VehicleId == v.Id;
                var label = MissionUIFactory.ButtonLabel(c.Button);
                if (label != null)
                {
                    label.text = v.Name +
                                 "    LEO " + F(v.LeoKg, "0") + " kg   /   TLI " +
                                 (v.TliKg > 0 ? F(v.TliKg, "0") + " kg" : "-- NO TLI --") +
                                 "    $" + F(v.CostMillions, "0") + "M    reliability " +
                                 F(v.Reliability * 100.0, "0") + "%";
                }
                MissionUIFactory.SetButtonVisual(c.Button,
                    sel ? MissionUIFactory.Selected : MissionUIFactory.PanelAlt,
                    sel ? MissionUIFactory.Green : MissionUIFactory.TextMain);
            }

            double capacity = r.Mass.CapacityKg;
            double wet = r.Mass.WetKg;
            double pct = capacity > 0 ? wet / capacity : 1.0;
            var color = capacity <= 0 ? MissionUIFactory.Red
                : (wet <= r.Mass.LimitKg ? MissionUIFactory.Green
                    : (wet <= capacity ? MissionUIFactory.Amber : MissionUIFactory.Red));
            MissionUIFactory.SetBar(_massBar, (float)pct, color);

            _massText.text = capacity > 0
                ? F(wet, "0") + " kg  /  " + F(capacity, "0") + " kg to " + r.Mass.CapacityBasis +
                  "  (" + F(pct * 100.0, "0") + "%)   limit " + F(r.Mass.LimitKg, "0") + " kg"
                : MissionStrings.NoOrbit + " on " + r.Vehicle.Name;
            _massText.color = color;

            _launchInfo.text =
                "<color=#7DFFB2>LAUNCH WINDOW</color>\n" +
                "Any day. The polar orbit is reached after a 3-5 day chemical transfer" +
                (r.Propulsion.Electric
                    ? " (this design uses a " + F(r.Propulsion.SpiralMonths, "0.0") + " month electric spiral)."
                    : ".") + "\n\n" +
                "<color=#7DFFB2>VEHICLE</color>\n" + r.Vehicle.Name + " - " + r.Vehicle.Note + "\n\n" +
                "<color=#7DFFB2>COST / RELIABILITY</color>\n" +
                "$" + F(r.Vehicle.CostMillions, "0") + "M   //   " + F(r.Vehicle.Reliability * 100.0, "0") +
                "% launch reliability   //   " + F(r.Mass.UsageFraction * 100.0, "0") + "% of capacity used";
        }

        // ---- 4 analysis -------------------------------------------------------

        GameObject BuildAnalysis()
        {
            RectTransform p;
            var page = NewPage("PageAnalysis", out p);
            PageTitle(p, MissionStrings.AnalysisTitle);
            MissionUIFactory.AddText(p, "Legend", PageW - 460, 4, 460, 22,
                "<color=#7DFFB2>PASS</color>    <color=#FFC857>WARN</color>    <color=#FF6B6B>FAIL</color>",
                11, MissionUIFactory.TextDim, TextAlignmentOptions.TopRight, _font);

            float y = 46f;
            AddAnalysis(p, 0, y, 770, MissionStrings.SectionMass,
                new[] { "DRY MASS", "PROPELLANT", "WET / LIMIT" }, MissionStrings.Why.Mass);
            y += 136f;
            AddAnalysis(p, 0, y, 770, MissionStrings.SectionPower,
                new[] { "CRUISE", "SCIENCE", "SCIENCE+DOWNLINK", "SAFE", "BATTERY" }, MissionStrings.Why.Power);
            y += 196f;
            AddAnalysis(p, 0, y, 770, MissionStrings.SectionDeltaV,
                new[] { "DELTA-V", "PROPELLANT FRACTION", "TRANSFER" }, MissionStrings.Why.DeltaV);

            y = 46f;
            AddAnalysis(p, 794, y, 770, MissionStrings.SectionLink,
                new[] { "EIRP", "SPACE LOSS", "DATA RATE", "LINK MARGIN" }, MissionStrings.Why.Link);
            y += 176f;
            AddAnalysis(p, 794, y, 770, MissionStrings.SectionData,
                new[] { "SCIENCE / DAY", "DOWNLINK / DAY", "RETURNED" }, MissionStrings.Why.Data);
            y += 136f;
            AddAnalysis(p, 794, y, 770, MissionStrings.SectionDose,
                new[] { "DOSE", "MONTHLY RISK" }, MissionStrings.Why.Radiation);
            y += 116f;
            AddAnalysis(p, 794, y, 770, MissionStrings.SectionCost,
                new[] { "HARDWARE", "LAUNCH + GROUND", "TOTAL / CAP" }, MissionStrings.Why.Cost);

            _analysisHint = MissionUIFactory.AddText(p, "Hint", 0, 792, PageW, 20,
                "HOVER A SECTION TO READ WHY IT MATTERS.", 10, MissionUIFactory.TextDim,
                TextAlignmentOptions.TopLeft, _font);

            return page;
        }

        void OnAnalysisHover(string hint)
        {
            if (_analysisHint == null) return;
            bool active = !string.IsNullOrEmpty(hint);
            _analysisHint.text = active ? hint : "HOVER A SECTION TO READ WHY IT MATTERS.";
            _analysisHint.color = active ? MissionUIFactory.Green : MissionUIFactory.TextDim;
        }

        void AddAnalysis(RectTransform page, float x, float y, float w, string title, string[] rows, string why)
        {
            float h = 36f + rows.Length * 20f + 30f;
            var box = MissionUIFactory.PanelBox(page, "A_" + title, x, y, w, h,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);

            var section = new AnalysisSection
            {
                Title = title,
                Names = new TMP_Text[rows.Length],
                Values = new TMP_Text[rows.Length],
                Status = new TMP_Text[rows.Length]
            };

            MissionUIFactory.AddText(box.rectTransform, "Title", 12, 8, w - 24, 18, title, 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);

            for (int i = 0; i < rows.Length; i++)
            {
                float rowY = 30f + i * 20f;
                section.Names[i] = MissionUIFactory.AddText(box.rectTransform, "N" + i, 12, rowY, 248, 18,
                    rows[i], 11, MissionUIFactory.TextDim, TextAlignmentOptions.MidlineLeft, _font);
                section.Values[i] = MissionUIFactory.AddText(box.rectTransform, "V" + i, 270, rowY, 280, 18,
                    "", 11.5f, MissionUIFactory.TextMain, TextAlignmentOptions.MidlineLeft, _font);
                section.Status[i] = MissionUIFactory.AddText(box.rectTransform, "S" + i, 562, rowY, 190, 18,
                    "", 10.5f, MissionUIFactory.TextDim, TextAlignmentOptions.MidlineLeft, _font);
            }

            section.Why = MissionUIFactory.AddText(box.rectTransform, "Why",
                12, 30f + rows.Length * 20f + 2f, w - 24, 26, why, 9.5f,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            var hoverRt = MissionUIFactory.Stretch(box.rectTransform, "Hover", 0, 0, 0, 0);
            MissionUIFactory.AddImage(hoverRt, new Color(0f, 0f, 0f, 0f), null, false, true);
            var hover = hoverRt.gameObject.AddComponent<UIPointerHint>();
            hover.Hint = why;
            hover.OnHover = OnAnalysisHover;

            _analysis.Add(section);
        }

        void SetAnalysis(AnalysisSection s, int row, string value, string status, Color color)
        {
            if (s == null || row < 0 || row >= s.Values.Length) return;
            s.Values[row].text = value;
            s.Status[row].text = status;
            s.Status[row].color = color;
            s.Values[row].color = color == MissionUIFactory.Red ? MissionUIFactory.Red : MissionUIFactory.TextMain;
        }

        void RefreshAnalysis()
        {
            var r = MissionResultStore.Report;

            // 0 mass
            var mass = _analysis[0];
            SetAnalysis(mass, 0, F(r.Mass.DryKg, "0") + " kg", "INFO", MissionUIFactory.TextDim);
            SetAnalysis(mass, 1, F(r.Mass.PropellantKg, "0") + " kg", "INFO", MissionUIFactory.TextDim);
            SetAnalysis(mass, 2, F(r.Mass.WetKg, "0") + " / " + F(r.Mass.LimitKg, "0") + " kg",
                r.Mass.Pass ? MissionStrings.StatusPass : MissionStrings.StatusFail,
                MissionUIFactory.StatusColor(r.Mass.Pass));

            // 1 power
            var power = _analysis[1];
            for (int i = 0; i < r.Power.Modes.Length; i++)
            {
                var m = r.Power.Modes[i];
                SetAnalysis(power, i, F(m.Margin * 100.0, "0") + "% margin  (" + F(m.LoadW, "0") + " W load)",
                    m.Pass ? MissionStrings.StatusPass : MissionStrings.StatusFail,
                    MissionUIFactory.StatusColor(m.Pass));
            }
            SetAnalysis(power, 4, F(r.Power.BatteryUsableWh, "0") + " / " + F(r.Power.EclipseEnergyWh, "0") + " Wh",
                r.Power.BatteryPass ? MissionStrings.StatusPass : MissionStrings.StatusFail,
                MissionUIFactory.StatusColor(r.Power.BatteryPass));

            // 2 delta-v
            var dv = _analysis[2];
            SetAnalysis(dv, 0, F(r.Propulsion.DeltaVKmS, "0.00") + " km/s", "INFO", MissionUIFactory.TextDim);
            bool propOk = r.Propulsion.PropellantFraction < 0.55;
            SetAnalysis(dv, 1, F(r.Propulsion.PropellantFraction * 100.0, "0") + "% of wet mass",
                propOk ? MissionStrings.StatusPass : MissionStrings.StatusWarn,
                MissionUIFactory.StatusColor(propOk, !propOk));
            SetAnalysis(dv, 2, r.Propulsion.Electric
                    ? F(r.Propulsion.SpiralMonths, "0.0") + " month spiral"
                    : "CHEMICAL 3-5 days",
                r.Propulsion.SpiralMonths <= 12.0 ? MissionStrings.StatusPass : MissionStrings.StatusWarn,
                MissionUIFactory.StatusColor(r.Propulsion.SpiralMonths <= 12.0, true));

            // 3 link
            var link = _analysis[3];
            SetAnalysis(link, 0, F(r.Link.EirpDbi, "0.0") + " dBW", "INFO", MissionUIFactory.TextDim);
            SetAnalysis(link, 1, F(r.Link.FsplDb, "0.0") + " dB", "INFO", MissionUIFactory.TextDim);
            SetAnalysis(link, 2, F(r.Link.UsedRateBps / 1e6, "0.00") + " Mbps (" + r.Link.Band + ")",
                "INFO", MissionUIFactory.TextDim);
            SetAnalysis(link, 3, F(r.Link.MarginDb, "0.0") + " dB",
                r.Link.Pass ? MissionStrings.StatusPass : MissionStrings.StatusFail,
                MissionUIFactory.StatusColor(r.Link.Pass));

            // 4 data
            var data = _analysis[4];
            SetAnalysis(data, 0, F(r.Data.ScienceGbPerDay, "0.0") + " GB/day", "INFO", MissionUIFactory.TextDim);
            SetAnalysis(data, 1, F(r.Data.DownlinkGbPerDay, "0.0") + " GB/day", "INFO", MissionUIFactory.TextDim);
            bool dataOk = r.Data.Fraction >= 0.999;
            bool dataWarn = r.Data.Fraction >= 0.5;
            SetAnalysis(data, 2, F(r.Data.Fraction * 100.0, "0") + "%",
                dataOk ? MissionStrings.StatusPass : (dataWarn ? MissionStrings.StatusWarn : MissionStrings.StatusFail),
                MissionUIFactory.StatusColor(dataOk, dataWarn));

            // 5 radiation
            var rad = _analysis[5];
            SetAnalysis(rad, 0, F(r.Radiation.DoseKrad, "0.0") + " / " + F(r.Radiation.ToleranceKrad, "0") + " krad",
                r.Radiation.Pass ? MissionStrings.StatusPass : MissionStrings.StatusFail,
                MissionUIFactory.StatusColor(r.Radiation.Pass));
            SetAnalysis(rad, 1, F(r.Radiation.MonthlyFailureProbability * 100.0, "0") + "% / month",
                r.Radiation.Pass ? "LOW" : "HIGH",
                MissionUIFactory.StatusColor(r.Radiation.Pass));

            // 6 cost
            var cost = _analysis[6];
            SetAnalysis(cost, 0, F(r.Cost.DevelopmentMillions + r.Cost.ContingencyMillions, "0.0") + " $M",
                "INFO", MissionUIFactory.TextDim);
            SetAnalysis(cost, 1, F(r.Cost.LaunchMillions + r.Cost.GroundMillions + r.Cost.OpsMillions, "0.0") + " $M",
                "INFO", MissionUIFactory.TextDim);
            SetAnalysis(cost, 2, F(r.Cost.TotalMillions, "0.0") + " / " + F(r.Cost.CapMillions, "0") + " $M",
                r.Cost.Pass ? MissionStrings.StatusPass : MissionStrings.StatusFail,
                MissionUIFactory.StatusColor(r.Cost.Pass));
        }

        // ---- 5 launch control -------------------------------------------------

        GameObject BuildLaunchControl()
        {
            RectTransform p;
            var page = NewPage("PageLaunchControl", out p);
            PageTitle(p, MissionStrings.LaunchControlTitle);

            var left = MissionUIFactory.PanelBox(p, "Readiness", 0, 44, 770, 780,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(left.rectTransform, "T", 14, 10, 740, 20,
                MissionStrings.ReadinessTitle, 13, MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);

            string[] items =
            {
                "ORBIT CAPABILITY", "MASS LIMIT (95%)", "BUDGET (350 $M)",
                "POWER MARGIN (15%)", "DATA DOWNLINK", "RADIATION DOSE"
            };
            for (int i = 0; i < items.Length; i++)
            {
                float y = 42f + i * 30f;
                MissionUIFactory.AddText(left.rectTransform, "R" + i, 14, y, 420, 24, items[i], 12,
                    MissionUIFactory.TextMain, TextAlignmentOptions.MidlineLeft, _font);
                _readinessStatus[i] = MissionUIFactory.AddText(left.rectTransform, "RS" + i, 540, y, 210, 24,
                    "", 12, MissionUIFactory.Green, TextAlignmentOptions.MidlineRight, _font);
            }

            _readinessWhy = MissionUIFactory.AddText(left.rectTransform, "Why", 14, 232, 740, 40, "", 10.5f,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            var mcBox = MissionUIFactory.PanelBox(left.rectTransform, "MC", 14, 282, 742, 180,
                MissionUIFactory.PanelAlt, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(mcBox.rectTransform, "T", 12, 8, 400, 18,
                MissionStrings.SuccessTitle, 11, MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _successBig = MissionUIFactory.AddText(mcBox.rectTransform, "Big", 12, 30, 350, 60, "0%", 42,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
            _successSub = MissionUIFactory.AddText(mcBox.rectTransform, "Sub", 12, 100, 716, 70, "", 12,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _successSub.lineSpacing = 4f;

            var tips = MissionUIFactory.PanelBox(left.rectTransform, "Tips", 14, 478, 742, 286,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(tips.rectTransform, "T", 12, 8, 400, 18,
                MissionStrings.DesignTipTitle, 11, MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _debriefTips = MissionUIFactory.AddText(tips.rectTransform, "Body", 12, 30, 716, 246, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _debriefTips.lineSpacing = 6f;

            var right = MissionUIFactory.PanelBox(p, "Actions", 794, 44, 770, 780,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);

            _beforeLaunchGroup = MissionUIFactory.Rect(right.rectTransform, "BeforeLaunch", 0, 0, 770, 780).gameObject;
            MissionUIFactory.AddText((RectTransform)_beforeLaunchGroup.transform, "T", 20, 20, 730, 100,
                "Launch is allowed even with warnings.\nThe checklist is advice, not a lock.",
                14, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);

            _launchButton = MissionUIFactory.AddButton(right.rectTransform, "Launch", 20, 620, 730, 90,
                MissionStrings.LaunchButtonText, 34, _font, OnLaunch, TextAlignmentOptions.Center, _buttonSprite);
            MissionUIFactory.SetButtonVisual(_launchButton, MissionUIFactory.Selected, MissionUIFactory.Green);

            _debriefGroup = MissionUIFactory.Rect(right.rectTransform, "Debrief", 0, 0, 770, 780).gameObject;
            _debriefGroup.SetActive(false);

            _debriefRating = MissionUIFactory.AddText((RectTransform)_debriefGroup.transform, "Rating",
                20, 16, 730, 48, "", 30, MissionUIFactory.Green, TextAlignmentOptions.TopLeft, _font);
            _debriefBody = MissionUIFactory.AddText((RectTransform)_debriefGroup.transform, "Body",
                20, 74, 730, 400, "", 12.5f, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _debriefBody.lineSpacing = 6f;

            _redesignButton = MissionUIFactory.AddButton((RectTransform)_debriefGroup.transform, "Redesign",
                20, 620, 350, 60, MissionStrings.RedesignText, 18, _font, OnRedesign,
                TextAlignmentOptions.Center, _buttonSprite);
            _closeTerminalButton = MissionUIFactory.AddButton((RectTransform)_debriefGroup.transform, "CloseTerminal",
                400, 620, 350, 60, MissionStrings.CloseTerminalText, 18, _font, Close,
                TextAlignmentOptions.Center, _buttonSprite);

            return page;
        }

        void RefreshLaunchControl()
        {
            var r = MissionResultStore.Report;
            var mc = MissionResultStore.MonteCarloResult;

            bool[] pass =
            {
                r.OrbitFeasible,
                r.Mass.Pass,
                r.Cost.Pass,
                r.Power.Pass,
                r.Data.Fraction >= 0.999,
                r.Radiation.Pass
            };
            string[] labels =
            {
                r.OrbitFeasible ? MissionStrings.StatusPass : MissionStrings.StatusFail,
                r.Mass.Pass ? MissionStrings.StatusPass : MissionStrings.StatusFail,
                r.Cost.Pass ? MissionStrings.StatusPass : MissionStrings.StatusFail,
                r.Power.Pass ? MissionStrings.StatusPass : MissionStrings.StatusFail,
                r.Data.Fraction >= 0.999 ? MissionStrings.StatusPass
                    : (r.Data.Fraction >= 0.5 ? MissionStrings.StatusWarn : MissionStrings.StatusFail),
                r.Radiation.Pass ? MissionStrings.StatusPass : MissionStrings.StatusFail
            };

            for (int i = 0; i < _readinessStatus.Length; i++)
            {
                _readinessStatus[i].text = labels[i];
                _readinessStatus[i].color = pass[i] ? MissionUIFactory.Green
                    : (labels[i] == MissionStrings.StatusWarn ? MissionUIFactory.Amber : MissionUIFactory.Red);
            }

            _readinessWhy.text = r.OrbitFeasible
                ? "Warnings are survivable: the Monte Carlo shows what the mission really returns."
                : "No orbit, no science. Fix the launch vehicle or the mass before launching.";

            _successBig.text = F(mc.SuccessRate * 100.0, "0.0") + "%";
            _successBig.color = mc.SuccessRate >= 0.70f ? MissionUIFactory.Green
                : (mc.SuccessRate >= 0.40f ? MissionUIFactory.Amber : MissionUIFactory.Red);
            _successSub.text =
                "FULL " + F(mc.FullProbability * 100.0, "0.0") + "%   //   PARTIAL " +
                F(mc.PartialProbability * 100.0, "0.0") + "%   //   FAIL " +
                F(mc.FailProbability * 100.0, "0.0") + "%\n" +
                "EXPECTED SCORE " + F(mc.ExpectedPoints, "0") + " PTS    //    MAX " +
                F(MissionObjectives.MaxPoints(r), "0") + " PTS";

            var tips = MissionObjectives.AutoTips(r);
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < tips.Count && i < 4; i++)
            {
                if (i > 0) sb.Append('\n');
                sb.Append("- ").Append(tips[i]);
            }
            _debriefTips.text = sb.ToString();

            var timeline = MissionResultStore.LastTimeline;
            bool showDebrief = _debrief && timeline != null;
            MissionUIFactory.SetActive(_debriefGroup, showDebrief);
            MissionUIFactory.SetActive(_beforeLaunchGroup, !showDebrief);
            MissionUIFactory.SetActive(_launchButton.gameObject, !showDebrief);

            if (!showDebrief) return;

            _debriefRating.text = MissionStrings.Rating(timeline.Rating);
            _debriefRating.color = timeline.Rating == 2 ? MissionUIFactory.Green
                : (timeline.Rating == 1 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            var def = MissionDefinition.Get(MissionSelection.Key);
            var body = new System.Text.StringBuilder();
            body.Append("POINTS: ").Append(F(timeline.Points, "0")).Append("   //   ")
                .Append(timeline.OrbitReached ? "ORBIT REACHED" : "NO ORBIT").Append('\n');

            if (timeline.ObjectivePoints != null && def.Objectives != null)
            {
                for (int i = 0; i < def.Objectives.Length && i < timeline.ObjectivePoints.Length; i++)
                {
                    body.Append(def.Objectives[i].Id).Append("  ")
                        .Append(def.Objectives[i].Title).Append("   ")
                        .Append(F(timeline.ObjectivePoints[i], "0")).Append(" pts\n");
                }
            }

            if (timeline.MissionLost && !string.IsNullOrEmpty(timeline.FailureReason))
                body.Append("\nFAILURE: ").Append(timeline.FailureReason).Append('\n');

            body.Append("\nWEAKEST CONSTRAINT TIPS\n");
            int shown = 0;
            for (int i = 0; i < timeline.Tips.Count && shown < 3; i++)
            {
                body.Append("- ").Append(timeline.Tips[i]).Append('\n');
                shown++;
            }
            _debriefBody.text = body.ToString();
        }

        void OnLaunch()
        {
            if (_launching) return;

            var timeline = MissionResultStore.Launch();
            if (timeline == null) return;

            _launching = true;
            _skipLaunch = false;
            _launchLines.Clear();

            if (_launchLog != null) _launchLog.text = string.Empty;
            if (_launchPhase != null)
            {
                _launchPhase.text = "PRE-LAUNCH // " + MissionResultStore.Report.Vehicle.Name.ToUpperInvariant();
                _launchPhase.color = MissionUIFactory.Amber;
            }
            if (_launchMonth != null) _launchMonth.text = "MONTH 00 / 12";
            if (_launchStatus != null)
            {
                _launchStatus.text = "NOMINAL";
                _launchStatus.color = MissionUIFactory.Green;
            }
            if (_launchBar != null) MissionUIFactory.SetBar(_launchBar, 0f, MissionUIFactory.Green);

            if (_launchOverlay != null)
            {
                _launchOverlay.SetActive(true);
                _launchOverlay.transform.SetAsLastSibling();
            }

            MissionLog.Add("LAUNCH COMMAND ACCEPTED // " + MissionResultStore.Design.VehicleId.ToUpperInvariant());
            UIAnimations.Run(LaunchSequenceRoutine(timeline));
        }

        IEnumerator LaunchSequenceRoutine(MissionTimeline timeline)
        {
            AppendLaunchLine("<color=#4E8A66>LAUNCH CONTROL // " +
                             MissionResultStore.Report.Vehicle.Name + " // " +
                             F(MissionResultStore.Report.Mass.WetKg, "0") + " kg</color>");
            yield return LaunchWait(0.25f);
            if (this == null) yield break;

            for (int i = 0; i < timeline.Events.Count; i++)
            {
                if (this == null) yield break;

                if (_skipLaunch)
                {
                    for (int j = i; j < timeline.Events.Count; j++) AppendTimelineEvent(timeline.Events[j]);
                    break;
                }

                AppendTimelineEvent(timeline.Events[i]);
                yield return LaunchWait(timeline.Events[i].Month > 0 ? 0.30f : 0.45f);
            }

            if (this == null) yield break;
            if (!_skipLaunch) yield return LaunchWait(0.55f);
            if (this == null) yield break;

            EndLaunchSequence(timeline);
        }

        IEnumerator LaunchWait(float seconds)
        {
            float t = 0f;
            while (t < seconds && !_skipLaunch)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        void AppendTimelineEvent(TimelineEvent e)
        {
            string color = e.Alarm ? "#FF6B6B" : "#7DFFB2";
            AppendLaunchLine("<color=" + color + ">" + e.Phase + "</color>  " + e.Text);
            MissionLog.Add(e.Phase + ": " + e.Text);

            if (e.Month > 0)
            {
                int m = Mathf.Clamp(e.Month, 0, 12);
                if (_launchMonth != null) _launchMonth.text = "MONTH " + m.ToString("00") + " / 12";
                if (_launchBar != null)
                    MissionUIFactory.SetBar(_launchBar, m / 12f, e.Alarm ? MissionUIFactory.Amber : MissionUIFactory.Green);
            }

            if (_launchPhase != null)
            {
                _launchPhase.text = e.Phase + (e.Alarm ? " // CAUTION" : " // NOMINAL");
                _launchPhase.color = e.Alarm ? MissionUIFactory.Red : MissionUIFactory.Green;
            }
            if (_launchStatus != null && e.Alarm)
            {
                _launchStatus.text = "EVENT DETECTED";
                _launchStatus.color = MissionUIFactory.Amber;
            }
        }

        void AppendLaunchLine(string line)
        {
            _launchLines.Add(line);
            while (_launchLines.Count > 20) _launchLines.RemoveAt(0);
            if (_launchLog != null) _launchLog.text = string.Join("\n", _launchLines.ToArray());
        }

        void EndLaunchSequence(MissionTimeline timeline)
        {
            _launching = false;
            _skipLaunch = false;
            _debrief = true;

            int month = timeline.MissionLost ? 0 : 12;
            if (timeline.MissionLost && timeline.Events.Count > 0)
                month = timeline.Events[timeline.Events.Count - 1].Month;
            if (_launchMonth != null) _launchMonth.text = "MONTH " + month.ToString("00") + " / 12";
            if (_launchBar != null)
                MissionUIFactory.SetBar(_launchBar, month / 12f,
                    timeline.MissionLost ? MissionUIFactory.Red : MissionUIFactory.Green);

            if (_launchPhase != null)
            {
                _launchPhase.text = timeline.MissionLost ? "MISSION LOST" : "MISSION COMPLETE // DEBRIEF READY";
                _launchPhase.color = timeline.MissionLost ? MissionUIFactory.Red : MissionUIFactory.Green;
            }
            if (_launchStatus != null)
            {
                _launchStatus.text = timeline.MissionLost
                    ? "ABORT // " + (timeline.FailureReason ?? "unknown failure")
                    : "ALL PHASES NOMINAL // " + F(timeline.Points, "0") + " PTS";
                _launchStatus.color = timeline.MissionLost ? MissionUIFactory.Red : MissionUIFactory.Green;
            }

            MissionLog.Add("DEBRIEF: " + MissionStrings.Rating(timeline.Rating) + " - " +
                           F(timeline.Points, "0") + " PTS");

            if (_launchOverlay != null) _launchOverlay.SetActive(false);
            RefreshAll();
        }

        void OnRedesign()
        {
            var d = MissionResultStore.Design;
            d.ResetToDefault();
            MissionResultStore.Invalidate();
            _debrief = false;
            _launching = false;
            _skipLaunch = false;
            if (_launchOverlay != null) _launchOverlay.SetActive(false);
            MissionLog.Add("REDESIGN: defaults restored.");
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
            _anim = UIAnimations.CRTOn(_frame, _group, _bootText, MissionStrings.TerminalBoot, null);

            MissionLog.Add("TERMINAL ONLINE // " + MissionDefinition.Get(MissionSelection.Key).CodeName);
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

            MissionLog.Add("TERMINAL OFFLINE");
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

            var design = MissionResultStore.Design;
            if (_mcVersion != design.Version)
            {
                MissionResultStore.RunMonteCarlo(1000);
                _mcVersion = design.Version;
            }

            var report = MissionResultStore.Report;
            RefreshStatus(report);
            RefreshTabs();

            if (_pageRefresh[_tab] != null) _pageRefresh[_tab]();

            RefreshConsole();
            _refreshing = false;
        }

        void RefreshStatus(EngineeringReport report)
        {
            var def = MissionDefinition.Get(MissionSelection.Key);
            _missionValue.text = def.CodeName;

            double cap = def.BudgetCapMillions > 0 ? def.BudgetCapMillions : 350.0;
            double cost = report.Cost.TotalMillions;
            _budgetValue.text = "$" + F(cost, "0.0") + " / " + F(cap, "0");
            _budgetValue.color = cost <= cap ? MissionUIFactory.Green
                : (cost <= cap * 1.1 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            double capacity = report.Mass.CapacityKg;
            _massValue.text = capacity > 0
                ? F(report.Mass.WetKg, "0") + " / " + F(capacity, "0") + " kg"
                : "NO TLI";
            _massValue.color = capacity <= 0 ? MissionUIFactory.Red
                : (report.Mass.WetKg <= report.Mass.LimitKg ? MissionUIFactory.Green
                    : (report.Mass.WetKg <= capacity ? MissionUIFactory.Amber : MissionUIFactory.Red));

            _powerValue.text = F(report.Power.MinMargin * 100.0, "0") + "%";
            _powerValue.color = report.Power.Pass ? MissionUIFactory.Green
                : (report.Power.MinMargin >= 0.05 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            _dvValue.text = F(report.Propulsion.DeltaVKmS, "0.00") + " km/s";
            _dvValue.color = report.Vehicle.ReachesMoon ? MissionUIFactory.Green : MissionUIFactory.Red;

            _linkValue.text = F(report.Link.MarginDb, "0.0") + " dB";
            _linkValue.color = report.Link.Pass ? MissionUIFactory.Green
                : (report.Link.MarginDb > 0.0 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            var mc = MissionResultStore.MonteCarloResult;
            float success = mc.SuccessRate;
            _successValue.text = F(success * 100.0, "0.0") + "%";
            _successValue.color = success >= 0.70f ? MissionUIFactory.Green
                : (success >= 0.40f ? MissionUIFactory.Amber : MissionUIFactory.Red);
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

        static string F(double value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
