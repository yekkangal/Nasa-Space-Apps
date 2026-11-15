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
    /// Asteroid handbook on the B key: live S1-S5 objectives, glossary, tips and NASA sources.
    /// Same book look and animations as the Moon/Mars handbooks.
    /// </summary>
    public sealed class AsteroidHandbookUI : MonoBehaviour
    {
        public static AsteroidHandbookUI Instance { get; private set; }
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

        int _page;
        Coroutine _anim;
        Coroutine _flip;
        bool _flipping;

        public static AsteroidHandbookUI Create(Transform parent, TMP_FontAsset font, Sprite panelSprite, Sprite buttonSprite)
        {
            if (Instance != null) return Instance;

            var go = new GameObject("AsteroidHandbookUI");
            if (parent != null) go.transform.SetParent(parent, false);

            var ui = go.AddComponent<AsteroidHandbookUI>();
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

            _canvas = MissionUIFactory.CreateCanvas("AsteroidHandbookCanvas", 150, transform);
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
            MissionUIFactory.AddText(hintPanel.rectTransform, "T", 10, 4, 184, 32,
                "B  HANDBOOK", 13, MissionUIFactory.Green, TextAlignmentOptions.Midline, _font);
        }

        void BuildBook()
        {
            _book = MissionUIFactory.Rect(_root, "Book", _bookHome.x, -_bookHome.y, 1240f, 820f);
            MissionUIFactory.AddImage(_book, MissionUIFactory.Cover);
            var edge = MissionUIFactory.Stretch(_book, "Edge", 4, 4, 4, 4);
            var edgeImg = MissionUIFactory.AddImage(edge, MissionUIFactory.CoverEdge);
            edgeImg.raycastTarget = false;
            var inside = MissionUIFactory.Stretch(_book, "Inside", 8, 8, 8, 8);
            MissionUIFactory.AddImage(inside, MissionUIFactory.Cover);

            MissionUIFactory.AddText(_book, "BookTitle", 26, 12, 1100, 24,
                "ASTEROID HANDBOOK   //   SAMPLE RETURN", 13,
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
                "PREV", 13, _font, () => ChangePage(-1), TextAlignmentOptions.Center, _buttonSprite);
            _nextButton = MissionUIFactory.AddButton(_book, "Next", 1066f, 768f, 150f, 34f,
                "NEXT", 13, _font, () => ChangePage(1), TextAlignmentOptions.Center, _buttonSprite);

            _pageLabel = MissionUIFactory.AddText(_book, "PageLabel", 540f, 768f, 160f, 34f, "1/5", 13,
                MissionUIFactory.InkDim, TextAlignmentOptions.Center, _font);
        }

        void BuildPaper(RectTransform page)
        {
            MissionUIFactory.AddImage(page, MissionUIFactory.Paper);
            var top = MissionUIFactory.Rect(page, "TopRule", 18f, 10f, 548f, 1f);
            MissionUIFactory.AddImage(top, MissionUIFactory.PaperLine);
            var bottom = MissionUIFactory.Rect(page, "BottomRule", 18f, 700f, 548f, 1f);
            MissionUIFactory.AddImage(bottom, MissionUIFactory.PaperLine);
        }

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
            var report = AsteroidResultStore.Report;
            var objectives = AsteroidObjectives.Evaluate(report, report.Definition);

            if (_page == 0)
            {
                _leftText.text = SpreadObjectives(report, objectives);
                _rightText.text = SpreadNumbers(report);
            }
            else if (_page == 1)
            {
                _leftText.text = SpreadGlossary(0, 5);
                _rightText.text = SpreadGlossary(5, 10);
            }
            else if (_page == 2)
            {
                _leftText.text = SpreadTips(report, 0, 4);
                _rightText.text = SpreadTips(report, 4, 8);
            }
            else if (_page == 3)
            {
                _leftText.text = SpreadSources(0, 3, false);
                _rightText.text = SpreadSources(3, 6, true);
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

        string SpreadObjectives(AsteroidReport report, AsteroidObjectiveStatus[] objectives)
        {
            var def = report.Definition;
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>").Append(def.Name).Append("</b></size>\n");
            sb.Append("<color=").Append(PendingColor).Append(">").Append(def.CodeName)
              .Append("  //  60 g MINIMUM SAMPLE</color>\n\n");
            sb.Append(Wrap(def.Brief, 62)).Append("\n\n");
            sb.Append("<b>MISSION LEVELS</b>\n");

            for (int i = 0; i < objectives.Length; i++)
            {
                var o = objectives[i];
                string color = o.State == AsteroidObjectiveState.Met ? MetColor
                    : (o.State == AsteroidObjectiveState.NotMet ? NotMetColor : PendingColor);
                string mark = o.State == AsteroidObjectiveState.Met ? "MET"
                    : (o.State == AsteroidObjectiveState.NotMet ? "NOT MET" : "PENDING");
                sb.Append("<color=").Append(color).Append(">").Append(mark).Append("</color> ");
                sb.Append("<b>").Append(o.Id).Append("</b> ").Append(o.Title);
                if (o.Points > 0f)
                    sb.Append("<color=").Append(PendingColor).Append(">  (").Append(F(o.Points, "0")).Append(" pts)</color>");
                sb.Append('\n');
                sb.Append("<color=").Append(PendingColor).Append(">    ").Append(o.CurrentValue).Append("</color>\n");
            }
            return sb.ToString();
        }

        string SpreadNumbers(AsteroidReport report)
        {
            var mc = AsteroidResultStore.MonteCarloResult;
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>KEY NUMBERS</b></size>\n\n");
            sb.Append(Line("TARGET", report.Target.Name));
            sb.Append(Line("DIAMETER", F(report.Target.DiameterM, "0") + " m"));
            sb.Append(Line("ROTATION", F(report.Target.RotationHours, "0.0") + " h"));
            sb.Append(Line("ESCAPE VELOCITY", F(report.Target.EscapeVelocityMps, "0.000") + " m/s"));
            sb.Append(Line("MEAN DISTANCE", F(report.Target.MeanAu, "0.00") + " AU"));
            sb.Append(Line("LIGHT TIME", F(report.Target.LightTimeMinutes, "0.0") + " min"));
            sb.Append(Line("SAMPLE TARGET", F(AsteroidConstants.SampleRequirementG, "0") + " g"));
            sb.Append(Line("RETURNED SAMPLE", F(AsteroidConstants.ReturnedSampleG, "0.0") + " g"));
            sb.Append(Line("CAPSULE", F(AsteroidConstants.ReferenceCapsuleKg, "0") + " kg"));
            sb.Append(Line("SAMPLING ARM", F(AsteroidConstants.ReferenceArmM, "2") + " m"));
            sb.Append(Line("BUDGET CAP", F(report.Cost.CapM, "0") + " $M"));
            sb.Append('\n');
            sb.Append("<b>CURRENT DESIGN</b>\n");
            sb.Append(Line("TRAJECTORY", report.Trajectory.TypeName));
            sb.Append(Line("WET MASS", F(report.Mass.WetKg, "0") + " / " + F(report.Mass.LimitKg, "0") + " kg"));
            sb.Append(Line("TAG", report.ExpectedGrams + " g expected"));
            sb.Append(Line("CAPSULE", Pct(report.Capsule.SurvivalProbability, "0")));
            sb.Append(Line("COST", F(report.Cost.TotalM, "0") + " / " + F(report.Cost.CapM, "0") + " $M"));
            sb.Append(Line("SUCCESS RATE", Pct(mc.SuccessRate, "0.0")));
            return sb.ToString();
        }

        string SpreadGlossary(int from, int to)
        {
            var glossary = new AsteroidDefinition().Glossary;
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>HOW IT WORKS</b></size>\n\n");
            for (int i = from; i < to && i < glossary.Length; i++)
                sb.Append(GlossaryLine(glossary[i])).Append('\n');
            return sb.ToString();
        }

        string SpreadTips(AsteroidReport report, int from, int to)
        {
            var tips = report.Definition.Tips;
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>ENGINEER'S TIPS</b></size>\n\n");
            for (int i = from; i < to && i < tips.Length; i++)
                sb.Append("<color=").Append(PendingColor).Append(">").Append(i + 1).Append(".</color> ")
                  .Append(Wrap(tips[i], 56)).Append("\n\n");

            if (to >= tips.Length)
            {
                sb.Append("<color=").Append(PendingColor)
                  .Append(">Balance target physics, TAG, capsule and budget - FULL needs all of them.</color>");
            }
            return sb.ToString();
        }

        string SpreadSources(int from, int to, bool withFooter)
        {
            var refs = AsteroidSources.All;
            var sb = new System.Text.StringBuilder();
            sb.Append("<size=40><b>DATA SOURCES</b></size>\n\n");
            for (int i = from; i < to && i < refs.Length; i++)
            {
                var s = refs[i];
                sb.Append("<b>").Append(s.Id).Append(". ").Append(Wrap(s.Citation, 52)).Append("</b>\n");
                sb.Append("<color=").Append(PendingColor).Append(">").Append(WrapUrl(s.Url, 56)).Append("</color>\n");
                sb.Append(Wrap(s.Supplies, 56)).Append("\n\n");
            }

            if (withFooter)
            {
                sb.Append("<b>CREDIT</b>\n").Append(Wrap(AsteroidSources.Credit, 54)).Append("\n\n");
                sb.Append("<color=").Append(PendingColor).Append(">")
                  .Append(Wrap(AsteroidSources.VerifyNote, 54)).Append("</color>");
            }
            return sb.ToString();
        }

        // ---- helpers ----------------------------------------------------------

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
                return "<b>" + term + "</b>\n<color=" + PendingColor + ">" + Wrap(body, 58) + "</color>\n";
            }
            return Wrap(entry, 58) + "\n";
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

        static string WrapUrl(string url, int width)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            var sb = new System.Text.StringBuilder();
            int i = 0;
            while (i < url.Length)
            {
                int n = Math.Min(width, url.Length - i);
                sb.Append(url, i, n);
                i += n;
                if (i < url.Length) sb.Append('\n');
            }
            return sb.ToString();
        }

        static string F(double value, string format)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        static string Pct(double value, string format)
        {
            return (value * 100.0).ToString(format, CultureInfo.InvariantCulture) + "%";
        }
    }
}
