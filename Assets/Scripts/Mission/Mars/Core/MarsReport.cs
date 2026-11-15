// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    public sealed class MarsTransferReport
    {
        public int OffsetDays;
        public double C3Km2S2;
        public double CruiseDays;
        public double EarthMarsDistanceMkm;
        public double ArrivalVinfKmS;
        public double LightTimeMinutes;
        public string WindowNote;
    }

    public sealed class MarsMassReport
    {
        public double BusKg;
        public double MobilityKg;
        public double ArmKg;
        public double PowerKg;
        public double BatteryKg;
        public double ThermalKg;
        public double TrnKg;
        public double CommsKg;
        public double InstrumentKg;
        public double EntryHardwareKg;
        public double RoverKg;
        public double LandedKg;
        public double LaunchKg;
        public double LandedFraction;
        public double CapacityKg;
        public double LimitKg;
        public double UsageFraction;
        public bool Pass;
    }

    public sealed class MarsEdlReport
    {
        public string SystemName;
        public double BaseSuccess;
        public double MassFactor;
        public double SiteHazard;
        public double TrnBonus;
        public double ShieldFactor;
        public double ParachuteFactor;
        public double SuccessProbability;
        public double LandedLimitKg;
        public double SiteScienceMultiplier;
        public bool Pass;
    }

    public sealed class MarsPowerReport
    {
        public bool Mmrtg;
        public double GenerationWhSol;
        public double WorstGenerationWhSol;
        public double LoadWhSol;
        public double Margin;
        public double WorstMargin;
        public double HousekeepingWh;
        public double HeaterWh;
        public double DriveWh;
        public double ScienceWh;
        public double DrillWh;
        public double CommsWh;
        public double BatteryNeedWh;
        public double BatteryUsableWh;
        public bool BatteryPass;
        public bool Pass;
    }

    public sealed class MarsCommsReport
    {
        public bool RelayEnabled;
        public double RelayRateMbps;
        public int RelayPasses;
        public double RelayMbSol;
        public double DteMbSol;
        public double TotalMbSol;
        public double LightTimeMinutes;
        public double ConjunctionDays;
        public bool Pass;
    }

    public sealed class MarsDataReport
    {
        public double ScienceMbSol;
        public double DownlinkMbSol;
        public double Fraction;
        public double BlackoutLoss;
        public bool Pass;
    }

    public sealed class MarsSurfaceReport
    {
        public double SolHours;
        public double DaylightHours;
        public double DriveHours;
        public double ScienceHours;
        public double DrillHours;
        public double CommsHours;
        public double DistanceMPerSol;
        public double SamplesPerSol;
        public double ExpectedSamples;
        public double ActiveInstrumentPowerW;
        public double ActiveDataMbPerHour;
        public double SampleSuccessChance;
        public bool Pass;
    }

    public sealed class MarsRadiationReport
    {
        public double CruiseMsv;
        public double SurfaceMsv;
        public double TotalMsv;
        public double EquivalentKrad;
        public double ToleranceKrad;
        public double SurfaceToCruiseRatio;
        public bool Pass;
    }

    public sealed class MarsCostReport
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

    public sealed class MarsReliabilityReport
    {
        public double LaunchReliability;
        public double CruiseSuccess;
        public double EdlSuccess;
        public double MonthlyFailure;
        public double SampleSuccess;
    }

    /// <summary>Deterministic evaluation of one Mars design.</summary>
    public sealed class MarsReport
    {
        public MarsDesign Design;
        public MarsDefinition Definition;

        public MarsTransferReport Transfer;
        public MarsMassReport Mass;
        public MarsEdlReport Edl;
        public MarsPowerReport Power;
        public MarsCommsReport Comms;
        public MarsDataReport Data;
        public MarsSurfaceReport Surface;
        public MarsRadiationReport Radiation;
        public MarsCostReport Cost;
        public MarsReliabilityReport Reliability;

        public MarsLaunchVehicle Vehicle;
        public MarsRoverBus Bus;
        public MarsMobility Mobility;
        public MarsThermalOption Thermal;
        public MarsEdlSystem EdlSystem;
        public MarsHeatShield Shield;
        public MarsSite Site;
        public RadiationOption RadiationClass;

        public bool BudgetPass;
        public bool Level1;
        public bool Level2;
        public bool Level3;
        public bool Level4;
        public int ExpectedSamples;
        public float SciencePotential;
        public float ScienceEarned;
        public float AchievablePoints;
        public int DeterministicRating;  // 0 FAIL, 1 PARTIAL, 2 FULL
    }
}
