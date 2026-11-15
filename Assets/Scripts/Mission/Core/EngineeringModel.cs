// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>
    /// Deterministic engineering model: mass, delta-v, power, communications, data,
    /// radiation, cost. Pure C#, no Unity types, safe to call every frame.
    /// </summary>
    public static class EngineeringModel
    {
        /// <summary>Delta-v needed by the chosen propulsion type, in km/s.</summary>
        public static double RequiredDeltaVKmS(MissionDesign d)
        {
            var engine = MissionCatalog.Engine(d.PropulsionId);
            double years = MissionConstants.MissionMonths / 12.0;
            if (engine.Electric)
                return MissionConstants.ElectricSpiralKmS * MissionConstants.ChemicalMarginFactor;
            double raw = MissionConstants.ChemicalMidcourseKmS
                         + MissionConstants.ChemicalLoiKmS
                         + MissionConstants.ChemicalTrimKmS
                         + MissionConstants.ChemicalStationKeepKmSPerYear * years
                         + MissionConstants.ChemicalDesatKmS;
            return raw * MissionConstants.ChemicalMarginFactor;
        }

        public static EngineeringReport Evaluate(MissionDesign design)
        {
            var d = design ?? MissionDesign.CreateDefault();

            var vehicle = MissionCatalog.Vehicle(d.VehicleId);
            var engine = MissionCatalog.Engine(d.PropulsionId);
            var antenna = MissionCatalog.Antenna(d.AntennaId);
            var station = MissionCatalog.Station(d.GroundStationId);
            var rad = MissionCatalog.Radiation(d.RadiationId);
            var rf = MissionCatalog.RfPowers[MissionCatalog.RfIndex(d.RfPowerW)];
            var array = MissionCatalog.Arrays[MissionCatalog.ArrayIndex(d.ArrayAreaM2)];
            var battery = MissionCatalog.Batteries[MissionCatalog.BatteryIndex(d.BatteryWh)];

            var report = new EngineeringReport
            {
                Design = d,
                Vehicle = vehicle,
                Engine = engine,
                Antenna = antenna,
                Station = station,
                RadiationClass = rad
            };

            double years = MissionConstants.MissionMonths / 12.0;

            // ---------------- mass -------------------------------------------------
            double instMass = d.InstrumentMassKg;
            double radMassFactor = rad.MassMultiplier;
            double subsystemMass =
                (40.0 * radMassFactor)                 // bus avionics
                + engine.MassKg * radMassFactor        // propulsion
                + array.MassKg
                + battery.MassKg
                + antenna.MassKg
                + rf.MassKg;
            if (d.Redundancy) subsystemMass *= 1.08;

            double structure = (instMass + subsystemMass) * MissionConstants.StructureFraction;
            double baseDry = instMass + subsystemMass + structure;

            double dv = RequiredDeltaVKmS(d);
            double dvMps = dv * 1000.0;
            double massRatio = Math.Exp(dvMps / (engine.IspSeconds * MissionConstants.G0)) - 1.0;

            double margin = 1.0 + MissionConstants.MassMargin;
            double tankCoef = MissionConstants.TankFraction * margin;
            double propellant;
            double denom = 1.0 - tankCoef * massRatio;
            if (denom <= 0.02) denom = 0.02; // degenerate super-heavy designs
            propellant = margin * baseDry * massRatio / denom;
            double tank = MissionConstants.TankFraction * propellant;
            double dry = (baseDry + tank) * margin;
            double wet = dry + propellant;

            double capacity = engine.Electric ? vehicle.LeoKg : vehicle.TliKg;
            double limit = capacity * MissionConstants.CapacityMassLimit;

            report.Mass = new MassReport
            {
                InstrumentsKg = instMass,
                SubsystemsKg = subsystemMass,
                StructureKg = structure,
                BaseDryKg = baseDry,
                TankKg = tank,
                DryKg = dry,
                PropellantKg = propellant,
                WetKg = wet,
                CapacityKg = capacity,
                LimitKg = limit,
                UsageFraction = capacity > 0 ? wet / capacity : 999.0,
                Pass = capacity > 0 && wet > 0 && wet <= limit,
                CapacityBasis = engine.Electric ? "LEO" : "TLI"
            };

            // ---------------- propulsion / spiral --------------------------------
            double arrayBol = array.AreaM2 * MissionConstants.ArrayWPerM2 * MissionConstants.ArrayCosFactor;
            double arrayEol = arrayBol * Math.Pow(MissionConstants.ArrayDegradationPerYear, years);

            double spiralMonths = 0.0;
            double thrust = 0.0;
            double accel = 0.0;
            if (engine.Electric)
            {
                double spiralPower = arrayBol * 0.85;              // array pointed at the sun during the spiral
                thrust = 2.0 * MissionConstants.HallEfficiency * spiralPower / (engine.IspSeconds * MissionConstants.G0);
                double mAvg = Math.Max(1.0, (wet + dry) * 0.5);
                accel = thrust / mAvg;
                if (accel > 0.0)
                    spiralMonths = (dvMps / accel) / (86400.0 * 30.44);
            }

            report.Propulsion = new PropulsionReport
            {
                Electric = engine.Electric,
                DeltaVKmS = dv,
                IspSeconds = engine.IspSeconds,
                PropellantKg = propellant,
                PropellantFraction = wet > 0 ? propellant / wet : 0.0,
                SpiralMonths = spiralMonths,
                ThrustN = thrust,
                AccelerationMps2 = accel,
                MassRatio = massRatio
            };

            // ---------------- power ----------------------------------------------
            double commsDc = rf.Watts / MissionConstants.CommsChainEfficiency;
            double busThermal = MissionConstants.BusPowerW + MissionConstants.ThermalPowerW;
            double instPower = d.InstrumentPowerW;
            double enginePower = engine.Electric ? engine.PowerW : 0.0;

            var modes = new PowerModeResult[4];
            double averageGeneration = arrayEol * (1.0 - MissionConstants.EclipseFractionAverage);
            modes[0] = MakeMode("CRUISE", averageGeneration, busThermal + commsDc + enginePower);
            modes[1] = MakeMode("SCIENCE", averageGeneration, busThermal + instPower);
            modes[2] = MakeMode("SCIENCE+DOWNLINK", averageGeneration, busThermal + commsDc + instPower);
            modes[3] = MakeMode("SAFE", averageGeneration, busThermal + commsDc * 0.10);

            double worstLoad = 0.0;
            double minMargin = double.MaxValue;
            for (int i = 0; i < modes.Length; i++)
            {
                if (modes[i].LoadW > worstLoad) worstLoad = modes[i].LoadW;
                if (modes[i].Margin < minMargin) minMargin = modes[i].Margin;
            }

            double periodHours = MissionConstants.OrbitPeriodSeconds() / 3600.0;
            double eclipseHours = MissionConstants.EclipseFractionWorst * periodHours;
            double eclipseEnergy = worstLoad * eclipseHours;
            double batteryUsable = battery.CapacityWh * MissionConstants.BatteryUsableDod;
            bool batteryPass = batteryUsable >= eclipseEnergy;

            report.Power = new PowerReport
            {
                ArrayBeginningOfLifeW = arrayBol,
                ArrayEndOfLifeW = arrayEol,
                AverageGenerationW = averageGeneration,
                WorstLoadW = worstLoad,
                EclipsePeriodHours = eclipseHours,
                EclipseEnergyWh = eclipseEnergy,
                BatteryUsableWh = batteryUsable,
                BatteryWh = battery.CapacityWh,
                BatteryPass = batteryPass,
                MinMargin = minMargin,
                Pass = minMargin >= MissionConstants.MinPowerMargin && batteryPass,
                Modes = modes
            };

            // ---------------- communications --------------------------------------
            double lambda = MissionConstants.LightSpeedKmS * 1000.0 / (antenna.FrequencyMHz * 1e6);
            double txGain = antenna.ExplicitGainDbi > 0.0
                ? antenna.ExplicitGainDbi
                : 10.0 * Math.Log10(0.55 * Math.Pow(Math.PI * antenna.DiameterM / lambda, 2.0));
            double eirp = 10.0 * Math.Log10(rf.Watts) + txGain - 1.5;
            double groundGain = 10.0 * Math.Log10(0.55 * Math.Pow(Math.PI * station.DiameterM / lambda, 2.0));
            double groundGt = groundGain - 10.0 * Math.Log10(station.SystemTempK);
            double fspl = 20.0 * Math.Log10(MissionConstants.EarthMoonDistanceKm)
                          + 20.0 * Math.Log10(antenna.FrequencyMHz) + 32.44;
            double cn0 = eirp + groundGt - fspl - MissionConstants.LinkMiscDb + 228.6;

            double maxRate = Math.Pow(10.0, (cn0 - MissionConstants.LinkRequiredEbNoDb) / 10.0);
            if (maxRate > antenna.BandCapBps) maxRate = antenna.BandCapBps;
            double usedRate = maxRate;
            double linkMargin = cn0 - 10.0 * Math.Log10(Math.Max(1.0, usedRate)) - MissionConstants.LinkRequiredEbNoDb;

            double contactHours = station.VisibilityHoursPerDay * 0.5;   // polar orbit occultation

            report.Link = new LinkReport
            {
                TxGainDbi = txGain,
                EirpDbi = eirp,
                GroundGainDbi = groundGain,
                GroundGOverT = groundGt,
                FsplDb = fspl,
                CarrierToNoiseDb = cn0,
                MaxRateBps = maxRate,
                BandCapBps = antenna.BandCapBps,
                UsedRateBps = usedRate,
                MarginDb = linkMargin,
                ContactHoursPerDay = contactHours,
                FrequencyMHz = antenna.FrequencyMHz,
                Band = antenna.Band,
                Pass = linkMargin >= MissionConstants.LinkThresholdDb
            };

            // ---------------- data -------------------------------------------------
            double scienceGb = d.ScienceRawGbPerDay / 2.0;                    // 2:1 compression
            double downlinkGb = usedRate * (contactHours * 3600.0) * 0.9 / 8.0 / 1e9;   // 10% overhead
            double dataFraction = scienceGb > 0.0001 ? downlinkGb / scienceGb : 1.0;
            if (dataFraction > 1.0) dataFraction = 1.0;

            report.Data = new DataReport
            {
                ScienceGbPerDay = scienceGb,
                DownlinkGbPerDay = downlinkGb,
                Fraction = dataFraction,
                Pass = dataFraction >= 0.999
            };

            // ---------------- radiation -------------------------------------------
            double dose = MissionConstants.RadiationKradPerYear * years
                          + MissionConstants.BeltDoseKradPerMonth * spiralMonths;
            bool radPass = dose <= rad.ToleranceKrad;

            report.Radiation = new RadiationReport
            {
                DoseKrad = dose,
                ToleranceKrad = rad.ToleranceKrad,
                SpiralMonths = spiralMonths,
                Pass = radPass,
                MonthlyFailureProbability = radPass ? 0.002 : 0.08
            };

            // ---------------- cost -------------------------------------------------
            double radiationCost = rad.CostMultiplier;
            double parts = (8.0                                        // bus avionics
                            + engine.CostMillions
                            + array.CostMillions
                            + battery.CostMillions
                            + antenna.CostMillions
                            + rf.CostMillions) * radiationCost;
            if (d.Redundancy) parts *= 1.10;
            double instruments = d.InstrumentCostMillions * radiationCost;
            double development = parts + instruments;
            double contingency = development * MissionConstants.ContingencyFactor;
            double ground = station.CostPerMonthMillions * MissionConstants.MissionMonths;
            double ops = MissionConstants.OpsCostPerMonth * (MissionConstants.MissionMonths + spiralMonths);
            double total = development + contingency + vehicle.CostMillions + ground + ops;
            double cap = MissionDefinition.MoonDefinition.BudgetCapMillions;

            report.Cost = new CostReport
            {
                PartsMillions = parts,
                InstrumentsMillions = instruments,
                DevelopmentMillions = development,
                ContingencyMillions = contingency,
                LaunchMillions = vehicle.CostMillions,
                GroundMillions = ground,
                OpsMillions = ops,
                TotalMillions = total,
                CapMillions = cap,
                Pass = total <= cap
            };

            // ---------------- reliability -----------------------------------------
            double busRel = d.Redundancy ? 0.995 : 0.98;
            double monthlyBusFail = 1.0 - Math.Pow(busRel, 1.0 / 12.0);
            report.Reliability = new ReliabilityReport
            {
                LaunchReliability = vehicle.Reliability,
                BusAnnualReliability = busRel,
                MonthlyBusFailure = monthlyBusFail,
                Redundancy = d.Redundancy
            };

            // ---------------- feasibility & points --------------------------------
            report.OrbitFeasible = report.Mass.Pass && vehicle.ReachesMoon;
            float sciencePotential = d.InstrumentPoints;
            float deterministic =
                30f
                + sciencePotential * (float)dataFraction
                + (report.Cost.Pass ? 10f : 0f)
                + (report.Power.Pass ? 10f : 0f);
            report.SciencePoints = sciencePotential;
            report.AchievablePoints = deterministic;

            return report;
        }

        static PowerModeResult MakeMode(string name, double generation, double load)
        {
            var m = new PowerModeResult
            {
                Name = name,
                GenerationW = generation,
                LoadW = load,
                Margin = load > 0.001 ? (generation - load) / load : 999.0
            };
            m.Pass = m.Margin >= MissionConstants.MinPowerMargin;
            return m;
        }

        /// <summary>Rocket equation helper: propellant for a given dry mass and delta-v.</summary>
        public static double PropellantFor(double dryMassKg, double deltaVMps, double ispSeconds)
        {
            if (dryMassKg <= 0.0 || ispSeconds <= 0.0) return 0.0;
            return dryMassKg * (Math.Exp(deltaVMps / (ispSeconds * MissionConstants.G0)) - 1.0);
        }

        /// <summary>Rocket equation helper: delta-v for a mass ratio.</summary>
        public static double DeltaVFromMassRatio(double wetKg, double dryKg, double ispSeconds)
        {
            if (wetKg <= 0.0 || dryKg <= 0.0 || wetKg <= dryKg) return 0.0;
            return ispSeconds * MissionConstants.G0 * Math.Log(wetKg / dryKg);
        }

        public static double DownlinkGbPerDay(double rateBps, double contactHours)
        {
            return rateBps * (contactHours * 3600.0) * 0.9 / 8.0 / 1e9;
        }
    }
}
