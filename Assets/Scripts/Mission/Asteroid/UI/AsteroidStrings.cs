// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    /// <summary>English UI strings for the asteroid terminal.</summary>
    public static class AsteroidStrings
    {
        public const string TerminalBoot = "ASTEROID MISSION CONTROL ... READY";
        public const string CloseGlyph = "X";

        public const string TabTarget = "1  TARGET";
        public const string TabSpacecraft = "2  SPACECRAFT";
        public const string TabTrajectory = "3  TRAJECTORY";
        public const string TabProx = "4  PROX-OPS";
        public const string TabReview = "5  REVIEW";

        public const string TargetTitle = "TARGET SELECTION";
        public const string SpacecraftTitle = "SPACECRAFT DESIGN";
        public const string TrajectoryTitle = "TRAJECTORY AND LAUNCH";
        public const string ProxTitle = "PROXIMITY OPERATIONS AND TAG";
        public const string ReviewTitle = "MISSION REVIEW";

        public const string StatBudget = "BUDGET";
        public const string StatMass = "WET MASS";
        public const string StatDeltaV = "DELTA-V";
        public const string StatSample = "EXPECTED SAMPLE";
        public const string StatCapsule = "CAPSULE";
        public const string StatSuccess = "SUCCESS";

        public const string SectionBus = "BUS";
        public const string SectionPropulsion = "PROPULSION";
        public const string SectionArray = "SOLAR ARRAY";
        public const string SectionBattery = "BATTERY";
        public const string SectionAntenna = "X-BAND ANTENNA";
        public const string SectionRf = "RF POWER";
        public const string SectionSampler = "SAMPLING SYSTEM";
        public const string SectionCapsule = "RETURN CAPSULE";
        public const string SectionInstruments = "INSTRUMENTS";

        public const string SectionTrajectory = "TRAJECTORY TYPE";
        public const string SectionVehicle = "LAUNCH VEHICLE";
        public const string SectionDeltaV = "DELTA-V BUDGET";
        public const string SectionMass = "MASS / CAPACITY";

        public const string SectionOrbit = "ORBIT RADIUS";
        public const string SectionCampaign = "SURVEY CAMPAIGN";
        public const string SectionSite = "SAMPLING SITE";
        public const string SectionTag = "TAG SETTINGS";
        public const string SectionData = "MAPPING DATA";

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
