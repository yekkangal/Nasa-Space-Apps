// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;
using System.Collections.Generic;

namespace MissionArchitect
{
    public struct MonteCarloResult
    {
        public int Runs;
        public int FullCount;
        public int PartialCount;
        public int FailCount;
        public float ExpectedPoints;

        public float FullProbability => Runs > 0 ? (float)FullCount / Runs : 0f;
        public float PartialProbability => Runs > 0 ? (float)PartialCount / Runs : 0f;
        public float FailProbability => Runs > 0 ? (float)FailCount / Runs : 0f;
        public float SuccessRate => FullProbability + PartialProbability;
        public int Rating => SuccessRate >= 0.999f ? 2 : (SuccessRate >= 0.5f ? 1 : 0);
    }

    public struct TimelineEvent
    {
        public string Phase;
        public string Text;
        public int Month;      // 0 = cruise / arrival, 1..12 = mission months
        public bool Alarm;

        public TimelineEvent(string phase, string text, int month, bool alarm)
        {
            Phase = phase;
            Text = text;
            Month = month;
            Alarm = alarm;
        }
    }

    public sealed class MissionTimeline
    {
        public List<TimelineEvent> Events = new List<TimelineEvent>();
        public bool OrbitReached;
        public bool MissionLost;
        public string FailureReason;
        public float Points;
        public int Rating;              // 0 FAIL, 1 PARTIAL, 2 FULL
        public float DataFraction;
        public double FinalDoseKrad;
        public List<string> Tips = new List<string>();
        public EngineeringReport Report;
        public float[] ObjectivePoints;
    }

    /// <summary>
    /// Mission timeline and Monte Carlo. The Monte Carlo loop is allocation-free and
    /// deterministic for a given seed; 1000 runs take well under 100 ms.
    /// </summary>
    public static class MissionSimulator
    {
        const int Months = 12;

