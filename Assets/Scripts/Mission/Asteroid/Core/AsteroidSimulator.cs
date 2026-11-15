// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;
using System.Collections.Generic;

namespace MissionArchitect
{
    public struct AsteroidMonteCarloResult
    {
        public int Runs;
        public int FullCount;
        public int PartialCount;
        public int FailCount;
        public float ExpectedPoints;
        public float ExpectedGrams;

        public float FullProbability => Runs > 0 ? (float)FullCount / Runs : 0f;
        public float PartialProbability => Runs > 0 ? (float)PartialCount / Runs : 0f;
        public float FailProbability => Runs > 0 ? (float)FailCount / Runs : 0f;
        public float SuccessRate => FullProbability + PartialProbability;
    }

    public struct AsteroidTimelineEvent
    {
        public string Phase;
        public string Text;
        public float Progress;     // 0..1
        public bool Alarm;

        public AsteroidTimelineEvent(string phase, string text, float progress, bool alarm)
        {
            Phase = phase;
            Text = text;
            Progress = progress;
            Alarm = alarm;
        }
    }

    public sealed class AsteroidTimeline
    {
        public List<AsteroidTimelineEvent> Events = new List<AsteroidTimelineEvent>();
        public bool Arrived;
        public bool Tagged;
        public bool CapsuleRecovered;
        public double Grams;
        public float DataFraction;
        public float Points;
        public int Rating;
        public bool Level1;
        public bool Level2;
        public bool Level3;
        public bool Level4;
        public List<string> Tips = new List<string>();
        public AsteroidReport Report;
        public float[] ObjectivePoints;
    }

    /// <summary>Asteroid timeline and allocation-free 1000-run Monte Carlo.</summary>
    public static class AsteroidSimulator
    {
        public static AsteroidTimeline RunTimeline(AsteroidDesign design, uint seed)
        {
            var report = AsteroidEngineeringModel.Evaluate(design);
            var timeline = new AsteroidTimeline { Report = report };
            var rng = new Random(unchecked((int)seed));

            timeline.Events.Add(new AsteroidTimelineEvent("TARGET",
                report.Target.Name + ": " + report.Target.DiameterM.ToString("0") + " m, escape " +
                report.Target.EscapeVelocityMps.ToString("0.00") + " m/s, " +
                report.Target.MeanAu.ToString("0.00") + " AU.", 0.02f, false));

            if (!report.Mass.Pass)
            {
                timeline.Events.Add(new AsteroidTimelineEvent("LAUNCH",
                    "Wet mass " + report.Mass.WetKg.ToString("0") + " kg exceeds the " +
                    report.Mass.LimitKg.ToString("0") + " kg limit.", 0.03f, true));
                Finalize(timeline, report);
                return timeline;
            }

            if (!report.Target.OrbitPass)
            {
                timeline.Events.Add(new AsteroidTimelineEvent("PROX-OPS",
                    "The selected orbit is unstable against solar radiation pressure.", 0.03f, true));
                Finalize(timeline, report);
                return timeline;
            }

            if (rng.NextDouble() > report.Vehicle.Reliability)
            {
                timeline.Events.Add(new AsteroidTimelineEvent("LAUNCH", "Launch vehicle failure.", 0.05f, true));
                Finalize(timeline, report);
                return timeline;
            }
            timeline.Events.Add(new AsteroidTimelineEvent("LAUNCH",
                report.Vehicle.Name + " lifts " + report.Mass.WetKg.ToString("0") + " kg at C3 " +
                report.Trajectory.C3Km2S2.ToString("0.0") + ".", 0.08f, false));

            if (report.Trajectory.Ega)
            {
                if (rng.NextDouble() > report.Reliability.EgaSuccess)
                {
                    timeline.Events.Add(new AsteroidTimelineEvent("EGA", "Earth gravity assist failed.", 0.12f, true));
                    Finalize(timeline, report);
                    return timeline;
                }
                timeline.Events.Add(new AsteroidTimelineEvent("EGA",
                    "Earth gravity assist complete; " + report.Trajectory.CruiseDays.ToString("0") +
                    " day cruise.", 0.14f, false));
            }
            else
            {
                timeline.Events.Add(new AsteroidTimelineEvent("CRUISE",
                    "Direct transfer; " + report.Trajectory.CruiseDays.ToString("0") + " day cruise.", 0.14f, false));
            }

            if (rng.NextDouble() > report.Reliability.ArrivalSuccess)
            {
                timeline.Events.Add(new AsteroidTimelineEvent("ARRIVAL", "Rendezvous failed - asteroid missed.", 0.2f, true));
                Finalize(timeline, report);
                return timeline;
            }

            timeline.Arrived = true;
            timeline.Events.Add(new AsteroidTimelineEvent("ARRIVAL",
                "Rendezvous at " + report.Target.OrbitRadiusM.ToString("0") + " m; period " +
                report.Target.OrbitPeriodHours.ToString("0.0") + " h.", 0.25f, false));

            int missionMonths = (int)Math.Ceiling(report.Trajectory.MissionYears * 12.0);
            double survival = Math.Pow(1.0 - AsteroidConstants.SpacecraftFailurePerMonth, missionMonths);
            if (rng.NextDouble() > survival)
            {
                timeline.Events.Add(new AsteroidTimelineEvent("SPACECRAFT",
                    "Spacecraft failure during the long cruise/proximity campaign.", 0.3f, true));
                Finalize(timeline, report);
                return timeline;
            }

            if (!report.Power.Pass)
                timeline.Events.Add(new AsteroidTimelineEvent("POWER",
                    "Power negative; science and TAG capability degraded.", 0.3f, true));

            double data = report.Comms.Fraction * (report.Power.Pass ? 1.0 : 0.5);
            if (data > 1.0) data = 1.0;

            timeline.Events.Add(new AsteroidTimelineEvent("SURVEY",
                "Global mapping over " + design.CampaignYears.ToString("0.0") + " years; " +
                (data * 100.0).ToString("0") + "% of data returned.", 0.4f, false));

            double grams = 0.0;
            double sigma = report.Sampler.Sigma;
            double baseGrams = report.Tag.PerAttemptGrams / Math.Exp(sigma * sigma * 0.5);
            int attempts = Math.Max(1, Math.Min(AsteroidConstants.MaxTagAttempts, design.Attempts));

            for (int i = 1; i <= attempts; i++)
            {
                bool success = rng.NextDouble() < report.Tag.SuccessProbability;
                if (success)
                {
                    double z = NextGaussian(rng);
                    double sample = baseGrams * Math.Exp(sigma * z);
                    if (sample < 0.0) sample = 0.0;
                    if (sample > 300.0) sample = 300.0;
                    grams += sample;
                    timeline.Events.Add(new AsteroidTimelineEvent("TAG " + i,
                        "Touch-and-go success at " + report.Site.Name + ": " +
                        sample.ToString("0.0") + " g.", 0.45f + i * 0.05f, false));
                }
                else
                {
                    timeline.Events.Add(new AsteroidTimelineEvent("TAG " + i,
                        "TAG attempt " + i + " failed - bounce or boulder collision.", 0.45f + i * 0.05f, true));
                }
            }

            timeline.Grams = grams;
            timeline.Tagged = grams > 0.0;
            timeline.DataFraction = (float)data;

            timeline.Events.Add(new AsteroidTimelineEvent("DEPARTURE",
                "Departure burn complete; " + grams.ToString("0") + " g stowed.", 0.72f, false));

            bool capsule = rng.NextDouble() < report.Capsule.SurvivalProbability;
            timeline.CapsuleRecovered = capsule;
            if (capsule)
                timeline.Events.Add(new AsteroidTimelineEvent("RE-ENTRY",
                    "Capsule recovered with " + grams.ToString("0") + " g.", 0.92f, false));
            else
                timeline.Events.Add(new AsteroidTimelineEvent("RE-ENTRY",
                    "Capsule failed at 12.4 km/s re-entry.", 0.92f, true));

            Finalize(timeline, report);
            return timeline;
        }

