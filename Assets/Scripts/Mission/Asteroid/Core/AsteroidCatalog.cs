// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    public sealed class AsteroidTarget
    {
        public string Id;
        public string Name;
        public double DiameterM;
        public double DensityKgM3;
        public double RotationHours;
        public double OrbitYears;
        public double MeanAu;
        public double CloseApproachKm;
        public double BoulderM;
        public double ScienceMultiplier;
        public double SampleRichness;
        public double HazardFactor;
        public double C3Bonus;
        public double DvBonusKmS;
        public double DefaultOrbitRadiusM;
        public string Note;
    }

    public sealed class AsteroidBus
    {
        public string Id;
        public string Name;
        public double MassKg;
        public double CostM;
    }

    public sealed class AsteroidPropulsion
    {
        public string Id;
        public string Name;
        public double Isp;
        public double MassKg;
        public double PowerW;
        public double CostM;
        public bool Electric;
    }

    public sealed class AsteroidArray
    {
        public double AreaM2;
        public double MassKg => AreaM2 * 5.0;      // game
        public double CostM => AreaM2 * 1.5;       // game
        public string Label => AreaM2.ToString("0") + " m2";
    }

    public sealed class AsteroidBattery
    {
        public double CapacityWh;
        public double MassKg => CapacityWh / AsteroidConstants.BatteryWhPerKg;
        public double CostM => MassKg * 0.1;      // game
        public string Label => CapacityWh.ToString("0") + " Wh";
    }

    public sealed class AsteroidSampler
    {
        public string Id;
        public string Name;
        public double MassKg;
        public double CostM;
        public double MedianGrams;
        public double Sigma;
        public double FailureBase;
        public string Note;
    }

    public sealed class AsteroidCapsule
    {
        public string Id;
        public string Name;
        public double MassKg;
        public double CostM;
        public double BaseSurvival;
        public double SpeedLimitKmS;
        public string Note;
    }

    public sealed class AsteroidSite
    {
        public string Id;
        public string Name;
        public double Hazard;
        public double SampleFactor;
        public string Note;
    }

    public sealed class AsteroidInstrument
    {
        public string Id;
        public string Name;
        public double MassKg;
        public double PowerW;
        public double DataGbPerDay;
        public double CostM;
        public float Points;
        public string Note;
    }

    public sealed class AsteroidLaunchVehicle
    {
        public string Id;
        public string Name;
        public double Cap10Kg;
        public double Cap20Kg;
        public double Cap30Kg;
        public double CostM;
        public double Reliability;
        public string Note;
    }

    /// <summary>Asteroid-only catalog; Moon and Mars catalogs are read-only where data is shared.</summary>
    public static class AsteroidCatalog
    {
        public static readonly AsteroidTarget[] Targets =
        {
            new AsteroidTarget
            {
                Id = "bennu", Name = "Bennu-like", DiameterM = AsteroidConstants.BennuDiameterM,
                DensityKgM3 = AsteroidConstants.BennuDensityKgM3,
                RotationHours = AsteroidConstants.BennuRotationHours,
                OrbitYears = AsteroidConstants.BennuOrbitYears,
                MeanAu = AsteroidConstants.BennuMeanDistanceMkm / Asteroids.AuMkm,
                CloseApproachKm = AsteroidConstants.BennuCloseApproachKm,
                BoulderM = AsteroidConstants.BennuBoulderM,
                ScienceMultiplier = 1.0, SampleRichness = 1.0, HazardFactor = 1.0,
                C3Bonus = 0.0, DvBonusKmS = 0.0,
                DefaultOrbitRadiusM = AsteroidConstants.ReferenceOrbitRadiusM,
                Note = "Carbon-rich top-shaped rubble pile. Medium accessibility, high science."
            }, // src: [3] // verify density
            new AsteroidTarget
            {
                Id = "fast", Name = "Fast rotator", DiameterM = 150, DensityKgM3 = 1500,
                RotationHours = 0.333, OrbitYears = 0.8, MeanAu = 0.9, CloseApproachKm = 50000000,
                BoulderM = 2, ScienceMultiplier = 0.8, SampleRichness = 0.25, HazardFactor = 1.15,
                C3Bonus = -4, DvBonusKmS = -0.1, DefaultOrbitRadiusM = 300,
                Note = "Tiny 20-minute spinner with almost no regolith. Cheap to reach, hard to sample."
            }, // game
            new AsteroidTarget
            {
                Id = "metal", Name = "Metal-rich body", DiameterM = 25000, DensityKgM3 = 7000,
                RotationHours = 6, OrbitYears = 3.0, MeanAu = 2.0, CloseApproachKm = 400000000,
                BoulderM = 10, ScienceMultiplier = 1.5, SampleRichness = 0.30, HazardFactor = 1.10,
                C3Bonus = 6, DvBonusKmS = 0.4, DefaultOrbitRadiusM = 37500,
                Note = "25 km dense body with a deep gravity well. Very high science, little loose regolith."
            } // game
        };

        public static readonly AsteroidBus[] Buses =
        {
            new AsteroidBus { Id = "small", Name = "Small bus", MassKg = 160, CostM = 150 },   // game
            new AsteroidBus { Id = "medium", Name = "Medium bus", MassKg = 260, CostM = 260 }, // game
            new AsteroidBus { Id = "large", Name = "OSIRIS-REx-class bus", MassKg = 340, CostM = 400 } // game
        };

        public static readonly AsteroidPropulsion[] Propulsion =
        {
            new AsteroidPropulsion { Id = "mono", Name = "Hydrazine", Isp = 225, MassKg = 20, PowerW = 20, CostM = 12 }, // game
            new AsteroidPropulsion { Id = "biprop", Name = "Bipropellant", Isp = 315, MassKg = 35, PowerW = 30, CostM = 20 }, // game
            new AsteroidPropulsion { Id = "electric", Name = "Electric Hall", Isp = 1800, MassKg = 40, PowerW = 1500, CostM = 30, Electric = true } // game
        };

        public static readonly AsteroidArray[] Arrays =
        {
            new AsteroidArray { AreaM2 = 4 }, // game
            new AsteroidArray { AreaM2 = 6 }, // game
            new AsteroidArray { AreaM2 = 8 }, // game
            new AsteroidArray { AreaM2 = 10 } // game
        };

        public static readonly AsteroidBattery[] Batteries =
        {
            new AsteroidBattery { CapacityWh = 0 }, // game
            new AsteroidBattery { CapacityWh = 500 }, // game
            new AsteroidBattery { CapacityWh = 1000 }, // game
            new AsteroidBattery { CapacityWh = 2000 } // game
        };

        public static readonly string[] AntennaIds = { "x05", "x10", "x15" };

        public static readonly AsteroidSampler[] Samplers =
        {
            new AsteroidSampler { Id = "tagsam", Name = "TAGSAM-like arm", MassKg = 40, CostM = 90, MedianGrams = 100, Sigma = 0.45, FailureBase = 0.08, Note = "3.35 m arm, reference sample 121.6 g." }, // game (arm/reference [1][3])
            new AsteroidSampler { Id = "scoop", Name = "Simple scoop", MassKg = 15, CostM = 30, MedianGrams = 35, Sigma = 0.70, FailureBase = 0.18, Note = "Cheap and light, small sample. // game" },
            new AsteroidSampler { Id = "sticky", Name = "Sticky pad", MassKg = 8, CostM = 20, MedianGrams = 12, Sigma = 0.90, FailureBase = 0.32, Note = "Very light, very low sample mass. // game" }
        };

        public static readonly AsteroidCapsule[] Capsules =
        {
            new AsteroidCapsule { Id = "light", Name = "Light capsule", MassKg = 30, CostM = 25, BaseSurvival = 0.85, SpeedLimitKmS = 11.5, Note = "Cheap, marginal for 12.4 km/s re-entry. // game" },
            new AsteroidCapsule { Id = "standard", Name = "Standard capsule 46 kg", MassKg = AsteroidConstants.ReferenceCapsuleKg, CostM = 40, BaseSurvival = 0.96, SpeedLimitKmS = 12.6, Note = "Reference sample return capsule." }, // game (mass src: [1])
            new AsteroidCapsule { Id = "heavy", Name = "Heavy capsule", MassKg = 70, CostM = 60, BaseSurvival = 0.985, SpeedLimitKmS = 13.5, Note = "Extra heat-shield margin, more mass. // game" }
        };

        public static readonly AsteroidSite[] Sites =
        {
            new AsteroidSite { Id = "safe", Name = "Safe Plain", Hazard = 0.05, SampleFactor = 0.70, Note = "Low hazard, low sample volume. // game" },
            new AsteroidSite { Id = "nightingale", Name = "Nightingale Crater", Hazard = 0.18, SampleFactor = 1.15, Note = "Reference sampling site, rich regolith." }, // game (site reference [4])
            new AsteroidSite { Id = "boulder", Name = "Boulder Field", Hazard = 0.35, SampleFactor = 1.30, Note = "Highest sample volume, boulders up to 22 m." }, // game (boulder size src: [3])
        };

        public static readonly AsteroidInstrument[] Instruments =
        {
            new AsteroidInstrument { Id = "ocams", Name = "OCAMS", MassKg = 12, PowerW = 20, DataGbPerDay = 1.800, CostM = 25, Points = 20, Note = "Camera suite. // game" },
            new AsteroidInstrument { Id = "ola", Name = "OLA", MassKg = 8, PowerW = 15, DataGbPerDay = 0.700, CostM = 15, Points = 10, Note = "Laser altimeter. // game" },
            new AsteroidInstrument { Id = "ovirs", Name = "OVIRS", MassKg = 10, PowerW = 12, DataGbPerDay = 0.900, CostM = 18, Points = 12, Note = "Visible/IR spectrometer. // game" },
            new AsteroidInstrument { Id = "otes", Name = "OTES", MassKg = 8, PowerW = 10, DataGbPerDay = 0.500, CostM = 15, Points = 10, Note = "Thermal emission spectrometer. // game" },
            new AsteroidInstrument { Id = "rexis", Name = "REXIS", MassKg = 6, PowerW = 8, DataGbPerDay = 0.100, CostM = 12, Points = 8, Note = "X-ray imaging spectrometer. // game" }
        };

        public static readonly AsteroidLaunchVehicle[] Vehicles =
        {
            new AsteroidLaunchVehicle { Id = "kestrel1", Name = "Kestrel-1", Cap10Kg = 0, Cap20Kg = 0, Cap30Kg = 0, CostM = 8, Reliability = 0.93, Note = "No asteroid capability. // game" },
            new AsteroidLaunchVehicle { Id = "condor5", Name = "Condor-5", Cap10Kg = 900, Cap20Kg = 500, Cap30Kg = 250, CostM = 55, Reliability = 0.95, Note = "Small sample-return launcher. // game" },
            new AsteroidLaunchVehicle { Id = "condor9", Name = "Condor-9", Cap10Kg = 3400, Cap20Kg = 2400, Cap30Kg = 1600, CostM = 95, Reliability = 0.97, Note = "Atlas V 411 class." }, // game (Atlas reference [1] only)
            new AsteroidLaunchVehicle { Id = "titanh", Name = "Titan-H", Cap10Kg = 11000, Cap20Kg = 8000, Cap30Kg = 5500, CostM = 240, Reliability = 0.96, Note = "Heavy lift; rarely economical. // game" }
        };

        // ---- lookups ---------------------------------------------------------

        public static AsteroidTarget Target(string id) => Find(Targets, t => t.Id == id) ?? Targets[0];
        public static AsteroidBus Bus(string id) => Find(Buses, b => b.Id == id) ?? Buses[2];
        public static AsteroidPropulsion Engine(string id) => Find(Propulsion, p => p.Id == id) ?? Propulsion[1];
        public static AsteroidSampler Sampler(string id) => Find(Samplers, s => s.Id == id) ?? Samplers[0];
        public static AsteroidCapsule Capsule(string id) => Find(Capsules, c => c.Id == id) ?? Capsules[1];
        public static AsteroidSite Site(string id) => Find(Sites, s => s.Id == id) ?? Sites[1];
        public static AsteroidInstrument Instrument(string id) => Find(Instruments, i => i.Id == id);
        public static AsteroidLaunchVehicle Vehicle(string id) => Find(Vehicles, v => v.Id == id) ?? Vehicles[2];

        public static AntennaOption Antenna(string id) => MissionCatalog.Antenna(id);
        public static RfPowerOption RfPower(double watts) => MissionCatalog.RfPowers[MissionCatalog.RfIndex(watts)];
        public static GroundStationOption Station(string id) => MissionCatalog.Station(id);
        public static RadiationOption Radiation(string id) => MissionCatalog.Radiation(id);

        public static int ArrayIndex(double area)
        {
            for (int i = 0; i < Arrays.Length; i++)
                if (Math.Abs(Arrays[i].AreaM2 - area) < 0.01) return i;
            return 2;
        }

        public static int BatteryIndex(double wh)
        {
            for (int i = 0; i < Batteries.Length; i++)
                if (Math.Abs(Batteries[i].CapacityWh - wh) < 0.01) return i;
            return 1;
        }

        public static int InstrumentIndex(string id)
        {
            for (int i = 0; i < Instruments.Length; i++)
                if (Instruments[i].Id == id) return i;
            return -1;
        }

        public static double CapacityAtC3(AsteroidLaunchVehicle vehicle, double c3)
        {
            if (vehicle == null || vehicle.Cap10Kg <= 0) return 0;
            c3 = AsteroidConstants.Clamp(c3, 10.0, 30.0);
            if (c3 <= 20.0)
                return vehicle.Cap10Kg + (vehicle.Cap20Kg - vehicle.Cap10Kg) * (c3 - 10.0) / 10.0;
            return vehicle.Cap20Kg + (vehicle.Cap30Kg - vehicle.Cap20Kg) * (c3 - 20.0) / 10.0;
        }

        static T Find<T>(T[] list, Func<T, bool> match) where T : class
        {
            for (int i = 0; i < list.Length; i++)
                if (match(list[i])) return list[i];
            return null;
        }
    }

    /// <summary>Small helper so the target definitions can convert the Bennu distance once.</summary>
    public static class Asteroids
    {
        public const double AuMkm = 149.5978707; // million km per AU
    }
}
