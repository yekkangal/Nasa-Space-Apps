// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>
    /// Deterministic Mars engineering model: transfer window, launch capacity, mass chain,
    /// EDL, surface power, relay data, radiation, operations and cost.
    /// Pure C#; no Unity types and no allocations in the Monte Carlo path.
    /// </summary>
    public static class MarsEngineeringModel
    {
        public static double C3Km2S2(int offsetDays)
        {
            int d = (int)MarsConstants.Clamp(offsetDays, -60, 60);
            return MarsConstants.HohmannC3 + 0.004 * d * d;    // game curve, anchored on src: [1]
        }

        public static double CruiseDays(int offsetDays)
        {
            return 205.0 + 0.25 * Math.Abs((double)offsetDays); // game, anchored on src: [2] 203 days
        }

        public static double EarthMarsDistanceMkm(int offsetDays)
        {
            double distance = 160.0 + 1.2 * Math.Abs((double)offsetDays); // game
            return MarsConstants.Clamp(distance, MarsConstants.MinEarthMarsMkm, MarsConstants.MaxEarthMarsMkm);
        }

        public static double LaunchCapacityKg(string vehicleId, double c3)
        {
            var vehicle = MarsCatalog.Vehicle(vehicleId);
            double factor = 1.0 - 0.045 * (c3 - 10.0);          // game
            if (factor < 0.40) factor = 0.40;
            return vehicle.Cap10Kg * factor;
        }

        public static MarsReport Evaluate(MarsDesign design)
        {
            var d = design ?? MarsDesign.CreateDefault();
            var def = new MarsDefinition();

            var vehicle = MarsCatalog.Vehicle(d.VehicleId);
            var bus = MarsCatalog.Bus(d.BusId);
            var wheels = MarsCatalog.Wheels(d.MobilityId);
            var thermal = MarsCatalog.ThermalOption(d.ThermalId);
            var edl = MarsCatalog.EdlSystem(d.EdlSystemId);
            var shield = MarsCatalog.Shield(d.HeatShieldId);
            var site = MarsCatalog.Site(d.SiteId);
            var rad = MissionCatalog.Radiation(d.RadiationId);

            var report = new MarsReport
            {
                Design = d,
                Definition = def,
                Vehicle = vehicle,
                Bus = bus,
                Mobility = wheels,
                Thermal = thermal,
                EdlSystem = edl,
                Shield = shield,
                Site = site,
                RadiationClass = rad
            };

            // ---- transfer --------------------------------------------------------
            int offset = (int)MarsConstants.Clamp(d.LaunchOffsetDays, -60, 60);
            double c3 = C3Km2S2(offset);
            double cruise = CruiseDays(offset);
            double distance = EarthMarsDistanceMkm(offset);
            double arrivalVinf = MarsConstants.HohmannArrivalVinfKmS + 0.002 * Math.Abs(offset); // game

            report.Transfer = new MarsTransferReport
            {
                OffsetDays = offset,
                C3Km2S2 = c3,
                CruiseDays = cruise,
                EarthMarsDistanceMkm = distance,
                ArrivalVinfKmS = arrivalVinf,
                LightTimeMinutes = MarsConstants.LightTimeMinutes(distance),
                WindowNote = offset == 0 ? "OPTIMAL WINDOW"
                    : (Math.Abs(offset) <= 15 ? "GOOD WINDOW"
                        : (Math.Abs(offset) <= 30 ? "MARGINAL WINDOW" : "OFF-OPTIMAL WINDOW"))
            };

            // ---- mass chain ------------------------------------------------------
            double radMass = rad.MassMultiplier;
            double busKg = bus.MassKg * radMass;
            double instKg = d.InstrumentMassKg * radMass;
            double mobilityKg = wheels.MassKg;
            double armKg = d.RoboticArm ? MarsCatalog.ArmMassKg : 0.0;
            bool solar = d.IsSolar;
            double powerMass = solar ? 0.0 : MarsCatalog.Mmrtg.MassKg;
            double arrayMass = solar ? d.SolarAreaM2 * 5.0 : 0.0;
            double batteryMass = d.BatteryWh / MarsConstants.BatteryWhPerKg;
            double thermalKg = thermal.MassKg;
            double trnKg = d.Trn ? MarsCatalog.TrnMassKg : 0.0;
            double commsKg = (d.RelayEnabled ? MarsCatalog.RelayMassKg : 0.0)
                             + (d.DirectToEarth ? MarsCatalog.DteMassKg : 0.0);

            double baseRover = busKg + instKg + mobilityKg + armKg + powerMass + arrayMass
                               + batteryMass + thermalKg + trnKg + commsKg;
            double rover = baseRover * MarsConstants.RoverMassMargin;
            double entryHardware = shield.MassKg + (d.Parachute ? MarsCatalog.ParachuteMassKg : 0.0);
            double landed = rover;
            double launch = landed / edl.LandedFraction + entryHardware;

            double capacity = LaunchCapacityKg(d.VehicleId, c3);
            double limit = capacity * MarsConstants.LaunchCapacityMargin;

            report.Mass = new MarsMassReport
            {
                BusKg = busKg,
                MobilityKg = mobilityKg,
                ArmKg = armKg,
                PowerKg = powerMass + arrayMass,
                BatteryKg = batteryMass,
                ThermalKg = thermalKg,
                TrnKg = trnKg,
                CommsKg = commsKg,
                InstrumentKg = instKg,
                EntryHardwareKg = entryHardware,
                RoverKg = rover,
                LandedKg = landed,
                LaunchKg = launch,
                LandedFraction = edl.LandedFraction,
                CapacityKg = capacity,
                LimitKg = limit,
                UsageFraction = capacity > 0 ? launch / capacity : 99.0,
                Pass = capacity > 0 && launch <= limit
            };

            // ---- EDL -------------------------------------------------------------
            double massRatio = landed / edl.LandedLimitKg;
            double massFactor = massRatio <= 0.8
                ? 1.0
                : MarsConstants.Clamp(1.0 - 0.45 * (massRatio - 0.8) / 0.2, 0.55, 1.0);
            double trnBonus = d.Trn && site.Id != "safe" ? MarsCatalog.TrnBonus : 0.0;
            double chuteFactor = d.Parachute ? 1.0 : MarsCatalog.NoParachuteFactor;

            double edlSuccess = (edl.BaseSuccess * massFactor * site.HazardFactor + trnBonus)
                                * shield.SuccessFactor * chuteFactor;
            edlSuccess = MarsConstants.Clamp(edlSuccess, 0.02, 0.98);

            report.Edl = new MarsEdlReport
            {
                SystemName = edl.Name,
                BaseSuccess = edl.BaseSuccess,
                MassFactor = massFactor,
                SiteHazard = site.HazardFactor,
                TrnBonus = trnBonus,
                ShieldFactor = shield.SuccessFactor,
                ParachuteFactor = chuteFactor,
                SuccessProbability = edlSuccess,
                LandedLimitKg = edl.LandedLimitKg,
                SiteScienceMultiplier = site.ScienceMultiplier,
                Pass = edlSuccess >= 0.60 && landed <= edl.LandedLimitKg * 1.05
            };

            // ---- surface power ---------------------------------------------------
            double missionYears = (cruise + MarsConstants.PrimeMissionSols) / 365.25;
            double mmrtgW = MarsConstants.MmrtgPowerW * Math.Pow(MarsConstants.MmrtgDecayPerYear, missionYears);
            double solarNominal = d.SolarAreaM2 * MarsConstants.ArrayEfficiency * MarsConstants.SolarIrradianceWm2
                                  * MarsConstants.ArraySunHours * MarsConstants.SeasonalNominalFactor;
            double solarWorst = d.SolarAreaM2 * MarsConstants.ArrayEfficiency * MarsConstants.SolarIrradianceWm2
                                * MarsConstants.ArraySunHours * MarsConstants.SeasonalWorstFactor
                                * MarsConstants.DustStormWorstMarginFactor;

            double genNominal = solar ? solarNominal : mmrtgW * MarsConstants.SolHours;
            double genWorst = solar ? solarWorst : mmrtgW * MarsConstants.SolHours;

            double driveHours = d.DriveWeight * MarsConstants.DaylightHours;
            double scienceHours = d.ScienceWeight * MarsConstants.DaylightHours;
            double drillHours = d.DrillWeight * MarsConstants.DaylightHours;
            double commsHours = d.CommsWeight * MarsConstants.DaylightHours;

            double distanceMPerSol = driveHours * MarsConstants.DriveSpeedMPerHour;
            double samplesPerSol = drillHours / MarsConstants.DrillHoursPerSample;
            double activePower = d.InstrumentPowerW;

            double house = MarsConstants.HousekeepingWhSol;
            double heater = thermal.HeaterWhSol;
            double driveWh = distanceMPerSol * MarsConstants.DriveWhPerMeter;
            double scienceWh = activePower * scienceHours;
            double drillWh = samplesPerSol * MarsConstants.DrillWhPerSample;
            int passes = d.RelayEnabled ? Math.Max(0, Math.Min(2, d.RelayPasses)) : 0;
            double commsWh = passes * MarsConstants.CommWhPerPass;
            double load = house + heater + driveWh + scienceWh + drillWh + commsWh;

            double nightEnergy = (house + heater) * MarsConstants.NightFraction;
            double mmrtgNight = solar ? 0.0 : mmrtgW * (MarsConstants.SolHours - MarsConstants.DaylightHours);
            double batteryNeed = Math.Max(0.0, nightEnergy - mmrtgNight) / MarsConstants.BatteryDod;
            double batteryUsable = d.BatteryWh * MarsConstants.BatteryDod;
            bool batteryPass = batteryUsable + 0.001 >= batteryNeed;

            report.Power = new MarsPowerReport
            {
                Mmrtg = !solar,
                GenerationWhSol = genNominal,
                WorstGenerationWhSol = genWorst,
                LoadWhSol = load,
                Margin = load > 0.001 ? (genNominal - load) / load : 9.99,
                WorstMargin = load > 0.001 ? (genWorst - load) / load : 9.99,
                HousekeepingWh = house,
                HeaterWh = heater,
                DriveWh = driveWh,
                ScienceWh = scienceWh,
                DrillWh = drillWh,
                CommsWh = commsWh,
                BatteryNeedWh = batteryNeed,
                BatteryUsableWh = batteryUsable,
                BatteryPass = batteryPass,
                Pass = genWorst >= load && batteryPass
            };

            // ---- comms and data --------------------------------------------------
            double relayRate = d.RelayEnabled ? MarsConstants.RelayTypicalMbps : 0.0;
            double relayMb = relayRate * 1e6 * (MarsConstants.RelayPassMinutes * 60.0) * passes / 1e6;
            double dteMb = d.DirectToEarth
                ? MarsConstants.DteRateBps * MarsConstants.DteHoursPerSol * 3600.0 / 1e6
                : 0.0;
            double downlink = relayMb + dteMb;

            report.Comms = new MarsCommsReport
            {
                RelayEnabled = d.RelayEnabled,
                RelayRateMbps = relayRate,
                RelayPasses = passes,
                RelayMbSol = relayMb,
                DteMbSol = dteMb,
                TotalMbSol = downlink,
                LightTimeMinutes = report.Transfer.LightTimeMinutes,
                ConjunctionDays = MarsConstants.ConjunctionDays,
                Pass = downlink >= 20.0
            };

            double scienceMb = d.InstrumentDataMbPerHour * scienceHours
                               + samplesPerSol * MarsConstants.SampleDataMb;
            double fraction = scienceMb > 0.001 ? downlink / scienceMb : 1.0;
            if (fraction > 1.0) fraction = 1.0;
            double blackoutLoss = MarsConstants.ConjunctionDataLoss;
            double effectiveFraction = fraction * (1.0 - blackoutLoss);

            report.Surface = new MarsSurfaceReport
            {
                SolHours = MarsConstants.SolHours,
                DaylightHours = MarsConstants.DaylightHours,
                DriveHours = driveHours,
                ScienceHours = scienceHours,
                DrillHours = drillHours,
                CommsHours = commsHours,
                DistanceMPerSol = distanceMPerSol,
                SamplesPerSol = samplesPerSol,
                ExpectedSamples = samplesPerSol * MarsConstants.PrimeMissionSols * MarsConstants.SampleSuccessChance,
                ActiveInstrumentPowerW = activePower,
                ActiveDataMbPerHour = d.InstrumentDataMbPerHour,
                SampleSuccessChance = MarsConstants.SampleSuccessChance,
                Pass = samplesPerSol >= 0
            };

            report.Data = new MarsDataReport
            {
                ScienceMbSol = scienceMb,
                DownlinkMbSol = downlink,
                Fraction = effectiveFraction,
                BlackoutLoss = blackoutLoss,
                Pass = effectiveFraction >= MarsConstants.BaselineScienceFraction
            };

            // ---- radiation -------------------------------------------------------
            double cruiseMsv = MarsConstants.CruiseDoseMsvDay * cruise;      // src: [8]
            double surfaceMsv = MarsConstants.SurfaceDoseMsvDay * MarsConstants.PrimeMissionSols; // src: [8]
            double totalMsv = cruiseMsv + surfaceMsv;
            double equivalentKrad = totalMsv / 10000.0;                      // game conversion

            report.Radiation = new MarsRadiationReport
            {
                CruiseMsv = cruiseMsv,
                SurfaceMsv = surfaceMsv,
                TotalMsv = totalMsv,
                EquivalentKrad = equivalentKrad,
                ToleranceKrad = rad.ToleranceKrad,
                SurfaceToCruiseRatio = MarsConstants.SurfaceDoseMsvDay / MarsConstants.CruiseDoseMsvDay, // src: [8] ~0.37
                Pass = equivalentKrad <= rad.ToleranceKrad
            };

            // ---- cost ------------------------------------------------------------
            double parts =
                bus.CostM * rad.CostMultiplier
                + wheels.CostM
                + (d.RoboticArm ? MarsCatalog.ArmCostM : 0.0)
                + (solar ? d.SolarAreaM2 * 1.5 : MarsCatalog.Mmrtg.CostM)
                + batteryMass * 0.1
                + thermal.CostM
                + (d.Trn ? MarsCatalog.TrnCostM : 0.0)
                + (d.RelayEnabled ? MarsCatalog.RelayCostM : 0.0)
                + (d.DirectToEarth ? MarsCatalog.DteCostM : 0.0)
                + edl.CostM
                + shield.CostM
                + (d.Parachute ? MarsCatalog.ParachuteCostM : 0.0);

            double instruments = d.InstrumentCostM * rad.CostMultiplier;
            double development = parts + instruments;
            double contingency = development * MarsConstants.Contingency;
            double ops = MarsConstants.OpsCostMPerMonth * MarsConstants.OpsMonths;

            report.Cost = new MarsCostReport
            {
                PartsM = parts,
                InstrumentsM = instruments,
                DevelopmentM = development,
                ContingencyM = contingency,
                LaunchM = vehicle.CostM,
                GroundM = MarsConstants.GroundCostM,
                OpsM = ops,
                TotalM = development + contingency + vehicle.CostM + MarsConstants.GroundCostM + ops,
                CapM = def.BudgetCapM,
                Pass = development + contingency + vehicle.CostM + MarsConstants.GroundCostM + ops <= def.BudgetCapM
            };

            report.Reliability = new MarsReliabilityReport
            {
                LaunchReliability = vehicle.Reliability,
                CruiseSuccess = 0.98,                             // game
                EdlSuccess = edlSuccess,
                MonthlyFailure = MarsConstants.SurfaceFailurePerMonth,
                SampleSuccess = MarsConstants.SampleSuccessChance
            };

            // ---- levels and points ----------------------------------------------
            float sciencePotential = d.InstrumentPoints * (float)site.ScienceMultiplier;
            float earned = sciencePotential * (float)effectiveFraction;
            int samples = (int)Math.Floor(report.Surface.ExpectedSamples);

            bool level1 = report.Mass.Pass && report.Edl.Pass;
            bool level2 = sciencePotential > 0.01f && effectiveFraction >= MarsConstants.BaselineScienceFraction;
            bool level3 = samples >= MarsConstants.SampleTarget;
            bool level4 = report.Power.Pass;
            bool budget = report.Cost.Pass;

            float l3Points = Math.Min(MarsConstants.Level3MaxPoints, samples * 3f);
            float achievable = (level1 ? MarsConstants.Level1Points : 0f)
                               + earned
                               + l3Points
                               + (level4 ? MarsConstants.Level4Points : 0f)
                               + (budget ? MarsConstants.BudgetPoints : 0f);

            report.SciencePotential = sciencePotential;
            report.ScienceEarned = earned;
            report.ExpectedSamples = samples;
            report.BudgetPass = budget;
            report.Level1 = level1;
            report.Level2 = level2;
            report.Level3 = level3;
            report.Level4 = level4;
            report.AchievablePoints = achievable;

            if (level1 && level2 && level3 && budget) report.DeterministicRating = 2;
            else if (level1 && sciencePotential > 0.01f && effectiveFraction > 0.001f) report.DeterministicRating = 1;
            else report.DeterministicRating = 0;

            return report;
        }
    }
}