        static void Finalize(AsteroidTimeline timeline, AsteroidReport report)
        {
            var objectives = AsteroidObjectives.Evaluate(report, report.Definition);
            timeline.ObjectivePoints = new float[objectives.Length];
            for (int i = 0; i < objectives.Length; i++)
                timeline.ObjectivePoints[i] = objectives[i].EarnedPoints;

            if (!timeline.Arrived)
            {
                for (int i = 0; i < timeline.ObjectivePoints.Length; i++) timeline.ObjectivePoints[i] = 0f;
                timeline.Points = 0f;
                timeline.Rating = 0;
                timeline.Tips = AsteroidObjectives.AutoTips(report);
                return;
            }

            float science = report.SciencePotential * timeline.DataFraction;
            float samplePoints = AsteroidEngineeringModel.SamplePoints((float)timeline.Grams);
            if (!timeline.CapsuleRecovered) timeline.Level4 = false;

            timeline.ObjectivePoints[0] = AsteroidConstants.Level1Points;
            if (timeline.ObjectivePoints.Length > 1) timeline.ObjectivePoints[1] = science;
            if (timeline.ObjectivePoints.Length > 2) timeline.ObjectivePoints[2] = samplePoints;
            if (timeline.ObjectivePoints.Length > 3) timeline.ObjectivePoints[3] =
                timeline.CapsuleRecovered ? AsteroidConstants.Level4Points : 0f;
            if (timeline.ObjectivePoints.Length > 4) timeline.ObjectivePoints[4] =
                report.BudgetPass ? AsteroidConstants.BudgetPoints : 0f;

            float points = 0f;
            for (int i = 0; i < timeline.ObjectivePoints.Length; i++) points += timeline.ObjectivePoints[i];

            timeline.Level1 = true;
            timeline.Level2 = timeline.DataFraction >= 0.5f;
            timeline.Level3 = timeline.Grams >= AsteroidConstants.SampleRequirementG;
            timeline.Level4 = timeline.CapsuleRecovered;
            timeline.Points = points;
            timeline.Rating = AsteroidObjectives.Rating(true, timeline.CapsuleRecovered,
                timeline.Grams, timeline.DataFraction, report.BudgetPass);
            timeline.Tips = AsteroidObjectives.AutoTips(report);
        }

