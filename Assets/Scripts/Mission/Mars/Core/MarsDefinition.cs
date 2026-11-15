// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    public enum MarsObjectiveKind
    {
        Landing = 0,
        Science = 1,
        Samples = 2,
        Extended = 3,
        Budget = 4
    }

    public sealed class MarsObjectiveDefinition
    {
        public string Id;
        public string Title;
        public string MetricLabel;
        public string Requirement;
        public string Why;
        public float Points;
        public bool Mandatory;
        public MarsObjectiveKind Kind;

        public MarsObjectiveDefinition(string id, string title, string metric, string requirement,
            string why, float points, bool mandatory, MarsObjectiveKind kind)
        {
            Id = id;
            Title = title;
            MetricLabel = metric;
            Requirement = requirement;
            Why = why;
            Points = points;
            Mandatory = mandatory;
            Kind = kind;
        }
    }

    /// <summary>Data-driven Mars mission descriptor (one mission, five live objectives).</summary>
    public sealed class MarsDefinition
    {
        public const string Key = "Mars";

        public string Name => "MARS ASTROBIOLOGY ROVER";
        public string CodeName => "MISSION MARS";
        public string Brief =>
            "Land a rover in a Jezero-like crater, search for signs of ancient microbial life and cache " +
            "rock samples for a future return mission. Prime mission: 1 Mars year (687 Earth days, " +
            "about 668 sols)."; // src: [3]

        public double BudgetCapM => MarsConstants.DefaultBudgetCapM;   // src: [2] 2.4 B$ build+launch + 0.3 B$ ops
        public double PrimeSols => MarsConstants.PrimeMissionSols;      // src: [3]
        public double LaunchWindowMonths => MarsConstants.LaunchWindowMonths; // src: [3]
        public double MinDistanceMkm => MarsConstants.MinEarthMarsMkm;  // src: [1]
        public double MaxDistanceMkm => MarsConstants.MaxEarthMarsMkm;  // src: [1]
        public double AverageTempF => MarsConstants.AverageTempF;       // src: [9]

        public MarsObjectiveDefinition[] Objectives
        {
            get
            {
                return new[]
                {
                    new MarsObjectiveDefinition("L1", "Safe landing",
                        "EDL %", "Entry, descent and landing succeeds - mandatory",
                        "The thin atmosphere gives parachutes almost nothing to work with; EDL is the riskiest phase.",
                        30f, true, MarsObjectiveKind.Landing),
                    new MarsObjectiveDefinition("L2", "Baseline science",
                        "INSTRUMENT PTS", "Earn at least 60% of installed instrument points from returned data",
                        "Instruments only count once their data reaches Earth - relay passes and site value decide the return.",
                        0f, false, MarsObjectiveKind.Science),
                    new MarsObjectiveDefinition("L3", "Sample caching",
                        "SAMPLES", "Cache at least 10 rock samples for a future return mission",
                        "Drilling time competes with driving and science: samples are a long-term investment.",
                        30f, false, MarsObjectiveKind.Samples),
                    new MarsObjectiveDefinition("L4", "Extended mission",
                        "SOL 668", "Rover alive and power-positive at the end of the prime mission",
                        "Dust storms, cold nights and battery ageing decide whether the rover survives a full Mars year.",
                        15f, false, MarsObjectiveKind.Extended),
                    new MarsObjectiveDefinition("B", "Budget",
                        "$M TOTAL", "Total cost <= 2,700 $M",
                        "A Perseverance-class mission costs about 2.7 B$ including launch and prime operations.",
                        10f, false, MarsObjectiveKind.Budget)
                };
            }
        }

        public string[] Tips
        {
            get
            {
                return new[]
                {
                    "Launching away from the optimal date raises C3: the rocket must push harder and carries less rover.",
                    "Sky crane lands more mass than airbags, but airbags can be the cheap route for a small rover.",
                    "MMRTG power is dust-proof but heavy and expensive; solar arrays are light and cheap until a dust storm.",
                    "Two 8-minute relay passes per sol beat direct-to-Earth by orders of magnitude - but the relay link is shared.",
                    "The Delta Crater returns 15% more science value but raises landing risk; TRN buys part of that back.",
                    "Drilling 10 samples takes hundreds of hours of the same daylight you need for driving and science.",
                    "Active heaters keep electronics alive through -81 F nights but eat the battery every single sol.",
                    "FULL requires landing, 60% of instrument science, 10 samples and the budget: balance beats maximalism."
                };
            }
        }

        public string[] Glossary
        {
            get
            {
                return new[]
                {
                    "C3: launch energy, the square of the hyperbolic excess velocity - lower is easier for the rocket.",
                    "V-INFINITY: the speed left over relative to Mars when the cruise stage arrives; the EDL system must remove it.",
                    "EDL: Entry, Descent and Landing - the seven minutes between the top of the atmosphere and touchdown.",
                    "SOL: one Martian day, 24 hours 39 minutes; the prime mission is 668 sols.",
                    "MMRTG: Multi-Mission Radioisotope Thermoelectric Generator, about 110 W continuous from Pu-238 decay.",
                    "UHF RELAY: short high-rate passes through an orbiter, about 8 minutes at up to 2 Mbps.",
                    "CONJUNCTION: the Sun sits between Earth and Mars for about two weeks and commands are blocked.",
                    "TRN: Terrain-Relative Navigation, a camera map matched to the surface to avoid landing hazards."
                };
            }
        }
    }
}
