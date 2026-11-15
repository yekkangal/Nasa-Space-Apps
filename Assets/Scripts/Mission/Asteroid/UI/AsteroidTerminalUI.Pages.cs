// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MissionArchitect
{
    public sealed partial class AsteroidTerminalUI
    {
        TMP_Text _targetPhysics, _targetTradeoffs;
        TMP_Text _craftSummary, _craftStats;
        TMP_Text _trajDv, _trajInfo, _massText;
        Image _massBar;

        // ======================================================================
        // 1 TARGET
        // ======================================================================

        GameObject BuildTargetPage()
        {
            RectTransform p;
            var page = NewPage("PageTarget", out p);
            PageTitle(p, AsteroidStrings.TargetTitle);

            for (int i = 0; i < AsteroidCatalog.Targets.Length; i++)
            {
                var t = AsteroidCatalog.Targets[i];
                AddChoice(p, 0, 44 + i * 54, PageW, "target", t.Id, i, t.Name);
            }

            var physics = MissionUIFactory.PanelBox(p, "TargetPhysics", 0, 230, 760, 570,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            _targetPhysics = MissionUIFactory.AddText(physics.rectTransform, "Text", 16, 14, 728, 540, "",
                11.5f, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _targetPhysics.lineSpacing = 6f;

            var trade = MissionUIFactory.PanelBox(p, "TargetTrade", 790, 230, 770, 570,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            _targetTradeoffs = MissionUIFactory.AddText(trade.rectTransform, "Text", 16, 14, 738, 540, "",
                11.5f, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _targetTradeoffs.lineSpacing = 6f;

            return page;
        }

        void RefreshTarget()
        {
            var report = AsteroidResultStore.Report;
            var t = report.Target;
            var target = report.TargetInfo;
            var d = report.Design;

            for (int i = 0; i < _choices.Count; i++)
            {
                var c = _choices[i];
                if (c.Group != "target") continue;
                var info = AsteroidCatalog.Targets[c.Index];
                bool selected = d.TargetId == info.Id;
                var label = MissionUIFactory.ButtonLabel(c.Button);
                if (label != null)
                {
                    label.text = info.Name + "   " + F(info.DiameterM, "0") + " m   " +
                                 F(info.RotationHours, "0.0") + " h rotation   " +
                                 F(info.MeanAu, "0.00") + " AU   " +
                                 (info.ScienceMultiplier >= 1.3 ? "VERY HIGH SCIENCE" :
                                     (info.ScienceMultiplier >= 1.0 ? "HIGH SCIENCE" : "LOW SCIENCE"));
                }
                MissionUIFactory.SetButtonVisual(c.Button,
                    selected ? MissionUIFactory.Selected : MissionUIFactory.PanelAlt,
                    selected ? MissionUIFactory.Green : MissionUIFactory.TextMain);
            }

            _targetPhysics.text =
                "<size=17><b>" + t.Name.ToUpperInvariant() + "</b></size>\n\n" +
                "DIAMETER            " + F(t.DiameterM, "0") + " m\n" +
                "RADIUS              " + F(t.RadiusM, "0") + " m\n" +
                "ROTATION PERIOD     " + F(t.RotationHours, "0.0") + " h\n" +
                "ORBIT PERIOD        " + F(t.OrbitYears, "0.0") + " years\n" +
                "MEAN DISTANCE       " + F(t.MeanAu, "0.00") + " AU  (" +
                F(t.MeanAu * 149.6, "0") + " million km)\n" +
                "CLOSE APPROACH      " + F(t.CloseApproachKm, "0") + " km\n" +
                "LIGHT TIME          " + F(t.LightTimeMinutes, "0.0") + " min one-way\n\n" +
                "<b>PHYSICS</b>\n" +
                "MASS                " + Sci(t.MassKg, "0.00E+00") + " kg\n" +
                "GM                  " + F(t.GmM3S2, "0.0") + " m3/s2\n" +
                "SURFACE GRAVITY     " + Sci(t.SurfaceGravityMps2, "0.00000") + " m/s2\n" +
                "ESCAPE VELOCITY     " + F(t.EscapeVelocityMps, "0.000") + " m/s\n" +
                "BOULDERS            up to " + F(t.BoulderM, "0") + " m\n\n" +
                "<b>ORBIT AT " + F(t.OrbitRadiusM, "0") + " m</b>\n" +
                "PERIOD              " + F(t.OrbitPeriodHours, "0.0") + " h\n" +
                "CIRCULAR SPEED      " + F(t.CircularSpeedMps, "0.000") + " m/s\n" +
                "SRP / GRAVITY       " + Pct(t.SrpRatio, "0.00") + "\n" +
                "STABILITY           " + Pct(t.Stability, "0") + "  " +
                (t.OrbitPass ? AsteroidStrings.StatusPass : AsteroidStrings.StatusFail);

            _targetTradeoffs.text =
                "<size=17><b>MISSION TRADE</b></size>\n\n" +
                "SCIENCE VALUE       x" + F(t.ScienceMultiplier, "2") + "\n" +
                "SAMPLE RICHNESS     x" + F(t.SampleRichness, "2") + "\n" +
                "HAZARD              x" + F(target.HazardFactor, "2") + "\n" +
                "C3 BONUS            " + (target.C3Bonus >= 0 ? "+" : "") + F(target.C3Bonus, "0.0") +
                " km2/s2\n" +
                "DELTA-V BONUS       " + (target.DvBonusKmS >= 0 ? "+" : "") +
                F(target.DvBonusKmS, "2") + " km/s\n" +
                "DEFAULT ORBIT       " + F(target.DefaultOrbitRadiusM, "0") + " m\n\n" +
                "ACCESSIBILITY       " + Accessibility(target) + "\n\n" +
                "<b>BRIEF</b>\n" + Wrap(target.Note, 58) + "\n\n" +
                "<color=#7F9A88>The Bennu-like target is the balanced science choice. The fast rotator is " +
                "cheap but sample-poor; the metal body is a deep well with high science.</color>";
            _targetTradeoffs.richText = true;
        }

        static string Accessibility(AsteroidTarget t)
        {
            double difficulty = t.C3Bonus + t.DvBonusKmS * 10.0;
            if (difficulty <= -2) return "EASY";
            if (difficulty <= 2) return "MEDIUM";
            return "HARD";
        }

        // ======================================================================
        // 2 SPACECRAFT
        // ======================================================================

        GameObject BuildSpacecraftPage()
        {
            RectTransform p;
            var page = NewPage("PageSpacecraft", out p);
            PageTitle(p, AsteroidStrings.SpacecraftTitle);

            AddSectionHeader(p, 0, 44, 760, AsteroidStrings.SectionBus);
            for (int i = 0; i < AsteroidCatalog.Buses.Length; i++)
                AddChoice(p, 0, 70 + i * 30, 760, "bus", AsteroidCatalog.Buses[i].Id, i, AsteroidCatalog.Buses[i].Name);

            AddSectionHeader(p, 0, 170, 760, AsteroidStrings.SectionPropulsion);
            for (int i = 0; i < AsteroidCatalog.Propulsion.Length; i++)
                AddChoice(p, 0, 196 + i * 30, 760, "prop", AsteroidCatalog.Propulsion[i].Id, i,
                    AsteroidCatalog.Propulsion[i].Name);

            AddSectionHeader(p, 0, 296, 760, AsteroidStrings.SectionArray);
            for (int i = 0; i < AsteroidCatalog.Arrays.Length; i++)
                AddChoice(p, 0, 322 + i * 30, 760, "array", "array" + i, i, AsteroidCatalog.Arrays[i].Label);

            AddSectionHeader(p, 0, 452, 760, AsteroidStrings.SectionBattery);
            for (int i = 0; i < AsteroidCatalog.Batteries.Length; i++)
                AddChoice(p, 0, 478 + i * 30, 760, "battery", "battery" + i, i,
                    AsteroidCatalog.Batteries[i].Label + " battery");

            AddSectionHeader(p, 790, 44, 770, AsteroidStrings.SectionAntenna);
            for (int i = 0; i < AsteroidCatalog.AntennaIds.Length; i++)
            {
                string id = AsteroidCatalog.AntennaIds[i];
                AddChoice(p, 790, 70 + i * 30, 770, "antenna", id, i, MissionCatalog.Antenna(id).Name);
            }

            AddSectionHeader(p, 790, 170, 770, AsteroidStrings.SectionRf);
            for (int i = 0; i < MissionCatalog.RfPowers.Length; i++)
            {
                var r = MissionCatalog.RfPowers[i];
                AddChoice(p, 790, 196 + i * 30, 770, "rf", "rf" + i, i, r.Label);
            }

            AddSectionHeader(p, 790, 316, 770, AsteroidStrings.SectionSampler);
            for (int i = 0; i < AsteroidCatalog.Samplers.Length; i++)
                AddChoice(p, 790, 342 + i * 30, 770, "sampler", AsteroidCatalog.Samplers[i].Id, i,
                    AsteroidCatalog.Samplers[i].Name);

            AddSectionHeader(p, 790, 442, 770, AsteroidStrings.SectionCapsule);
            for (int i = 0; i < AsteroidCatalog.Capsules.Length; i++)
                AddChoice(p, 790, 468 + i * 30, 770, "capsule", AsteroidCatalog.Capsules[i].Id, i,
                    AsteroidCatalog.Capsules[i].Name);

            AddSectionHeader(p, 790, 568, 770, AsteroidStrings.SectionInstruments);
            for (int i = 0; i < AsteroidCatalog.Instruments.Length; i++)
                AddChoice(p, 790, 594 + i * 30, 770, "instrument", "inst" + i, i,
                    AsteroidCatalog.Instruments[i].Name);

            var summary = MissionUIFactory.PanelBox(p, "CraftSummary", 0, 600, 760, 200,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(summary.rectTransform, "L", 14, 10, 500, 20, "DESIGN SUMMARY", 11.5f,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _craftSummary = MissionUIFactory.AddText(summary.rectTransform, "Text", 14, 36, 732, 154, "",
                11.5f, MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _craftSummary.lineSpacing = 5f;

            _craftStats = MissionUIFactory.AddText(p, "CraftStats", 790, 760, 770, 40, "", 10.5f,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            return page;
        }

        void RefreshSpacecraft()
        {
            var d = AsteroidResultStore.Design;
            var r = AsteroidResultStore.Report;

            for (int i = 0; i < _choices.Count; i++)
            {
                var c = _choices[i];
                bool sel = false;
                string label = null;

                switch (c.Group)
                {
                    case "bus":
                        {
                            var b = AsteroidCatalog.Buses[c.Index];
                            sel = d.BusId == c.Value;
                            label = b.Name + "   " + F(b.MassKg, "0") + " kg   $" + F(b.CostM, "0") + "M";
                            break;
                        }
                    case "prop":
                        {
                            var e = AsteroidCatalog.Propulsion[c.Index];
                            sel = d.PropulsionId == c.Value;
                            label = e.Name + "   Isp " + F(e.Isp, "0") + " s   " + F(e.MassKg, "0") +
                                    " kg   " + F(e.PowerW, "0") + " W   $" + F(e.CostM, "0") + "M";
                            break;
                        }
                    case "array":
                        {
                            var a = AsteroidCatalog.Arrays[c.Index];
                            sel = Mathf.Abs((float)d.ArrayAreaM2 - (float)a.AreaM2) < 0.01f;
                            label = a.Label + " array   " + F(a.MassKg, "0") + " kg   $" +
                                    F(a.CostM, "0.0") + "M";
                            break;
                        }
                    case "battery":
                        {
                            var b = AsteroidCatalog.Batteries[c.Index];
                            sel = Mathf.Abs((float)d.BatteryWh - (float)b.CapacityWh) < 0.01f;
                            label = b.Label + "   " + F(b.MassKg, "0.0") + " kg   $" + F(b.CostM, "0.00") + "M";
                            break;
                        }
                    case "antenna":
                        {
                            var a = MissionCatalog.Antenna(c.Value);
                            sel = d.AntennaId == c.Value;
                            label = a.Name + "   " + F(a.MassKg, "0") + " kg   $" + F(a.CostMillions, "0") + "M";
                            break;
                        }
                    case "rf":
                        {
                            var rf = MissionCatalog.RfPowers[c.Index];
                            sel = Mathf.Abs((float)d.RfPowerW - (float)rf.Watts) < 0.01f;
                            label = rf.Label + " RF   " + F(rf.MassKg, "0") + " kg   $" +
                                    F(rf.CostMillions, "0") + "M";
                            break;
                        }
                    case "sampler":
                        {
                            var s = AsteroidCatalog.Samplers[c.Index];
                            sel = d.SamplerId == c.Value;
                            label = s.Name + "   median " + F(s.MedianGrams, "0") + " g   " +
                                    F(s.MassKg, "0") + " kg   $" + F(s.CostM, "0") + "M";
                            break;
                        }
                    case "capsule":
                        {
                            var cp = AsteroidCatalog.Capsules[c.Index];
                            sel = d.CapsuleId == c.Value;
                            label = cp.Name + "   " + F(cp.MassKg, "0") + " kg   limit " +
                                    F(cp.SpeedLimitKmS, "0.0") + " km/s   $" + F(cp.CostM, "0") + "M";
                            break;
                        }
                    case "instrument":
                        {
                            var ins = AsteroidCatalog.Instruments[c.Index];
                            sel = d.InstrumentEnabled[c.Index];
                            label = (sel ? "ON   " : "OFF  ") + ins.Name + "   " + F(ins.MassKg, "0") +
                                    " kg   " + F(ins.PowerW, "0") + " W   " +
                                    F(ins.DataGbPerDay, "0.00") + " GB/day   +" + F(ins.Points, "0") + " pts";
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

            _craftSummary.text =
                "ACTIVE INSTRUMENTS   " + d.ActiveInstrumentCount + " / " + AsteroidCatalog.Instruments.Length +
                "     MASS " + F(d.InstrumentMassKg, "0") + " kg     POWER " + F(d.InstrumentPowerW, "0") +
                " W     DATA " + F(d.InstrumentDataGbPerDay, "0.00") + " GB/day     PTS " +
                F(d.InstrumentPoints, "0") + "\n" +
                "SAMPLER  " + r.Sampler.Name + "     CAPSULE  " + r.CapsuleInfo.Name + "\n" +
                "EXPECTED SAMPLE  " + r.ExpectedGrams + " g at " + r.Site.Name + "\n" +
                "SOLAR ARRAY  " + F(d.ArrayAreaM2, "0") + " m2     BATTERY  " + F(d.BatteryWh, "0") + " Wh";

            _craftStats.text = "DRY " + F(r.Mass.DryKg, "0") + " kg   //   PROP " +
                               F(r.Mass.PropellantKg, "0") + " kg   //   WET " + F(r.Mass.WetKg, "0") +
                               " kg   //   POWER " + Pct(r.Power.Margin, "0") + "   //   DATA " +
                               Pct(r.Comms.Fraction, "0") + "   //   TAG " +
                               Pct(r.Tag.SuccessProbability, "0");
        }

        // ======================================================================
        // 3 TRAJECTORY
        // ======================================================================

        GameObject BuildTrajectoryPage()
        {
            RectTransform p;
            var page = NewPage("PageTrajectory", out p);
            PageTitle(p, AsteroidStrings.TrajectoryTitle);

            AddSectionHeader(p, 0, 44, 760, AsteroidStrings.SectionTrajectory);
            AddChoice(p, 0, 70, 760, "traj", "direct", 0, "DIRECT");
            AddChoice(p, 0, 112, 760, "traj", "ega", 1, "EARTH GRAVITY ASSIST");

            AddSectionHeader(p, 0, 166, 760, AsteroidStrings.SectionVehicle);
            for (int i = 0; i < AsteroidCatalog.Vehicles.Length; i++)
                AddChoice(p, 0, 192 + i * 30, 760, "vehicle", AsteroidCatalog.Vehicles[i].Id, i,
                    AsteroidCatalog.Vehicles[i].Name);

            var mass = MissionUIFactory.PanelBox(p, "TrajMass", 0, 336, 760, 170,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(mass.rectTransform, "L", 14, 10, 500, 20, AsteroidStrings.SectionMass, 11.5f,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _massBar = MissionUIFactory.Bar(mass.rectTransform, "MassBar", 14, 42, 730, 24);
            _massText = MissionUIFactory.AddText(mass.rectTransform, "Text", 14, 76, 730, 80, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _massText.lineSpacing = 4f;

            var dv = MissionUIFactory.PanelBox(p, "TrajDv", 790, 44, 770, 330,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(dv.rectTransform, "L", 14, 10, 500, 20, AsteroidStrings.SectionDeltaV, 11.5f,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _trajDv = MissionUIFactory.AddText(dv.rectTransform, "Text", 14, 38, 742, 280, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _trajDv.lineSpacing = 6f;

            var info = MissionUIFactory.PanelBox(p, "TrajInfo", 790, 390, 770, 300,
                MissionUIFactory.PanelFill, MissionUIFactory.BorderColor, _panelSprite);
            MissionUIFactory.AddText(info.rectTransform, "L", 14, 10, 500, 20, "MISSION TIMELINE", 11.5f,
                MissionUIFactory.GreenDim, TextAlignmentOptions.TopLeft, _font);
            _trajInfo = MissionUIFactory.AddText(info.rectTransform, "Text", 14, 38, 742, 250, "", 11.5f,
                MissionUIFactory.TextMain, TextAlignmentOptions.TopLeft, _font);
            _trajInfo.lineSpacing = 6f;

            MissionUIFactory.AddText(p, "Hint", 0, 530, 760, 120,
                "Direct transfer needs a higher C3 and a bigger delta-v budget. Earth gravity assist trades " +
                "about a year of cruise for lower launch energy and rendezvous delta-v.", 10.5f,
                MissionUIFactory.TextDim, TextAlignmentOptions.TopLeft, _font);

            return page;
        }

        void RefreshTrajectory()
        {
            var d = AsteroidResultStore.Design;
            var r = AsteroidResultStore.Report;

            for (int i = 0; i < _choices.Count; i++)
            {
                var c = _choices[i];
                bool sel = false;
                string label = null;

                switch (c.Group)
                {
                    case "traj":
                        {
                            bool direct = c.Value == "direct";
                            bool isSel = (d.TrajectoryType == "direct") == direct;
                            sel = isSel;
                            double c3 = (direct ? AsteroidConstants.DirectC3 : AsteroidConstants.EgaC3) +
                                        r.TargetInfo.C3Bonus;
                            double dv = (direct ? AsteroidConstants.DirectRendezvousKmS
                                                : AsteroidConstants.EgaRendezvousKmS) +
                                        (d.CampaignYears * AsteroidConstants.ProxOpsKmSPerYear) +
                                        AsteroidConstants.DepartureKmS + AsteroidConstants.ReturnTrimKmS;
                            label = (direct ? "DIRECT" : "EARTH GRAVITY ASSIST") + "   C3 " +
                                    F(c3, "0.0") + "   arrival " +
                                    F(direct ? AsteroidConstants.DirectArrivalYears : AsteroidConstants.EgaArrivalYears, "1.0") +
                                    " yr   dv base " + F(dv, "0.00") + " km/s";
                            break;
                        }
                    case "vehicle":
                        {
                            var v = AsteroidCatalog.Vehicles[c.Index];
                            sel = d.VehicleId == v.Id;
                            double cap = AsteroidCatalog.CapacityAtC3(v, r.Trajectory.C3Km2S2);
                            label = v.Name + "   CAP " + F(cap, "0") + " kg at current C3   $" +
                                    F(v.CostM, "0") + "M   REL " + F(v.Reliability * 100.0, "0") + "%";
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

            double capacity = r.Mass.CapacityKg;
            double pct = capacity > 0 ? r.Mass.WetKg / capacity : 1.0;
            Color color = capacity <= 0 ? MissionUIFactory.Red
                : (r.Mass.Pass ? MissionUIFactory.Green
                    : (r.Mass.WetKg <= capacity ? MissionUIFactory.Amber : MissionUIFactory.Red));
            MissionUIFactory.SetBar(_massBar, (float)pct, color);
            _massText.text = capacity > 0
                ? "WET " + F(r.Mass.WetKg, "0") + " kg / " + F(capacity, "0") + " kg capacity  (" +
                  F(pct * 100.0, "0") + "%)   limit " + F(r.Mass.LimitKg, "0") + " kg\n" +
                  "DRY " + F(r.Mass.DryKg, "0") + " kg   PROP " + F(r.Mass.PropellantKg, "0") +
                  " kg   WET/DRY " + F(r.Mass.WetDryRatio, "2")
                : "NO ASTEROID CAPACITY WITH " + r.Vehicle.Name;
            _massText.color = color;

            _trajDv.text =
                "TYPE            " + r.Trajectory.TypeName + "\n" +
                "C3              " + F(r.Trajectory.C3Km2S2, "0.0") + " km2/s2\n" +
                "RENDEZVOUS      " + F(r.Trajectory.RendezvousKmS, "0.00") + " km/s\n" +
                "PROX OPS        " + F(r.Trajectory.ProxOpsKmS, "0.00") + " km/s  (" +
                F(d.CampaignYears, "0.0") + " yr)\n" +
                "TAG MANEUVERS   " + F(r.Trajectory.TagKmS, "0.00") + " km/s  (" + d.Attempts + " attempts)\n" +
                "DEPARTURE       " + F(r.Trajectory.DepartureKmS, "0.00") + " km/s\n" +
                "RETURN TRIM     " + F(AsteroidConstants.ReturnTrimKmS, "2") + " km/s\n" +
                "10% MARGIN      INCLUDED\n" +
                "<b>TOTAL           " + F(r.Trajectory.TotalKmS, "2") + " km/s</b>";

            _trajInfo.text =
                "CRUISE          " + F(r.Trajectory.CruiseDays, "0") + " days  (" +
                F(r.Trajectory.ArrivalYears, "1.0") + " years)\n" +
                "SURVEY          " + F(d.CampaignYears, "1.0") + " years\n" +
                "RETURN          " + F(AsteroidConstants.ReturnCruiseYears, "1.0") + " years\n" +
                "TOTAL MISSION   " + F(r.Trajectory.MissionYears, "1.0") + " years\n" +
                "RETURN WINDOW   " + r.Trajectory.ReturnWindow + "\n\n" +
                "LAUNCH CAPACITY " + F(r.Trajectory.CapacityKg, "0") + " kg at C3 " +
                F(r.Trajectory.C3Km2S2, "0.0") + "\n" +
                "95% LIMIT       " + F(r.Trajectory.LimitKg, "0") + " kg\n\n" +
                "<color=#7F9A88>Propellant is sized from the rocket equation with Isp " +
                F(r.Engine.Isp, "0") + " s; the tank is 8% of propellant.</color>";
        }
    }
}
// asteroid pages