        public static AsteroidMonteCarloResult MonteCarlo(AsteroidDesign design, int runs, uint seed)
        {
            var report = AsteroidEngineeringModel.Evaluate(design);
            var result = new AsteroidMonteCarloResult { Runs = runs };
            if (runs <= 0) return result;

            bool massPass = report.Mass.Pass;
            bool orbitPass = report.Target.OrbitPass;
            bool budgetPass = report.BudgetPass;
            bool powerPass = report.Power.Pass;
            double launchRel = report.Vehicle.Reliability;
            double egaSuccess = report.Trajectory.Ega ? report.Reliability.EgaSuccess : 1.0;
            double arrivalSuccess = report.Reliability.ArrivalSuccess;
            double tagSuccess = report.Tag.SuccessProbability;
            double capsuleSurvival = report.Capsule.SurvivalProbability;
            double dataFraction = report.Comms.Fraction * (powerPass ? 1.0 : 0.5);
            if (dataFraction > 1.0) dataFraction = 1.0;
            float potential = report.SciencePotential;
            double sigma = report.Sampler.Sigma;
            double baseGrams = report.Tag.PerAttemptGrams / Math.Exp(sigma * sigma * 0.5);
            int attempts = Math.Max(1, Math.Min(AsteroidConstants.MaxTagAttempts, design.Attempts));
            int months = (int)Math.Ceiling(report.Trajectory.MissionYears * 12.0);

            int full = 0, partial = 0, fail = 0;
            double totalPoints = 0, totalGrams = 0;

            for (int run = 0; run < runs; run++)
            {
                uint state = seed + (uint)(run * 2654435761u);
                if (state == 0) state = 0x9E3779B9u;

                if (!massPass || !orbitPass || Next01(ref state) > launchRel)
                {
                    fail++;
                    continue;
                }

                if (Next01(ref state) > egaSuccess || Next01(ref state) > arrivalSuccess)
                {
                    fail++;
                    continue;
                }

                bool alive = true;
                double loss = 0.0;
                for (int m = 0; m < months; m++)
                {
                    if (Next01(ref state) < AsteroidConstants.SpacecraftFailurePerMonth)
                    {
                        alive = false;
                        break;
                    }
                    if (!powerPass && Next01(ref state) < AsteroidConstants.PowerFailurePerMonth)
                    {
                        alive = false;
                        break;
                    }
                    if (Next01(ref state) < AsteroidConstants.SafeModePerMonth)
                        loss += 0.05;
                }

                if (!alive)
                {
                    fail++;
                    continue;
                }

                double data = dataFraction * (1.0 - Math.Min(0.3, loss));
                double grams = 0.0;
                for (int a = 0; a < attempts; a++)
                {
                    if (Next01(ref state) < tagSuccess)
                    {
                        double z = NextGaussian(ref state);
                        double sample = baseGrams * Math.Exp(sigma * z);
                        if (sample < 0.0) sample = 0.0;
                        if (sample > 300.0) sample = 300.0;
                        grams += sample;
                    }
                }

                bool capsule = Next01(ref state) < capsuleSurvival;
                bool level2 = data >= 0.5;
                bool level3 = grams >= AsteroidConstants.SampleRequirementG;
                bool level4 = capsule;

                bool isFull = level4 && level3 && budgetPass;
                bool isPartial = (level4 && grams > 0.0) || level2;

                float points = AsteroidConstants.Level1Points
                               + potential * (float)data
                               + AsteroidEngineeringModel.SamplePoints((float)grams)
                               + (level4 ? AsteroidConstants.Level4Points : 0f)
                               + (budgetPass ? AsteroidConstants.BudgetPoints : 0f);

                totalPoints += points;
                totalGrams += grams;

                if (isFull) full++;
                else if (isPartial) partial++;
                else fail++;
            }

            result.FullCount = full;
            result.PartialCount = partial;
            result.FailCount = fail;
            result.ExpectedPoints = (float)(totalPoints / runs);
            result.ExpectedGrams = (float)(totalGrams / runs);
            return result;
        }

        static double Next01(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (state & 0x00FFFFFFu) / 16777216.0;
        }

        static double NextGaussian(ref uint state)
        {
            double u1 = Math.Max(1e-9, Next01(ref state));
            double u2 = Next01(ref state);
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        static double NextGaussian(Random rng)
        {
            double u1 = Math.Max(1e-9, rng.NextDouble());
            double u2 = rng.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }
    }
}
