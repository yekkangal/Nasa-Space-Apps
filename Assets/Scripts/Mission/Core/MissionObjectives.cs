// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;
using System.Collections.Generic;

namespace MissionArchitect
{
    public sealed class ObjectiveStatus
    {
        public string Id;
        public string Title;
        public string MetricLabel;
        public string Requirement;
        public string CurrentValue;
        public string Why;
        public ObjectiveState State;
        public float Points;
        public float EarnedPoints;
        public bool Mandatory;
    }

    /// <summary>
    /// Single source of truth for live objective status. The terminal BRIEFING tab and the
    /// handbook both consume this - no duplicated rules anywhere else (spec section 3).
    /// </summary>
    public static class MissionObjectives
    {
        public static ObjectiveStatus[] Evaluate(EngineeringReport report, MissionDefinition definition)
        {
            var def = definition ?? MissionDefinition.MoonDefinition;
            var objectives = def.Objectives ?? new ObjectiveDefinition[0];
            var result = new ObjectiveStatus[objectives.Length];

            for (int i = 0; i < objectives.Length; i++)
            {
                var o = objectives[i];
                var st = new ObjectiveStatus
                {
                    Id = o.Id,
                    Title = o.Title,
                    MetricLabel = o.MetricLabel,
                    Requirement = o.Requirement,
                    Why = o.Why,
                    Points = o.Points,
                    EarnedPoints = 0f,
                    Mandatory = o.Mandatory,
                    State = ObjectiveState.Pending,
                    CurrentValue = "-"
                };

                if (report == null || !def.Playable)
                {
                    result[i] = st;
                    continue;
                }

                switch (o.Kind)
                {
                    case ObjectiveKind.Orbit:
                        st.State = report.OrbitFeasible ? ObjectiveState.Met : ObjectiveState.NotMet;
                        st.CurrentValue = report.OrbitFeasible
                            ? "ORBIT ACHIEVABLE"
                            : (report.Vehicle.ReachesMoon ? "MASS OVER LIMIT" : "VEHICLE CANNOT REACH MOON");
                        st.EarnedPoints = report.OrbitFeasible ? 30f : 0f;
                        break;

                    case ObjectiveKind.Science:
                        {
                            double frac = report.Data != null ? report.Data.Fraction : 0.0;
                            st.EarnedPoints = report.SciencePoints * (float)frac;
                            st.State = frac >= 0.999 && report.SciencePoints > 0f
                                ? ObjectiveState.Met : ObjectiveState.NotMet;
                            st.CurrentValue = report.SciencePoints.ToString("0")
                                              + " PTS  /  " + (frac * 100.0).ToString("0") + "% DATA";
                            break;
                        }

                    case ObjectiveKind.WaterIce:
                        {
                            bool neutron = report.Design != null && report.Design.HasInstrument("ns");
                            double frac = report.Data != null ? report.Data.Fraction : 0.0;
                            bool met = neutron && frac >= 0.5;
                            st.State = met ? ObjectiveState.Met : ObjectiveState.NotMet;
                            st.CurrentValue = neutron
                                ? "NEUTRON SPEC / " + (frac * 100.0).ToString("0") + "% DATA"
                                : "NEUTRON SPEC NOT INSTALLED";
                            break;
                        }

                    case ObjectiveKind.Budget:
                        st.State = report.Cost.Pass ? ObjectiveState.Met : ObjectiveState.NotMet;
                        st.EarnedPoints = report.Cost.Pass ? 10f : 0f;
                        st.CurrentValue = report.Cost.TotalMillions.ToString("0.0") + " / "
                                          + report.Cost.CapMillions.ToString("0") + " $M";
                        break;

                    case ObjectiveKind.Power:
                        st.State = report.Power.Pass ? ObjectiveState.Met : ObjectiveState.NotMet;
                        st.EarnedPoints = report.Power.Pass ? 10f : 0f;
                        st.CurrentValue = (report.Power.MinMargin * 100.0).ToString("0") + "% min"
                                          + (report.Power.BatteryPass ? " / BATT OK" : " / BATT SHORT");
                        break;

                    case ObjectiveKind.Mass:
                        st.State = report.Mass.Pass ? ObjectiveState.Met : ObjectiveState.NotMet;
                        st.CurrentValue = report.Mass.WetKg.ToString("0") + " / "
                                          + report.Mass.LimitKg.ToString("0") + " kg";
                        break;
                }

                result[i] = st;
            }

            return result;
        }

        public static float TotalPoints(ObjectiveStatus[] statuses)
        {
            if (statuses == null) return 0f;
            float p = 0f;
            for (int i = 0; i < statuses.Length; i++) p += statuses[i].EarnedPoints;
            return p;
        }

        /// <summary>Points reachable with the currently installed instruments.</summary>
        public static float MaxPoints(EngineeringReport report)
        {
            if (report == null) return 0f;
            return 30f + report.SciencePoints + 10f + 10f;
        }

        public static bool Met(ObjectiveStatus[] statuses, string id)
        {
            if (statuses == null) return false;
            for (int i = 0; i < statuses.Length; i++)
                if (statuses[i].Id == id) return statuses[i].State == ObjectiveState.Met;
            return false;
        }

        /// <summary>0 = FAIL, 1 = PARTIAL, 2 = FULL (spec section 3 rating rules).</summary>
        public static int Rating(int points, bool o1, bool o4, bool o6)
        {
            if (points >= MissionConstants.PointsFull && o1 && o4 && o6) return 2;
            if (points >= MissionConstants.PointsPartial && o1) return 1;
            return 0;
        }

        public static int Rating(ObjectiveStatus[] statuses)
        {
            float pts = TotalPoints(statuses);
            return Rating((int)pts, Met(statuses, "O1"), Met(statuses, "O4"), Met(statuses, "O6"));
        }

        /// <summary>Design tips pointing at the weakest constraint, for the debrief.</summary>
        public static List<string> AutoTips(EngineeringReport report)
        {
            var tips = new List<string>();
            if (report == null) return tips;

            if (!report.Mass.Pass)
                tips.Add("MASS: the wet mass exceeds 95% of " + report.Vehicle.Name +
                         " capacity. Drop an instrument, shrink the array or pick a bigger launcher.");
            if (!report.Cost.Pass)
                tips.Add("BUDGET: total cost is " + report.Cost.TotalMillions.ToString("0") +
                         " $M against a 350 $M cap. Cheaper launcher, ground station or fewer instruments.");
            if (!report.Power.Pass)
            {
                if (!report.Power.BatteryPass)
                    tips.Add("POWER: the battery cannot carry the eclipse load. Choose a larger battery or a lighter mode load.");
                else
                    tips.Add("POWER: worst-mode margin is " + (report.Power.MinMargin * 100.0).ToString("0") +
                             "%. Add array area or cut instrument power.");
            }
            if (report.Data.Fraction < 0.999)
                tips.Add("DATA LINK: only " + (report.Data.Fraction * 100.0).ToString("0") +
                         "% of science data comes home. A bigger dish, X/Ka band or a better ground station fixes it.");
            if (!report.Radiation.Pass)
                tips.Add("RADIATION: " + report.Radiation.DoseKrad.ToString("0") + " krad against " +
                         report.Radiation.ToleranceKrad.ToString("0") + " krad tolerance. Use a harder part class or a chemical transfer.");
            if (!report.Vehicle.ReachesMoon)
                tips.Add("LAUNCH: " + report.Vehicle.Name + " has no lunar TLI capability. Select Condor-5 or larger.");
            if (tips.Count == 0)
                tips.Add("Design is balanced: keep the mass and budget reserve for integration growth.");
            return tips;
        }
    }
}
