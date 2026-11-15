// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System.Collections.Generic;

namespace MissionArchitect
{
    public sealed class MarsObjectiveStatus
    {
        public string Id;
        public string Title;
        public string MetricLabel;
        public string Requirement;
        public string CurrentValue;
        public string Why;
        public MarsObjectiveState State;
        public float Points;
        public float EarnedPoints;
        public bool Mandatory;
    }

    public enum MarsObjectiveState
    {
        Pending = 0,
        NotMet = 1,
        Met = 2
    }

    /// <summary>Single source of truth for the live L1-L4 + budget levels.</summary>
    public static class MarsObjectives
    {
        public static MarsObjectiveStatus[] Evaluate(MarsReport report, MarsDefinition definition)
        {
            var def = definition ?? new MarsDefinition();
            var objectives = def.Objectives;
            var result = new MarsObjectiveStatus[objectives.Length];

            for (int i = 0; i < objectives.Length; i++)
            {
                var o = objectives[i];
                var st = new MarsObjectiveStatus
                {
                    Id = o.Id,
                    Title = o.Title,
                    MetricLabel = o.MetricLabel,
                    Requirement = o.Requirement,
                    Why = o.Why,
                    Points = o.Points,
                    Mandatory = o.Mandatory,
                    State = MarsObjectiveState.Pending,
                    CurrentValue = "-"
                };

                if (report == null)
                {
                    result[i] = st;
                    continue;
                }

                switch (o.Kind)
                {
                    case MarsObjectiveKind.Landing:
                        st.State = report.Level1 ? MarsObjectiveState.Met : MarsObjectiveState.NotMet;
                        st.EarnedPoints = report.Level1 ? MarsConstants.Level1Points : 0f;
                        st.CurrentValue = "P(EDL) " + (report.Edl.SuccessProbability * 100.0).ToString("0")
                                          + "%  /  " + report.Mass.LandedKg.ToString("0") + " kg landed";
                        break;

                    case MarsObjectiveKind.Science:
                        st.EarnedPoints = report.ScienceEarned;
                        st.State = report.Level2 ? MarsObjectiveState.Met : MarsObjectiveState.NotMet;
                        st.CurrentValue = report.ScienceEarned.ToString("0") + " / "
                                          + report.SciencePotential.ToString("0") + " pts  ("
                                          + (report.Data.Fraction * 100.0).ToString("0") + "% data)";
                        break;

                    case MarsObjectiveKind.Samples:
                        st.EarnedPoints = System.Math.Min(MarsConstants.Level3MaxPoints,
                            report.ExpectedSamples * 3f);
                        st.State = report.Level3 ? MarsObjectiveState.Met : MarsObjectiveState.NotMet;
                        st.CurrentValue = report.ExpectedSamples + " / " + MarsConstants.SampleTarget + " samples";
                        break;

                    case MarsObjectiveKind.Extended:
                        st.State = report.Level4 ? MarsObjectiveState.Met : MarsObjectiveState.NotMet;
                        st.EarnedPoints = report.Level4 ? MarsConstants.Level4Points : 0f;
                        st.CurrentValue = (report.Power.WorstMargin * 100.0).ToString("0")
                                          + "% worst margin  /  sol 668: "
                                          + (report.Level4 ? "ALIVE" : "AT RISK");
                        break;

                    case MarsObjectiveKind.Budget:
                        st.State = report.BudgetPass ? MarsObjectiveState.Met : MarsObjectiveState.NotMet;
                        st.EarnedPoints = report.BudgetPass ? MarsConstants.BudgetPoints : 0f;
                        st.CurrentValue = report.Cost.TotalM.ToString("0") + " / "
                                          + report.Cost.CapM.ToString("0") + " $M";
                        break;
                }

                result[i] = st;
            }

            return result;
        }

        public static float TotalPoints(MarsObjectiveStatus[] statuses)
        {
            if (statuses == null) return 0f;
            float p = 0f;
            for (int i = 0; i < statuses.Length; i++) p += statuses[i].EarnedPoints;
            return p;
        }

        public static bool Met(MarsObjectiveStatus[] statuses, string id)
        {
            if (statuses == null) return false;
            for (int i = 0; i < statuses.Length; i++)
                if (statuses[i].Id == id) return statuses[i].State == MarsObjectiveState.Met;
            return false;
        }

        /// <summary>0 = FAIL, 1 = PARTIAL, 2 = FULL (spec section 2 rating).</summary>
        public static int Rating(bool level1, bool level2, bool level3, bool budget)
        {
            if (level1 && level2 && level3 && budget) return 2;
            if (level1) return 1;
            return 0;
        }

        public static List<string> AutoTips(MarsReport report)
        {
            var tips = new List<string>();
            if (report == null) return tips;

            if (!report.Mass.Pass)
                tips.Add("MASS: launch mass is " + report.Mass.LaunchKg.ToString("0") +
                         " kg against a " + report.Mass.LimitKg.ToString("0") +
                         " kg limit. Shrink the rover, the instruments or pick a larger vehicle.");
            if (!report.Edl.Pass)
                tips.Add("EDL: P(landing) is only " + (report.Edl.SuccessProbability * 100.0).ToString("0") +
                         "%. Use a sky crane, a safer site, TRN or a lighter rover.");
            if (!report.Power.Pass)
                tips.Add("POWER: worst-case generation is " + report.Power.WorstGenerationWhSol.ToString("0") +
                         " Wh/sol against a " + report.Power.LoadWhSol.ToString("0") +
                         " Wh load. MMRTG, more array area or fewer heaters.");
            if (!report.Data.Pass)
                tips.Add("DATA: only " + (report.Data.Fraction * 100.0).ToString("0") +
                         "% of the science data comes back. Add relay passes or reduce instrument data.");
            if (report.ExpectedSamples < MarsConstants.SampleTarget)
                tips.Add("SAMPLES: " + report.ExpectedSamples + " of " + MarsConstants.SampleTarget +
                         " planned. Raise the drilling share of the daily policy.");
            if (!report.Cost.Pass)
                tips.Add("BUDGET: " + report.Cost.TotalM.ToString("0") +
                         " $M against a 2,700 $M cap. Cheaper bus, launcher or radiation class.");
            if (report.Transfer.OffsetDays != 0)
                tips.Add("WINDOW: launching " + report.Transfer.OffsetDays +
                         " days off optimum costs C3 and capacity. Try the optimal date.");
            if (report.Site.Id != "delta" && report.Edl.Pass)
                tips.Add("SITE: Delta Crater returns 15% more science for the same instruments if EDL risk allows.");
            if (tips.Count == 0)
                tips.Add("Design is balanced: landing, science, samples and budget all line up.");
            return tips;
        }
    }
}
