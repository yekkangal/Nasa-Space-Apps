// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>Shared Mars state for the terminal and the handbook (same pattern as the Moon store).</summary>
    public static class MarsResultStore
    {
        static MarsDesign _design;
        static MarsReport _report;
        static int _designVersion = -1;

        static MarsMonteCarloResult _monteCarlo;
        static int _monteCarloVersion = -1;
        static int _monteCarloSeed = 20260214;

        static MarsTimeline _timeline;
        static int _launchCount;

        public static MarsDesign Design
        {
            get
            {
                if (_design == null) _design = MarsDesign.CreateDefault();
                return _design;
            }
            set
            {
                _design = value ?? MarsDesign.CreateDefault();
                _designVersion = -1;
            }
        }

        public static MarsReport Report
        {
            get
            {
                if (_design == null) _design = MarsDesign.CreateDefault();
                if (_report == null || _designVersion != _design.Version)
                {
                    _report = MarsEngineeringModel.Evaluate(_design);
                    _designVersion = _design.Version;
                }
                return _report;
            }
        }

        public static MarsObjectiveStatus[] Objectives
        {
            get
            {
                var report = Report;
                return MarsObjectives.Evaluate(report, report.Definition);
            }
        }

        public static MarsMonteCarloResult MonteCarloResult
        {
            get
            {
                if (_monteCarloVersion != Design.Version || _monteCarlo.Runs == 0)
                    RunMonteCarlo(1000);
                return _monteCarlo;
            }
        }

        public static MarsTimeline LastTimeline => _timeline;

        public static void Invalidate()
        {
            _designVersion = -1;
            _report = null;
            _monteCarloVersion = -1;
        }

        public static MarsMonteCarloResult RunMonteCarlo(int runs)
        {
            _monteCarlo = MarsSimulator.MonteCarlo(Design, runs, (uint)_monteCarloSeed);
            _monteCarloVersion = Design.Version;
            return _monteCarlo;
        }

        public static MarsTimeline Launch()
        {
            _launchCount++;
            int seed = _monteCarloSeed + _launchCount * 7919;
            _timeline = MarsSimulator.RunTimeline(Design, unchecked((uint)seed));
            return _timeline;
        }

        public static void ResetAll()
        {
            _design = MarsDesign.CreateDefault();
            _designVersion = -1;
            _report = null;
            _monteCarloVersion = -1;
            _monteCarlo = default(MarsMonteCarloResult);
            _timeline = null;
        }
    }
}
