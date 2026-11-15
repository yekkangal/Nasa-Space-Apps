// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;
using System.Collections.Generic;

namespace MissionArchitect
{
    public struct MarsMonteCarloResult
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
    }

    public struct MarsTimelineEvent
    {
        public string Phase;
        public string Text;
        public int Sol;
        public bool Alarm;

        public MarsTimelineEvent(string phase, string text, int sol, bool alarm)
        {
            Phase = phase;
            Text = text;
            Sol = sol;
            Alarm = alarm;
        }
    }

    public sealed class MarsTimeline
    {
        public List<MarsTimelineEvent> Events = new List<MarsTimelineEvent>();
        public bool Landed;
        public bool Alive;
        public string FailureReason;
        public int Samples;
        public float DataFraction;
        public float Points;
        public int Rating;
        public bool Level1;
        public bool Level2;
        public bool Level3;
        public bool Level4;
        public List<string> Tips = new List<string>();
        public MarsReport Report;
        public float[] ObjectivePoints;
    }

    /// <summary>Mars timeline and allocation-free 1000-run Monte Carlo.</summary>
    public static class MarsSimulator
    {
        const int Months = 22;
        const double SolsPerMonth = 30.44;
        const int TotalSols = 668;

        public static MarsTimeline RunTimeline(MarsDesign design, uint seed)
        {
            var report = MarsEngineeringModel.Evaluate(design);
            var timeline = new MarsTimeline { Report = report };
            var rng = new Random(unchecked((int)seed));

            timeline.Events.Add(new MarsTimelineEvent("WINDOW",
                "Launch " + report.Transfer.OffsetDays + " days from optimum: C3 " +
                report.Transfer.C3Km2S2.ToString("0.0") + " km2/s2, cruise " +
                report.Transfer.CruiseDays.ToString("0") + " days.", 0, false));

            if (!report.Mass.Pass)
            {
                timeline.FailureReason = "Launch mass " + report.Mass.LaunchKg.ToString("0") +
                                         " kg exceeds the " + report.Mass.LimitKg.ToString("0") + " kg limit.";
                timeline.Events.Add(new MarsTimelineEvent("LAUNCH", timeline.FailureReason, 0, true));
                Finalize(timeline, report);
                return timeline;
            }

            if (rng.NextDouble() > report.Vehicle.Reliability)
            {
                timeline.FailureReason = "Launch vehicle failure - rover lost.";
                timeline.Events.Add(new MarsTimelineEvent("LAUNCH", timeline.FailureReason, 0, true));
                Finalize(timeline, report);
                return timeline;
            }
            timeline.Events.Add(new MarsTimelineEvent("LAUNCH",
                report.Vehicle.Name + " lifts " + report.Mass.LaunchKg.ToString("0") + " kg to Mars.", 0, false));

            if (rng.NextDouble() > report.Reliability.CruiseSuccess)
            {
                timeline.FailureReason = "Cruise stage failure during the interplanetary transit.";
                timeline.Events.Add(new MarsTimelineEvent("CRUISE", timeline.FailureReason, 0, true));
                Finalize(timeline, report);
                return timeline;
            }
            timeline.Events.Add(new MarsTimelineEvent("CRUISE",
                report.Transfer.CruiseDays.ToString("0") + " days of cruise, " +
                report.Radiation.CruiseMsv.ToString("0") + " mSv crew-free electronics dose.", 0, false));

            // ---- EDL countdown ----------------------------------------------------
            string[] edl =
            {
                "ENTRY INTERFACE - heat shield " + report.Shield.Name,
                "PEAK HEATING - thin atmosphere, no second chances",
                report.Design.Parachute ? "PARACHUTE DEPLOY" : "NO PARACHUTE - PROPULSIVE ONLY",
                report.Design.Trn ? "TERRAIN-RELATIVE NAVIGATION - hazard map locked" : "TRN OFF - flying blind",
                report.EdlSystem.Name.ToUpperInvariant() + " SEQUENCE",
                "TOUCHDOWN at " + report.Site.Name
            };
            for (int i = 0; i < edl.Length; i++)
                timeline.Events.Add(new MarsTimelineEvent("EDL " + (i + 1) + "/6", edl[i], 0, false));

            if (rng.NextDouble() > report.Edl.SuccessProbability)
            {
                timeline.FailureReason = "Landing failure at " + report.Site.Name +
                                         " - P(EDL) " + (report.Edl.SuccessProbability * 100.0).ToString("0") + "%.";
                timeline.Events.Add(new MarsTimelineEvent("EDL", timeline.FailureReason, 0, true));
                Finalize(timeline, report);
                return timeline;
            }

            timeline.Landed = true;
            timeline.Alive = true;
            timeline.Events.Add(new MarsTimelineEvent("TOUCHDOWN",
                "Rover is alive on the surface. Sol 0 begins.", 0, false));

            // ---- surface operations ----------------------------------------------
            double samples = 0.0;
            int lostSols = 0;
            double sampleRate = report.Surface.SamplesPerSol * MarsConstants.SampleSuccessChance;

            for (int m = 1; m <= Months; m++)
            {
                if (rng.NextDouble() < MarsConstants.SurfaceFailurePerMonth)
                {
                    timeline.Alive = false;
                    timeline.FailureReason = "Surface hardware failure in month " + m + ".";
                    timeline.Events.Add(new MarsTimelineEvent("SOL " + (m * 30), timeline.FailureReason, m * 30, true));
                    break;
                }

                if (report.Design.IsSolar && rng.NextDouble() < MarsConstants.DustStormMonthlyChance)
                {
                    if (!report.Power.Pass && rng.NextDouble() < 0.30)
                    {
                        timeline.Alive = false;
                        timeline.FailureReason = "Dust storm in month " + m +
                                                 " - solar generation collapsed and the battery ran out.";
                        timeline.Events.Add(new MarsTimelineEvent("SOL " + (m * 30), timeline.FailureReason, m * 30, true));
                        break;
                    }
                    lostSols += 10;
                    timeline.Events.Add(new MarsTimelineEvent("SOL " + (m * 30),
                        "DUST STORM - solar output reduced, rover sheltering.", m * 30, true));
                }

                if (rng.NextDouble() < MarsConstants.SafeModePerMonth)
                {
                    lostSols += 7;
                    timeline.Events.Add(new MarsTimelineEvent("SOL " + (m * 30),
                        "Safe mode - one week of science lost.", m * 30, true));
                }

                if (rng.NextDouble() < MarsConstants.SpeMonthlyChance)
                {
                    if (rng.NextDouble() < SpeFailureChance(report.Radiation.ToleranceKrad))
                    {
                        timeline.Alive = false;
                        timeline.FailureReason = "Solar particle event in month " + m +
                                                 " - " + report.RadiationClass.Name +
                                                 " electronics failed.";
                        timeline.Events.Add(new MarsTimelineEvent("SOL " + (m * 30),
                            timeline.FailureReason, m * 30, true));
                        break;
                    }
                    lostSols += 10;
                    timeline.Events.Add(new MarsTimelineEvent("SOL " + (m * 30),
                        "Solar particle event - electronics recovered after 10 sols.", m * 30, true));
                }

                samples += sampleRate * SolsPerMonth;

                if (m % 4 == 0)
                {
                    timeline.Events.Add(new MarsTimelineEvent("SOL " + (m * 30),
                        "Science campaign: " + Samples(samples) + " samples cached, rover nominal.", m * 30, false));
                }
            }

            timeline.Samples = Math.Max(0, (int)Math.Floor(samples));
            timeline.Level1 = true;
            timeline.Alive = timeline.Alive && report.Power.Pass;
            timeline.Level4 = timeline.Alive && report.Power.Pass;

            double loss = 1.0 - Math.Min(0.9, lostSols / (double)TotalSols);
            double fraction = report.Data.Fraction * loss * (1.0 - MarsConstants.ConjunctionDataLoss);
            timeline.DataFraction = (float)MarsConstants.Clamp(fraction, 0.0, 1.0);

            timeline.Level2 = report.SciencePotential > 0.01f
                              && timeline.DataFraction >= MarsConstants.BaselineScienceFraction;
            timeline.Level3 = timeline.Samples >= MarsConstants.SampleTarget;

            Finalize(timeline, report);
            return timeline;
        }

        static void Finalize(MarsTimeline timeline, MarsReport report)
        {
            var objectives = MarsObjectives.Evaluate(report, report.Definition);
            timeline.ObjectivePoints = new float[objectives.Length];
            for (int i = 0; i < objectives.Length; i++)
                timeline.ObjectivePoints[i] = objectives[i].EarnedPoints;

            if (!timeline.Landed)
            {
                for (int i = 0; i < timeline.ObjectivePoints.Length; i++)
                    timeline.ObjectivePoints[i] = 0f;
                timeline.Points = 0f;
                timeline.Rating = 0;
                timeline.Tips = MarsObjectives.AutoTips(report);
                return;
            }

            float science = report.SciencePotential * timeline.DataFraction;
            float samplePoints = Math.Min(MarsConstants.Level3MaxPoints, timeline.Samples * 3f);
            if (!timeline.Alive) timeline.Level4 = false;

            timeline.ObjectivePoints[0] = MarsConstants.Level1Points;
            if (timeline.ObjectivePoints.Length > 1) timeline.ObjectivePoints[1] = science;
            if (timeline.ObjectivePoints.Length > 2) timeline.ObjectivePoints[2] = samplePoints;
            if (timeline.ObjectivePoints.Length > 3) timeline.ObjectivePoints[3] =
                timeline.Level4 ? MarsConstants.Level4Points : 0f;
            if (timeline.ObjectivePoints.Length > 4) timeline.ObjectivePoints[4] =
                report.BudgetPass ? MarsConstants.BudgetPoints : 0f;

            float points = 0f;
            for (int i = 0; i < timeline.ObjectivePoints.Length; i++) points += timeline.ObjectivePoints[i];

            timeline.Points = points;
            bool full = timeline.Level1 && timeline.Level2 && timeline.Level3 && report.BudgetPass;
            bool partial = timeline.Level1 && report.SciencePotential > 0.01f && timeline.DataFraction > 0.001f;
            timeline.Rating = full ? 2 : (partial ? 1 : 0);
            timeline.Tips = MarsObjectives.AutoTips(report);
        }

        public static MarsMonteCarloResult MonteCarlo(MarsDesign design, int runs, uint seed)
        {
            var report = MarsEngineeringModel.Evaluate(design);
            var result = new MarsMonteCarloResult { Runs = runs };
            if (runs <= 0) return result;

            bool massPass = report.Mass.Pass;
            bool solar = report.Design.IsSolar;
            bool powerPass = report.Power.Pass;
            bool budgetPass = report.BudgetPass;

            double launchRel = report.Vehicle.Reliability;
            double cruiseSuccess = report.Reliability.CruiseSuccess;
            double edlProb = report.Edl.SuccessProbability;
            double sampleRate = report.Surface.SamplesPerSol * MarsConstants.SampleSuccessChance;
            double samplesPerMonth = sampleRate * SolsPerMonth;
            double baseFraction = report.Data.Fraction;
            float potential = report.SciencePotential;
            bool hasScience = potential > 0.01f;
            double tolerance = report.Radiation.ToleranceKrad;

            int full = 0, partial = 0, fail = 0;
            double totalPoints = 0;

            for (int run = 0; run < runs; run++)
            {
                uint state = seed + (uint)(run * 2654435761u);
                if (state == 0) state = 0x9E3779B9u;

                if (!massPass || Next01(ref state) > launchRel)
                {
                    fail++;
                    continue;
                }

                if (Next01(ref state) > cruiseSuccess)
                {
                    fail++;
                    continue;
                }

                if (Next01(ref state) > edlProb)
                {
                    fail++;
                    continue;
                }

                double samples = 0.0;
                int lostSols = 0;
                bool alive = true;

                for (int m = 0; m < Months; m++)
                {
                    if (Next01(ref state) < MarsConstants.SurfaceFailurePerMonth)
                    {
                        alive = false;
                        break;
                    }

                    if (solar && Next01(ref state) < MarsConstants.DustStormMonthlyChance)
                    {
                        if (!powerPass && Next01(ref state) < 0.30)
                        {
                            alive = false;
                            break;
                        }
                        lostSols += 10;
                    }

                    if (Next01(ref state) < MarsConstants.SafeModePerMonth)
                        lostSols += 7;

                    if (Next01(ref state) < MarsConstants.SpeMonthlyChance)
                    {
                        if (Next01(ref state) < SpeFailureChance(tolerance))
                        {
                            alive = false;
                            break;
                        }
                        lostSols += 10;
                    }

                    samples += samplesPerMonth;
                }

                double loss = 1.0 - Math.Min(0.9, lostSols / (double)TotalSols);
                double fraction = baseFraction * loss * (1.0 - MarsConstants.ConjunctionDataLoss);
                if (fraction < 0.0) fraction = 0.0;
                if (fraction > 1.0) fraction = 1.0;

                bool level2 = hasScience && fraction >= MarsConstants.BaselineScienceFraction;
                bool level3 = samples >= MarsConstants.SampleTarget;
                bool level4 = alive && powerPass;

                float points = MarsConstants.Level1Points
                               + potential * (float)fraction
                               + Math.Min(MarsConstants.Level3MaxPoints, (float)samples * 3f)
                               + (level4 ? MarsConstants.Level4Points : 0f)
                               + (budgetPass ? MarsConstants.BudgetPoints : 0f);
                totalPoints += points;

                if (level2 && level3 && budgetPass)
                    full++;
                else if (hasScience && fraction > 0.001)
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

        static double SpeFailureChance(double toleranceKrad)
        {
            if (toleranceKrad <= 10.0) return MarsConstants.SpeCotsFailChance;
            if (toleranceKrad <= 30.0) return MarsConstants.SpeMilFailChance;
            return MarsConstants.SpeHardFailChance;
        }

        static string Samples(double value)
        {
            return value.ToString("0");
        }
    }
}
