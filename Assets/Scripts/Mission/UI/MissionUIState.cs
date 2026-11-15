// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    /// <summary>Tracks which full-screen Mission Architect panel is open so input locks stay correct.</summary>
    public static class MissionUIState
    {
        public static bool TerminalOpen;
        public static bool HandbookOpen;

        public static bool AnyOpen => TerminalOpen || HandbookOpen;

        public static void SyncInputLock()
        {
            GameState.InputLocked = AnyOpen;
        }

        public static void Reset()
        {
            TerminalOpen = false;
            HandbookOpen = false;
            GameState.InputLocked = false;
        }
    }
}
