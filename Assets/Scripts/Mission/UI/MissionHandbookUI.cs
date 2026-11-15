// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MissionArchitect
{
    /// <summary>
    /// Pixel-art mission handbook on the B key. Three spreads: live objectives, glossary,
    /// engineer's tips. Opens with a slide/cover animation and flips pages with a quick
    /// X-scale page turn. Shares MissionResultStore with the terminal - no duplicated rules.
    /// </summary>
    public sealed class MissionHandbookUI : MonoBehaviour
    {
        public static MissionHandbookUI Instance { get; private set; }
        public bool IsOpen { get; private set; }

        const string MetColor = "#2E7D32";
        const string NotMetColor = "#B3261E";
        const string PendingColor = "#8A7A5E";

        TMP_FontAsset _font;
        Sprite _panelSprite;
        Sprite _buttonSprite;

        Canvas _canvas;
        RectTransform _root;
        CanvasGroup _group;
        RectTransform _book;
        Vector2 _bookHome = new Vector2(340f, -130f);
        RectTransform _leftPage, _rightPage;
        TMP_Text _leftText, _rightText;
        TMP_Text _pageLabel;
        Button _prevButton, _nextButton;
        GameObject _hint;
        TMP_Text _hintText;

        int _page;
        Coroutine _anim;
        Coroutine _flip;
        bool _flipping;

        // ======================================================================
        // creation
        // ======================================================================

        public static MissionHandbookUI Create(Transform parent, TMP_FontAsset font, Sprite panelSprite, Sprite buttonSprite)
        {
            if (Instance != null) return Instance;

            var go = new GameObject("MissionHandbookUI");
            if (parent != null) go.transform.SetParent(parent, false);

            var ui = go.AddComponent<MissionHandbookUI>();
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

            _canvas = MissionUIFactory.CreateCanvas("MissionHandbookCanvas", 150, transform);

            _root = MissionUIFactory.Stretch(_canvas.transform, "Root", 0, 0, 0, 0);
            _group = _root.gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;

            var dim = MissionUIFactory.Stretch(_root, "Dim", 0, 0, 0, 0);
            MissionUIFactory.AddImage(dim, new Color(0.02f, 0.03f, 0.025f, 0.65f));

            BuildHint();
            BuildBook();

            _root.gameObject.SetActive(false);
            RefreshPages();
        }

        void BuildHint()
        {
            var hintPanel = MissionUIFactory.PanelBox(_canvas.transform as RectTransform, "HandbookHint",
                1700, 1024, 204, 40, MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            _hint = hintPanel.gameObject;
            _hintText = MissionUIFactory.AddText(hintPanel.rectTransform, "T", 10, 4, 184, 32,
                MissionStrings.HandbookHint, 13, MissionUIFactory.Green, TextAlignmentOptions.Midline, _font);
        }

        void BuildBook()
        {
            _book = MissionUIFactory.Rect(_root, "Book", _bookHome.x, _bookHome.y, 1240f, 820f);
            MissionUIFactory.AddImage(_book, MissionUIFactory.Cover);
            var edge = MissionUIFactory.Stretch(_book, "Edge", 4, 4, 4, 4);
            var edgeImg = MissionUIFactory.AddImage(edge, MissionUIFactory.CoverEdge);
            edgeImg.raycastTarget = false;
            var inside = MissionUIFactory.Stretch(_book, "Inside", 8, 8, 8, 8);
            MissionUIFactory.AddImage(inside, MissionUIFactory.Cover);

            MissionUIFactory.AddText(_book, "BookTitle", 26, 12, 1100, 24,
                MissionStrings.HandbookTitle + "   //   LUNAR POLAR ORBITER", 13,
                MissionUIFactory.Paper, TextAlignmentOptions.MidlineLeft, _font);

            var close = MissionUIFactory.AddButton(_book, "CloseBook", 1184, 8, 40, 28, "X", 14, _font, Close,
                TextAlignmentOptions.Center, _buttonSprite);
            MissionUIFactory.SetButtonVisual(close, MissionUIFactory.CoverEdge, MissionUIFactory.Paper);

            _leftPage = MissionUIFactory.Rect(_book, "LeftPage", 24f, 44f, 584f, 712f);
            BuildPaper(_leftPage);

            _rightPage = MissionUIFactory.Rect(_book, "RightPage", 632f, 44f, 584f, 712f);
            BuildPaper(_rightPage);

            var spine = MissionUIFactory.Rect(_book, "Spine", 610f, 44f, 20f, 712f);
            MissionUIFactory.AddImage(spine, MissionUIFactory.PaperLine);

            _leftText = MissionUIFactory.AddText(_leftPage, "Text", 22f, 16f, 540f, 680f, "", 13f,
                MissionUIFactory.Ink, TextAlignmentOptions.TopLeft, _font);
            _leftText.lineSpacing = 5f;

            _rightText = MissionUIFactory.AddText(_rightPage, "Text", 22f, 16f, 540f, 680f, "", 13f,
                MissionUIFactory.Ink, TextAlignmentOptions.TopLeft, _font);
            _rightText.lineSpacing = 5f;

            _prevButton = MissionUIFactory.AddButton(_book, "Prev", 24f, 768f, 150f, 34f,
                MissionStrings.PrevPage, 13, _font, () => ChangePage(-1), TextAlignmentOptions.Center, _buttonSprite);
            _nextButton = MissionUIFactory.AddButton(_book, "Next", 1066f, 768f, 150f, 34f,
                MissionStrings.NextPage, 13, _font, () => ChangePage(1), TextAlignmentOptions.Center, _buttonSprite);

            _pageLabel = MissionUIFactory.AddText(_book, "PageLabel", 540f, 768f, 160f, 34f, "1/5", 13,
                MissionUIFactory.InkDim, TextAlignmentOptions.Center, _font);
        }

        void BuildPaper(RectTransform page)
        {
            MissionUIFactory.AddImage(page, MissionUIFactory.Paper);

            // top and bottom rules, like a printed page
            var top = MissionUIFactory.Rect(page, "TopRule", 18f, 10f, 548f, 1f);
            MissionUIFactory.AddImage(top, MissionUIFactory.PaperLine);
            var bottom = MissionUIFactory.Rect(page, "BottomRule", 18f, 700f, 548f, 1f);
            MissionUIFactory.AddImage(bottom, MissionUIFactory.PaperLine);
        }

        // ======================================================================
        // input
        // ======================================================================

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            bool terminal = MissionUIState.TerminalOpen;
            if (_hint != null)
            {
                bool want = !terminal && !IsOpen;
                if (_hint.activeSelf != want) _hint.SetActive(want);
            }

            if (terminal) return;

            if (kb.bKey.wasPressedThisFrame)
            {
                Toggle();
                return;
            }

            if (!IsOpen) return;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }

            if (_flipping) return;
            if (kb.leftArrowKey.wasPressedThisFrame) ChangePage(-1);
            if (kb.rightArrowKey.wasPressedThisFrame) ChangePage(1);
        }

        // ======================================================================
        // open / close / pages
        // ======================================================================

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            _page = 0;
            MissionUIState.HandbookOpen = true;
            MissionUIState.SyncInputLock();
            MissionUIFactory.EnsureEventSystem();

            _canvas.gameObject.SetActive(true);
            _root.gameObject.SetActive(true);
            _group.alpha = 1f;
            if (_hint != null) _hint.SetActive(false);

            UIAnimations.Stop(ref _flip);
            _flipping = false;
            UIAnimations.ResetScale(_leftPage);
            UIAnimations.ResetScale(_rightPage);

            RefreshPages();

            UIAnimations.Stop(ref _anim);
            UIAnimations.ResetTransform(_book, _bookHome, _group, 1f);
            _anim = UIAnimations.HandbookOpen(_book, _bookHome, _group, null);
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            MissionUIState.HandbookOpen = false;
            MissionUIState.SyncInputLock();

            UIAnimations.Stop(ref _flip);
            _flipping = false;
            UIAnimations.ResetScale(_leftPage);
            UIAnimations.ResetScale(_rightPage);

            UIAnimations.Stop(ref _anim);
            UIAnimations.ResetTransform(_book, _bookHome, _group, 1f);
            _anim = UIAnimations.HandbookClose(_book, _bookHome, _group, () =>
            {
                if (!IsOpen) _root.gameObject.SetActive(false);
            });
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        void ChangePage(int delta)
        {
            if (!IsOpen || _flipping) return;
            int next = _page + delta;
            if (next < 0 || next > 4) return;

            _flipping = true;
            UIAnimations.Stop(ref _flip);
            UIAnimations.ResetScale(_leftPage);
            UIAnimations.ResetScale(_rightPage);
            int target = next;
            _flip = UIAnimations.PageFlip(_leftPage, _rightPage, () =>
            {
                _page = target;
                RefreshPages();
            }, () => { _flipping = false; });
        }

        // ======================================================================
        // content
        // ======================================================================

        void RefreshPages()
        {
            var def = MissionDefinition.Get(MissionSelection.Key);
            var report = MissionResultStore.Report;
            var objectives = MissionObjectives.Evaluate(report, def);
            var mc = MissionResultStore.MonteCarloResult;

            if (_page == 0)
            {
                _leftText.text = SpreadObjectivesLeft(def, objectives);
                _rightText.text = SpreadObjectivesRight(def, report, mc);
            }
            else if (_page == 1)
            {
                _leftText.text = SpreadGlossaryLeft();
                _rightText.text = SpreadGlossaryRight(report);
            }
            else if (_page == 2)
            {
                _leftText.text = SpreadTips(def, 0, 4);
                _rightText.text = SpreadTips(def, 4, 8);
            }
            else if (_page == 3)
            {
                _leftText.text = SpreadSourcesLeft();
                _rightText.text = SpreadSourcesRight();
            }
            else
            {
                _leftText.text = SIDisclosureText.HandbookLeft();
                _rightText.text = SIDisclosureText.HandbookRight();
            }

            if (_pageLabel != null) _pageLabel.text = (_page + 1) + "/5";

            if (_prevButton != null)
            {
                _prevButton.interactable = _page > 0;
                MissionUIFactory.SetButtonVisual(_prevButton,
                    _page > 0 ? MissionUIFactory.Paper : MissionUIFactory.PaperLine,
                    _page > 0 ? MissionUIFactory.Ink : MissionUIFactory.InkDim);
            }
            if (_nextButton != null)
            {
                _nextButton.interactable = _page < 4;
                MissionUIFactory.SetButtonVisual(_nextButton,
                    _page < 4 ? MissionUIFactory.Paper : MissionUIFactory.PaperLine,
                    _page < 4 ? MissionUIFactory.Ink : MissionUIFactory.InkDim);
            }
        }

        string SpreadObjectivesLeft(MissionDefinition def, ObjectiveStatus[] objectives)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>").Append(def.Name).Append("</b></size>\n");
            sb.Append("<color=").Append(PendingColor).Append(">").Append(def.CodeName)
              .Append("  //  ").Append(def.MissionMonths).Append(" MONTHS ON STATION</color>\n\n");
            sb.Append(Wrap(def.Brief, 64)).Append("\n\n");
            sb.Append("<b><color=#2B2118>OBJECTIVES</color></b>\n");

            if (!def.Playable)
            {
                sb.Append("\n").Append(MissionStrings.DataLinkOffline);
                return sb.ToString();
            }

            for (int i = 0; i < objectives.Length; i++)
            {
                var o = objectives[i];
                string color = o.State == ObjectiveState.Met ? MetColor
                    : (o.State == ObjectiveState.NotMet ? NotMetColor : PendingColor);
                string mark = o.State == ObjectiveState.Met ? "MET"
                    : (o.State == ObjectiveState.NotMet ? "NOT MET" : "PENDING");
                sb.Append("<color=").Append(color).Append(">").Append(mark).Append("</color> ");
                sb.Append("<b>").Append(o.Id).Append("</b> ").Append(o.Title);
                if (o.Points > 0f)
                    sb.Append("<color=").Append(PendingColor).Append(">  (").Append(F(o.Points, "0")).Append(" pts)</color>");
                sb.Append('\n');
                sb.Append("<color=").Append(PendingColor).Append(">    ").Append(o.CurrentValue).Append("</color>\n");
            }
            return sb.ToString();
        }

        string SpreadObjectivesRight(MissionDefinition def, EngineeringReport report, MonteCarloResult mc)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>KEY NUMBERS</b></size>\n\n");
            sb.Append(Line("EARTH-MOON DISTANCE", F(def.EarthDistanceKm, "0,0") + " km"));
            sb.Append(Line("ONE-WAY LIGHT TIME", F(def.LightTimeOneWayS, "0.00") + " s"));
            sb.Append(Line("CHEMICAL TRANSFER", def.TransferWindow));
            sb.Append(Line("ORBIT PERIOD", F(def.OrbitPeriodMin, "0") + " min"));
            sb.Append(Line("LUNAR NIGHT", F(def.LunarNightDays, "0.00") + " Earth days"));
            sb.Append(Line("DELTA-V BUDGET", F(def.DeltaVBudgetKmS, "0.00") + " km/s"));
            sb.Append(Line("BUDGET CAP", F(def.BudgetCapMillions, "0") + " $M"));
            sb.Append(Line("MASS LIMIT", "95% of launch capacity"));
            sb.Append('\n');
            sb.Append("<b>CURRENT DESIGN</b>\n");
            sb.Append(Line("SPACECRAFT", report.Vehicle.Name + " / " + report.Engine.Name));
            sb.Append(Line("WET MASS", F(report.Mass.WetKg, "0") + " / " + F(report.Mass.CapacityKg, "0") + " kg"));
            sb.Append(Line("TOTAL COST", F(report.Cost.TotalMillions, "0.0") + " / " + F(report.Cost.CapMillions, "0") + " $M"));
            sb.Append(Line("LINK MARGIN", F(report.Link.MarginDb, "0.0") + " dB"));
            sb.Append(Line("DATA RETURN", F(report.Data.Fraction * 100.0, "0") + "%"));
            sb.Append(Line("SUCCESS RATE", F(mc.SuccessRate * 100.0, "0.0") + "%"));
            sb.Append('\n');
            sb.Append("<color=").Append(PendingColor)
              .Append(">The BRIEFING tab in the terminal shows the same live table.</color>");
            return sb.ToString();
        }

        string SpreadGlossaryLeft()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>HOW IT WORKS</b></size>\n\n");
            for (int i = 0; i < 5 && i < MissionStrings.Glossary.Length; i++)
                sb.Append(GlossaryLine(MissionStrings.Glossary[i])).Append('\n');
            return sb.ToString();
        }

        string SpreadGlossaryRight(EngineeringReport report)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>HOW IT WORKS</b></size>\n\n");
            for (int i = 5; i < MissionStrings.Glossary.Length; i++)
                sb.Append(GlossaryLine(MissionStrings.Glossary[i])).Append('\n');

            sb.Append("\n<b>YOUR MISSION TODAY</b>\n");
            sb.Append("The orbiter is captured into a 100 km polar orbit, so it sees every latitude " +
                      "including the poles where water ice hides.\n\n");
            sb.Append("<color=").Append(PendingColor).Append(">Orbit period: ")
              .Append(F(MissionConstants.OrbitPeriodMinutes(), "0"))
              .Append(" min. Every orbit spends about 40% of its time in eclipse.</color>");
            return sb.ToString();
        }

        string SpreadTips(MissionDefinition def, int from, int to)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>ENGINEER'S TIPS</b></size>\n\n");
            var tips = def.Tips;
            for (int i = from; i < to && i < tips.Length; i++)
            {
                sb.Append("<color=").Append(PendingColor).Append(">").Append(i + 1).Append(".</color> ")
                  .Append(Wrap(tips[i], 58)).Append("\n\n");
            }
            if (to >= tips.Length)
            {
                sb.Append("<color=").Append(PendingColor)
                  .Append(">Trade-offs are the whole game: every kilogram and every watt is a choice.</color>");
            }
            return sb.ToString();
        }

        string SpreadSourcesLeft()
        {
            var source = MissionSources.Moon[0];
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>DATA SOURCES</b></size>\n\n");
            sb.Append("<b>").Append(source.Id).Append(". ").Append(source.Citation).Append("</b>\n");
            sb.Append("<color=").Append(PendingColor).Append(">").Append(source.Url).Append("</color>\n\n");
            sb.Append("<b>Supplies</b>\n").Append(Wrap(source.Supplies, 56)).Append("\n\n");
            sb.Append("<color=").Append(PendingColor)
              .Append(">Constants in code carry src ").Append(source.Id)
              .Append(". Values marked game are design assumptions, not NASA data.</color>");
            return sb.ToString();
        }

        string SpreadSourcesRight()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>HOW TO READ THIS PAGE</b></size>\n\n");
            sb.Append(Wrap("Every NASA number used by this mission points to the reference number on the left. " +
                           "The values do not change with the design; only the game assumptions around them do.", 56));
            sb.Append("\n\n<b>CREDIT</b>\n");
            sb.Append(Wrap(MissionSources.Credit, 56)).Append("\n\n");
            sb.Append("<color=").Append(PendingColor).Append(">")
              .Append(Wrap(MissionSources.VerifyNote, 56)).Append("</color>");
            return sb.ToString();
        }

        // ---- text helpers -----------------------------------------------------

        static string Line(string key, string value)
        {
            return "<color=" + PendingColor + ">" + key + "</color>\n  <b>" + value + "</b>\n";
        }

        static string GlossaryLine(string entry)
        {
            int colon = entry.IndexOf(':');
            if (colon > 0)
            {
                string term = entry.Substring(0, colon);
                string body = entry.Substring(colon + 1).Trim();
                return "<b>" + term + "</b>\n<color=" + PendingColor + ">" + Wrap(body, 60) + "</color>\n";
            }
            return Wrap(entry, 60) + "\n";
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

        static string F(double value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }
    }
}
