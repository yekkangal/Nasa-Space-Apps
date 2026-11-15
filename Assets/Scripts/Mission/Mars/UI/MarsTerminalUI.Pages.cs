// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MissionArchitect
{
    public sealed partial class MarsTerminalUI
    {
        readonly TMP_Text[] _windowCards = new TMP_Text[5];

        // ======================================================================
        // 1 WINDOW
        // ======================================================================

        GameObject BuildWindowPage()
        {
            RectTransform p;
            var page = NewPage("PageWindow", out p);
            PageTitle(p, MarsStrings.WindowTitle);

            MissionUIFactory.AddText(p, "Sub", 0, 36, PageW, 22,
                "The launch date is a decision: C3, cruise time and payload capacity all react.",
                11.5f, MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            MissionUIFactory.AddText(p, "SliderLabel", 0, 68, 190, 26, MarsStrings.WindowSlider, 11,
                MissionUIFactory.GreenDim, TextAlignmentOptions.MidlineLeft, _font);
            _windowSlider = MarsUIWidgets.AddSlider(p, "WindowSlider", 200, 64, 900, 32, -60, 60, true, OnWindowSlider);
            _windowLabel = MissionUIFactory.AddText(p, "WindowValue", 1120, 64, 462, 32, "T+0 d", 13,
                MissionUIFactory.Amber, TextAlignmentOptions.MidlineLeft, _font);

            WindowCard(p, 0, "C3 (km2/s2)", out _windowCards[0]);
            WindowCard(p, 312, "CRUISE (days)", out _windowCards[1]);
            WindowCard(p, 624, "DISTANCE (Mkm)", out _windowCards[2]);
            WindowCard(p, 936, "ARRIVAL V-INF (km/s)", out _windowCards[3]);
            WindowCard(p, 1248, "ONE-WAY LIGHT (min)", out _windowCards[4]);

            var note = MissionUIFactory.PanelBox(p, "WindowNote", 0, 232, PageW, 70,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            _windowNote = MissionUIFactory.AddText(note.rectTransform, "Text", 14, 10, PageW - 28, 50, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _windowNote.lineSpacing = 2f;

            AddSectionHeader(p, 0, 318, PageW, MarsStrings.SectionVehicle);
            for (int i = 0; i < MarsCatalog.Vehicles.Length; i++)
            {
                var v = MarsCatalog.Vehicles[i];
                AddChoice(p, 0, 346 + i * 60, PageW, "vehicle", v.Id, i, v.Name + "  //  " + v.Note);
            }

            var cap = MissionUIFactory.PanelBox(p, "Capacity", 0, 598, PageW, 170,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(cap.rectTransform, "L", 14, 10, 500, 20,
                MarsStrings.SectionCapacity, 12, MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _capacityBar = MissionUIFactory.Bar(cap.rectTransform, "CapBar", 14, 40, 1200, 26);
            _capacityText = MissionUIFactory.AddText(cap.rectTransform, "Text", 14, 74, 1530, 24, "", 13,
                MissionUIFactory.TextMain, TextAlignmentOptions.MidlineLeft, _font);
            MissionUIFactory.AddText(cap.rectTransform, "Hint", 14, 106, 1530, 48,
                "Capacity falls with C3 (40% floor). Launch mass must stay below 95% of capacity. " +
                "The Mars window repeats every ~26 months.", 10.5f,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font); // src: [3]

            MissionUIFactory.AddText(p, "Footer", 0, 792, PageW, 20,
                "C3 grows with the square of the date offset: a late launch costs payload.",
                10, MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            return page;
        }

        void WindowCard(RectTransform p, float x, string label, out TMP_Text value)
        {
            var box = MissionUIFactory.PanelBox(p, "Card_" + label, x, 120, 300, 96,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(box.rectTransform, "L", 12, 8, 276, 18, label, 10,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            value = MissionUIFactory.AddText(box.rectTransform, "V", 12, 32, 276, 52, "-", 21,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
        }

        void RefreshWindow()
        {
            var report = MarsResultStore.Report;
            var d = report.Design;

            _updating = true;
            MarsUIWidgets.SetSlider(_windowSlider, d.LaunchOffsetDays);
            _updating = false;

            string offset = d.LaunchOffsetDays == 0
                ? "T+0 d  //  OPTIMAL"
                : (d.LaunchOffsetDays > 0
                    ? "T+" + d.LaunchOffsetDays + " d  //  " + MarsStrings.WindowLate
                    : "T-" + Mathf.Abs(d.LaunchOffsetDays) + " d  //  " + MarsStrings.WindowEarly);
            _windowLabel.text = offset + "  //  " + F(report.Transfer.C3Km2S2, "0.0") + " km2/s2";
            _windowLabel.color = report.Transfer.C3Km2S2 <= 10.0 ? MissionUIFactory.Green
                : (report.Transfer.C3Km2S2 <= 14.0 ? MissionUIFactory.Amber : MissionUIFactory.Red);

            _windowCards[0].text = F(report.Transfer.C3Km2S2, "0.0");
            _windowCards[1].text = F(report.Transfer.CruiseDays, "0");
            _windowCards[2].text = F(report.Transfer.EarthMarsDistanceMkm, "0");
            _windowCards[3].text = F(report.Transfer.ArrivalVinfKmS, "0.00");
            _windowCards[4].text = F(report.Transfer.LightTimeMinutes, "0.0");

            _windowNote.text = "<color=#FFC857>" + report.Transfer.WindowNote + "</color>\n" +
                               "Thin atmosphere (under 1/100 of Earth pressure) is why EDL stays hard."; // src: [4]

            for (int i = 0; i < _choices.Count; i++)
            {
                var c = _choices[i];
                if (c.Group != "vehicle") continue;
                var v = MarsCatalog.Vehicles[c.Index];
                double cap = MarsEngineeringModel.LaunchCapacityKg(v.Id, report.Transfer.C3Km2S2);
                bool sel = d.VehicleId == v.Id;
                var label = MissionUIFactory.ButtonLabel(c.Button);
                if (label != null)
                {
                    label.text = v.Name + "    CAP " + F(cap, "0") + " kg    $" + F(v.CostM, "0") +
                                 "M    REL " + F(v.Reliability * 100.0, "0") + "%    " + v.Note;
                }
                MissionUIFactory.SetButtonVisual(c.Button,
                    sel ? MissionUIFactory.Selected : MissionUIFactory.PanelAlt,
                    sel ? MissionUIFactory.Green : MissionUIFactory.TextMain);
            }

            double capacity = report.Mass.CapacityKg;
            double pct = capacity > 0 ? report.Mass.LaunchKg / capacity : 1.0;
            Color color = capacity <= 0 ? MissionUIFactory.Red
                : (report.Mass.Pass ? MissionUIFactory.Green
                    : (report.Mass.LaunchKg <= capacity ? MissionUIFactory.Amber : MissionUIFactory.Red));
            MissionUIFactory.SetBar(_capacityBar, (float)pct, color);
            _capacityText.text = capacity > 0
                ? "LAUNCH " + F(report.Mass.LaunchKg, "0") + " kg  /  " + F(capacity, "0") +
                  " kg capacity  (" + F(pct * 100.0, "0") + "%)   limit " + F(report.Mass.LimitKg, "0") + " kg"
                : "NO MARS CAPACITY WITH " + report.Vehicle.Name;
            _capacityText.color = color;
        }

        // ======================================================================
        // 2 ROVER
        // ======================================================================

        GameObject BuildRoverPage()
        {
            RectTransform p;
            var page = NewPage("PageRover", out p);
            PageTitle(p, MarsStrings.RoverTitle);

            AddSectionHeader(p, 0, 44, 760, MarsStrings.SectionBus);
            for (int i = 0; i < MarsCatalog.Buses.Length; i++)
                AddChoice(p, 0, 68 + i * 30, 760, "bus", MarsCatalog.Buses[i].Id, i, MarsCatalog.Buses[i].Name);

            AddSectionHeader(p, 0, 160, 760, MarsStrings.SectionMobility);
            for (int i = 0; i < MarsCatalog.Mobility.Length; i++)
                AddChoice(p, 0, 184 + i * 30, 760, "mobility", MarsCatalog.Mobility[i].Id, i, MarsCatalog.Mobility[i].Name);

            AddSectionHeader(p, 0, 246, 760, MarsStrings.SectionArm);
            AddChoice(p, 0, 270, 760, "arm", "arm", 0, MarsStrings.SectionArm);

            AddSectionHeader(p, 0, 302, 760, MarsStrings.SectionThermal);
            for (int i = 0; i < MarsCatalog.Thermal.Length; i++)
                AddChoice(p, 0, 326 + i * 30, 760, "thermal", MarsCatalog.Thermal[i].Id, i, MarsCatalog.Thermal[i].Name);

            AddSectionHeader(p, 0, 388, 760, MarsStrings.SectionPower);
            AddChoice(p, 0, 412, 760, "power", "mmrtg", 0, "MMRTG");
            AddChoice(p, 0, 442, 760, "power", "solar", 1, "SOLAR ARRAY");

            AddSectionHeader(p, 0, 474, 760, MarsStrings.SectionArray);
            for (int i = 0; i < MarsCatalog.Arrays.Length; i++)
                AddChoice(p, 0, 498 + i * 30, 760, "array", "array" + i, i, "SOLAR " + MarsCatalog.Arrays[i].Label);

            AddSectionHeader(p, 0, 620, 760, MarsStrings.SectionBattery);
            for (int i = 0; i < MarsCatalog.Batteries.Length; i++)
                AddChoice(p, 0, 644 + i * 30, 760, "battery", "battery" + i, i,
                    MarsCatalog.Batteries[i].Label + " battery");

            AddSectionHeader(p, 790, 44, 770, MarsStrings.SectionInstruments);
            for (int i = 0; i < MarsCatalog.Instruments.Length; i++)
                AddChoice(p, 790, 68 + i * 34, 770, "instrument", "inst" + i, i, MarsCatalog.Instruments[i].Name);

            var summary = MissionUIFactory.PanelBox(p, "RoverSummary", 790, 320, 770, 120,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(summary.rectTransform, "L", 12, 8, 500, 18, "ACTIVE PAYLOAD", 11,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _roverSummary = MissionUIFactory.AddText(summary.rectTransform, "Text", 12, 30, 746, 80, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _roverSummary.lineSpacing = 4f;

            var stats = MissionUIFactory.PanelBox(p, "RoverStats", 790, 456, 770, 180,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(stats.rectTransform, "L", 12, 8, 500, 18, "MASS / POWER", 11,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _roverStats = MissionUIFactory.AddText(stats.rectTransform, "Text", 12, 30, 746, 140, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _roverStats.lineSpacing = 4f;

            MissionUIFactory.AddText(p, "Hint", 790, 650, 770, 80,
                "Solar arrays are lighter and cheaper than an MMRTG but a dust storm can end the mission. " +
                "Instruments cost mass, power, data and money.", 10.5f,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            return page;
        }

        void RefreshRover()
        {
            var d = MarsResultStore.Design;
            var r = MarsResultStore.Report;

            for (int i = 0; i < _choices.Count; i++)
            {
                var c = _choices[i];
                bool sel = false;
                string label = null;

                switch (c.Group)
                {
                    case "bus":
                        {
                            var b = MarsCatalog.Buses[c.Index];
                            sel = d.BusId == c.Value;
                            label = b.Name + "   " + F(b.MassKg, "0") + " kg   $" + F(b.CostM, "0") + "M";
                            break;
                        }
                    case "mobility":
                        {
                            var m = MarsCatalog.Mobility[c.Index];
                            sel = d.MobilityId == c.Value;
                            label = m.Name + "   " + F(m.MassKg, "0") + " kg   $" + F(m.CostM, "0") + "M";
                            break;
                        }
                    case "arm":
                        sel = d.RoboticArm;
                        label = (sel ? "ON   " : "OFF  ") + "Robotic arm   " + F(MarsCatalog.ArmMassKg, "0") +
                                " kg   $" + F(MarsCatalog.ArmCostM, "0") + "M";
                        break;
                    case "thermal":
                        {
                            var t = MarsCatalog.Thermal[c.Index];
                            sel = d.ThermalId == c.Value;
                            label = t.Name + "   " + F(t.HeaterWhSol, "0") + " Wh/sol   " +
                                    F(t.MassKg, "0") + " kg   $" + F(t.CostM, "0") + "M";
                            break;
                        }
                    case "power":
                        sel = d.PowerType == c.Value;
                        label = c.Value == "mmrtg"
                            ? "MMRTG 110 W continuous   " + F(MarsCatalog.Mmrtg.MassKg, "0") + " kg   $" +
                              F(MarsCatalog.Mmrtg.CostM, "0") + "M   (dust independent)"
                            : "Solar array + battery   seasonal and dust dependent";
                        break;
                    case "array":
                        {
                            var a = MarsCatalog.Arrays[c.Index];
                            sel = d.IsSolar && Mathf.Abs((float)d.SolarAreaM2 - (float)a.AreaM2) < 0.01f;
                            label = a.Label + " array   " + F(a.MassKg, "0") + " kg   $" +
                                    F(a.CostM, "0.0") + "M   " + (d.IsSolar ? "" : "(MMRTG selected)");
                            break;
                        }
                    case "battery":
                        {
                            var b = MarsCatalog.Batteries[c.Index];
                            sel = Mathf.Abs((float)d.BatteryWh - (float)b.CapacityWh) < 0.01f;
                            label = b.Label + "   " + F(b.MassKg, "0.0") + " kg   $" + F(b.CostM, "0.00") + "M";
                            break;
                        }
                    case "instrument":
                        {
                            var ins = MarsCatalog.Instruments[c.Index];
                            sel = d.InstrumentEnabled[c.Index];
                            label = (sel ? "ON   " : "OFF  ") + ins.Name + "   " + F(ins.MassKg, "0") + " kg   " +
                                    F(ins.PowerW, "0") + " W   " + F(ins.DataMbPerHour, "0") + " Mb/h   +" +
                                    F(ins.Points, "0") + " pts";
                            break;
                        }
                }

                if (label != null)
                {
                    var labelText = MissionUIFactory.ButtonLabel(c.Button);
                    if (labelText != null) labelText.text = label;
                }

                bool interactable = c.Group != "array" || d.IsSolar;
                c.Button.interactable = interactable;
                MissionUIFactory.SetButtonVisual(c.Button,
                    sel ? MissionUIFactory.Selected : MissionUIFactory.PanelAlt,
                    sel ? MissionUIFactory.Green : (interactable ? MissionUIFactory.TextMain : MissionUIFactory.TextDim));
            }

            _roverSummary.text =
                "INSTRUMENTS  " + d.ActiveInstrumentCount + " / " + MarsCatalog.Instruments.Length +
                "     MASS  " + F(d.InstrumentMassKg, "0") + " kg     POWER  " + F(d.InstrumentPowerW, "0") +
                " W     DATA  " + F(d.InstrumentDataMbPerHour, "0") + " Mb/h     POINTS  " +
                F(d.InstrumentPoints, "0") + "\n" +
                "SITE SCIENCE MULTIPLIER  x" + F(r.Site.ScienceMultiplier, "2") +
                "     RADIATION CLASS  " + r.RadiationClass.Name;

            _roverStats.text =
                "ROVER  " + F(r.Mass.RoverKg, "0") + " kg     LANDED  " + F(r.Mass.LandedKg, "0") +
                " kg     LAUNCH  " + F(r.Mass.LaunchKg, "0") + " kg\n" +
                "POWER SYSTEM  " + (d.IsSolar ? "SOLAR " + F(d.SolarAreaM2, "0") + " m2" : "MMRTG") +
                "     BATTERY  " + F(d.BatteryWh, "0") + " Wh\n" +
                "GENERATION  " + F(r.Power.GenerationWhSol, "0") + " Wh/sol     LOAD  " +
                F(r.Power.LoadWhSol, "0") + " Wh/sol     MARGIN  " + Pct(r.Power.Margin, "0") + "\n" +
                "WORST CASE (winter + dust)  " + F(r.Power.WorstGenerationWhSol, "0") +
                " Wh/sol     " + Pct(r.Power.WorstMargin, "0") + " margin";
        }

        // ======================================================================
        // 3 EDL
        // ======================================================================

        GameObject BuildEdlPage()
        {
            RectTransform p;
            var page = NewPage("PageEdl", out p);
            PageTitle(p, MarsStrings.EdlTitle);

            AddSectionHeader(p, 0, 44, 760, MarsStrings.SectionEdl);
            for (int i = 0; i < MarsCatalog.EdlSystems.Length; i++)
                AddChoice(p, 0, 68 + i * 30, 760, "edl", MarsCatalog.EdlSystems[i].Id, i, MarsCatalog.EdlSystems[i].Name);

            AddSectionHeader(p, 0, 164, 760, MarsStrings.SectionShield);
            for (int i = 0; i < MarsCatalog.HeatShields.Length; i++)
                AddChoice(p, 0, 188 + i * 30, 760, "shield", MarsCatalog.HeatShields[i].Id, i,
                    MarsCatalog.HeatShields[i].Name);

            AddSectionHeader(p, 0, 254, 760, MarsStrings.SectionParachute);
            AddChoice(p, 0, 278, 760, "parachute", "parachute", 0, MarsStrings.SectionParachute);

            AddSectionHeader(p, 0, 314, 760, MarsStrings.SectionTrn);
            AddChoice(p, 0, 338, 760, "trn", "trn", 0, MarsStrings.SectionTrn);

            AddSectionHeader(p, 0, 374, 760, MarsStrings.SectionSite);
            for (int i = 0; i < MarsCatalog.Sites.Length; i++)
                AddChoice(p, 0, 398 + i * 30, 760, "site", MarsCatalog.Sites[i].Id, i, MarsCatalog.Sites[i].Name);

            var note = MissionUIFactory.PanelBox(p, "EdlNote", 0, 508, 760, 150,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(note.rectTransform, "Text", 14, 10, 732, 130,
                "Mars surface pressure is under 1/100 of Earth's.\n\n" +
                "Sky crane lands the most mass but is the most complex. Airbags are cheap but capped " +
                "at 600 kg landed. TRN adds +6% on hazardous sites.", 10.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font); // src: [4]

            var big = MissionUIFactory.PanelBox(p, "EdlBig", 790, 44, 770, 180,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(big.rectTransform, "L", 14, 10, 500, 20, "EDL SUCCESS PROBABILITY", 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _edlBig = MissionUIFactory.AddText(big.rectTransform, "Big", 14, 34, 740, 80, "0%", 48,
                MissionUIFactory.Green, TextAlignmentOptions.MidlineLeft, _font);
            _edlDetail = MissionUIFactory.AddText(big.rectTransform, "Detail", 14, 120, 740, 44, "", 12,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);

            var landed = MissionUIFactory.PanelBox(p, "Landed", 790, 240, 770, 150,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(landed.rectTransform, "L", 14, 10, 500, 20, "LANDED MASS / SYSTEM LIMIT", 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _landedBar = MissionUIFactory.Bar(landed.rectTransform, "LandedBar", 14, 44, 740, 24);
            _landedText = MissionUIFactory.AddText(landed.rectTransform, "Text", 14, 80, 740, 52, "", 12,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);

            var factors = MissionUIFactory.PanelBox(p, "Factors", 790, 406, 770, 250,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(factors.rectTransform, "L", 14, 10, 500, 20, "PROBABILITY FACTORS", 12,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _edlFactors = MissionUIFactory.AddText(factors.rectTransform, "Text", 14, 36, 742, 200, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _edlFactors.lineSpacing = 5f;

            MissionUIFactory.AddText(p, "Hint", 790, 674, 770, 60,
                "Landing is the riskiest phase: the same mass that makes science possible makes EDL harder.",
                10.5f, MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            return page;
        }

        void RefreshEdl()
        {
            var d = MarsResultStore.Design;
            var r = MarsResultStore.Report;

            for (int i = 0; i < _choices.Count; i++)
            {
                var c = _choices[i];
                bool sel = false;
                string label = null;

                switch (c.Group)
                {
                    case "edl":
                        {
                            var e = MarsCatalog.EdlSystems[c.Index];
                            sel = d.EdlSystemId == c.Value;
                            label = e.Name + "   base " + Pct(e.BaseSuccess, "0") + "   landed fraction " +
                                    Pct(e.LandedFraction, "0") + "   limit " + F(e.LandedLimitKg, "0") +
                                    " kg   $" + F(e.CostM, "0") + "M";
                            break;
                        }
                    case "shield":
                        {
                            var s = MarsCatalog.HeatShields[c.Index];
                            sel = d.HeatShieldId == c.Value;
                            label = s.Name + "   " + F(s.MassKg, "0") + " kg   $" + F(s.CostM, "0") +
                                    "M   EDL x" + F(s.SuccessFactor, "2");
                            break;
                        }
                    case "parachute":
                        sel = d.Parachute;
                        label = (sel ? "ON   " : "OFF  ") + "Parachute   " +
                                F(MarsCatalog.ParachuteMassKg, "0") + " kg   $" +
                                F(MarsCatalog.ParachuteCostM, "0") + "M";
                        break;
                    case "trn":
                        sel = d.Trn;
                        label = (sel ? "ON   " : "OFF  ") + "Terrain-relative navigation   +6% on hazard sites";
                        break;
                    case "site":
                        {
                            var s = MarsCatalog.Sites[c.Index];
                            sel = d.SiteId == c.Value;
                            label = s.Name + "   hazard x" + F(s.HazardFactor, "2") + "   science x" +
                                    F(s.ScienceMultiplier, "2") + "   " + s.Note;
                            break;
                        }
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

            var edl = r.Edl;
            _edlBig.text = Pct(edl.SuccessProbability, "0");
            _edlBig.color = edl.SuccessProbability >= 0.80 ? MissionUIFactory.Green
                : (edl.SuccessProbability >= 0.60 ? MissionUIFactory.Amber : MissionUIFactory.Red);
            _edlDetail.text = edl.SystemName.ToUpperInvariant() + "  //  " + r.Site.Name.ToUpperInvariant() +
                              "  //  " + F(edl.SuccessProbability * 100.0, "1") + "% landing probability";

            double ratio = edl.LandedLimitKg > 0 ? r.Mass.LandedKg / edl.LandedLimitKg : 1.0;
            Color landedColor = ratio <= 0.8 ? MissionUIFactory.Green
                : (ratio <= 1.0 ? MissionUIFactory.Amber : MissionUIFactory.Red);
            MissionUIFactory.SetBar(_landedBar, (float)ratio, landedColor);
            _landedText.text = F(r.Mass.LandedKg, "0") + " kg landed  /  " + F(edl.LandedLimitKg, "0") +
                               " kg limit  (" + F(ratio * 100.0, "0") + "%)   " +
                               (ratio <= 1.0 ? "WITHIN LIMIT" : "OVER LIMIT");
            _landedText.color = landedColor;

            _edlFactors.text =
                "BASE (SYSTEM)          " + Pct(edl.BaseSuccess, "0") + "\n" +
                "MASS FACTOR            x" + F(edl.MassFactor, "2") + "\n" +
                "SITE HAZARD            x" + F(edl.SiteHazard, "2") + "\n" +
                "TRN BONUS              +" + Pct(edl.TrnBonus, "0") + "\n" +
                "HEAT SHIELD            x" + F(edl.ShieldFactor, "2") + "\n" +
                "PARACHUTE              x" + F(edl.ParachuteFactor, "2");
        }
    }
}
