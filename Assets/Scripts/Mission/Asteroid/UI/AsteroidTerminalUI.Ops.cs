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
    public sealed partial class AsteroidTerminalUI
    {
        Slider _orbitSlider, _campaignSlider, _contactSlider;
        TMP_Text _orbitLabel, _campaignLabel, _orbitInfo, _tagInfo, _proxData;
        Image _stabilityBar;

        TMP_Text _reviewBig, _reviewSub, _reviewTips;
        readonly TMP_Text[] _readiness = new TMP_Text[7];
        GameObject _beforeLaunch, _debriefGroup;
        Button _launchButton;
        TMP_Text _debriefRating, _debriefBody;
        bool _debrief;

        GameObject _launchOverlay;
        TMP_Text _launchLog, _launchPhase, _launchMonth, _launchStatus;
        Image _launchBar;
        bool _launching, _skipLaunch;
        readonly System.Collections.Generic.List<string> _launchLines =
            new System.Collections.Generic.List<string>();

        // ======================================================================
        // 4 PROX-OPS
        // ======================================================================

        GameObject BuildProxPage()
        {
            RectTransform p;
            var page = NewPage("PageProx", out p);
            PageTitle(p, AsteroidStrings.ProxTitle);

            AddSectionHeader(p, 0, 44, 780, AsteroidStrings.SectionOrbit);
            _orbitSlider = MarsUIWidgets.AddSlider(p, "OrbitSlider", 0, 70, 760, 30, 100, 10000, false, OnOrbitSlider);
            _orbitLabel = MissionUIFactory.AddText(p, "OrbitLabel", 0, 104, 760, 24, "", 11.5f,
                MissionUIFactory.Amber, TextAlignmentOptions.MidlineLeft, _font);

            AddSectionHeader(p, 0, 140, 780, AsteroidStrings.SectionCampaign);
            _campaignSlider = MarsUIWidgets.AddSlider(p, "CampaignSlider", 0, 166, 760, 30, 0.25f, 3f, false,
                OnCampaignSlider);
            _campaignLabel = MissionUIFactory.AddText(p, "CampaignLabel", 0, 200, 760, 22, "", 11,
                MissionUIFactory.Amber, TextAlignmentOptions.MidlineLeft, _font);

            AddSectionHeader(p, 0, 226, 780, AsteroidStrings.SectionSite);
            for (int i = 0; i < AsteroidCatalog.Sites.Length; i++)
                AddChoice(p, 0, 250 + i * 30, 780, "site", AsteroidCatalog.Sites[i].Id, i,
                    AsteroidCatalog.Sites[i].Name);

            AddSectionHeader(p, 0, 346, 780, AsteroidStrings.SectionTag);
            _contactSlider = MarsUIWidgets.AddSlider(p, "ContactSlider", 0, 372, 760, 30, 2, 40, true, OnContactSlider);
            AddChoice(p, 0, 410, 780, "nav", "nav", 0, "OPTICAL NAVIGATION");
            AddChoice(p, 0, 446, 780, "attempts", "1", 1, "1 attempt");
            AddChoice(p, 0, 476, 780, "attempts", "2", 2, "2 attempts");
            AddChoice(p, 0, 506, 780, "attempts", "3", 3, "3 attempts");

            var orbit = MissionUIFactory.PanelBox(p, "OrbitInfo", 800, 44, 782, 230,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(orbit.rectTransform, "L", 14, 10, 500, 20, "ORBIT PHYSICS", 11.5f,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _orbitInfo = MissionUIFactory.AddText(orbit.rectTransform, "Text", 14, 36, 754, 150, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _orbitInfo.lineSpacing = 5f;
            _stabilityBar = MissionUIFactory.Bar(orbit.rectTransform, "StabilityBar", 14, 192, 754, 22);

            var tag = MissionUIFactory.PanelBox(p, "TagInfo", 800, 288, 782, 260,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(tag.rectTransform, "L", 14, 10, 500, 20, "TAG OUTCOME", 11.5f,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _tagInfo = MissionUIFactory.AddText(tag.rectTransform, "Text", 14, 36, 754, 210, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _tagInfo.lineSpacing = 5f;

            var data = MissionUIFactory.PanelBox(p, "ProxData", 800, 562, 782, 180,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(data.rectTransform, "L", 14, 10, 500, 20, AsteroidStrings.SectionData, 11.5f,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _proxData = MissionUIFactory.AddText(data.rectTransform, "Text", 14, 36, 754, 130, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _proxData.lineSpacing = 5f;

            return page;
        }

        void RefreshProx()
        {
            var d = AsteroidResultStore.Design;
            var r = AsteroidResultStore.Report;

            double minRadius = r.Target.RadiusM * 1.2;
            double maxRadius = r.Target.RadiusM * 12.0;
            _updating = true;
            _orbitSlider.minValue = (float)minRadius;
            _orbitSlider.maxValue = (float)maxRadius;

            MarsUIWidgets.SetSlider(_orbitSlider, (float)r.Target.OrbitRadiusM);
            MarsUIWidgets.SetSlider(_campaignSlider, (float)d.CampaignYears);
            MarsUIWidgets.SetSlider(_contactSlider, (float)d.ContactSpeedCmS);
            _updating = false;

            _orbitLabel.text = "ORBIT RADIUS  " + F(r.Target.OrbitRadiusM, "0") + " m   //   RANGE " +
                               F(minRadius, "0") + "-" + F(maxRadius, "0") + " m   //   " +
                               (r.Target.OrbitPass ? "STABLE" : "UNSTABLE");
            _campaignLabel.text = "SURVEY LENGTH  " + F(d.CampaignYears, "2") + " years   (" +
                                  F(d.CampaignYears * 365.25, "0") + " days)   //   PROX OPS " +
                                  F(r.Trajectory.ProxOpsKmS, "2") + " km/s";

            for (int i = 0; i < _choices.Count; i++)
            {
                var c = _choices[i];
                bool sel = false;
                string label = null;

                switch (c.Group)
                {
                    case "site":
                        {
                            var s = AsteroidCatalog.Sites[c.Index];
                            sel = d.SiteId == c.Value;
                            label = s.Name + "   hazard x" + F(s.Hazard, "2") + "   sample x" +
                                    F(s.SampleFactor, "2");
                            break;
                        }
                    case "nav":
                        sel = d.Navigation;
                        label = (sel ? "ON   " : "OFF  ") + "Optical natural-feature navigation";
                        break;
                    case "attempts":
                        sel = d.Attempts == c.Index;
                        label = c.Index + " attempt" + (c.Index > 1 ? "s" : "") + "   +" +
                                F(AsteroidConstants.TagManeuverKmS * c.Index, "2") + " km/s";
                        break;
                }

                if (label != null)
                {
                    var labelText = MissionUIFactory.ButtonLabel(c.Button);
                    if (labelText != null) labelText.text = label;
                }
                MissionUIFactory.SetButtonVisual(c.Button,
                    sel ? MissionUIFactory.Selected : MissionUIFactory.PanelAlt,
                    sel ? MissionUIFactory.Green : MissionUIFactory.TextMain);
            }

            _orbitInfo.text =
                "RADIUS            " + F(r.Target.OrbitRadiusM, "0") + " m\n" +
                "PERIOD            " + F(r.Target.OrbitPeriodHours, "1.0") + " h\n" +
                "CIRCULAR SPEED    " + F(r.Target.CircularSpeedMps, "0.000") + " m/s\n" +
                "ESCAPE VELOCITY   " + F(r.Target.EscapeVelocityMps, "0.000") + " m/s\n" +
                "SURFACE GRAVITY   " + Sci(r.Target.SurfaceGravityMps2, "0.00000") + " m/s2\n" +
                "SRP ACCEL         " + Sci(r.Target.SrpAccelMps2, "0.00E+00") + " m/s2\n" +
                "GRAVITY ACCEL     " + Sci(r.Target.GravityAccelMps2, "0.00E+00") + " m/s2\n" +
                "SRP / GRAVITY     " + Pct(r.Target.SrpRatio, "0.00") + "  (limit " +
                Pct(AsteroidConstants.StabilityLimit, "0") + ")\n" +
                "STABILITY         " + Pct(r.Target.Stability, "0") + "   " +
                (r.Target.OrbitPass ? AsteroidStrings.StatusPass : AsteroidStrings.StatusFail);

            MissionUIFactory.SetBar(_stabilityBar, (float)r.Target.Stability,
                r.Target.OrbitPass ? MissionUIFactory.Green : MissionUIFactory.Red);

            _tagInfo.text =
                "SAMPLER           " + r.Sampler.Name + "\n" +
                "SITE              " + r.Site.Name + "  (hazard x" + F(r.Site.Hazard, "2") + ")\n" +
                "CONTACT SPEED     " + F(d.ContactSpeedCmS, "0") + " cm/s   (optimum " +
                F(AsteroidConstants.TagOptimumSpeedCmS, "0") + ")\n" +
                "NAVIGATION        " + (d.Navigation ? "ON" : "OFF") + "\n" +
                "ATTEMPTS          " + r.Tag.Attempts + "\n" +
                "SUCCESS / ATTEMPT " + Pct(r.Tag.SuccessProbability, "0") + "\n" +
                "EXPECTED / ATTEMPT " + F(r.Tag.PerAttemptGrams, "0.0") + " g\n" +
                "EXPECTED TOTAL    " + F(r.Tag.ExpectedGrams, "0.0") + " g   " +
                (r.Tag.Pass ? AsteroidStrings.StatusPass : AsteroidStrings.StatusFail);

            _proxData.text =
                "MAPPING DATA      " + F(r.Comms.ScienceGbPerDay, "0.00") + " GB/day\n" +
                "DOWNLINK          " + F(r.Comms.DownlinkGbPerDay, "0.000") + " GB/day\n" +
                "RETURNED          " + Pct(r.Comms.Fraction, "0") + " over " +
                F(d.CampaignYears, "0.0") + " years\n" +
                "LIGHT TIME        " + F(r.Comms.LightTimeMinutes, "0.0") + " min\n" +
                "CAMPAIGN          " + F(d.CampaignYears, "0.0") + " years   (" +
                F(d.CampaignYears * 365.25, "0") + " days)";
        }

        // ======================================================================
        // 5 REVIEW
        // ======================================================================

        GameObject BuildReviewPage()
        {
            RectTransform p;
            var page = NewPage("PageReview", out p);
            PageTitle(p, AsteroidStrings.ReviewTitle);

            var left = MissionUIFactory.PanelBox(p, "ReviewLeft", 0, 44, 770, 780,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(left.rectTransform, "L", 14, 10, 600, 20,
                AsteroidStrings.ReadinessTitle, 12, MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);

            string[] items =
            {
                "LAUNCH MASS (95%)",
                "ORBIT STABILITY",
                "SURFACE POWER",
                "MAPPING DATA",
                "TAG SAMPLE (60 g)",
                "CAPSULE RE-ENTRY",
                "BUDGET (1,050 $M)"
            };
            for (int i = 0; i < items.Length; i++)
            {
                float y = 42f + i * 28f;
                MissionUIFactory.AddText(left.rectTransform, "R" + i, 14, y, 420, 22, items[i], 11.5f,
                    MissionUIFactory.TextMain, TextAlignmentOptions.MidlineLeft, _font);
                _readiness[i] = MissionUIFactory.AddText(left.rectTransform, "RS" + i, 520, y, 230, 22,
                    "", 11.5f, MissionUIFactory.Green, TextAlignmentOptions.MidlineRight, _font);
            }

            var mc = MissionUIFactory.PanelBox(left.rectTransform, "MC", 14, 252, 742, 180,
                MissionUIFactory.PanelAlt, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(mc.rectTransform, "L", 12, 8, 500, 18,
                AsteroidStrings.MonteCarloTitle, 11, MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _reviewBig = MissionUIFactory.AddText(mc.rectTransform, "Big", 12, 28, 700, 64, "0%", 40,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
            _reviewSub = MissionUIFactory.AddText(mc.rectTransform, "Sub", 12, 96, 716, 74, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _reviewSub.lineSpacing = 4f;

            var tips = MissionUIFactory.PanelBox(left.rectTransform, "Tips", 14, 444, 742, 322,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(tips.rectTransform, "L", 12, 8, 500, 18, "DESIGN TIPS", 11,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _reviewTips = MissionUIFactory.AddText(tips.rectTransform, "Text", 12, 32, 716, 278, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _reviewTips.lineSpacing = 6f;

            var right = MissionUIFactory.PanelBox(p, "ReviewRight", 794, 44, 770, 780,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);

            _beforeLaunch = MissionUIFactory.Rect(right.rectTransform, "BeforeLaunch", 0, 0, 770, 780).gameObject;
            MissionUIFactory.AddText((RectTransform)_beforeLaunch.transform, "Text", 20, 20, 730, 120,
                "Launch is allowed even with warnings.\nThe checklist is advice, not a lock.",
                13, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);

            _launchButton = MissionUIFactory.AddButton(right.rectTransform, "Launch", 20, 620, 730, 90,
                AsteroidStrings.LaunchButton, 34, _font, OnLaunch, TextAlignmentOptions.Center, _buttonSprite);
            MissionUIFactory.SetButtonVisual(_launchButton, MissionUIFactory.Selected, MissionUIFactory.Green);

            _debriefGroup = MissionUIFactory.Rect(right.rectTransform, "Debrief", 0, 0, 770, 780).gameObject;
            _debriefGroup.SetActive(false);

            _debriefRating = MissionUIFactory.AddText((RectTransform)_debriefGroup.transform, "Rating",
                20, 16, 730, 48, "", 30, MissionUIFactory.Green, TextAlignmentOptions.TopLeft, _font);
            _debriefBody = MissionUIFactory.AddText((RectTransform)_debriefGroup.transform, "Body",
                20, 72, 730, 470, "", 11.5f, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _debriefBody.lineSpacing = 5f;

            MissionUIFactory.AddButton((RectTransform)_debriefGroup.transform, "Redesign",
                20, 620, 350, 60, AsteroidStrings.Redesign, 17, _font, OnRedesign,
                TextAlignmentOptions.Center, _buttonSprite);
            MissionUIFactory.AddButton((RectTransform)_debriefGroup.transform, "CloseTerminal",
                400, 620, 350, 60, AsteroidStrings.CloseTerminal, 17, _font, Close,
                TextAlignmentOptions.Center, _buttonSprite);

            return page;
        }

        void RefreshReview()
        {
            var r = AsteroidResultStore.Report;
            var mc = AsteroidResultStore.MonteCarloResult;

            bool[] pass =
            {
                r.Mass.Pass,
                r.Target.OrbitPass,
                r.Power.Pass,
                r.Comms.Fraction >= 0.5,
                r.Tag.Pass,
                r.Capsule.Pass,
                r.Cost.Pass
            };
            string[] labels =
            {
                pass[0] ? AsteroidStrings.StatusPass : AsteroidStrings.StatusFail,
                pass[1] ? AsteroidStrings.StatusPass : AsteroidStrings.StatusWarn,
                pass[2] ? AsteroidStrings.StatusPass : AsteroidStrings.StatusFail,
                pass[3] ? AsteroidStrings.StatusPass : AsteroidStrings.StatusWarn,
                pass[4] ? AsteroidStrings.StatusPass : AsteroidStrings.StatusFail,
                pass[5] ? AsteroidStrings.StatusPass : AsteroidStrings.StatusFail,
                pass[6] ? AsteroidStrings.StatusPass : AsteroidStrings.StatusFail
            };

            for (int i = 0; i < _readiness.Length; i++)
            {
                _readiness[i].text = labels[i];
                _readiness[i].color = pass[i] ? MissionUIFactory.Green
                    : (labels[i] == AsteroidStrings.StatusWarn ? MissionUIFactory.Amber : MissionUIFactory.Red);
            }

            _reviewBig.text = Pct(mc.SuccessRate, "0.0");
            _reviewBig.color = mc.SuccessRate >= 0.60f ? MissionUIFactory.Green
                : (mc.SuccessRate >= 0.35f ? MissionUIFactory.Amber : MissionUIFactory.Red);
            _reviewSub.text =
                "FULL " + Pct(mc.FullProbability, "0.0") + "   //   PARTIAL " +
                Pct(mc.PartialProbability, "0.0") + "   //   FAIL " + Pct(mc.FailProbability, "0.0") + "\n" +
                "EXPECTED " + F(mc.ExpectedPoints, "0") + " PTS   //   " +
                F(mc.ExpectedGrams, "0") + " g   //   DESIGN " + F(r.AchievablePoints, "0") + " PTS";

            var tips = AsteroidObjectives.AutoTips(r);
            var sb = new StringBuilder();
            for (int i = 0; i < tips.Count && i < 5; i++)
            {
                if (i > 0) sb.Append('\n');
                sb.Append("- ").Append(tips[i]);
            }
            _reviewTips.text = sb.ToString();

            var timeline = AsteroidResultStore.LastTimeline;
            bool showDebrief = _debrief && timeline != null;
            MissionUIFactory.SetActive(_debriefGroup, showDebrief);
            MissionUIFactory.SetActive(_beforeLaunch, !showDebrief);
            MissionUIFactory.SetActive(_launchButton.gameObject, !showDebrief);

            if (!showDebrief) return;

            _debriefRating.text = AsteroidStrings.Rating(timeline.Rating);
            _debriefRating.color = timeline.Rating == 2 ? MissionUIFactory.Green
                : (timeline.Rating == 1 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            var def = r.Definition;
            var objectives = def.Objectives;
            var levels = new[]
            {
                timeline.Level1, timeline.Level2, timeline.Level3, timeline.Level4, r.BudgetPass
            };

            var body = new StringBuilder();
            body.Append("MISSION LEVELS\n");
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

            body.Append("\nSAMPLE RETURNED ").Append(F(timeline.Grams, "0")).Append(" g   //   ")
                .Append(F(timeline.DataFraction * 100.0, "0")).Append("% data   //   TOTAL ")
                .Append(F(timeline.Points, "0")).Append(" PTS\n");

            if (!timeline.Arrived)
                body.Append("\nFAILURE: the spacecraft did not reach the asteroid.\n");
            else if (!timeline.CapsuleRecovered)
                body.Append("\nFAILURE: the sample capsule did not survive re-entry.\n");

            body.Append("\nWEAKEST CONSTRAINT TIPS\n");
            for (int i = 0; i < timeline.Tips.Count && i < 4; i++)
                body.Append("- ").Append(timeline.Tips[i]).Append('\n');

            _debriefBody.text = body.ToString();
        }

        // ======================================================================
        // launch sequence
        // ======================================================================

        void BuildLaunchOverlay()
        {
            var rt = MissionUIFactory.Stretch(_content, "LaunchOverlay", 16, 16, 16, 16);
            _launchOverlay = rt.gameObject;
            MissionUIFactory.AddImage(rt, MissionUIFactory.Background, null, false, true);

            MissionUIFactory.AddText(rt, "Title", 0, 0, PageW, 32, AsteroidStrings.LaunchSequence, 26,
                MissionUIFactory.Green, TextAlignmentOptions.TopLeft, _font);
            _launchPhase = MissionUIFactory.AddText(rt, "Phase", 0, 36, PageW, 22, "PRE-LAUNCH", 12,
                MissionUIFactory.Amber, TextAlignmentOptions.TopLeft, _font);
            MissionUIFactory.AddText(rt, "SkipHint", PageW - 360, 4, 360, 22, AsteroidStrings.SkipHint, 11,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopRight, _font);

            _launchLog = MissionUIFactory.AddText(rt, "Log", 0, 70, PageW, 600, "", 12.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _launchLog.lineSpacing = 4f;

            _launchMonth = MissionUIFactory.AddText(rt, "Milestone", 0, 686, 900, 24, "LAUNCH", 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.MidlineLeft, _font);
            _launchBar = MissionUIFactory.Bar(rt, "ProgressBar", 0, 714, PageW, 24);
            _launchStatus = MissionUIFactory.AddText(rt, "Status", 0, 752, PageW, 24, "NOMINAL", 12,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
            MissionUIFactory.AddText(rt, "Footer", 0, 784, PageW, 20,
                "TIME ACCELERATED // LAUNCH, CRUISE, PROXIMITY OPS, TAG, RETURN", 10,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            _launchOverlay.SetActive(false);
        }

        void OnLaunch()
        {
            if (_launching) return;
            var timeline = AsteroidResultStore.Launch();
            if (timeline == null) return;

            _launching = true;
            _skipLaunch = false;
            _launchLines.Clear();

            if (_launchLog != null) _launchLog.text = string.Empty;
            if (_launchPhase != null)
            {
                _launchPhase.text = "PRE-LAUNCH // " + AsteroidResultStore.Report.Vehicle.Name.ToUpperInvariant();
                _launchPhase.color = MissionUIFactory.Amber;
            }
            if (_launchMonth != null) _launchMonth.text = "LAUNCH";
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

            MissionLog.Add("ASTEROID LAUNCH COMMAND ACCEPTED // " +
                           AsteroidResultStore.Report.Vehicle.Name.ToUpperInvariant());
            UIAnimations.Run(LaunchSequenceRoutine(timeline));
        }

        IEnumerator LaunchSequenceRoutine(AsteroidTimeline timeline)
        {
            AppendLaunchLine("<color=#4E8A66>ASTEROID LAUNCH CONTROL // " +
                             AsteroidResultStore.Report.Vehicle.Name + "</color>");
            yield return LaunchWait(0.25f);
            if (this == null) yield break;

            for (int i = 0; i < timeline.Events.Count; i++)
            {
                if (this == null) yield break;

                if (_skipLaunch)
                {
                    for (int j = i; j < timeline.Events.Count; j++) AppendAsteroidEvent(timeline.Events[j]);
                    break;
                }

                AppendAsteroidEvent(timeline.Events[i]);
                yield return LaunchWait(0.45f);
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

        void AppendAsteroidEvent(AsteroidTimelineEvent e)
        {
            string color = e.Alarm ? "#FF6B6B" : "#7DFFB2";
            AppendLaunchLine("<color=" + color + ">" + e.Phase + "</color>  " + e.Text);
            MissionLog.Add(e.Phase + ": " + e.Text);

            if (_launchMonth != null) _launchMonth.text = e.Phase.ToUpperInvariant();
            if (_launchBar != null)
                MissionUIFactory.SetBar(_launchBar, e.Progress,
                    e.Alarm ? MissionUIFactory.Amber : MissionUIFactory.Green);
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

        void EndLaunchSequence(AsteroidTimeline timeline)
        {
            _launching = false;
            _skipLaunch = false;
            _debrief = true;

            bool complete = timeline.Rating == 2;
            Color color = timeline.Rating == 2 ? MissionUIFactory.Green
                : (timeline.Rating == 1 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            if (_launchBar != null) MissionUIFactory.SetBar(_launchBar,
                complete ? 1f : (timeline.Arrived ? 0.9f : 0.1f), color);
            if (_launchPhase != null)
            {
                _launchPhase.text = complete ? "MISSION COMPLETE // DEBRIEF READY"
                    : (timeline.Arrived ? "PARTIAL MISSION // DEBRIEF READY" : "MISSION LOST");
                _launchPhase.color = color;
            }
            if (_launchStatus != null)
            {
                _launchStatus.text = "SAMPLE " + F(timeline.Grams, "0") + " g   //   " +
                                     F(timeline.Points, "0") + " PTS   //   " +
                                     AsteroidStrings.Rating(timeline.Rating);
                _launchStatus.color = color;
            }
            if (_launchMonth != null) _launchMonth.text = timeline.Arrived ? "RETURN" : "FAILURE";

            MissionLog.Add("DEBRIEF: " + AsteroidStrings.Rating(timeline.Rating) + " - " +
                           F(timeline.Points, "0") + " PTS - " + F(timeline.Grams, "0") + " g");

            if (_launchOverlay != null) _launchOverlay.SetActive(false);
            RefreshAll();
        }

        void OnRedesign()
        {
            AsteroidResultStore.ResetAll();
            _mcVersion = -1;
            _debrief = false;
            _launching = false;
            _skipLaunch = false;
            if (_launchOverlay != null) _launchOverlay.SetActive(false);
            MissionLog.Add("REDESIGN: asteroid defaults restored.");
            RefreshAll();
        }
    }
}
