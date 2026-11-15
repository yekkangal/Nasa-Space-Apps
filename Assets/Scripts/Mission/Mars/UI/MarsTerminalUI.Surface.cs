// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MissionArchitect
{
    public sealed partial class MarsTerminalUI
    {
        // ======================================================================
        // 4 SURFACE - sol planner
        // ======================================================================

        GameObject BuildSurfacePage()
        {
            RectTransform p;
            var page = NewPage("PageSurface", out p);
            PageTitle(p, MarsStrings.SurfaceTitle);

            AddSectionHeader(p, 0, 44, 780, MarsStrings.SectionPolicy);

            string[] labels =
            {
                MarsStrings.PolicyDrive,
                MarsStrings.PolicyScience,
                MarsStrings.PolicyDrill,
                MarsStrings.PolicyComms,
                MarsStrings.PolicyRest
            };

            for (int i = 0; i < 5; i++)
            {
                float y = 76f + i * 54f;
                MissionUIFactory.AddText(p, "PolicyLabel" + i, 0, y, 140, 28, labels[i], 11,
                    MissionUIFactory.GreenDim, TextAlignmentOptions.MidlineLeft, _font);
                int index = i;
                _policySliders[i] = MarsUIWidgets.AddSlider(p, "Policy" + i, 150, y, 470, 30, 0, 100, true,
                    OnPolicySlider);
                _policyValues[i] = MissionUIFactory.AddText(p, "PolicyValue" + i, 632, y, 148, 28, "0%", 11.5f,
                    MissionUIFactory.TextMain, TextAlignmentOptions.MidlineLeft, _font);
            }

            MissionUIFactory.AddButton(p, "Normalise", 0, 348, 220, 30, MarsStrings.Normalize, 11.5f,
                _font, OnNormalisePolicy, TextAlignmentOptions.MidlineLeft, _buttonSprite);

            MissionUIFactory.AddText(p, "DayLabel", 0, 396, 300, 20, MarsStrings.MiniDay, 11.5f,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _daySegments = MarsUIWidgets.AddSegmentedBar(p, "DayBar", 0, 420, 760, 30, 5,
                new[]
                {
                    MissionUIFactory.Amber,
                    MissionUIFactory.Green,
                    MissionUIFactory.Hex("#7DD3FF"),
                    MissionUIFactory.Hex("#C792EA"),
                    MissionUIFactory.Hex("#4E5A53")
                });
            _dayLegend = MissionUIFactory.AddText(p, "DayLegend", 0, 456, 780, 22, "", 10.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);

            MissionUIFactory.AddText(p, "PolicyHint", 0, 486, 780, 90,
                "Weights are normalised into a 12-hour daylight shift. Rest covers sleep and night heaters. " +
                "Drilling one sample takes 60 hours, so samples compete directly with driving and science.", 10.5f,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            var power = MissionUIFactory.PanelBox(p, "PlannerPower", 800, 44, 782, 218,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(power.rectTransform, "L", 14, 10, 500, 20, MarsStrings.SectionPowerBudget, 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _plannerPower = MissionUIFactory.AddText(power.rectTransform, "Text", 14, 34, 754, 174, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _plannerPower.lineSpacing = 6f;

            var perSol = MissionUIFactory.PanelBox(p, "PerSol", 800, 276, 782, 190,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(perSol.rectTransform, "L", 14, 10, 500, 20, MarsStrings.SectionPerSol, 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _plannerPerSol = MissionUIFactory.AddText(perSol.rectTransform, "Text", 14, 34, 754, 146, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _plannerPerSol.lineSpacing = 6f;

            var season = MissionUIFactory.PanelBox(p, "Season", 800, 480, 782, 280,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(season.rectTransform, "L", 14, 10, 500, 20, MarsStrings.SectionSeason, 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _plannerSeason = MissionUIFactory.AddText(season.rectTransform, "Text", 14, 34, 754, 236, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _plannerSeason.lineSpacing = 6f;

            return page;
        }

        void RefreshSurface()
        {
            var d = MarsResultStore.Design;
            var r = MarsResultStore.Report;
            int[] values = { d.DrivePct, d.SciencePct, d.DrillPct, d.CommsPct, d.RestPct };
            double[] weights = { d.DriveWeight, d.ScienceWeight, d.DrillWeight, d.CommsWeight, d.RestWeight };

            _updating = true;
            for (int i = 0; i < 5; i++)
            {
                MarsUIWidgets.SetSlider(_policySliders[i], values[i]);
                if (_policyValues[i] != null)
                {
                    _policyValues[i].text = values[i] + "%  (" + F(weights[i] * 100.0, "0") + "% share)";
                    _policyValues[i].color = values[i] > 0 ? MissionUIFactory.TextMain : MissionUIFactory.TextDim;
                }
            }
            _updating = false;

            MarsUIWidgets.SetSegments(_daySegments,
                new[] { (float)weights[0], (float)weights[1], (float)weights[2], (float)weights[3], (float)weights[4] },
                760f);

            _dayLegend.text =
                "<color=#FFC857>DRIVE " + values[0] + "%</color>   " +
                "<color=#7DFFB2>SCIENCE " + values[1] + "%</color>   " +
                "<color=#7DD3FF>DRILL " + values[2] + "%</color>   " +
                "<color=#C792EA>COMMS " + values[3] + "%</color>   " +
                "<color=#9AA8A0>REST " + values[4] + "%</color>";

            _plannerPower.text =
                "GENERATION (NOMINAL)   " + F(r.Power.GenerationWhSol, "0") + " Wh/sol\n" +
                "GENERATION (WORST)     " + F(r.Power.WorstGenerationWhSol, "0") + " Wh/sol\n" +
                "LOAD                   " + F(r.Power.LoadWhSol, "0") + " Wh/sol\n" +
                "  housekeeping  " + F(r.Power.HousekeepingWh, "0") +
                "   heaters  " + F(r.Power.HeaterWh, "0") + "\n" +
                "  drive  " + F(r.Power.DriveWh, "0") + "   science  " + F(r.Power.ScienceWh, "0") +
                "   drill  " + F(r.Power.DrillWh, "0") + "\n" +
                "MARGIN   " + Pct(r.Power.Margin, "0") + " nominal  /  " + Pct(r.Power.WorstMargin, "0") + " worst\n" +
                "BATTERY  " + F(r.Power.BatteryUsableWh, "0") + " / " + F(r.Power.BatteryNeedWh, "0") +
                " Wh usable  " + (r.Power.BatteryPass ? MarsStrings.StatusPass : MarsStrings.StatusFail);

            _plannerPerSol.text =
                "DRIVING           " + F(r.Surface.DistanceMPerSol, "0") + " m/sol\n" +
                "SCIENCE           " + F(r.Surface.ScienceHours, "1") + " h  ->  " +
                F(r.Data.ScienceMbSol, "0") + " Mb/sol\n" +
                "DRILLING          " + F(r.Surface.DrillHours, "1") + " h  ->  " +
                F(r.Surface.SamplesPerSol, "3") + " samples/sol\n" +
                "DOWNLINK          " + F(r.Data.DownlinkMbSol, "0") + " Mb/sol  (" +
                Pct(r.Data.Fraction, "0") + " of science)\n" +
                "EXPECTED SAMPLES  " + r.ExpectedSamples + " over 668 sols";

            _plannerSeason.text = SeasonLine("NOMINAL", r.Power.GenerationWhSol, r.Power.LoadWhSol) +
                                  SeasonLine("WINTER 0.70", WinterGeneration(r), r.Power.LoadWhSol) +
                                  SeasonLine("DUST STORM 0.5", DustGeneration(r), r.Power.LoadWhSol) +
                                  "\n" +
                                  (r.Power.Mmrtg
                                      ? "MMRTG output is dust independent; it decays about 4%/year. // game"
                                      : "Dust storms last 20-60 sols and cut solar output to 20-50%. // game");
        }

        double WinterGeneration(MarsReport r)
        {
            if (r.Power.Mmrtg) return r.Power.GenerationWhSol;
            return r.Power.GenerationWhSol * (MarsConstants.SeasonalWorstFactor / MarsConstants.SeasonalNominalFactor);
        }

        double DustGeneration(MarsReport r)
        {
            if (r.Power.Mmrtg) return r.Power.GenerationWhSol;
            return r.Power.GenerationWhSol * MarsConstants.DustStormWorstMarginFactor;
        }

        string SeasonLine(string name, double generation, double load)
        {
            double margin = load > 0.001 ? (generation - load) / load : 9.99;
            bool pass = margin >= 0.0;
            string color = pass ? "#7DFFB2" : "#FF6B6B";
            return "<color=#7F9A88>" + name.PadRight(14) + "</color> GEN " + F(generation, "0") +
                   " Wh  LOAD " + F(load, "0") + " Wh  <color=" + color + ">" +
                   (pass ? "PASS " : "FAIL ") + Pct(margin, "0") + " margin</color>\n";
        }

        // ======================================================================
        // 5 COMMS
        // ======================================================================

        GameObject BuildCommsPage()
        {
            RectTransform p;
            var page = NewPage("PageComms", out p);
            PageTitle(p, MarsStrings.CommsTitle);

            AddSectionHeader(p, 0, 44, 760, MarsStrings.SectionRelay);
            AddChoice(p, 0, 68, 760, "relay", "relay", 0, MarsStrings.SectionRelay);

            AddSectionHeader(p, 0, 112, 760, MarsStrings.SectionPasses);
            AddChoice(p, 0, 136, 760, "passes", "1", 1, "1 pass per sol");
            AddChoice(p, 0, 166, 760, "passes", "2", 2, "2 passes per sol");

            AddSectionHeader(p, 0, 210, 760, MarsStrings.SectionDte);
            AddChoice(p, 0, 234, 760, "dte", "dte", 0, MarsStrings.SectionDte);

            var notes = MissionUIFactory.PanelBox(p, "CommsNotes", 0, 286, 760, 240,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(notes.rectTransform, "Text", 14, 10, 732, 220,
                "UHF relay: up to 2 Mbps during ~8 minute orbiter passes, 1-2 passes per sol.\n\n" +
                "Typical Mars Relay Network rate at greatest distance: 0.1-0.5 Mbps.\n\n" +
                "Direct-to-Earth X-band: 160-500 bps - useful for telemetry, hopeless for images.\n\n" +
                "The relay link is shared with every other spacecraft on the network, so the " +
                "conservative 0.5 Mbps rate is used here.", 10.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font); // src: [5][6][7] // game rate

            var big = MissionUIFactory.PanelBox(p, "CommsBig", 790, 44, 770, 190,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(big.rectTransform, "L", 14, 10, 500, 20, "DATA RETURNED", 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _commsBig = MissionUIFactory.AddText(big.rectTransform, "Big", 14, 32, 742, 70, "0%", 44,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
            _commsBar = MissionUIFactory.Bar(big.rectTransform, "CommsBar", 14, 118, 742, 24);
            MissionUIFactory.AddText(big.rectTransform, "Hint", 14, 148, 742, 30,
                "Share of the daily science data that reaches Earth.", 10,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            var breakdown = MissionUIFactory.PanelBox(p, "CommsBreakdown", 790, 250, 770, 226,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(breakdown.rectTransform, "L", 14, 10, 500, 20, MarsStrings.SectionDataBudget, 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _commsBreakdown = MissionUIFactory.AddText(breakdown.rectTransform, "Text", 14, 34, 742, 184, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _commsBreakdown.lineSpacing = 5f;

            var conjunction = MissionUIFactory.PanelBox(p, "Conjunction", 790, 492, 770, 190,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(conjunction.rectTransform, "L", 14, 10, 500, 20, MarsStrings.Conjunction, 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _commsNote = MissionUIFactory.AddText(conjunction.rectTransform, "Text", 14, 34, 742, 146, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _commsNote.lineSpacing = 5f;

            return page;
        }

        void RefreshComms()
        {
            var d = MarsResultStore.Design;
            var r = MarsResultStore.Report;

            for (int i = 0; i < _choices.Count; i++)
            {
                var c = _choices[i];
                if (c.Group != "relay" && c.Group != "passes" && c.Group != "dte") continue;

                bool sel = false;
                string label = null;
                switch (c.Group)
                {
                    case "relay":
                        sel = d.RelayEnabled;
                        label = (sel ? "ON   " : "OFF  ") + "UHF relay  0.5 Mbps x 8 min/pass";
                        break;
                    case "passes":
                        sel = d.RelayPasses == c.Index;
                        label = c.Index + " pass" + (c.Index > 1 ? "es" : "") + " per sol   " +
                                F(MarsConstants.RelayTypicalMbps * 1e6 * MarsConstants.RelayPassMinutes * 60.0 *
                                  c.Index / 1e6, "0") + " Mb/sol";
                        break;
                    case "dte":
                        sel = d.DirectToEarth;
                        label = (sel ? "ON   " : "OFF  ") + "Direct-to-Earth X-band  300 bps fallback";
                        break;
                }

                var labelText = MissionUIFactory.ButtonLabel(c.Button);
                if (label != null && labelText != null) labelText.text = label;
                MissionUIFactory.SetButtonVisual(c.Button,
                    sel ? MissionUIFactory.Selected : MissionUIFactory.PanelAlt,
                    sel ? MissionUIFactory.Green : MissionUIFactory.TextMain);
            }

            _commsBig.text = Pct(r.Data.Fraction, "0");
            _commsBig.color = r.Data.Pass ? MissionUIFactory.Green
                : (r.Data.Fraction >= 0.3 ? MissionUIFactory.Amber : MissionUIFactory.Red);
            MissionUIFactory.SetBar(_commsBar, (float)r.Data.Fraction,
                r.Data.Pass ? MissionUIFactory.Green
                    : (r.Data.Fraction >= 0.3 ? MissionUIFactory.Amber : MissionUIFactory.Red));

            _commsBreakdown.text =
                "RELAY RATE       " + F(r.Comms.RelayRateMbps, "1") + " Mbps   x " + r.Comms.RelayPasses +
                " passes   = " + F(r.Comms.RelayMbSol, "0") + " Mb/sol\n" +
                "DIRECT TO EARTH  " + F(r.Comms.DteMbSol, "1") + " Mb/sol\n" +
                "TOTAL DOWNLINK   " + F(r.Comms.TotalMbSol, "0") + " Mb/sol\n" +
                "SCIENCE DEMAND   " + F(r.Data.ScienceMbSol, "0") + " Mb/sol\n" +
                "RETURNED         " + Pct(r.Data.Fraction, "1") + " after " +
                Pct(r.Data.BlackoutLoss, "0") + " conjunction loss\n" +
                "LIGHT TIME       " + F(r.Comms.LightTimeMinutes, "1") + " min one-way";

            _commsNote.text =
                "Every ~26 months the Sun sits between Earth and Mars.\n\n" +
                "Blackout: " + F(r.Comms.ConjunctionDays, "0") +
                " sols with no commands; the rover runs stored sequences and loses about " +
                Pct(r.Data.BlackoutLoss, "0") + " of the year's data.\n\n" +
                "Distance range 55-400 million km -> 3-22 minute one-way light time."; // src: [1][3] // game blackout
        }
    }
}
