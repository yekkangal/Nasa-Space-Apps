// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>
    /// Asteroid sample-return constants. NASA-derived values carry "// src: [N]",
    /// game assumptions "// game", and values needing a human check "// verify".
    /// </summary>
    public static class AsteroidConstants
    {
        // ---- physical / NASA data -------------------------------------------
        public const double GravitationalConstant = 6.67430e-11; // physics

        public const double BennuDiameterM = 490.0;            // src: [3]
        public const double BennuRotationHours = 4.3;          // src: [3]
        public const double BennuOrbitYears = 1.2;             // src: [3]
        public const double BennuMeanDistanceMkm = 168.0;      // src: [3]
        public const double BennuCloseApproachKm = 299000.0;   // src: [3]
        public const double BennuBoulderM = 22.0;              // src: [3]
        public const double ReturnedSampleG = 121.6;           // src: [3]
        public const double SampleRequirementG = 60.0;         // src: [4]
        public const double BennuDensityKgM3 = 1190.0;         // verify (bulk density 1.19 g/cm3; A3 does not list it)
        public const double TagContactSpeedCmS = 10.0;         // verify (A4 does not list contact speed)
        public const double TagBackAwaySpeedCmS = 40.0;        // verify (A4 does not list back-away speed)

        public const double ReferenceDryMassKg = 880.0;        // src: [1]
        public const double ReferenceWetMassKg = 2110.0;       // src: [1]
        public const double ReferenceCapsuleKg = 46.0;         // src: [1]
        public const double ReferenceArrayMinW = 1226.0;       // src: [1]
        public const double ReferenceArrayMaxW = 3000.0;       // src: [1]
        public const double ReferenceArmM = 3.35;              // src: [1]
        public const double ReferenceMissionCostM = 800.0;     // src: [5] // verify
        public const double ReferenceLaunchCostM = 183.5;      // src: [5] // verify
        public const double ReferenceOrbitRadiusM = 1750.0;    // src: [6]
        public const double ReferenceArrivalSpeedKmS = 19.0;   // src: [4][6] // verify
        public const double CapsuleEntrySpeedKmS = 12.4;       // verify (A4/A6 do not list entry speed)

        // ---- conversion / solar ----------------------------------------------
        public const double AuKm = 149597870.7;                 // physics
        public const double SolarConstantWm2 = 1361.0;          // physics
        public const double ArrayEfficiency = 0.28;             // game
        public const double ArrayCosFactor = 0.90;              // game
        public const double ArrayDegradationPerYear = 0.98;     // game
        public const double SrpPressureAt1Au = 4.56e-6;         // game (N/m2)
        public const double SrpReflectivity = 1.2;              // game
        public const double StabilityLimit = 0.05;              // game

        // ---- trajectory (game) -----------------------------------------------
        public const double DirectC3 = 32.0;                    // game
        public const double EgaC3 = 26.0;                       // game
        public const double DirectArrivalYears = 1.4;           // game
        public const double EgaArrivalYears = 2.2;              // game
        public const double DirectRendezvousKmS = 1.4;          // game
        public const double EgaRendezvousKmS = 0.9;             // game
        public const double ProxOpsKmSPerYear = 0.15;           // game
        public const double DepartureKmS = 0.9;                 // game
        public const double ReturnTrimKmS = 0.05;               // game
        public const double TagManeuverKmS = 0.05;              // game per attempt
        public const double DeltaVMargin = 1.10;                // game
        public const double ReturnCruiseYears = 1.0;            // game

        // ---- mass / structure -------------------------------------------------
        public const double StructureFraction = 0.18;           // game
        public const double MassMargin = 1.15;                  // game
        public const double TankFraction = 0.08;                // game
        public const double LaunchCapacityMargin = 0.95;        // game
        public const double BatteryWhPerKg = 150.0;             // game
        public const double BatteryDod = 0.60;                  // game
        public const double PeakLoadHours = 0.5;               // game (TAG peak only, no eclipse)

        // ---- power / comms ----------------------------------------------------
        public const double HousekeepingW = 60.0;               // game
        public const double ThermalW = 30.0;                    // game
        public const double SamplerStandbyW = 20.0;             // game
        public const double CommsEfficiency = 0.35;             // game
        public const double ContactHoursPerDay = 8.0;           // game
        public const double DownlinkOverhead = 0.90;            // game

        // ---- TAG ---------------------------------------------------------------
        public const double TagOptimumSpeedCmS = 10.0;          // game
        public const double TagSpeedSigmaCmS = 12.0;            // game
        public const double TagLognormalSigma = 0.45;           // game
        public const int MaxTagAttempts = 3;                    // game

        // ---- cost / programme --------------------------------------------------
        public const double BudgetCapM = 1050.0;                // src: [5] // game cap
        public const double OpsCostMPerMonth = 0.8;             // game
        public const double Contingency = 0.20;                 // game
        public const float Level1Points = 20f;                  // game
        public const float Level3MaxPoints = 40f;               // game
        public const float Level4Points = 25f;                  // game
        public const float BudgetPoints = 10f;                  // game

        // ---- Monte Carlo -------------------------------------------------------
        public const double CruiseSuccess = 0.97;               // game
        public const double EgaSuccess = 0.97;                  // game
        public const double ArrivalSuccess = 0.98;              // game
        public const double PowerFailurePerMonth = 0.01;        // game
        public const double SpacecraftFailurePerMonth = 0.005;  // game (long cruise + surface campaign)
        public const double SafeModePerMonth = 0.02;            // game

        public static double LightTimeMinutes(double distanceKm)
        {
            return distanceKm / 299792.458 / 60.0;
        }

        public static double AuToKm(double au)
        {
            return au * AuKm;
        }

        public static double Clamp(double v, double min, double max)
        {
            return v < min ? min : (v > max ? max : v);
        }
    }
}