        /// <summary>Runs the 12-month mission once and returns a printable timeline.</summary>
        public static MissionTimeline RunTimeline(MissionDesign design, uint seed)
        {
            var report = EngineeringModel.Evaluate(design);
            var def = MissionDefinition.MoonDefinition;
            var timeline = new MissionTimeline { Report = report };
            var rng = new Random(unchecked((int)seed));

            // --- launch & transfer ------------------------------------------------
            if (!report.Vehicle.ReachesMoon || !report.Mass.Pass)
            {
                timeline.MissionLost = true;
                timeline.FailureReason = !report.Vehicle.ReachesMoon
                    ? report.Vehicle.Name + " cannot reach the Moon (no TLI capability)."
                    : "Wet mass exceeds the launch capacity to " + report.Mass.CapacityBasis + ".";
                timeline.Events.Add(new TimelineEvent("LAUNCH", timeline.FailureReason, 0, true));
                Finalize(timeline, report);
                return timeline;
            }

            if (rng.NextDouble() > report.Vehicle.Reliability)
            {
                timeline.MissionLost = true;
                timeline.FailureReason = "Launch vehicle failure - payload lost.";
                timeline.Events.Add(new TimelineEvent("LAUNCH", timeline.FailureReason, 0, true));
                Finalize(timeline, report);
                return timeline;
            }
            timeline.Events.Add(new TimelineEvent("LAUNCH",
                report.Vehicle.Name + " lift-off clean. " + report.Mass.WetKg.ToString("0") + " kg to "
                + report.Mass.CapacityBasis + ".", 0, false));

            if (rng.NextDouble() > MissionConstants.TliSuccessProbability)
            {
                timeline.MissionLost = true;
                timeline.FailureReason = "TLI burn underperformed - the spacecraft missed the Moon.";
                timeline.Events.Add(new TimelineEvent("TLI", timeline.FailureReason, 0, true));
                Finalize(timeline, report);
                return timeline;
            }
            timeline.Events.Add(new TimelineEvent("TLI",
                "Trans-lunar injection complete, " + report.Propulsion.DeltaVKmS.ToString("0.00") + " km/s budget.", 0, false));

            if (report.Propulsion.Electric && report.Propulsion.SpiralMonths > 0.1)
            {
                int spiral = (int)Math.Ceiling(report.Propulsion.SpiralMonths);
                timeline.Events.Add(new TimelineEvent("SPIRAL",
                    "Hall thruster spiral: " + report.Propulsion.SpiralMonths.ToString("0.0") + " months, "
                    + report.Propulsion.ThrustN.ToString("0.000") + " N.", 0, false));
                for (int i = 1; i <= spiral; i++)
                    timeline.Events.Add(new TimelineEvent("SPIRAL", "Spiral month " + i + "/" + spiral
                        + " - belt dose accumulating.", 0, false));
            }

            if (rng.NextDouble() > MissionConstants.LoiSuccessProbability)
            {
                timeline.MissionLost = true;
                timeline.FailureReason = "LOI burn failed - no capture into the 100 km polar orbit.";
                timeline.Events.Add(new TimelineEvent("LOI", timeline.FailureReason, 0, true));
                Finalize(timeline, report);
                return timeline;
            }
            timeline.OrbitReached = true;
            timeline.Events.Add(new TimelineEvent("LOI",
                "Lunar orbit insertion complete. 100 km polar orbit, period "
                + MissionConstants.OrbitPeriodMinutes().ToString("0") + " min.", 0, false));

            // --- 12 mission months -------------------------------------------------
            double dose = MissionConstants.BeltDoseKradPerMonth * report.Propulsion.SpiralMonths;
            double lost = 0.0;                      // fraction of the year's data lost

            for (int m = 1; m <= Months; m++)
            {
                dose += MissionConstants.RadiationKradPerYear / 12.0;
                bool alarm = false;
                string text = "Month " + m.ToString("00") + ": nominal science operations.";

                if (rng.NextDouble() < report.Reliability.MonthlyBusFailure)
                {
                    timeline.MissionLost = true;
                    timeline.FailureReason = "Bus failure in month " + m + ".";
                    timeline.Events.Add(new TimelineEvent("MONTH " + m.ToString("00"), timeline.FailureReason, m, true));
                    Finalize(timeline, report);
                    return timeline;
                }

                if (!report.Radiation.Pass && rng.NextDouble() < 0.06)
                {
                    timeline.MissionLost = true;
                    timeline.FailureReason = "Radiation-induced electronics failure in month " + m
                                            + " (" + dose.ToString("0") + " krad).";
                    timeline.Events.Add(new TimelineEvent("MONTH " + m.ToString("00"), timeline.FailureReason, m, true));
                    Finalize(timeline, report);
                    return timeline;
                }

                if (!report.Power.Pass && rng.NextDouble() < 0.04)
                {
                    timeline.MissionLost = true;
                    timeline.FailureReason = "Power-negative bus in month " + m
                                            + " - instruments shed until the spacecraft went silent.";
                    timeline.Events.Add(new TimelineEvent("MONTH " + m.ToString("00"), timeline.FailureReason, m, true));
                    Finalize(timeline, report);
                    return timeline;
                }

                if (rng.NextDouble() < 0.02)   // solar particle event
                {
                    double penalty = report.Radiation.ToleranceKrad < 30 ? 0.12 : 0.03;
                    lost += penalty;
                    alarm = true;
                    text = "Month " + m.ToString("00") + ": SOLAR PARTICLE EVENT - "
                           + (report.Radiation.ToleranceKrad < 30 ? "commercial parts degraded." : "screened parts held.");
                }

                if (rng.NextDouble() < 0.01)   // reaction wheel failure
                {
                    if (report.Reliability.Redundancy)
                    {
                        lost += 0.02;
                        alarm = true;
                        text = "Month " + m.ToString("00") + ": reaction wheel failed - redundant unit took over.";
                    }
                    else
                    {
                        timeline.MissionLost = true;
                        timeline.FailureReason = "Reaction wheel failure in month " + m + " with no backup.";
                        timeline.Events.Add(new TimelineEvent("MONTH " + m.ToString("00"), timeline.FailureReason, m, true));
                        Finalize(timeline, report);
                        return timeline;
                    }
                }

                if (rng.NextDouble() < 0.03)   // safe mode, one week of data lost
                {
                    lost += 0.028;
                    alarm = true;
                    text = "Month " + m.ToString("00") + ": safe mode - one week of science lost.";
                }

                if (lost > 0.9) lost = 0.9;
                timeline.Events.Add(new TimelineEvent("MONTH " + m.ToString("00"), text, m, alarm));
            }

            timeline.DataFraction = (float)Math.Max(0.0, report.Data.Fraction * (1.0 - lost));
            timeline.FinalDoseKrad = dose;
            if (dose > report.Radiation.ToleranceKrad)
                timeline.Events.Add(new TimelineEvent("END", "Cumulative dose " + dose.ToString("0")
                    + " krad exceeds the " + report.RadiationClass.Name + " tolerance.", 12, true));

            Finalize(timeline, report);
            return timeline;
        }

