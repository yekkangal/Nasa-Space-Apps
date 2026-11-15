// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
using TMPro;
using UnityEngine;

namespace MissionArchitect
{
    /// <summary>
    /// Scene entry point for the Mission Lab. Creates the terminal UI and the handbook UI in
    /// code, ensures the Input System EventSystem exists, and binds them to the lab terminal.
    /// Sprite and font fields are assigned by Assets/Editor/MissionLabSetup.cs.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class MissionLabBootstrap : MonoBehaviour
    {
        public Sprite panelSprite;
        public Sprite buttonSprite;
        public TMP_FontAsset font;
        public MissionTerminal terminal;

        void Start()
        {
            MissionUIFactory.EnsureEventSystem();

            if (!string.IsNullOrEmpty(GameState.SelectedMission))
                MissionSelection.Key = GameState.SelectedMission;

            if (terminal == null) terminal = FindFirstObjectByType<MissionTerminal>();

            if (MissionSelection.Key == AsteroidDefinition.Key)
            {
                var asteroid = AsteroidTerminalUI.Create(transform, font, panelSprite, buttonSprite);
                AsteroidHandbookUI.Create(transform, font, panelSprite, buttonSprite);
                if (terminal != null) terminal.Bind(asteroid);
                MissionLog.Clear();
                MissionLog.Add("ASTEROID LAB ONLINE // WALK TO THE TERMINAL AND PRESS E");
                return;
            }

            if (MissionSelection.Key == MarsDefinition.Key)
            {
                var mars = MarsTerminalUI.Create(transform, font, panelSprite, buttonSprite);
                MarsHandbookUI.Create(transform, font, panelSprite, buttonSprite);
                if (terminal != null) terminal.Bind(mars);
                MissionLog.Clear();
                MissionLog.Add("MARS LAB ONLINE // WALK TO THE TERMINAL AND PRESS E");
                return;
            }

            var ui = MissionTerminalUI.Create(transform, font, panelSprite, buttonSprite);
            MissionHandbookUI.Create(transform, font, panelSprite, buttonSprite);
            if (terminal != null) terminal.Bind(ui);

            MissionLog.Clear();
            MissionLog.Add("MISSION LAB ONLINE // WALK TO THE TERMINAL AND PRESS E");
        }
    }
}
