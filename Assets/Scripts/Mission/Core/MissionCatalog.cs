// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    public sealed class LaunchVehicleOption
    {
        public string Name;
        public string Id;
        public double LeoKg;
        public double TliKg;
        public double CostMillions;
        public double Reliability;
        public string Note;

        public bool ReachesMoon => TliKg > 0.0;
    }

    public sealed class PropulsionOption
    {
        public string Name;
        public string Id;
        public double IspSeconds;
        public double MassKg;
        public double PowerW;
        public double CostMillions;
        public bool Electric;
        public string Note;
    }

    public sealed class ArrayOption
    {
        public double AreaM2;
        public double MassKg => AreaM2 * 5.0;
        public double CostMillions => AreaM2 * 1.5;
        public string Label => AreaM2.ToString("0") + " m2";
    }

    public sealed class BatteryOption
    {
        public double CapacityWh;
        public double MassKg => CapacityWh / MissionConstants.BatteryWhPerKg;
        public double CostMillions => MassKg * 0.1;
        public string Label => CapacityWh.ToString("0") + " Wh";
    }

    public sealed class AntennaOption
    {
        public string Name;
        public string Id;
        public string Band;          // "S" | "X" | "Ka"
        public double DiameterM;
        public double ExplicitGainDbi; // 0 = compute from diameter
        public double MassKg;
        public double CostMillions;
        public double FrequencyMHz;
        public double BandCapBps;
    }

    public sealed class RfPowerOption
    {
        public double Watts;
        public double MassKg;
        public double CostMillions;
        public string Label => Watts.ToString("0") + " W";
    }

    public sealed class GroundStationOption
    {
        public string Name;
        public string Id;
        public double DiameterM;
        public double SystemTempK;
        public double VisibilityHoursPerDay;
        public double CostPerMonthMillions;
        public string Note;
    }

    public sealed class RadiationOption
    {
        public string Name;
        public string Id;
        public double ToleranceKrad;
        public double CostMultiplier;
        public double MassMultiplier;
        public string Note;
    }

    public sealed class InstrumentOption
    {
        public string Name;
        public string Id;
        public double MassKg;
        public double PowerW;
        public double RawGbPerDay;
        public double CostMillions;
        public float Points;
        public string Note;
    }

    /// <summary>Static catalog of every part. No ScriptableObjects, nothing wired by hand.</summary>
    public static class MissionCatalog
    {
        public static readonly LaunchVehicleOption[] LaunchVehicles =
        {
            new LaunchVehicleOption { Name = "Kestrel-1", Id = "kestrel1", LeoKg = 300, TliKg = 0, CostMillions = 8, Reliability = 0.93, Note = "Small-sat launcher - LEO only, cannot reach the Moon." }, // game
            new LaunchVehicleOption { Name = "Condor-5", Id = "condor5", LeoKg = 5500, TliKg = 1600, CostMillions = 55, Reliability = 0.95, Note = "Workhorse for small lunar orbiters." }, // game
            new LaunchVehicleOption { Name = "Condor-9", Id = "condor9", LeoKg = 17000, TliKg = 5500, CostMillions = 95, Reliability = 0.97, Note = "More capacity and better reliability for a price." }, // game
            new LaunchVehicleOption { Name = "Titan-H", Id = "titanh", LeoKg = 50000, TliKg = 18000, CostMillions = 240, Reliability = 0.96, Note = "Heavy lift - rarely the economical choice for one orbiter." } // game
        };

        public static readonly PropulsionOption[] Propulsion =
        {
            new PropulsionOption { Name = "Monopropellant", Id = "mono", IspSeconds = 225, MassKg = 12, PowerW = 20, CostMillions = 6, Electric = false, Note = "Simple and cheap, but thirsty." }, // game
            new PropulsionOption { Name = "Bipropellant", Id = "biprop", IspSeconds = 315, MassKg = 28, PowerW = 30, CostMillions = 14, Electric = false, Note = "Best mass fraction for a 1.32 km/s lunar budget." }, // game
            new PropulsionOption { Name = "Electric Hall (SEP)", Id = "hall", IspSeconds = 1800, MassKg = 35, PowerW = 1500, CostMillions = 22, Electric = true, Note = "Tiny propellant load, months-long spiral getting there." } // game
        };

        public static readonly ArrayOption[] Arrays =
        {
            new ArrayOption { AreaM2 = 2 }, // game
            new ArrayOption { AreaM2 = 4 }, // game
            new ArrayOption { AreaM2 = 6 }, // game
            new ArrayOption { AreaM2 = 8 } // game
        };

        public static readonly BatteryOption[] Batteries =
        {
            new BatteryOption { CapacityWh = 200 }, // game
            new BatteryOption { CapacityWh = 400 }, // game
            new BatteryOption { CapacityWh = 800 }, // game
            new BatteryOption { CapacityWh = 1600 }, // game
            new BatteryOption { CapacityWh = 3200 } // game
        };

        public static readonly AntennaOption[] Antennas =
        {
            new AntennaOption { Name = "LGA S-band", Id = "lga", Band = "S", DiameterM = 0, ExplicitGainDbi = 6, MassKg = 1, CostMillions = 1, FrequencyMHz = 2280, BandCapBps = 2e6 }, // game
            new AntennaOption { Name = "X-band dish 0.5 m", Id = "x05", Band = "X", DiameterM = 0.5, MassKg = 3, CostMillions = 3, FrequencyMHz = 8450, BandCapBps = 25e6 }, // game
            new AntennaOption { Name = "X-band dish 1.0 m", Id = "x10", Band = "X", DiameterM = 1.0, MassKg = 6, CostMillions = 6, FrequencyMHz = 8450, BandCapBps = 25e6 }, // game
            new AntennaOption { Name = "X-band dish 1.5 m", Id = "x15", Band = "X", DiameterM = 1.5, MassKg = 11, CostMillions = 10, FrequencyMHz = 8450, BandCapBps = 25e6 }, // game
            new AntennaOption { Name = "Ka-band dish 1.0 m", Id = "ka10", Band = "Ka", DiameterM = 1.0, MassKg = 8, CostMillions = 12, FrequencyMHz = 32000, BandCapBps = 150e6 } // game
        };

        public static readonly RfPowerOption[] RfPowers =
        {
            new RfPowerOption { Watts = 10, MassKg = 3, CostMillions = 2 }, // game
            new RfPowerOption { Watts = 25, MassKg = 5, CostMillions = 3 }, // game
            new RfPowerOption { Watts = 50, MassKg = 8, CostMillions = 5 }, // game
            new RfPowerOption { Watts = 100, MassKg = 12, CostMillions = 8 } // game
        };

        public static readonly GroundStationOption[] GroundStations =
        {
            new GroundStationOption { Name = "University 10 m", Id = "uni", DiameterM = 10, SystemTempK = 100, VisibilityHoursPerDay = 6, CostPerMonthMillions = 0.3, Note = "Cheap student station, 6 h/day of Moon visibility." }, // verify
            new GroundStationOption { Name = "Deep Space 34 m", Id = "dss", DiameterM = 34, SystemTempK = 30, VisibilityHoursPerDay = 10, CostPerMonthMillions = 2.0, Note = "Big aperture and cold receiver - ten times the price." } // verify
        };

        public static readonly RadiationOption[] RadiationClasses =
        {
            new RadiationOption { Name = "COTS", Id = "cots", ToleranceKrad = 10, CostMultiplier = 1.00, MassMultiplier = 1.00, Note = "Cheap commercial parts, 10 krad tolerance." }, // game
            new RadiationOption { Name = "Mil-spec", Id = "mil", ToleranceKrad = 30, CostMultiplier = 1.60, MassMultiplier = 1.08, Note = "Screened parts, 30 krad, heavier and pricier." }, // game
            new RadiationOption { Name = "Rad-hard", Id = "radhard", ToleranceKrad = 100, CostMultiplier = 2.80, MassMultiplier = 1.15, Note = "100 krad and expensive; needed for long electric spirals." } // game
        };

        public static readonly InstrumentOption[] Instruments =
        {
            new InstrumentOption { Name = "Narrow-angle camera", Id = "nac", MassKg = 20, PowerW = 30, RawGbPerDay = 30, CostMillions = 25, Points = 25, Note = "High-resolution surface imaging." }, // game
            new InstrumentOption { Name = "Multispectral imager", Id = "msi", MassKg = 12, PowerW = 15, RawGbPerDay = 12, CostMillions = 18, Points = 15, Note = "Mineralogy and composition mapping." }, // game
            new InstrumentOption { Name = "Laser altimeter", Id = "lalt", MassKg = 14, PowerW = 25, RawGbPerDay = 1, CostMillions = 20, Points = 15, Note = "Precise topography, tiny data volume." }, // game
            new InstrumentOption { Name = "Neutron spectrometer", Id = "ns", MassKg = 8, PowerW = 10, RawGbPerDay = 0.1, CostMillions = 10, Points = 25, Note = "The water-ice detector. Mandatory for O3." }, // game
            new InstrumentOption { Name = "Radar sounder", Id = "radar", MassKg = 35, PowerW = 60, RawGbPerDay = 8, CostMillions = 45, Points = 20, Note = "Subsurface layers; heavy and power hungry." }, // game
            new InstrumentOption { Name = "Magnetometer", Id = "mag", MassKg = 3, PowerW = 4, RawGbPerDay = 0.02, CostMillions = 4, Points = 8, Note = "Cheap, light, almost no data." } // game
        };

        // ----- lookup helpers -------------------------------------------------

        public static LaunchVehicleOption Vehicle(string id) => Find(LaunchVehicles, v => v.Id == id) ?? LaunchVehicles[1];
        public static PropulsionOption Engine(string id) => Find(Propulsion, v => v.Id == id) ?? Propulsion[1];
        public static AntennaOption Antenna(string id) => Find(Antennas, v => v.Id == id) ?? Antennas[2];
        public static GroundStationOption Station(string id) => Find(GroundStations, v => v.Id == id) ?? GroundStations[0];
        public static RadiationOption Radiation(string id) => Find(RadiationClasses, v => v.Id == id) ?? RadiationClasses[1];
        public static InstrumentOption Instrument(string id) => Find(Instruments, v => v.Id == id);

        public static int ArrayIndex(double areaM2)
        {
            for (int i = 0; i < Arrays.Length; i++)
                if (Math.Abs(Arrays[i].AreaM2 - areaM2) < 0.001) return i;
            return 1;
        }

        public static int BatteryIndex(double wh)
        {
            for (int i = 0; i < Batteries.Length; i++)
                if (Math.Abs(Batteries[i].CapacityWh - wh) < 0.001) return i;
            return 2;
        }

        public static int RfIndex(double watts)
        {
            for (int i = 0; i < RfPowers.Length; i++)
                if (Math.Abs(RfPowers[i].Watts - watts) < 0.001) return i;
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
