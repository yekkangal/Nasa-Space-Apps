// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    public struct PowerModeResult
    {
        public string Name;
        public double GenerationW;
        public double LoadW;
        public double Margin;
        public bool Pass;
    }

    public sealed class MassReport
    {
        public double InstrumentsKg;
        public double SubsystemsKg;
        public double StructureKg;
        public double BaseDryKg;        // subsystems + instruments + structure
        public double TankKg;
        public double DryKg;            // including 15% margin and tank
        public double PropellantKg;
        public double WetKg;
        public double CapacityKg;       // to TLI (chemical) or LEO (electric)
        public double LimitKg;          // 95% of capacity
        public double UsageFraction;    // wet / capacity
        public bool Pass;
        public string CapacityBasis;
    }

    public sealed class PropulsionReport
    {
        public bool Electric;
        public double DeltaVKmS;
        public double IspSeconds;
        public double PropellantKg;
        public double PropellantFraction;   // propellant / wet
        public double SpiralMonths;
        public double ThrustN;
        public double AccelerationMps2;
        public double MassRatio;
    }

    public sealed class PowerReport
    {
        public double ArrayBeginningOfLifeW;
        public double ArrayEndOfLifeW;
        public double AverageGenerationW;   // sunlit minus average eclipse fraction
        public double WorstLoadW;
        public double EclipsePeriodHours;
        public double EclipseEnergyWh;
        public double BatteryUsableWh;
        public double BatteryWh;
        public bool BatteryPass;
        public double MinMargin;
        public bool Pass;
        public PowerModeResult[] Modes;
    }

    public sealed class LinkReport
    {
        public double TxGainDbi;
        public double EirpDbi;
        public double GroundGainDbi;
        public double GroundGOverT;
        public double FsplDb;
        public double CarrierToNoiseDb;
        public double MaxRateBps;
        public double BandCapBps;
        public double UsedRateBps;
        public double MarginDb;
        public double ContactHoursPerDay;
        public double FrequencyMHz;
        public string Band;
        public bool Pass;
    }

    public sealed class DataReport
    {
        public double ScienceGbPerDay;
        public double DownlinkGbPerDay;
        public double Fraction;
        public bool Pass;
    }

    public sealed class CostReport
    {
        public double PartsMillions;
        public double InstrumentsMillions;
        public double DevelopmentMillions;
        public double ContingencyMillions;
        public double LaunchMillions;
        public double GroundMillions;
        public double OpsMillions;
        public double TotalMillions;
        public double CapMillions;
        public bool Pass;
    }

    public sealed class RadiationReport
    {
        public double DoseKrad;
        public double ToleranceKrad;
        public double SpiralMonths;
        public bool Pass;
        public double MonthlyFailureProbability;
    }

    public sealed class ReliabilityReport
    {
        public double LaunchReliability;
        public double BusAnnualReliability;
        public double MonthlyBusFailure;
        public bool Redundancy;
    }

    /// <summary>Full deterministic evaluation of one design. Recomputed on every edit.</summary>
    public sealed class EngineeringReport
    {
        public MissionDesign Design;
        public MassReport Mass;
        public PropulsionReport Propulsion;
        public PowerReport Power;
        public LinkReport Link;
        public DataReport Data;
        public CostReport Cost;
        public RadiationReport Radiation;
        public ReliabilityReport Reliability;

        public bool OrbitFeasible;       // can physically reach the 100 km polar orbit
        public float SciencePoints;      // O2 + O3 potential, data-fraction weighted later
        public float AchievablePoints;   // deterministic points with current design

        public LaunchVehicleOption Vehicle;
        public PropulsionOption Engine;
        public AntennaOption Antenna;
        public GroundStationOption Station;
        public RadiationOption RadiationClass;
    }
}
