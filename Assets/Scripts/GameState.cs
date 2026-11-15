// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
public static class GameState
{
    public static string SelectedMission = "";
    public static bool InputLocked = false;

    /// <summary>
    /// Internal mission ids stay "Ay", "Mars" and "Asteroit" (baked into the scene button
    /// arguments). English ids from code or from a future UI are mapped back to them here.
    /// </summary>
    public static string NormalizeMission(string mission)
    {
        if (string.IsNullOrEmpty(mission)) return "Ay";

        switch (mission.Trim().ToLowerInvariant())
        {
            case "mars":
                return "Mars";
            case "asteroid":
            case "asteroit":
                return "Asteroit";
            case "moon":
            case "ay":
                return "Ay";
            default:
                return mission;
        }
    }

    /// <summary>Player-visible mission name for an internal id.</summary>
    public static string DisplayName(string mission)
    {
        switch (NormalizeMission(mission))
        {
            case "Mars":
                return "Mars";
            case "Asteroit":
                return "Asteroid";
            default:
                return "Moon";
        }
    }
}
