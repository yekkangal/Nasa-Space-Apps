// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    public sealed class MarsLaunchVehicle
    {
        public string Id;
        public string Name;
        public double Cap10Kg;       // capacity to Mars at C3 = 10 km2/s2
        public double CostM;
        public double Reliability;
        public string Note;
    }

    public sealed class MarsRoverBus
    {
        public string Id;
        public string Name;
        public double MassKg;
        public double CostM;
    }

    public sealed class MarsMobility
    {
        public string Id;
        public string Name;
        public double MassKg;
        public double CostM;
    }

    public sealed class MarsPowerOption
    {
        public string Id;
        public string Name;
        public double MassKg;
        public double OutputW;
        public double CostM;
    }

    public sealed class MarsThermalOption
    {
        public string Id;
        public string Name;
        public double MassKg;
        public double HeaterWhSol;
        public double CostM;
    }

    public sealed class MarsArrayOption
    {
        public double AreaM2;
        public double MassKg => AreaM2 * 5.0;      // game
        public double CostM => AreaM2 * 1.5;       // game
        public string Label => AreaM2.ToString("0") + " m2";
    }

    public sealed class MarsBatteryOption
    {
        public double CapacityWh;
        public double MassKg => CapacityWh / MarsConstants.BatteryWhPerKg;
        public double CostM => MassKg * 0.1;       // game
        public string Label => CapacityWh.ToString("0") + " Wh";
    }

    public sealed class MarsEdlSystem
    {
        public string Id;
        public string Name;
        public double BaseSuccess;
        public double LandedFraction;
        public double LandedLimitKg;
        public double CostM;
        public string Note;
    }

    public sealed class MarsHeatShield
    {
        public string Id;
        public string Name;
        public double DiameterM;
        public double MassKg;
        public double CostM;
        public double SuccessFactor;
    }

    public sealed class MarsSite
    {
        public string Id;
        public string Name;
        public double HazardFactor;
        public double ScienceMultiplier;
        public string Note;
    }

    public sealed class MarsInstrument
    {
        public string Id;
        public string Name;
        public double MassKg;
        public double PowerW;
        public double DataMbPerHour;
        public double CostM;
        public float Points;
        public string Note;
    }

    /// <summary>Mars-only catalog. Moon catalogs are reused read-only where the data is shared.</summary>
    public static class MarsCatalog
    {
        public static readonly MarsLaunchVehicle[] Vehicles =
        {
            new MarsLaunchVehicle { Id = "kestrel1", Name = "Kestrel-1", Cap10Kg = 0, CostM = 8, Reliability = 0.93, Note = "Small-sat launcher - no Mars capability." }, // game
            new MarsLaunchVehicle { Id = "condor5", Name = "Condor-5", Cap10Kg = 1100, CostM = 55, Reliability = 0.95, Note = "Small Mars orbiter/rover class." }, // game
            new MarsLaunchVehicle { Id = "condor9", Name = "Condor-9", Cap10Kg = 3900, CostM = 95, Reliability = 0.97, Note = "Atlas V 541 class, Perseverance heritage." }, // game (Atlas V reference in [2] only)
            new MarsLaunchVehicle { Id = "titanh", Name = "Titan-H", Cap10Kg = 12000, CostM = 240, Reliability = 0.96, Note = "Heavy lift; rarely economical for one rover." } // game
        };

        public static readonly MarsRoverBus[] Buses =
        {
            new MarsRoverBus { Id = "small", Name = "Small rover", MassKg = 120, CostM = 180 }, // game
            new MarsRoverBus { Id = "medium", Name = "Medium rover", MassKg = 260, CostM = 450 }, // game
            new MarsRoverBus { Id = "large", Name = "Perseverance-class", MassKg = 400, CostM = 650 } // game
        };

        public static readonly MarsMobility[] Mobility =
        {
            new MarsMobility { Id = "standard", Name = "Standard wheels", MassKg = 60, CostM = 40 }, // game
            new MarsMobility { Id = "heavy", Name = "Heavy-duty wheels", MassKg = 120, CostM = 90 } // game
        };

        public static readonly MarsPowerOption Mmrtg =
            new MarsPowerOption { Id = "mmrtg", Name = "MMRTG 110 W", MassKg = 45, OutputW = MarsConstants.MmrtgPowerW, CostM = 150 }; // game

        public static readonly MarsArrayOption[] Arrays =
        {
            new MarsArrayOption { AreaM2 = 2 }, // game
            new MarsArrayOption { AreaM2 = 4 }, // game
            new MarsArrayOption { AreaM2 = 6 }, // game
            new MarsArrayOption { AreaM2 = 8 } // game
        };

        public static readonly MarsBatteryOption[] Batteries =
        {
            new MarsBatteryOption { CapacityWh = 0 }, // game
            new MarsBatteryOption { CapacityWh = 500 }, // game
            new MarsBatteryOption { CapacityWh = 1000 }, // game
            new MarsBatteryOption { CapacityWh = 2000 }, // game
            new MarsBatteryOption { CapacityWh = 3200 } // game
        };

        public static readonly MarsThermalOption[] Thermal =
        {
            new MarsThermalOption { Id = "passive", Name = "Passive insulation", MassKg = 10, HeaterWhSol = 400, CostM = 20 }, // game
            new MarsThermalOption { Id = "active", Name = "Active heaters", MassKg = 25, HeaterWhSol = 700, CostM = 40 } // game
        };

        public static readonly MarsEdlSystem[] EdlSystems =
        {
            new MarsEdlSystem { Id = "airbags", Name = "Airbags", BaseSuccess = 0.80, LandedFraction = 0.40, LandedLimitKg = 600, CostM = 60, Note = "Cheap and proven at small scale; hard 600 kg landed limit." }, // game
            new MarsEdlSystem { Id = "skycrane", Name = "Sky crane", BaseSuccess = 0.92, LandedFraction = 0.26, LandedLimitKg = 1200, CostM = 100, Note = "Curiosity/Perseverance heritage." }, // game (heritage only)
            new MarsEdlSystem { Id = "legs", Name = "Propulsive legs", BaseSuccess = 0.85, LandedFraction = 0.35, LandedLimitKg = 1000, CostM = 80, Note = "Simpler than a sky crane, less mass efficient." } // game
        };

        public static readonly MarsHeatShield[] HeatShields =
        {
            new MarsHeatShield { Id = "small", Name = "3.8 m shield", DiameterM = 3.8, MassKg = 70, CostM = 25, SuccessFactor = 0.98 }, // game
            new MarsHeatShield { Id = "large", Name = "4.5 m shield", DiameterM = 4.5, MassKg = 110, CostM = 35, SuccessFactor = 1.0 } // game
        };

        public static readonly MarsSite[] Sites =
        {
            new MarsSite { Id = "safe", Name = "Safe Plain", HazardFactor = 1.00, ScienceMultiplier = 0.85, Note = "Low hazard, ordinary geology." }, // game
            new MarsSite { Id = "delta", Name = "Delta Crater", HazardFactor = 0.88, ScienceMultiplier = 1.15, Note = "Ancient lake deposits - best astrobiology target, rougher terrain." }, // game
            new MarsSite { Id = "polar", Name = "Polar Ice Region", HazardFactor = 0.93, ScienceMultiplier = 1.00, Note = "Ice and seasonal frost; medium hazard." } // game
        };

        public static readonly MarsInstrument[] Instruments =
        {
            new MarsInstrument { Id = "mastcam", Name = "Mastcam-Z", MassKg = 5, PowerW = 10, DataMbPerHour = 30, CostM = 40, Points = 8, Note = "Zoom stereo cameras." }, // game
            new MarsInstrument { Id = "supercam", Name = "SuperCam", MassKg = 8, PowerW = 12, DataMbPerHour = 20, CostM = 80, Points = 15, Note = "Remote laser spectroscopy." }, // game
            new MarsInstrument { Id = "pixl", Name = "PIXL", MassKg = 5, PowerW = 10, DataMbPerHour = 15, CostM = 60, Points = 12, Note = "X-ray lithochemistry." }, // game
            new MarsInstrument { Id = "sherloc", Name = "SHERLOC", MassKg = 4, PowerW = 8, DataMbPerHour = 12, CostM = 50, Points = 10, Note = "Organics and Raman." }, // game
            new MarsInstrument { Id = "moxie", Name = "MOXIE", MassKg = 18, PowerW = 300, DataMbPerHour = 2, CostM = 70, Points = 6, Note = "Oxygen from CO2; very power hungry." }, // game
            new MarsInstrument { Id = "rimfax", Name = "RIMFAX", MassKg = 3, PowerW = 5, DataMbPerHour = 8, CostM = 25, Points = 8, Note = "Ground-penetrating radar." }, // game
            new MarsInstrument { Id = "meda", Name = "MEDA", MassKg = 5, PowerW = 3, DataMbPerHour = 1, CostM = 30, Points = 4, Note = "Weather station." } // game
        };

        public const double ParachuteMassKg = 60;   // game
        public const double ParachuteCostM = 15;    // game
        public const double NoParachuteFactor = 0.94; // game
        public const double TrnMassKg = 3;          // game
        public const double TrnCostM = 10;          // game
        public const double TrnBonus = 0.06;        // game hazardous sites only
        public const double RelayMassKg = 15;       // game
        public const double RelayCostM = 50;        // game
        public const double DteMassKg = 20;         // game
        public const double DteCostM = 50;          // game
        public const double ArmMassKg = 45;         // game (turret heritage; source TODO)
        public const double ArmCostM = 70;          // game

        // ---- lookups ---------------------------------------------------------

        public static MarsLaunchVehicle Vehicle(string id) => Find(Vehicles, v => v.Id == id) ?? Vehicles[2];
        public static MarsRoverBus Bus(string id) => Find(Buses, v => v.Id == id) ?? Buses[2];
        public static MarsMobility Wheels(string id) => Find(Mobility, v => v.Id == id) ?? Mobility[1];
        public static MarsThermalOption ThermalOption(string id) => Find(Thermal, v => v.Id == id) ?? Thermal[1];
        public static MarsEdlSystem EdlSystem(string id) => Find(EdlSystems, v => v.Id == id) ?? EdlSystems[1];
        public static MarsHeatShield Shield(string id) => Find(HeatShields, v => v.Id == id) ?? HeatShields[1];
        public static MarsSite Site(string id) => Find(Sites, v => v.Id == id) ?? Sites[1];
        public static MarsInstrument Instrument(string id) => Find(Instruments, v => v.Id == id);

        public static int ArrayIndex(double area)
        {
            for (int i = 0; i < Arrays.Length; i++)
                if (Math.Abs(Arrays[i].AreaM2 - area) < 0.01) return i;
            return 1;
        }

        public static int BatteryIndex(double wh)
        {
            for (int i = 0; i < Batteries.Length; i++)
                if (Math.Abs(Batteries[i].CapacityWh - wh) < 0.01) return i;
            return 2;
        }

        public static int InstrumentIndex(string id)
        {
            for (int i = 0; i < Instruments.Length; i++)
                if (Instruments[i].Id == id) return i;
            return -1;
        }

        static T Find<T>(T[] list, Func<T, bool> match) where T : class
        {
            for (int i = 0; i < list.Length; i++)
                if (match(list[i])) return list[i];
            return null;
        }
    }
}
