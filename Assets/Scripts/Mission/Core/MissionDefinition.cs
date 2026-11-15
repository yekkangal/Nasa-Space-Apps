// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    public enum ObjectiveState
    {
        Pending = 0,
        NotMet = 1,
        Met = 2
    }

    public enum ObjectiveKind
    {
        Orbit = 0,
        Science = 1,
        WaterIce = 2,
        Budget = 3,
        Power = 4,
        Mass = 5
    }

    /// <summary>One mission objective: identity, requirement text and point value.</summary>
    public sealed class ObjectiveDefinition
    {
        public string Id;
        public string Title;
        public string MetricLabel;
        public string Requirement;
        public string Why;
        public float Points;
        public bool Mandatory;
        public ObjectiveKind Kind;

        public ObjectiveDefinition(string id, string title, string metric, string requirement,
            string why, float points, bool mandatory, ObjectiveKind kind)
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

    /// <summary>
    /// Data-driven mission descriptor. Only the Moon ("Ay") is playable in Phase 1;
    /// Mars and Asteroit carry placeholder values and show the offline notice.
    /// </summary>
    public sealed class MissionDefinition
    {
        public string Key;              // GameState.SelectedMission value: "Ay" | "Mars" | "Asteroit"
        public string Name;
        public string CodeName;
        public string Brief;
        public bool Playable;

        public double EarthDistanceKm;
        public double LightTimeOneWayS;
        public string TransferWindow;
        public double OrbitPeriodMin;
        public double LunarNightDays;
        public double DeltaVBudgetKmS;

        public double BudgetCapMillions;
        public double MassFractionLimit;

        public int MissionMonths;

        public ObjectiveDefinition[] Objectives;
        public string[] Tips;

        public string OfflineNotice => MissionStrings.DataLinkOffline;

        public static MissionDefinition Moon()
        {
            var def = new MissionDefinition
            {
                Key = "Ay",
                Name = "LUNAR POLAR ORBITER",
                CodeName = "MISSION MOON",
                Playable = true,
                Brief =
                    "Place a science orbiter in a 100 km polar lunar orbit. Map the surface, detect water ice " +
                    "at the poles and return the data to Earth - inside budget and launch-vehicle limits. " +
                    "Mission life after arrival: 12 months.",
                EarthDistanceKm = MissionConstants.EarthMoonDistanceKm, // src: [10] derived
                LightTimeOneWayS = MissionConstants.EarthMoonDistanceKm / MissionConstants.LightSpeedKmS, // src: [10] derived
                TransferWindow = "Chemical transfer: 3-5 days", // verify
                OrbitPeriodMin = MissionConstants.OrbitPeriodMinutes(), // src: [10] derived
                LunarNightDays = 14.75, // verify
                DeltaVBudgetKmS = 1.32, // verify
                BudgetCapMillions = 350.0, // game
                MassFractionLimit = MissionConstants.CapacityMassLimit, // game
                MissionMonths = MissionConstants.MissionMonths, // game
                Objectives = new[] // game scoring
                {
                    new ObjectiveDefinition("O1", "Reach 100 km polar orbit",
                        "ORBIT", "Mandatory - launch + TLI + LOI all succeed", 
                        "No orbit, no mission: this is the gate everything else depends on.",
                        30f, true, ObjectiveKind.Orbit),
                    new ObjectiveDefinition("O2", "Science return",
                        "POINTS x DATA", "Each instrument earns its points x fraction of data downlinked (cap 1)",
                        "Instruments only pay off if the downlink can actually carry their data home.",
                        0f, false, ObjectiveKind.Science),
                    new ObjectiveDefinition("O3", "Water-ice detection",
                        "NEUTRON SPEC", "Neutron spectrometer installed and returning data",
                        "Hydrogen at the poles shows up as a neutron suppression - only that instrument sees it.",
                        0f, false, ObjectiveKind.WaterIce),
                    new ObjectiveDefinition("O4", "Budget",
                        "$M TOTAL", "Total cost <= 350 $M",
                        "Every kilogram and every instrument is money; the program cap is hard.",
                        10f, false, ObjectiveKind.Budget),
                    new ObjectiveDefinition("O5", "Power margin",
                        "% WORST MODE", ">= 15% margin in every mode, battery covers eclipse",
                        "A power-negative spacecraft loses instruments, then the mission.",
                        10f, false, ObjectiveKind.Power),
                    new ObjectiveDefinition("O6", "Mass limit",
                        "WET / CAPACITY", "Wet mass <= 95% of launch vehicle capacity",
                        "The 5% reserve protects the launch from mass-growth during integration.",
                        0f, true, ObjectiveKind.Mass)
                },
                Tips = new[]
                {
                    "More instruments means more mass and power, a bigger array and battery, a heavier spacecraft and a bigger rocket - and a bigger bill.",
                    "S-band is cheap but slow: it usually cannot downlink imaging data, so the science points never arrive.",
                    "A bipropellant stage is heavier than a monopropellant one but its Isp is far better for a 1.32 km/s lunar budget.",
                    "Electric propulsion saves propellant mass but the spiral takes months, adds belt radiation dose and costs operations money.",
                    "Rad-hard parts cost more and weigh more, but COTS parts fail once the dose climbs past 10 krad.",
                    "Redundancy costs 8% mass and 10% cost - worth it only when the design is otherwise comfortable.",
                    "The Deep Space ground station has 10 h/day of contact but costs 2.0 $M per month; the University station is nearly free but slow.",
                    "FULL needs 95 points with O1, O4 and O6 met: a balanced design beats a maxed-out one."
                }
            };
            return def;
        }

        public static MissionDefinition Mars()
        {
            var def = Placeholder("Mars", "MARS ORBITER", "MISSION MARS", 225000000.0, 687);
            return def;
        }

        public static MissionDefinition Asteroid()
        {
            var def = Placeholder("Asteroit", "ASTEROID PROSPECTOR", "MISSION ASTEROID", 180000000.0, 540);
            return def;
        }

        static MissionDefinition Placeholder(string key, string name, string code, double distanceKm, int months)
        {
            var def = new MissionDefinition
            {
                Key = key,
                Name = name,
                CodeName = code,
                Playable = false,
                Brief = "Mission data arrives in Phase 2.",
                EarthDistanceKm = distanceKm,
                LightTimeOneWayS = distanceKm / MissionConstants.LightSpeedKmS,
                TransferWindow = "TBD",
                OrbitPeriodMin = 0.0,
                LunarNightDays = 0.0,
                DeltaVBudgetKmS = 0.0,
                BudgetCapMillions = 0.0,
                MassFractionLimit = MissionConstants.CapacityMassLimit,
                MissionMonths = months,
                Objectives = new ObjectiveDefinition[0],
                Tips = new string[0]
            };
            return def;
        }

        static MissionDefinition _moon, _mars, _asteroid;

        public static MissionDefinition MoonDefinition => _moon ?? (_moon = Moon());
        public static MissionDefinition MarsDefinition => _mars ?? (_mars = Mars());
        public static MissionDefinition AsteroidDefinition => _asteroid ?? (_asteroid = Asteroid());

        public static MissionDefinition Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return MoonDefinition;
            switch (key)
            {
                case "Mars": return MarsDefinition;
                case "Asteroit":
                case "Asteroid": return AsteroidDefinition;
                case "Ay":
                case "Moon": return MoonDefinition;
                default: return MoonDefinition;
            }
        }
    }
}
