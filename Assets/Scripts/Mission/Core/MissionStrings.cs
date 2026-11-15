// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    /// <summary>All UI text in one place (English, international competition).</summary>
    public static class MissionStrings
    {
        public const string TerminalTitle = "MISSION CONTROL TERMINAL";
        public const string TerminalBoot = "MISSION CONTROL TERMINAL ... READY";
        public const string TerminalReady = "READY";
        public const string TerminalHint = "ESC CLOSE     1-5 TABS";
        public const string DataLinkOffline = "DATA LINK OFFLINE - mission data arrives in Phase 2";
        public const string CloseGlyph = "X";

        public const string TabBriefing = "1  BRIEFING";
        public const string TabSpacecraft = "2  SPACECRAFT";
        public const string TabLaunch = "3  LAUNCH";
        public const string TabAnalysis = "4  ANALYSIS";
        public const string TabLaunchControl = "5  LAUNCH CONTROL";

        public const string StatMission = "MISSION";
        public const string StatBudget = "BUDGET";
        public const string StatMass = "MASS";
        public const string StatPower = "POWER";
        public const string StatDeltaV = "DELTA-V";
        public const string StatLink = "LINK";
        public const string StatSuccess = "SUCCESS RATE";

        public const string RatingFull = "FULL SUCCESS";
        public const string RatingPartial = "PARTIAL SUCCESS";
        public const string RatingFail = "FAILURE";

        public const string StatusPass = "PASS";
        public const string StatusWarn = "WARN";
        public const string StatusFail = "FAIL";
        public const string StatusMet = "MET";
        public const string StatusNotMet = "NOT MET";
        public const string StatusPending = "PENDING";

        public const string LabelObjective = "OBJECTIVE";
        public const string LabelMetric = "METRIC";
        public const string LabelRequirement = "REQUIREMENT";
        public const string LabelCurrent = "CURRENT";
        public const string LabelStatus = "STATUS";
        public const string LabelPoints = "PTS";

        public const string BriefingTitle = "MISSION BRIEFING";
        public const string SpacecraftTitle = "SPACECRAFT - DESIGN CONSOLE";
        public const string LaunchTitle = "LAUNCH VEHICLE";
        public const string AnalysisTitle = "ENGINEERING ANALYSIS";
        public const string LaunchControlTitle = "LAUNCH CONTROL";

        public const string SectionPropulsion = "PROPULSION";
        public const string SectionArray = "SOLAR ARRAY";
        public const string SectionBattery = "BATTERY";
        public const string SectionRadiation = "RADIATION CLASS";
        public const string SectionRedundancy = "REDUNDANCY";
        public const string SectionAntenna = "ANTENNA";
        public const string SectionRf = "RF POWER";
        public const string SectionGround = "GROUND STATION";
        public const string SectionInstruments = "INSTRUMENTS";

        public const string SectionMass = "MASS";
        public const string SectionPower = "POWER BY MODE";
        public const string SectionDeltaV = "PROPELLANT / DELTA-V";
        public const string SectionLink = "LINK BUDGET";
        public const string SectionData = "DATA BUDGET";
        public const string SectionDose = "RADIATION";
        public const string SectionCost = "COST";

        public const string ReadinessTitle = "READINESS CHECKLIST";
        public const string SuccessTitle = "MONTE CARLO - 1000 RUNS";
        public const string DebriefTitle = "DEBRIEF";
        public const string LaunchButtonText = "LAUNCH";
        public const string RedesignText = "REDESIGN";
        public const string CloseTerminalText = "CLOSE TERMINAL";
        public const string NoOrbit = "NO LUNAR ORBIT CAPABILITY";

        public const string HandbookHint = "B  HANDBOOK";
        public const string HandbookTitle = "MISSION HANDBOOK";
        public const string Spread1 = "1/4   MISSION OBJECTIVES";
        public const string Spread2 = "2/4   HOW IT WORKS";
        public const string Spread3 = "3/4   ENGINEER'S TIPS";
        public const string Spread4 = "4/4   DATA SOURCES";
        public const string PrevPage = "PREV";
        public const string NextPage = "NEXT";

        public const string HoverHint = "Hover a row to read why it matters.";
        public const string DesignTipTitle = "DESIGN TIPS";

        public static string Rating(int kind)
        {
            switch (kind)
            {
                case 2: return RatingFull;
                case 1: return RatingPartial;
                default: return RatingFail;
            }
        }

        public static string StateText(ObjectiveState s)
        {
            switch (s)
            {
                case ObjectiveState.Met: return StatusMet;
                case ObjectiveState.NotMet: return StatusNotMet;
                default: return StatusPending;
            }
        }

        /// <summary>Short "why this matters" lines for the ANALYSIS page.</summary>
        public static class Why
        {
            public const string Mass = "The rocket can only lift so much: every kilogram competes with instruments and propellant.";
            public const string Power = "Generation must beat the load in every mode, and the battery must survive the eclipse.";
            public const string DeltaV = "This is the fuel bill for leaving Earth, braking at the Moon and staying in orbit for a year.";
            public const string Link = "Signal strength falls with distance; the margin says how much weather and pointing error you can afford.";
            public const string Data = "Science only counts once it is on the ground - this is instrument output versus downlink capacity.";
            public const string Radiation = "Lunar orbit plus the Van Allen belts slowly cook electronics; part class sets the dose limit.";
            public const string Cost = "Everything is inside this number: hardware, launch, ground stations, staff and contingency.";
        }

        /// <summary>Handbook glossary, one sentence each.</summary>
        public static readonly string[] Glossary =
        {
            "DELTA-V: the total velocity change a spacecraft can produce, the currency of every space mission.",
            "TLI: Trans-Lunar Injection - the burn that throws the spacecraft from Earth orbit toward the Moon.",
            "LOI: Lunar Orbit Insertion - the braking burn that captures the spacecraft into lunar orbit.",
            "ISP: specific impulse - how efficiently an engine turns propellant into velocity, measured in seconds.",
            "LINK MARGIN: how many dB of signal you have above the minimum the radio needs to close the link.",
            "ECLIPSE: the part of each orbit in the Moon's shadow, when solar arrays produce nothing and the battery runs the bus.",
            "RADIATION DOSE: energy deposited in electronics, measured in krad; parts have a tolerance limit.",
            "LAUNCH WINDOW: the period when orbital mechanics and lighting allow a launch to reach the target.",
            "PAYLOAD FRACTION: payload mass divided by total launch mass - the higher, the more efficient the design."
        };
    }
}
