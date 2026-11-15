// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    /// <summary>Common surface for the Moon and Mars terminal shells so the lab terminal can bind either.</summary>
    public interface IMissionTerminalUI
    {
        bool IsOpen { get; }
        void Open();
        void Close();
        void Toggle();
    }
}
