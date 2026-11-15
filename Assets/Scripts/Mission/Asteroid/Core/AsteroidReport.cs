// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    public sealed class AsteroidTargetReport
    {
        public string Name;
        public double DiameterM;
        public double RadiusM;
        public double MassKg;
        public double GmM3S2;
        public double SurfaceGravityMps2;
        public double EscapeVelocityMps;
        public double RotationHours;
        public double OrbitYears;
        public double MeanAu;
        public double CloseApproachKm;
        public double BoulderM;
        public double OrbitRadiusM;
        public double OrbitPeriodHours;
        public double CircularSpeedMps;
        public double SrpAccelMps2;
        public double GravityAccelMps2;
        public double SrpRatio;
        public double Stability;       // 0..1
        public double LightTimeMinutes;
        public bool OrbitPass;
        public double ScienceMultiplier;
        public double SampleRichness;
    }

    public sealed class AsteroidTrajectoryReport
    {
        public string TypeName;
        public bool Ega;
        public double C3Km2S2;
        public double ArrivalYears;
        public double CruiseDays;
        public double RendezvousKmS;
        public double ProxOpsKmS;
        public double DepartureKmS;
        public double TagKmS;
        public double TotalKmS;
        public double MissionYears;
        public double CapacityKg;
        public double LimitKg;
        public string ReturnWindow;
    }

    public sealed class AsteroidMassReport
    {
        public double BusKg;
        public double PropulsionKg;
        public double ArrayKg;
        public double BatteryKg;
        public double AntennaKg;
        public double RfKg;
        public double SamplerKg;
        public double CapsuleKg;
        public double InstrumentKg;
        public double BaseDryKg;
        public double StructureKg;
        public double DryNoTankKg;
        public double TankKg;
        public double DryKg;
        public double PropellantKg;
        public double WetKg;
        public double CapacityKg;
        public double LimitKg;
        public double UsageFraction;
        public double WetDryRatio;
        public bool Pass;
    }

    public sealed class AsteroidPowerReport
    {
        public double ArrayW;
        public double LoadW;
        public double PeakLoadW;
        public double Margin;
        public double BatteryNeedWh;
        public double BatteryUsableWh;
        public bool BatteryPass;
        public bool Pass;
    }

    public sealed class AsteroidCommsReport
    {
        public double TxGainDbi;
        public double EirpDbw;
        public double FsplDb;
        public double GroundGOverT;
        public double MaxRateBps;
        public double UsedRateBps;
        public double MarginDb;
        public double ContactHoursPerDay;
        public double DownlinkGbPerDay;
        public double ScienceGbPerDay;
        public double Fraction;
        public double LightTimeMinutes;
        public bool Pass;
    }

    public sealed class AsteroidTagReport
    {
        public string SamplerName;
        public string SiteName;
        public bool Navigation;
        public double ContactSpeedCmS;
        public int Attempts;
        public double SuccessProbability;
        public double PerAttemptGrams;
        public double ExpectedGrams;
        public double MedianGrams;
        public double FailureChance;
        public double ContactFactor;
        public bool Pass;
    }

    public sealed class AsteroidCapsuleReport
    {
        public string Name;
        public double MassKg;
        public double CostM;
        public double EntrySpeedKmS;
        public double SpeedLimitKmS;
        public double Margin;
        public double SurvivalProbability;
        public bool Pass;
    }

    public sealed class AsteroidCostReport
    {
        public double PartsM;
        public double InstrumentsM;
        public double DevelopmentM;
        public double ContingencyM;
        public double LaunchM;
        public double GroundM;
        public double OpsM;
        public double TotalM;
        public double CapM;
        public bool Pass;
    }

    public sealed class AsteroidReliabilityReport
    {
        public double LaunchReliability;
        public double EgaSuccess;
        public double ArrivalSuccess;
        public double TagSuccess;
        public double CapsuleSuccess;
    }

    /// <summary>Deterministic evaluation of one asteroid design.</summary>
    public sealed class AsteroidReport
    {
        public AsteroidDesign Design;
        public AsteroidDefinition Definition;

        public AsteroidTargetReport Target;
        public AsteroidTrajectoryReport Trajectory;
        public AsteroidMassReport Mass;
        public AsteroidPowerReport Power;
        public AsteroidCommsReport Comms;
        public AsteroidTagReport Tag;
        public AsteroidCapsuleReport Capsule;
        public AsteroidCostReport Cost;
        public AsteroidReliabilityReport Reliability;

        public AsteroidTarget TargetInfo;
        public AsteroidBus Bus;
        public AsteroidPropulsion Engine;
        public AsteroidSampler Sampler;
        public AsteroidCapsule CapsuleInfo;
        public AsteroidSite Site;
        public AsteroidLaunchVehicle Vehicle;
        public AntennaOption Antenna;
        public GroundStationOption Station;
        public RadiationOption RadiationClass;

        public bool BudgetPass;
        public bool Level1;
        public bool Level2;
        public bool Level3;
        public bool Level4;
        public float SciencePotential;
        public float ScienceEarned;
        public float SamplePoints;
        public float AchievablePoints;
        public int DeterministicRating;
        public int ExpectedGrams;
    }
}
