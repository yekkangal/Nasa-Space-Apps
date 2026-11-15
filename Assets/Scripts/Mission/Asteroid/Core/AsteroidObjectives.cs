// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections.Generic;

namespace MissionArchitect
{
    public enum AsteroidObjectiveState
    {
        Pending = 0,
        NotMet = 1,
        Met = 2
    }

    public sealed class AsteroidObjectiveStatus
    {
        public string Id;
        public string Title;
        public string MetricLabel;
        public string Requirement;
        public string CurrentValue;
        public string Why;
        public AsteroidObjectiveState State;
        public float Points;
        public float EarnedPoints;
        public bool Mandatory;
    }

    public static class AsteroidObjectives
    {
        public static AsteroidObjectiveStatus[] Evaluate(AsteroidReport report, AsteroidDefinition definition)
        {
            var def = definition ?? new AsteroidDefinition();
            var objectives = def.Objectives;
            var result = new AsteroidObjectiveStatus[objectives.Length];

            for (int i = 0; i < objectives.Length; i++)
            {
                var o = objectives[i];
                var st = new AsteroidObjectiveStatus
                {
                    Id = o.Id,
                    Title = o.Title,
                    MetricLabel = o.MetricLabel,
                    Requirement = o.Requirement,
                    Why = o.Why,
                    Points = o.Points,
                    Mandatory = o.Mandatory,
                    State = AsteroidObjectiveState.Pending,
                    CurrentValue = "-"
                };

                if (report == null)
                {
                    result[i] = st;
                    continue;
                }

                switch (o.Kind)
                {
                    case AsteroidObjectiveKind.Arrival:
                        st.State = report.Level1 ? AsteroidObjectiveState.Met : AsteroidObjectiveState.NotMet;
                        st.EarnedPoints = report.Level1 ? AsteroidConstants.Level1Points : 0f;
                        st.CurrentValue = report.Trajectory.TypeName + " // " + report.Mass.WetKg.ToString("0") +
                                          " kg";
                        break;

                    case AsteroidObjectiveKind.Science:
                        st.EarnedPoints = report.ScienceEarned;
                        st.State = report.Level2 ? AsteroidObjectiveState.Met : AsteroidObjectiveState.NotMet;
                        st.CurrentValue = report.ScienceEarned.ToString("0") + " / " +
                                          report.SciencePotential.ToString("0") + " pts  (" +
                                          (report.Comms.Fraction * 100.0).ToString("0") + "% data)";
                        break;

                    case AsteroidObjectiveKind.Sample:
                        st.EarnedPoints = report.SamplePoints;
                        st.State = report.Level3 ? AsteroidObjectiveState.Met : AsteroidObjectiveState.NotMet;
                        st.CurrentValue = report.ExpectedGrams + " g  //  target " +
                                          AsteroidConstants.SampleRequirementG.ToString("0") + " g";
                        break;

                    case AsteroidObjectiveKind.Return:
                        st.EarnedPoints = report.Level4 ? AsteroidConstants.Level4Points : 0f;
                        st.State = report.Level4 ? AsteroidObjectiveState.Met : AsteroidObjectiveState.NotMet;
                        st.CurrentValue = (report.Capsule.SurvivalProbability * 100.0).ToString("0") + "% capsule";
                        break;

                    case AsteroidObjectiveKind.Budget:
                        st.State = report.BudgetPass ? AsteroidObjectiveState.Met : AsteroidObjectiveState.NotMet;
                        st.EarnedPoints = report.BudgetPass ? AsteroidConstants.BudgetPoints : 0f;
                        st.CurrentValue = report.Cost.TotalM.ToString("0") + " / " +
                                          report.Cost.CapM.ToString("0") + " $M";
                        break;
                }

                result[i] = st;
            }

            return result;
        }

        public static float TotalPoints(AsteroidObjectiveStatus[] statuses)
        {
            if (statuses == null) return 0f;
            float p = 0f;
            for (int i = 0; i < statuses.Length; i++) p += statuses[i].EarnedPoints;
            return p;
        }

        public static bool Met(AsteroidObjectiveStatus[] statuses, string id)
        {
            if (statuses == null) return false;
            for (int i = 0; i < statuses.Length; i++)
                if (statuses[i].Id == id) return statuses[i].State == AsteroidObjectiveState.Met;
            return false;
        }

        public static int Rating(bool s1, bool s4, double grams, double dataFraction, bool budget)
        {
            if (s1 && s4 && grams >= AsteroidConstants.SampleRequirementG && budget) return 2;
            if (s1 && ((s4 && grams > 0.0) || dataFraction >= 0.5)) return 1;
            return 0;
        }

        public static List<string> AutoTips(AsteroidReport report)
        {
            var tips = new List<string>();
            if (report == null) return tips;

            if (!report.Mass.Pass)
                tips.Add("MASS: wet mass " + report.Mass.WetKg.ToString("0") + " kg exceeds the " +
                         report.Mass.LimitKg.ToString("0") + " kg limit. Use EGA, a smaller bus or a higher-Isp engine.");
            if (!report.Cost.Pass)
                tips.Add("BUDGET: " + report.Cost.TotalM.ToString("0") +
                         " $M against a 1,050 $M cap. Cheaper sampler, capsule, bus or launcher.");
            if (!report.Power.Pass)
                tips.Add("POWER: array " + report.Power.ArrayW.ToString("0") + " W against a " +
                         report.Power.LoadW.ToString("0") + " W load at this distance. Add array area or cut instruments.");
            if (report.Comms.Fraction < 0.5)
                tips.Add("DATA: only " + (report.Comms.Fraction * 100.0).ToString("0") +
                         "% of mapping data returns. A larger X-band dish or more RF power helps.");
            if (report.ExpectedGrams < AsteroidConstants.SampleRequirementG)
                tips.Add("SAMPLE: expected " + report.ExpectedGrams + " g. Use TAGSAM, Nightingale, 10 cm/s contact " +
                         "and optical navigation, or add an attempt.");
            if (!report.Capsule.Pass)
                tips.Add("RETURN: the " + report.Capsule.Name + " is marginal at 12.4 km/s. Choose a heavier heat shield.");
            if (report.Target.SrpRatio > AsteroidConstants.StabilityLimit)
                tips.Add("ORBIT: solar radiation pressure exceeds 5% of gravity. Fly lower or use a smaller array.");
            if (!report.Trajectory.Ega)
                tips.Add("TRAJECTORY: direct transfer costs C3 and delta-v. Earth gravity assist trades time for both.");
            if (tips.Count == 0)
                tips.Add("Design is balanced: trajectory, sample, capsule and budget all close.");
            return tips;
        }
    }
}
