// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MissionArchitect
{
    public sealed partial class MarsTerminalUI
    {
        // ======================================================================
        // 6 REVIEW
        // ======================================================================

        GameObject BuildReviewPage()
        {
            RectTransform p;
            var page = NewPage("PageReview", out p);
            PageTitle(p, MarsStrings.ReviewTitle);

            var left = MissionUIFactory.PanelBox(p, "ReviewLeft", 0, 44, 770, 780,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(left.rectTransform, "L", 14, 10, 600, 20,
                MarsStrings.ReadinessTitle, 12, MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);

            string[] items =
            {
                "LAUNCH WINDOW (C3)",
                "LAUNCH MASS (95%)",
                "ENTRY, DESCENT, LANDING",
                "SURFACE POWER",
                "DATA RETURN",
                "BUDGET (2,700 $M)"
            };
            for (int i = 0; i < items.Length; i++)
            {
                float y = 42f + i * 28f;
                MissionUIFactory.AddText(left.rectTransform, "R" + i, 14, y, 420, 22, items[i], 11.5f,
                    MissionUIFactory.TextMain, TextAlignmentOptions.MidlineLeft, _font);
                _readiness[i] = MissionUIFactory.AddText(left.rectTransform, "RS" + i, 520, y, 230, 22,
                    "", 11.5f, MissionUIFactory.Green, TextAlignmentOptions.MidlineRight, _font);
            }

            _reviewPhases = MissionUIFactory.AddText(left.rectTransform, "Phases", 14, 196, 742, 22, "", 10.5f,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            var mc = MissionUIFactory.PanelBox(left.rectTransform, "MC", 14, 226, 742, 180,
                MissionUIFactory.PanelAlt, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(mc.rectTransform, "L", 12, 8, 500, 18,
                MarsStrings.MonteCarloTitle, 11, MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _reviewBig = MissionUIFactory.AddText(mc.rectTransform, "Big", 12, 28, 700, 64, "0%", 40,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
            _reviewSub = MissionUIFactory.AddText(mc.rectTransform, "Sub", 12, 96, 716, 74, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _reviewSub.lineSpacing = 4f;

            var tips = MissionUIFactory.PanelBox(left.rectTransform, "Tips", 14, 418, 742, 348,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(tips.rectTransform, "L", 12, 8, 500, 18, "DESIGN TIPS", 11,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _reviewTips = MissionUIFactory.AddText(tips.rectTransform, "Text", 12, 32, 716, 304, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _reviewTips.lineSpacing = 6f;

            var right = MissionUIFactory.PanelBox(p, "ReviewRight", 794, 44, 770, 780,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);

            _beforeLaunch = MissionUIFactory.Rect(right.rectTransform, "BeforeLaunch", 0, 0, 770, 780).gameObject;
            MissionUIFactory.AddText((RectTransform)_beforeLaunch.transform, "Text", 20, 20, 730, 120,
                "Launch is allowed even with warnings.\nThe checklist is advice, not a lock.",
                13, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);

            _launchButton = MissionUIFactory.AddButton(right.rectTransform, "Launch", 20, 620, 730, 90,
                MarsStrings.LaunchButton, 34, _font, OnLaunch, TextAlignmentOptions.Center, _buttonSprite);
            MissionUIFactory.SetButtonVisual(_launchButton, MissionUIFactory.Selected, MissionUIFactory.Green);

            _debriefGroup = MissionUIFactory.Rect(right.rectTransform, "Debrief", 0, 0, 770, 780).gameObject;
            _debriefGroup.SetActive(false);

            _debriefRating = MissionUIFactory.AddText((RectTransform)_debriefGroup.transform, "Rating",
                20, 16, 730, 48, "", 30, MissionUIFactory.Green, TextAlignmentOptions.TopLeft, _font);
            _debriefBody = MissionUIFactory.AddText((RectTransform)_debriefGroup.transform, "Body",
                20, 72, 730, 470, "", 11.5f, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _debriefBody.lineSpacing = 5f;

            MissionUIFactory.AddButton((RectTransform)_debriefGroup.transform, "Redesign",
                20, 620, 350, 60, MarsStrings.Redesign, 17, _font, OnRedesign,
                TextAlignmentOptions.Center, _buttonSprite);
            MissionUIFactory.AddButton((RectTransform)_debriefGroup.transform, "CloseTerminal",
                400, 620, 350, 60, MarsStrings.CloseTerminal, 17, _font, Close,
                TextAlignmentOptions.Center, _buttonSprite);

            return page;
        }

        void RefreshReview()
        {
            var r = MarsResultStore.Report;
            var mc = MarsResultStore.MonteCarloResult;

            bool[] pass =
            {
                r.Transfer.C3Km2S2 <= 12.0,
                r.Mass.Pass,
                r.Edl.Pass,
                r.Power.Pass,
                r.Data.Pass,
                r.Cost.Pass
            };
            string[] labels =
            {
                pass[0] ? MarsStrings.StatusPass : MarsStrings.StatusWarn,
                pass[1] ? MarsStrings.StatusPass : MarsStrings.StatusFail,
                pass[2] ? MarsStrings.StatusPass : MarsStrings.StatusFail,
                pass[3] ? MarsStrings.StatusPass : MarsStrings.StatusFail,
                pass[4] ? MarsStrings.StatusPass : MarsStrings.StatusFail,
                pass[5] ? MarsStrings.StatusPass : MarsStrings.StatusFail
            };

            for (int i = 0; i < _readiness.Length; i++)
            {
                _readiness[i].text = labels[i];
                _readiness[i].color = pass[i] ? MissionUIFactory.Green
                    : (labels[i] == MarsStrings.StatusWarn ? MissionUIFactory.Amber : MissionUIFactory.Red);
            }

            bool launchPhase = r.Mass.Pass && r.Transfer.C3Km2S2 <= 14.0;
            bool cruisePhase = r.Reliability.CruiseSuccess > 0.90;
            bool surfacePhase = r.Power.Pass && r.Data.Pass && r.ExpectedSamples >= MarsConstants.SampleTarget;
            bool surfaceWarn = !surfacePhase && r.Power.Pass && r.Data.Pass;
            _reviewPhases.text = "PHASES   " + PhaseChip("LAUNCH", launchPhase) +
                                 PhaseChip("CRUISE", cruisePhase) +
                                 PhaseChip("EDL", r.Edl.Pass) +
                                 PhaseChip("SURFACE", surfacePhase, surfaceWarn);

            _reviewBig.text = Pct(mc.SuccessRate, "0.0");
            _reviewBig.color = mc.SuccessRate >= 0.65f ? MissionUIFactory.Green
                : (mc.SuccessRate >= 0.40f ? MissionUIFactory.Amber : MissionUIFactory.Red);
            _reviewSub.text =
                "FULL " + Pct(mc.FullProbability, "0.0") + "   //   PARTIAL " +
                Pct(mc.PartialProbability, "0.0") + "   //   FAIL " + Pct(mc.FailProbability, "0.0") + "\n" +
                "EXPECTED SCORE " + F(mc.ExpectedPoints, "0") + " PTS   //   DESIGN PTS " +
                F(r.AchievablePoints, "0");

            var tips = MarsObjectives.AutoTips(r);
            var sb = new StringBuilder();
            for (int i = 0; i < tips.Count && i < 5; i++)
            {
                if (i > 0) sb.Append('\n');
                sb.Append("- ").Append(tips[i]);
            }
            _reviewTips.text = sb.ToString();

            var timeline = MarsResultStore.LastTimeline;
            bool showDebrief = _debrief && timeline != null;
            MissionUIFactory.SetActive(_debriefGroup, showDebrief);
            MissionUIFactory.SetActive(_beforeLaunch, !showDebrief);
            MissionUIFactory.SetActive(_launchButton.gameObject, !showDebrief);

            if (!showDebrief) return;

            _debriefRating.text = MarsStrings.Rating(timeline.Rating);
            _debriefRating.color = timeline.Rating == 2 ? MissionUIFactory.Green
                : (timeline.Rating == 1 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            var def = r.Definition;
            var objectives = def.Objectives;
            var body = new StringBuilder();
            body.Append("MISSION LEVELS\n");

            bool[] levels = { timeline.Level1, timeline.Level2, timeline.Level3, timeline.Level4, r.BudgetPass };
            for (int i = 0; i < objectives.Length && i < levels.Length; i++)
            {
                float pts = timeline.ObjectivePoints != null && i < timeline.ObjectivePoints.Length
                    ? timeline.ObjectivePoints[i] : 0f;
                body.Append(levels[i] ? "<color=#7DFFB2>" : "<color=#FF6B6B>")
                    .Append(levels[i] ? "MET    " : "NOT MET ")
                    .Append("</color>")
                    .Append(objectives[i].Id).Append("  ").Append(objectives[i].Title)
                    .Append("   ").Append(F(pts, "0")).Append(" pts\n");
            }

            body.Append("\nTOTAL ").Append(F(timeline.Points, "0")).Append(" PTS   //   ")
                .Append(timeline.Samples).Append(" samples cached   //   ")
                .Append(Pct(timeline.DataFraction, "0")).Append(" data returned\n");

            if (!timeline.Landed)
                body.Append("\nFAILURE: ").Append(timeline.FailureReason ?? "landing failed").Append('\n');
            else if (!timeline.Alive)
                body.Append("\nSURFACE FAILURE: ").Append(timeline.FailureReason ?? "rover lost").Append('\n');

            body.Append("\nWEAKEST CONSTRAINT TIPS\n");
            for (int i = 0; i < timeline.Tips.Count && i < 4; i++)
                body.Append("- ").Append(timeline.Tips[i]).Append('\n');

            _debriefBody.text = body.ToString();
        }

        // ---- launch sequence --------------------------------------------------

        void OnLaunch()
        {
            if (_launching) return;

            var timeline = MarsResultStore.Launch();
            if (timeline == null) return;

            _launching = true;
            _skipLaunch = false;
            _launchLines.Clear();

            if (_launchLog != null) _launchLog.text = string.Empty;
            if (_launchPhase != null)
            {
                _launchPhase.text = "PRE-LAUNCH // " + MarsResultStore.Report.Vehicle.Name.ToUpperInvariant();
                _launchPhase.color = MissionUIFactory.Amber;
            }
            if (_launchMonth != null) _launchMonth.text = "CRUISE";
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

            MissionLog.Add("MARS LAUNCH COMMAND ACCEPTED // " + MarsResultStore.Report.Vehicle.Name.ToUpperInvariant());
            UIAnimations.Run(LaunchSequenceRoutine(timeline));
        }

        IEnumerator LaunchSequenceRoutine(MarsTimeline timeline)
        {
            AppendLaunchLine("<color=#4E8A66>MARS LAUNCH CONTROL // " + MarsResultStore.Report.Vehicle.Name +
                             " // " + F(MarsResultStore.Report.Mass.LaunchKg, "0") + " kg</color>");
            yield return LaunchWait(0.30f);
            if (this == null) yield break;

            for (int i = 0; i < timeline.Events.Count; i++)
            {
                if (this == null) yield break;

                if (_skipLaunch)
                {
                    for (int j = i; j < timeline.Events.Count; j++) AppendMarsEvent(timeline.Events[j]);
                    break;
                }

                AppendMarsEvent(timeline.Events[i]);
                yield return LaunchWait(timeline.Events[i].Sol > 0 ? 0.22f : 0.55f);
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

        void AppendMarsEvent(MarsTimelineEvent e)
        {
            string color = e.Alarm ? "#FF6B6B" : "#7DFFB2";
            AppendLaunchLine("<color=" + color + ">" + e.Phase + "</color>  " + e.Text);
            MissionLog.Add(e.Phase + ": " + e.Text);

            if (e.Sol > 0)
            {
                int sol = Mathf.Clamp(e.Sol, 0, 668);
                if (_launchMonth != null) _launchMonth.text = "SOL " + sol.ToString("000") + " / 668";
                if (_launchBar != null)
                    MissionUIFactory.SetBar(_launchBar, 0.15f + (sol / 668f) * 0.85f,
                        e.Alarm ? MissionUIFactory.Amber : MissionUIFactory.Green);
            }
            else
            {
                if (_launchMonth != null) _launchMonth.text = e.Phase.ToUpperInvariant();
                if (_launchBar != null)
                    MissionUIFactory.SetBar(_launchBar, PhaseProgress(e.Phase),
                        e.Alarm ? MissionUIFactory.Amber : MissionUIFactory.Green);
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

        void EndLaunchSequence(MarsTimeline timeline)
        {
            _launching = false;
            _skipLaunch = false;
            _debrief = true;

            bool complete = timeline.Landed && timeline.Alive;
            float progress = complete ? 1f : (timeline.Landed ? 0.6f : 0f);
            Color endColor = complete ? MissionUIFactory.Green
                : (timeline.Landed ? MissionUIFactory.Amber : MissionUIFactory.Red);

            if (_launchMonth != null)
                _launchMonth.text = complete ? "SURFACE OPERATIONS COMPLETE"
                    : (timeline.Landed ? "SURFACE OPERATIONS ENDED EARLY" : "MISSION LOST");
            if (_launchBar != null)
                MissionUIFactory.SetBar(_launchBar, progress, endColor);
            if (_launchPhase != null)
            {
                _launchPhase.text = complete ? "MISSION COMPLETE // DEBRIEF READY"
                    : (timeline.Landed ? "PARTIAL MISSION // DEBRIEF READY" : "MISSION LOST");
                _launchPhase.color = endColor;
            }
            if (_launchStatus != null)
            {
                _launchStatus.text = complete
                    ? "LEVELS REACHED // " + F(timeline.Points, "0") + " PTS // " + timeline.Samples + " SAMPLES"
                    : "ABORT // " + (timeline.FailureReason ?? "unknown failure");
                _launchStatus.color = endColor;
            }

            MissionLog.Add("DEBRIEF: " + MarsStrings.Rating(timeline.Rating) + " - " +
                           F(timeline.Points, "0") + " PTS");

            if (_launchOverlay != null) _launchOverlay.SetActive(false);
            RefreshAll();
        }

        void OnRedesign()
        {
            MarsResultStore.ResetAll();
            _mcVersion = -1;
            _debrief = false;
            _launching = false;
            _skipLaunch = false;
            if (_launchOverlay != null) _launchOverlay.SetActive(false);
            MissionLog.Add("REDESIGN: Mars defaults restored.");
            RefreshAll();
        }

        static string PhaseChip(string name, bool pass, bool warn = false)
        {
            string color = pass ? "#7DFFB2" : (warn ? "#FFC857" : "#FF6B6B");
            string state = pass ? "PASS" : (warn ? "WARN" : "FAIL");
            return "<color=" + color + ">" + name + " " + state + "</color>   ";
        }

        static float PhaseProgress(string phase)
        {
            if (string.IsNullOrEmpty(phase)) return 0.02f;
            if (phase.StartsWith("WINDOW")) return 0.02f;
            if (phase.StartsWith("LAUNCH")) return 0.05f;
            if (phase.StartsWith("CRUISE")) return 0.10f;
            if (phase.StartsWith("EDL")) return 0.14f;
            if (phase.StartsWith("TOUCHDOWN")) return 0.15f;
            return 0.15f;
        }
    }
}
