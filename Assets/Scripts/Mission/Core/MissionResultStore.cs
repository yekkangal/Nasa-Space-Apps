// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using System;

namespace MissionArchitect
{
    /// <summary>
    /// Cross-UI shared state: the design plus the last computed report, Monte Carlo and
    /// launch result. Static so the terminal and the handbook read exactly the same data.
    /// </summary>
    public static class MissionResultStore
    {
        static MissionDesign _design;
        static EngineeringReport _report;
        static MonteCarloResult _monteCarlo;
        static MissionTimeline _timeline;
        static int _designVersion = -1;
        static int _monteCarloSeed = 20260101;
        static int _monteCarloVersion = -1;
        static int _launchCount;

        public static MissionDesign Design
        {
            get
            {
                if (_design == null) _design = MissionDesign.CreateDefault();
                return _design;
            }
            set
            {
                _design = value ?? MissionDesign.CreateDefault();
                _designVersion = -1;
            }
        }

        public static MonteCarloResult MonteCarloResult
        {
            get
            {
                if (_monteCarloVersion != Design.Version || _monteCarlo.Runs == 0)
                    RunMonteCarlo(1000);
                return _monteCarlo;
            }
        }

        public static MissionTimeline LastTimeline => _timeline;
        public static int MonteCarloSeed => _monteCarloSeed;

        /// <summary>Recomputes only when the design version changed.</summary>
        public static EngineeringReport Report
        {
            get
            {
                if (_design == null) _design = MissionDesign.CreateDefault();
                if (_report == null || _designVersion != _design.Version)
                {
                    _report = EngineeringModel.Evaluate(_design);
                    _designVersion = _design.Version;
                }
                return _report;
            }
        }

        public static void Invalidate()
        {
            _designVersion = -1;
            _report = null;
        }

        public static ObjectiveStatus[] Objectives =>
            MissionObjectives.Evaluate(Report, MissionDefinition.Get(MissionSelection.Key));

        public static MonteCarloResult RunMonteCarlo(int runs)
        {
            _monteCarlo = MissionSimulator.MonteCarlo(Design, runs, (uint)_monteCarloSeed);
            _monteCarloVersion = Design.Version;
            return _monteCarlo;
        }

        public static MissionTimeline Launch()
        {
            // every launch gets a different draw so the timeline is not repeated verbatim
            _launchCount++;
            int seed = _monteCarloSeed + _launchCount * 7919;
            _timeline = MissionSimulator.RunTimeline(Design, unchecked((uint)seed));
            return _timeline;
        }

        public static void ResetAll()
        {
            _design = MissionDesign.CreateDefault();
            _designVersion = -1;
            _report = null;
            _monteCarlo = default(MonteCarloResult);
            _timeline = null;
        }

        public static void SetMission(string key)
        {
            MissionSelection.Key = string.IsNullOrEmpty(key) ? "Ay" : key;
        }
    }

    /// <summary>Which mission the player selected at the NPC (mirrors GameState.SelectedMission).</summary>
    public static class MissionSelection
    {
        public static string Key = "Ay";
    }
}
