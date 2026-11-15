// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    /// <summary>English UI strings for the Mars terminal (kept together for translation).</summary>
    public static class MarsStrings
    {
        public const string TerminalTitle = "MARS MISSION CONTROL";
        public const string TerminalBoot = "MARS MISSION CONTROL ... READY";
        public const string CloseGlyph = "X";

        public const string TabWindow = "1  WINDOW";
        public const string TabRover = "2  ROVER";
        public const string TabEdl = "3  EDL";
        public const string TabSurface = "4  SURFACE";
        public const string TabComms = "5  COMMS";
        public const string TabReview = "6  REVIEW";

        public const string WindowTitle = "LAUNCH WINDOW";
        public const string RoverTitle = "ROVER DESIGN";
        public const string EdlTitle = "ENTRY, DESCENT AND LANDING";
        public const string SurfaceTitle = "SURFACE OPERATIONS - SOL PLANNER";
        public const string CommsTitle = "COMMUNICATIONS";
        public const string ReviewTitle = "MISSION REVIEW";

        public const string StatBudget = "BUDGET";
        public const string StatMass = "LAUNCH MASS";
        public const string StatC3 = "C3";
        public const string StatEdl = "EDL";
        public const string StatPower = "POWER";
        public const string StatData = "DATA";
        public const string StatSuccess = "SUCCESS";

        public const string WindowSlider = "LAUNCH DATE OFFSET";
        public const string WindowOptimal = "OPTIMAL";
        public const string WindowLate = "days after optimum";
        public const string WindowEarly = "days before optimum";
        public const string SectionVehicle = "LAUNCH VEHICLE";
        public const string SectionCapacity = "CAPACITY AT CURRENT C3";

        public const string SectionBus = "ROVER BUS";
        public const string SectionMobility = "MOBILITY";
        public const string SectionArm = "ROBOTIC ARM";
        public const string SectionThermal = "THERMAL";
        public const string SectionPower = "POWER SYSTEM";
        public const string SectionArray = "SOLAR ARRAY";
        public const string SectionBattery = "BATTERY";
        public const string SectionInstruments = "INSTRUMENTS";

        public const string SectionEdl = "EDL SYSTEM";
        public const string SectionShield = "HEAT SHIELD";
        public const string SectionParachute = "PARACHUTE";
        public const string SectionTrn = "TERRAIN-RELATIVE NAV";
        public const string SectionSite = "LANDING SITE";

        public const string SectionPolicy = "DAILY POLICY";
        public const string PolicyDrive = "DRIVE";
        public const string PolicyScience = "SCIENCE";
        public const string PolicyDrill = "DRILL / SAMPLE";
        public const string PolicyComms = "COMMS";
        public const string PolicyRest = "REST";
        public const string Normalize = "NORMALISE TO 100%";
        public const string MiniDay = "MINI DAY";
        public const string SectionPowerBudget = "POWER BUDGET";
        public const string SectionPerSol = "EXPECTED PER SOL";
        public const string SectionSeason = "SEASON / DUST";

        public const string SectionRelay = "UHF RELAY";
        public const string SectionPasses = "RELAY PASSES";
        public const string SectionDte = "DIRECT TO EARTH";
        public const string SectionDataBudget = "DATA BUDGET";
        public const string Conjunction = "SOLAR CONJUNCTION";

        public const string ReadinessTitle = "READINESS CHECKLIST";
        public const string MonteCarloTitle = "MONTE CARLO - 1000 RUNS";
        public const string LaunchButton = "LAUNCH";
        public const string DebriefTitle = "DEBRIEF";
        public const string Redesign = "REDESIGN";
        public const string CloseTerminal = "CLOSE TERMINAL";
        public const string SkipHint = "PRESS SPACE TO SKIP";
        public const string LaunchSequence = "LAUNCH SEQUENCE";

        public const string StatusPass = "PASS";
        public const string StatusWarn = "WARN";
        public const string StatusFail = "FAIL";
        public const string StatusMet = "MET";
        public const string StatusNotMet = "NOT MET";

        public const string RatingFull = "FULL SUCCESS";
        public const string RatingPartial = "PARTIAL SUCCESS";
        public const string RatingFail = "FAILURE";

        public static string Rating(int rating)
        {
            return rating == 2 ? RatingFull : (rating == 1 ? RatingPartial : RatingFail);
        }
    }
}
