// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>Deterministic asteroid sample-return model: target physics, trajectory, mass, power, comms, TAG, capsule, cost.</summary>
    public static class AsteroidEngineeringModel
    {
        public static AsteroidReport Evaluate(AsteroidDesign design)
        {
            var d = design ?? AsteroidDesign.CreateDefault();
            var def = new AsteroidDefinition();

            var target = AsteroidCatalog.Target(d.TargetId);
            var bus = AsteroidCatalog.Bus(d.BusId);
            var engine = AsteroidCatalog.Engine(d.PropulsionId);
            var sampler = AsteroidCatalog.Sampler(d.SamplerId);
            var capsule = AsteroidCatalog.Capsule(d.CapsuleId);
            var site = AsteroidCatalog.Site(d.SiteId);
            var vehicle = AsteroidCatalog.Vehicle(d.VehicleId);
            var antenna = AsteroidCatalog.Antenna(d.AntennaId);
            var rf = AsteroidCatalog.RfPower(d.RfPowerW);
            var station = AsteroidCatalog.Station(d.GroundStationId);
            var rad = AsteroidCatalog.Radiation(d.RadiationId);
            var array = AsteroidCatalog.Arrays[ArrayCatalogIndex(d.ArrayAreaM2)];
            var battery = AsteroidCatalog.Batteries[BatteryCatalogIndex(d.BatteryWh)];

            var report = new AsteroidReport
            {
                Design = d,
                Definition = def,
                TargetInfo = target,
                Bus = bus,
                Engine = engine,
                Sampler = sampler,
                CapsuleInfo = capsule,
                Site = site,
                Vehicle = vehicle,
                Antenna = antenna,
                Station = station,
                RadiationClass = rad
            };

            // ---- target physics --------------------------------------------------
            double radius = target.DiameterM * 0.5;
            double volume = (4.0 / 3.0) * Math.PI * radius * radius * radius;
            double mass = volume * target.DensityKgM3;
            double gm = AsteroidConstants.GravitationalConstant * mass;
            double surfaceGravity = gm / (radius * radius);
            double escape = Math.Sqrt(2.0 * gm / radius);
            double au = target.MeanAu;
            double distanceKm = au * AsteroidConstants.AuKm;
            double orbitRadius = AsteroidConstants.Clamp(d.OrbitRadiusM, radius * 1.2, radius * 12.0);
            double orbitPeriod = 2.0 * Math.PI * Math.Sqrt(orbitRadius * orbitRadius * orbitRadius / gm);
            double circularSpeed = Math.Sqrt(gm / orbitRadius);

            report.Target = new AsteroidTargetReport
            {
                Name = target.Name,
                DiameterM = target.DiameterM,
                RadiusM = radius,
                MassKg = mass,
                GmM3S2 = gm,
                SurfaceGravityMps2 = surfaceGravity,
                EscapeVelocityMps = escape,
                RotationHours = target.RotationHours,
                OrbitYears = target.OrbitYears,
                MeanAu = au,
                CloseApproachKm = target.CloseApproachKm,
                BoulderM = target.BoulderM,
                OrbitRadiusM = orbitRadius,
                OrbitPeriodHours = orbitPeriod / 3600.0,
                CircularSpeedMps = circularSpeed,
                LightTimeMinutes = AsteroidConstants.LightTimeMinutes(distanceKm),
                ScienceMultiplier = target.ScienceMultiplier,
                SampleRichness = target.SampleRichness
            };

            // ---- trajectory -------------------------------------------------------
            bool ega = d.TrajectoryType == "ega";
            double c3 = (ega ? AsteroidConstants.EgaC3 : AsteroidConstants.DirectC3) + target.C3Bonus;
            double arrival = (ega ? AsteroidConstants.EgaArrivalYears : AsteroidConstants.DirectArrivalYears)
                             + (target.Id == "metal" ? 0.3 : (target.Id == "fast" ? -0.1 : 0.0));
            double campaign = AsteroidConstants.Clamp(d.CampaignYears, 0.25, 3.0);
            double rendezvous = (ega ? AsteroidConstants.EgaRendezvousKmS : AsteroidConstants.DirectRendezvousKmS)
                                + target.DvBonusKmS;
            double proxOps = AsteroidConstants.ProxOpsKmSPerYear * campaign;
            double tagDv = AsteroidConstants.TagManeuverKmS * Math.Max(1, Math.Min(3, d.Attempts));
            double dvBase = rendezvous + proxOps + tagDv + AsteroidConstants.DepartureKmS
                            + AsteroidConstants.ReturnTrimKmS;
            double dv = dvBase * AsteroidConstants.DeltaVMargin;
            double missionYears = arrival + campaign + AsteroidConstants.ReturnCruiseYears;
            double capacity = AsteroidCatalog.CapacityAtC3(vehicle, c3);
            double limit = capacity * AsteroidConstants.LaunchCapacityMargin;

            report.Trajectory = new AsteroidTrajectoryReport
            {
                TypeName = ega ? "EARTH GRAVITY ASSIST" : "DIRECT",
                Ega = ega,
                C3Km2S2 = c3,
                ArrivalYears = arrival,
                CruiseDays = arrival * 365.25,
                RendezvousKmS = rendezvous,
                ProxOpsKmS = proxOps,
                DepartureKmS = AsteroidConstants.DepartureKmS,
                TagKmS = tagDv,
                TotalKmS = dv,
                MissionYears = missionYears,
                CapacityKg = capacity,
                LimitKg = limit,
                ReturnWindow = "EARTH RETURN +" + missionYears.ToString("0.0") + " YEARS"
            };

            // ---- mass chain --------------------------------------------------------
            double radMass = rad.MassMultiplier;
            double busKg = bus.MassKg * radMass;
            double instKg = d.InstrumentMassKg * radMass;
            double arrayKg = array.MassKg;
            double batteryKg = battery.MassKg;
            double antennaKg = antenna.MassKg;
            double rfKg = rf.MassKg;

            double baseDry = busKg + engine.MassKg + arrayKg + batteryKg + antennaKg + rfKg
                             + sampler.MassKg + capsule.MassKg + instKg;
            double structure = baseDry * AsteroidConstants.StructureFraction;
            double dryNoTank = (baseDry + structure) * AsteroidConstants.MassMargin;

            double e = Math.Exp(dv * 1000.0 / (engine.Isp * 9.80665)) - 1.0;
            double denom = 1.0 - AsteroidConstants.TankFraction * e;
            if (denom < 0.02) denom = 0.02;
            double propellant = dryNoTank * e / denom;
            double tank = AsteroidConstants.TankFraction * propellant;
            double dry = dryNoTank + tank;
            double wet = dry + propellant;

            report.Mass = new AsteroidMassReport
            {
                BusKg = busKg,
                PropulsionKg = engine.MassKg,
                ArrayKg = arrayKg,
                BatteryKg = batteryKg,
                AntennaKg = antennaKg,
                RfKg = rfKg,
                SamplerKg = sampler.MassKg,
                CapsuleKg = capsule.MassKg,
                InstrumentKg = instKg,
                BaseDryKg = baseDry,
                StructureKg = structure,
                DryNoTankKg = dryNoTank,
                TankKg = tank,
                DryKg = dry,
                PropellantKg = propellant,
                WetKg = wet,
                CapacityKg = capacity,
                LimitKg = limit,
                UsageFraction = capacity > 0 ? wet / capacity : 99.0,
                WetDryRatio = dry > 0.001 ? wet / dry : 99.0,
                Pass = capacity > 0 && wet <= limit
            };

            // ---- power -------------------------------------------------------------
            double years = missionYears;
            double arrayW = array.AreaM2 * AsteroidConstants.ArrayEfficiency * AsteroidConstants.SolarConstantWm2
                            / (au * au) * AsteroidConstants.ArrayCosFactor
                            * Math.Pow(AsteroidConstants.ArrayDegradationPerYear, years);
            double load = AsteroidConstants.HousekeepingW + AsteroidConstants.ThermalW
                          + rf.Watts / AsteroidConstants.CommsEfficiency
                          + d.InstrumentPowerW + AsteroidConstants.SamplerStandbyW
                          + (engine.Electric ? engine.PowerW : 0.0);
            double peak = load + 100.0;
            double batteryNeed = peak * AsteroidConstants.PeakLoadHours / AsteroidConstants.BatteryDod;
            double batteryUsable = battery.CapacityWh * AsteroidConstants.BatteryDod;
            bool batteryPass = batteryUsable + 0.001 >= batteryNeed;

            report.Power = new AsteroidPowerReport
            {
                ArrayW = arrayW,
                LoadW = load,
                PeakLoadW = peak,
                Margin = load > 0.001 ? (arrayW - load) / load : 9.99,
                BatteryNeedWh = batteryNeed,
                BatteryUsableWh = batteryUsable,
                BatteryPass = batteryPass,
                Pass = arrayW >= load && batteryPass
            };

            // ---- SRP stability (needs wet mass) ------------------------------------
            double spacecraftArea = array.AreaM2 + 5.0;
            double srpAccel = AsteroidConstants.SrpPressureAt1Au / (au * au)
                              * AsteroidConstants.SrpReflectivity * spacecraftArea / Math.Max(1.0, wet);
            double gravityAccel = gm / (orbitRadius * orbitRadius);
            double srpRatio = gravityAccel > 0.0 ? srpAccel / gravityAccel : 99.0;
            double stability = 1.0 - srpRatio / AsteroidConstants.StabilityLimit;
            if (stability < 0.0) stability = 0.0;
            if (stability > 1.0) stability = 1.0;
            bool orbitPass = orbitRadius >= radius * 1.2
                             && orbitRadius <= radius * 12.0
                             && srpRatio <= AsteroidConstants.StabilityLimit;

            report.Target.SrpAccelMps2 = srpAccel;
            report.Target.GravityAccelMps2 = gravityAccel;
            report.Target.SrpRatio = srpRatio;
            report.Target.Stability = stability;
            report.Target.OrbitPass = orbitPass;

            // ---- communications ----------------------------------------------------
            double lambda = 299792458.0 / (antenna.FrequencyMHz * 1e6);
            double txGain = antenna.ExplicitGainDbi > 0
                ? antenna.ExplicitGainDbi
                : 10.0 * Math.Log10(0.55 * Math.Pow(Math.PI * antenna.DiameterM / lambda, 2.0));
            double eirp = 10.0 * Math.Log10(rf.Watts) + txGain - 1.5;
            double groundGain = 10.0 * Math.Log10(0.55 * Math.Pow(Math.PI * station.DiameterM / lambda, 2.0));
            double groundGt = groundGain - 10.0 * Math.Log10(station.SystemTempK);
            double fspl = 20.0 * Math.Log10(Math.Max(1.0, distanceKm))
                          + 20.0 * Math.Log10(antenna.FrequencyMHz) + 32.44;
            double cn0 = eirp + groundGt - fspl - 2.5 + 228.6;
            double maxRate = Math.Pow(10.0, (cn0 - 6.0) / 10.0);
            if (maxRate > antenna.BandCapBps) maxRate = antenna.BandCapBps;
            double margin = cn0 - 10.0 * Math.Log10(Math.Max(1.0, maxRate)) - 6.0;

            double contact = AsteroidConstants.ContactHoursPerDay;
            double downlink = maxRate * contact * 3600.0 * AsteroidConstants.DownlinkOverhead / 8.0 / 1e9;
            double science = d.InstrumentDataGbPerDay / 2.0;
            double dataFraction = science > 0.0001 ? downlink / science : 1.0;
            if (dataFraction > 1.0) dataFraction = 1.0;

            report.Comms = new AsteroidCommsReport
            {
                TxGainDbi = txGain,
                EirpDbw = eirp,
                FsplDb = fspl,
                GroundGOverT = groundGt,
                MaxRateBps = maxRate,
                UsedRateBps = maxRate,
                MarginDb = margin,
                ContactHoursPerDay = contact,
                DownlinkGbPerDay = downlink,
                ScienceGbPerDay = science,
                Fraction = dataFraction,
                LightTimeMinutes = AsteroidConstants.LightTimeMinutes(distanceKm),
                Pass = dataFraction >= 0.5
            };

            // ---- TAG ---------------------------------------------------------------
            double contactFactor = Math.Exp(-0.5 * Math.Pow(
                (d.ContactSpeedCmS - AsteroidConstants.TagOptimumSpeedCmS) / AsteroidConstants.TagSpeedSigmaCmS, 2.0));
            double navBonus = d.Navigation ? 0.10 : 0.0;
            double targetTagPenalty = target.Id == "fast" ? 0.15 : (target.Id == "metal" ? 0.10 : 0.0);
            double failure = sampler.FailureBase
                             + site.Hazard * 0.50
                             + targetTagPenalty
                             + (1.0 - contactFactor) * 0.20
                             - navBonus;
            failure = AsteroidConstants.Clamp(failure, 0.02, 0.95);
            int attempts = Math.Max(1, Math.Min(AsteroidConstants.MaxTagAttempts, d.Attempts));
            double logNormalMean = Math.Exp(sampler.Sigma * sampler.Sigma * 0.5);
            double perAttempt = sampler.MedianGrams * target.SampleRichness * site.SampleFactor
                                * contactFactor * (d.Navigation ? 1.05 : 1.0) * logNormalMean;
            double expected = perAttempt * (1.0 - failure) * attempts;

            report.Tag = new AsteroidTagReport
            {
                SamplerName = sampler.Name,
                SiteName = site.Name,
                Navigation = d.Navigation,
                ContactSpeedCmS = d.ContactSpeedCmS,
                Attempts = attempts,
                SuccessProbability = 1.0 - failure,
                PerAttemptGrams = perAttempt,
                ExpectedGrams = expected,
                MedianGrams = sampler.MedianGrams,
                FailureChance = failure,
                ContactFactor = contactFactor,
                Pass = expected >= AsteroidConstants.SampleRequirementG
            };

            // ---- capsule -----------------------------------------------------------
            double capsuleMargin = (capsule.SpeedLimitKmS - AsteroidConstants.CapsuleEntrySpeedKmS)
                                   / AsteroidConstants.CapsuleEntrySpeedKmS;
            double capsuleSurvival = capsule.BaseSurvival * (capsuleMargin >= 0.0 ? 1.0 : 0.35);
            capsuleSurvival = AsteroidConstants.Clamp(capsuleSurvival, 0.02, 0.995);

            report.Capsule = new AsteroidCapsuleReport
            {
                Name = capsule.Name,
                MassKg = capsule.MassKg,
                CostM = capsule.CostM,
                EntrySpeedKmS = AsteroidConstants.CapsuleEntrySpeedKmS,
                SpeedLimitKmS = capsule.SpeedLimitKmS,
                Margin = capsuleMargin,
                SurvivalProbability = capsuleSurvival,
                Pass = capsuleSurvival >= 0.5
            };

            // ---- cost --------------------------------------------------------------
            double parts = bus.CostM * rad.CostMultiplier
                           + engine.CostM
                           + array.CostM
                           + battery.CostM
                           + antenna.CostMillions
                           + rf.CostMillions
                           + sampler.CostM
                           + capsule.CostM;
            double instruments = d.InstrumentCostM * rad.CostMultiplier;
            double development = parts + instruments;
            double contingency = development * AsteroidConstants.Contingency;
            double months = missionYears * 12.0;
            double ground = station.CostPerMonthMillions * months;
            double ops = AsteroidConstants.OpsCostMPerMonth * months;
            double total = development + contingency + vehicle.CostM + ground + ops;

            report.Cost = new AsteroidCostReport
            {
                PartsM = parts,
                InstrumentsM = instruments,
                DevelopmentM = development,
                ContingencyM = contingency,
                LaunchM = vehicle.CostM,
                GroundM = ground,
                OpsM = ops,
                TotalM = total,
                CapM = def.BudgetCapM,
                Pass = total <= def.BudgetCapM
            };

            report.Reliability = new AsteroidReliabilityReport
            {
                LaunchReliability = vehicle.Reliability,
                EgaSuccess = ega ? AsteroidConstants.EgaSuccess : 1.0,
                ArrivalSuccess = AsteroidConstants.ArrivalSuccess,
                TagSuccess = 1.0 - failure,
                CapsuleSuccess = capsuleSurvival
            };

            // ---- levels and points -------------------------------------------------
            float sciencePotential = d.InstrumentPoints * (float)target.ScienceMultiplier;
            float scienceEarned = sciencePotential * (float)dataFraction;
            float samplePoints = SamplePoints(expected);
            int grams = (int)Math.Floor(expected + 0.5);

            bool level1 = report.Mass.Pass && capacity > 0.0 && report.Target.OrbitPass;
            bool level2 = sciencePotential > 0.01f && dataFraction >= 0.5;
            bool level3 = grams >= AsteroidConstants.SampleRequirementG;
            bool level4 = report.Capsule.Pass;
            bool budget = report.Cost.Pass;

            float achievable = (level1 ? AsteroidConstants.Level1Points : 0f)
                               + scienceEarned + samplePoints
                               + (level4 ? AsteroidConstants.Level4Points : 0f)
                               + (budget ? AsteroidConstants.BudgetPoints : 0f);

            report.Level1 = level1;
            report.Level2 = level2;
            report.Level3 = level3;
            report.Level4 = level4;
            report.BudgetPass = budget;
            report.SciencePotential = sciencePotential;
            report.ScienceEarned = scienceEarned;
            report.SamplePoints = samplePoints;
            report.AchievablePoints = achievable;
            report.ExpectedGrams = grams;

            if (level1 && level4 && level3 && budget) report.DeterministicRating = 2;
            else if (level1 && ((level4 && grams > 0) || dataFraction >= 0.5)) report.DeterministicRating = 1;
            else report.DeterministicRating = 0;

            return report;
        }

        public static float SamplePoints(double grams)
        {
            if (grams < 20.0) return 0f;
            if (grams < 60.0) return (float)(10.0 + (grams - 20.0) / 40.0 * 15.0);
            if (grams < 120.0) return 30f;
            return 40f;
        }

        static int ArrayCatalogIndex(double area)
        {
            for (int i = 0; i < AsteroidCatalog.Arrays.Length; i++)
                if (Math.Abs(AsteroidCatalog.Arrays[i].AreaM2 - area) < 0.01) return i;
            return 2;
        }

        static int BatteryCatalogIndex(double wh)
        {
            for (int i = 0; i < AsteroidCatalog.Batteries.Length; i++)
                if (Math.Abs(AsteroidCatalog.Batteries[i].CapacityWh - wh) < 0.01) return i;
            return 1;
        }
    }
}
