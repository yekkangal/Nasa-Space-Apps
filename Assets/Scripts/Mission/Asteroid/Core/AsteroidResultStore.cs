// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>Shared asteroid state for the terminal and handbook (same pattern as Moon/Mars).</summary>
    public static class AsteroidResultStore
    {
        static AsteroidDesign _design;
        static AsteroidReport _report;
        static int _designVersion = -1;

        static AsteroidMonteCarloResult _monteCarlo;
        static int _monteCarloVersion = -1;
        static int _monteCarloSeed = 20260301;

        static AsteroidTimeline _timeline;
        static int _launchCount;

        public static AsteroidDesign Design
        {
            get
            {
                if (_design == null) _design = AsteroidDesign.CreateDefault();
                return _design;
            }
            set
            {
                _design = value ?? AsteroidDesign.CreateDefault();
                _designVersion = -1;
            }
        }

        public static AsteroidReport Report
        {
            get
            {
                if (_design == null) _design = AsteroidDesign.CreateDefault();
                if (_report == null || _designVersion != _design.Version)
                {
                    _report = AsteroidEngineeringModel.Evaluate(_design);
                    _designVersion = _design.Version;
                }
                return _report;
            }
        }

        public static AsteroidObjectiveStatus[] Objectives
        {
            get
            {
                var report = Report;
                return AsteroidObjectives.Evaluate(report, report.Definition);
            }
        }

        public static AsteroidMonteCarloResult MonteCarloResult
        {
            get
            {
                if (_monteCarloVersion != Design.Version || _monteCarlo.Runs == 0)
                    RunMonteCarlo(1000);
                return _monteCarlo;
            }
        }

        public static AsteroidTimeline LastTimeline => _timeline;

        public static void Invalidate()
        {
            _designVersion = -1;
            _report = null;
            _monteCarloVersion = -1;
        }

        public static AsteroidMonteCarloResult RunMonteCarlo(int runs)
        {
            _monteCarlo = AsteroidSimulator.MonteCarlo(Design, runs, (uint)_monteCarloSeed);
            _monteCarloVersion = Design.Version;
            return _monteCarlo;
        }

        public static AsteroidTimeline Launch()
        {
            _launchCount++;
            int seed = _monteCarloSeed + _launchCount * 7919;
            _timeline = AsteroidSimulator.RunTimeline(Design, unchecked((uint)seed));
            return _timeline;
        }

        public static void ResetAll()
        {
            _design = AsteroidDesign.CreateDefault();
            _designVersion = -1;
            _report = null;
            _monteCarloVersion = -1;
            _monteCarlo = default(AsteroidMonteCarloResult);
            _timeline = null;
        }
    }
}