        static void Finalize(MissionTimeline timeline, EngineeringReport report)
        {
            var def = MissionDefinition.MoonDefinition;
            var statuses = MissionObjectives.Evaluate(report, def);

            timeline.ObjectivePoints = new float[statuses.Length];
            float total = 0f;
            for (int i = 0; i < statuses.Length; i++)
            {
                float pts = statuses[i].EarnedPoints;
                if (statuses[i].Id == "O2")
                    pts = report.SciencePoints * Math.Max(0f, timeline.DataFraction > 0f
                        ? timeline.DataFraction : (float)report.Data.Fraction);
                timeline.ObjectivePoints[i] = pts;
                total += pts;
            }

            if (!timeline.OrbitReached || timeline.MissionLost)
            {
                total = 0f;
                for (int i = 0; i < timeline.ObjectivePoints.Length; i++)
                    timeline.ObjectivePoints[i] = 0f;
            }

            timeline.Points = total;
            bool o1 = timeline.OrbitReached && !timeline.MissionLost;
            bool o4 = report.Cost.Pass;
            bool o6 = report.Mass.Pass;
            timeline.Rating = o1 ? MissionObjectives.Rating((int)Math.Round(total), o1, o4, o6) : 0;
            timeline.Tips = MissionObjectives.AutoTips(report);
        }

        /// <summary>
        /// 1000-run Monte Carlo. Reads the precomputed report only; no allocations inside the loop.
        /// </summary>
        public static MonteCarloResult MonteCarlo(MissionDesign design, int runs = 1000, uint seed = 20260101u)
        {
            var report = EngineeringModel.Evaluate(design);
            var result = new MonteCarloResult { Runs = runs };
            if (runs <= 0) return result;

            float sciPotential = report.SciencePoints;
            double baseFraction = report.Data.Fraction;
            double tolerance = report.Radiation.ToleranceKrad;
            bool redundancy = report.Reliability.Redundancy;
            bool costPass = report.Cost.Pass;
            bool massPass = report.Mass.Pass;
            bool canLaunch = report.Vehicle.ReachesMoon && report.Mass.Pass;
            double launchRel = report.Vehicle.Reliability;
            double monthlyBusFail = report.Reliability.MonthlyBusFailure;
            bool radiationPass = report.Radiation.Pass;
            bool powerPass = report.Power.Pass;
            double beltDose = MissionConstants.BeltDoseKradPerMonth * report.Propulsion.SpiralMonths;

            float costPoints = costPass ? 10f : 0f;
            float powerPoints = report.Power.Pass ? 10f : 0f;

            double totalPoints = 0.0;
            int full = 0, partial = 0, fail = 0;

            for (int run = 0; run < runs; run++)
            {
                uint state = seed + (uint)(run * 2654435761u);
                if (state == 0) state = 0x9E3779B9u;

                if (!canLaunch || Next01(ref state) > launchRel)
                {
                    fail++;
                    continue;
                }

                if (Next01(ref state) > MissionConstants.TliSuccessProbability)
                {
                    fail++;
                    continue;
                }

                if (Next01(ref state) > MissionConstants.LoiSuccessProbability)
                {
                    fail++;
                    continue;
                }

                double dose = beltDose;
                double lost = 0.0;
                bool alive = true;

                for (int m = 0; m < Months; m++)
                {
                    if (Next01(ref state) < monthlyBusFail)
                    {
                        alive = false;
                        break;
                    }

                    if (!radiationPass && Next01(ref state) < 0.06)
                    {
                        alive = false;
                        break;
                    }

                    if (!powerPass && Next01(ref state) < 0.04)   // power-negative spacecraft sheds instruments
                    {
                        alive = false;
                        break;
                    }

                    dose += MissionConstants.RadiationKradPerYear / 12.0;

                    if (Next01(ref state) < 0.02)                       // solar particle event
                        lost += tolerance < 30.0 ? 0.12 : 0.03;

                    if (Next01(ref state) < 0.01)                       // reaction wheel failure
                    {
                        if (!redundancy)
                        {
                            alive = false;
                            break;
                        }
                        lost += 0.02;
                    }

                    if (Next01(ref state) < 0.03) lost += 0.028;         // safe mode, one week
                }

                if (!alive)
                {
                    fail++;
                    continue;
                }

                double fraction = baseFraction * (1.0 - lost);
                if (fraction < 0.0) fraction = 0.0;
                if (fraction > 1.0) fraction = 1.0;

                double points = 30.0 + sciPotential * fraction + costPoints + powerPoints;
                totalPoints += points;

                if (points >= MissionConstants.PointsFull && costPass && massPass)
                    full++;
                else if (points >= MissionConstants.PointsPartial)
                    partial++;
                else
                    fail++;
            }

            result.FullCount = full;
            result.PartialCount = partial;
            result.FailCount = fail;
            result.ExpectedPoints = (float)(totalPoints / runs);
            return result;
        }

        static double Next01(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (state & 0x00FFFFFFu) / 16777216.0;
        }
    }
}
