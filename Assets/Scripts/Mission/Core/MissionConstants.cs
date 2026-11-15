// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>Physical and programmatic constants of the mission model (spec section 4).</summary>
    public static class MissionConstants
    {
        public const double G0 = 9.80665;                // m/s2 // verify
        public const double SolarConstant = 1361.0;      // W/m2 at 1 AU // verify
        public const double BoltzmannDbw = -228.6;       // dBW/K/Hz // verify
        public const double LightSpeedKmS = 299792.458;  // km/s // verify
        public const double MuMoon = 4902.8;             // km3/s2, src: [10]
        public const double MoonRadiusKm = 1737.4;       // src: [10]
        public const double EarthMoonDistanceKm = 384400.0; // src: [10]
        public const double OrbitAltitudeKm = 100.0;     // game (science orbit)

        public const double EclipseFractionAverage = 0.30; // verify
        public const double EclipseFractionWorst = 0.40; // verify

        public const double BatteryWhPerKg = 150.0; // game
        public const double BatteryUsableDod = 0.60; // game

        public const double TankFraction = 0.08;         // of propellant // game
        public const double StructureFraction = 0.18;    // of dry subsystems // game
        public const double MassMargin = 0.15;           // 15% on dry mass // game
        public const double CapacityMassLimit = 0.95;    // wet <= 95% of capacity // game

        public const double BusPowerW = 60.0; // game
        public const double ThermalPowerW = 30.0; // game
        public const double CommsChainEfficiency = 0.35; // game

        public const double ArrayWPerM2 = 0.28 * SolarConstant; // game
        public const double ArrayCosFactor = 0.90; // game
        public const double ArrayDegradationPerYear = 0.98; // game

        public const double ChemicalMidcourseKmS = 0.05; // verify
        public const double ChemicalLoiKmS = 0.90; // verify
        public const double ChemicalTrimKmS = 0.05; // verify
        public const double ChemicalStationKeepKmSPerYear = 0.15; // verify
        public const double ChemicalDesatKmS = 0.05; // verify
        public const double ChemicalMarginFactor = 1.10; // verify
        public const double ElectricSpiralKmS = 7.5; // game

        public const double HallEfficiency = 0.60; // game
        public const double RadiationKradPerYear = 3.0; // verify
        public const double BeltDoseKradPerMonth = 8.0; // verify

        public const double OpsCostPerMonth = 0.8;       // $M // game
        public const double ContingencyFactor = 0.20; // game
        public const int MissionMonths = 12; // game

        public const double MinPowerMargin = 0.15; // game
        public const double LinkThresholdDb = 3.0; // game
        public const double LinkMiscDb = 2.5; // game
        public const double LinkRequiredEbNoDb = 6.0;    // 3 dB threshold + 3 dB margin // game

        public const float PointsFull = 95f; // game
        public const float PointsPartial = 50f; // game

        public const double TliSuccessProbability = 0.97; // game
        public const double LoiSuccessProbability = 0.97; // game
        public const double SpiralSuccessProbability = 0.97; // game

        /// <summary>Orbital period of the 100 km science orbit in seconds.</summary>
        public static double OrbitPeriodSeconds()
        {
            double a = MoonRadiusKm + OrbitAltitudeKm;
            return 2.0 * Math.PI * Math.Sqrt(a * a * a / MuMoon);
        }

        /// <summary>Orbital period in minutes (~118 min).</summary>
        public static double OrbitPeriodMinutes() => OrbitPeriodSeconds() / 60.0;

        public static double TwoWayLightTimeSeconds() => 2.0 * EarthMoonDistanceKm / LightSpeedKmS;
    }
}
