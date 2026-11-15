// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>
    /// Physical constants and game assumptions for the Mars rover mission.
    /// NASA-derived values carry "// src: [N]" where N is a docs/MISSION_SPEC_MARS.md reference.
    /// Model assumptions carry "// game".
    /// </summary>
    public static class MarsConstants
    {
        // ---- NASA data -------------------------------------------------------
        public const double SurfaceGravity = 3.71;              // src: [1] m/s2
        public const double EscapeVelocityKmS = 5.03;           // src: [1]
        public const double RadiusKm = 3396.2;                  // src: [1]
        public const double MuKm3S2 = 42828.0;                  // src: [1]
        public const double SolarIrradianceWm2 = 588.0;         // src: [1] (586-589)
        public const double EarthSemiMajorAu = 1.000;           // src: [1]
        public const double MarsSemiMajorAu = 1.524;            // src: [1]

        public const double HohmannDepartureVinfKmS = 2.94;     // src: [1] derived from orbit radii
        public const double HohmannC3 = 8.6;                    // src: [1] derived (v_inf^2)
        public const double HohmannArrivalVinfKmS = 2.65;       // src: [1] derived
        public const double HohmannToFDays = 259.0;             // src: [1] derived

        public const double PerseveranceMassKg = 1025.0;        // src: [2]
        public const double MmrtgPowerW = 110.0;                // src: [2]
        public const double PerseveranceDistanceMkm = 471.0;    // src: [2]
        public const double ReferenceBuildCostM = 2400.0;       // src: [2]
        public const double ReferenceOpsCostM = 300.0;          // src: [2]

        public const double PrimeMissionDays = 687.0;           // src: [3] 1 Mars year
        public const double PrimeMissionSols = 668.0;           // src: [3]
        public const double LaunchWindowMonths = 26.0;          // src: [3]

        public const double SurfacePressureEarthFraction = 0.01; // src: [4] under 1/100 of Earth
        public const double AverageTempF = -81.0;               // src: [9]

        public const double CruiseDoseMsvDay = 1.8;             // src: [8]
        public const double SurfaceDoseMsvDay = 0.67;           // src: [8]

        public const double RelayMaxMbps = 2.0;                 // src: [5]
        public const double RelayPassMinutes = 8.0;             // src: [5]
        public const double RelayTypicalMbps = 0.5;             // src: [7] 0.1-0.5 at max distance
        public const double DteMinBps = 160.0;                  // src: [6]
        public const double DteMaxBps = 500.0;                  // src: [6]
        public const double MinEarthMarsMkm = 55.0;             // src: [1]
        public const double MaxEarthMarsMkm = 400.0;            // src: [1]

        // ---- game assumptions -------------------------------------------------
        public const double SolHours = 24.66;                   // game (1 sol = 24h 39m)
        public const double DaylightHours = 12.0;               // game
        public const double ArraySunHours = 5.0;                // game equivalent full-sun hours
        public const double ArrayEfficiency = 0.28;             // game (same as Moon model)
        public const double BatteryWhPerKg = 150.0;             // game
        public const double BatteryDod = 0.60;                  // game
        public const double MmrtgDecayPerYear = 0.96;           // game (about 4%/year)
        public const double NightFraction = 0.55;               // game share of housekeeping/heaters at night

        public const double HousekeepingWhSol = 120.0;          // game avionics + minimal heater control
        public const double DriveSpeedMPerHour = 40.0;          // game
        public const double DriveWhPerMeter = 0.30;             // game (30 Wh / 100 m)
        public const double DrillHoursPerSample = 60.0;         // game
        public const double DrillWhPerSample = 40.0;            // game
        public const double SampleDataMb = 1000.0;              // game
        public const double SampleSuccessChance = 0.90;         // game
        public const double CommWhPerPass = 10.0;               // game

        public const double RoverMassMargin = 1.15;             // game
        public const double LaunchCapacityMargin = 0.95;        // game (5% reserve)
        public const double Contingency = 0.20;                 // game
        public const double OpsCostMPerMonth = 0.45;            // game (spec formula)
        public const double OpsMonths = 22.0;                   // game
        public const double GroundCostM = 30.0;                 // game DSN/ground
        public const double DteHoursPerSol = 6.0;               // game
        public const double DteRateBps = 300.0;                 // game (160-500 src: [6])
        public const double ConjunctionDays = 14.0;             // game (about 2 weeks)

        public const double DustStormMonthlyChance = 0.03;      // game
        public const double DustStormMinSols = 20.0;            // game
        public const double DustStormMaxSols = 60.0;            // game
        public const double DustStormMinFactor = 0.20;          // game
        public const double DustStormMaxFactor = 0.50;          // game
        public const double DustStormWorstMarginFactor = 0.50;  // game (report worst case)
        public const double SeasonalWorstFactor = 0.70;         // game
        public const double SeasonalNominalFactor = 0.85;       // game

        public const double SurfaceFailurePerMonth = 0.006;     // game
        public const double SafeModePerMonth = 0.02;            // game
        public const double SpeMonthlyChance = 0.02;            // game solar particle event
        public const double SpeCotsFailChance = 0.30;           // game
        public const double SpeMilFailChance = 0.08;            // game
        public const double SpeHardFailChance = 0.01;           // game
        public const double ConjunctionDataLoss = 0.02;         // game 14/668 sols

        public const float Level1Points = 30f;       // game
        public const float Level3MaxPoints = 30f;    // game
        public const float Level4Points = 15f;       // game
        public const float BudgetPoints = 10f;       // game
        public const int SampleTarget = 10;          // game
        public const float BaselineScienceFraction = 0.60f; // game
        public const float DefaultBudgetCapM = 2700f; // src: [2] 2.4 B$ build+launch + 0.3 B$ ops

        /// <summary>One-way light time in minutes for a distance in million km.</summary>
        public static double LightTimeMinutes(double distanceMkm)
        {
            return distanceMkm * 1e9 / 299792458.0 / 60.0;
        }

        public static double Clamp(double v, double min, double max)
        {
            return v < min ? min : (v > max ? max : v);
        }
    }
}
