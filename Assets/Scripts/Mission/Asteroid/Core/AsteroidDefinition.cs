// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    public enum AsteroidObjectiveKind
    {
        Arrival = 0,
        Science = 1,
        Sample = 2,
        Return = 3,
        Budget = 4
    }

    public sealed class AsteroidObjectiveDefinition
    {
        public string Id;
        public string Title;
        public string MetricLabel;
        public string Requirement;
        public string Why;
        public float Points;
        public bool Mandatory;
        public AsteroidObjectiveKind Kind;

        public AsteroidObjectiveDefinition(string id, string title, string metric, string requirement,
            string why, float points, bool mandatory, AsteroidObjectiveKind kind)
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

    public sealed class AsteroidDefinition
    {
        public const string Key = "Asteroit";

        public string Name => "ASTEROID SAMPLE RETURN";
        public string CodeName => "MISSION ASTEROID";
        public string Brief =>
            "Rendezvous with a carbon-rich near-Earth asteroid, map it, pick a sampling site, collect " +
            "regolith with a touch-and-go arm and return the capsule to Earth. Reference mission: " +
            "OSIRIS-REx / Bennu (TAG 20 October 2020, 121.6 g returned)."; // src: [3][4]

        public double BudgetCapM => AsteroidConstants.BudgetCapM;
        public double SampleTargetG => AsteroidConstants.SampleRequirementG;

        public AsteroidObjectiveDefinition[] Objectives
        {
            get
            {
                return new[] // game scoring
                {
                    new AsteroidObjectiveDefinition("S1", "Arrive and match orbit",
                        "ORBIT", "Launch, cruise and rendezvous succeed - mandatory",
                        "Everything depends on matching the asteroid's orbit; a miss means no science and no sample.",
                        20f, true, AsteroidObjectiveKind.Arrival),
                    new AsteroidObjectiveDefinition("S2", "Global mapping + spectroscopy",
                        "INSTRUMENT PTS", "Each instrument earns its points x fraction of data returned (cap 1)",
                        "Asteroid science is remote sensing first: data must survive the long X-band downlink.",
                        0f, false, AsteroidObjectiveKind.Science),
                    new AsteroidObjectiveDefinition("S3", "Sample acquisition",
                        "GRAMS", "Collect at least 60 g of regolith (NASA minimum)",
                        "A sample return is only worth the trip if the capsule carries enough material home.",
                        40f, false, AsteroidObjectiveKind.Sample),
                    new AsteroidObjectiveDefinition("S4", "Safe return",
                        "CAPSULE %", "Capsule survives re-entry and is recovered",
                        "The return capsule hits the atmosphere at about 12.4 km/s; the heat shield decides everything.",
                        25f, false, AsteroidObjectiveKind.Return),
                    new AsteroidObjectiveDefinition("S5", "Budget",
                        "$M TOTAL", "Total cost <= 1,050 $M",
                        "The reference mission costs about 800 $M plus about 183.5 $M for the launch vehicle.",
                        10f, false, AsteroidObjectiveKind.Budget)
                };
            }
        }

        public string[] Tips
        {
            get
            {
                return new[]
                {
                    "Bennu pays in science and sample mass; the fast rotator is cheap but almost bare; the metal body is a deep gravity well.",
                    "Earth gravity assist saves launch energy and rendezvous delta-v but adds a year of cruise and mission cost.",
                    "TAGSAM is heavy and expensive, but it is the only sampler that reliably clears 60 g on a rubble pile.",
                    "Nightingale Crater is rich but hazardous; the safe plain rarely reaches the sample requirement.",
                    "Contact speed near 10 cm/s is the sweet spot: too fast bounces, too slow barely touches.",
                    "Optical natural-feature navigation cuts TAG failure probability for a few kilograms of avionics.",
                    "Every TAG attempt spends propellant: three tries can turn a mass margin into a mass failure.",
                    "The capsule is 46 kg of the dry mass; the heavy heat shield buys survival margin at a cost."
                };
            }
        }

        public string[] Glossary
        {
            get
            {
                return new[]
                {
                    "RENDEZVOUS: matching the asteroid's heliocentric orbit so the relative speed at arrival is nearly zero.",
                    "PROXIMITY OPERATIONS: flying close to a tiny body where gravity is measured in centimetres per second squared.",
                    "TAG: Touch-And-Go - touching the surface for a few seconds, blowing gas and backing away with regolith.",
                    "TAGSAM: the 3.35 m sampling arm on OSIRIS-REx that captured 121.6 g at Bennu.",
                    "GRAVITY ASSIST: stealing momentum from a planet - OSIRIS-REx orbited the Sun for a year and used Earth.",
                    "ESCAPE VELOCITY: the speed needed to leave the asteroid; only about 0.2 m/s at Bennu's surface.",
                    "REGOLITH: the loose rock and dust layer that a sampler can actually collect.",
                    "RUBBLE PILE: a small asteroid held together by gravity and friction, not solid rock.",
                    "SOLAR RADIATION PRESSURE: sunlight pushes a light spacecraft; close-in orbits can become unstable.",
                    "RE-ENTRY: the capsule's high-speed entry into Earth's atmosphere and recovery on the ground."
                };
            }
        }
    }
}
