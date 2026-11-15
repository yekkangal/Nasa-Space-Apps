// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.
// Disclosure: docs/SI_DISCLOSURE.md
namespace MissionArchitect
{
    /// <summary>Single source for the SI disclosure shown in-game and in the handbooks.</summary>
    public static class SIDisclosureText
    {
        public const string Label = "SI-assisted: AI-generated code and pixel art - press F1";

        public const string ShortText =
            "This project was built with SI assistance.\n\n" +
            "CODE: large language models generated the runtime and editor C# code. " +
            "Every SI-written file starts with an SI-assisted header and the assembly carries " +
            "an SI-assisted metadata attribute.\n\n" +
            "PIXEL ART: science_lab_interior, science_building, scientist, grass, ui_panel and " +
            "ui_button were generated with AI.\n\n" +
            "HUMAN-MADE: astronaut sprite by saraspiegelberg on itch.io - license TODO confirm; " +
            "door sprite - source TODO confirm.\n\n" +
            "NASA DATA: public NASA sources in docs/REFERENCES.md. Game design and mission concepts " +
            "were directed by the team.\n\n" +
            "No NASA logos, flags, mission patches or official mission identifier graphics are used.";

        public const string Tools =
            "Claude by Anthropic (assistant chatbot)\n" +
            "Gemini 4 Hydrogen\n" +
            "DeepSeek V4.1 Flash\n" +
            "GLM 5.3\n" +
            "MiMo 2.6 Pro\n" +
            "TODO confirm any model not used in a given session.";

        public const string Images =
            "science_lab_interior.png\nscience_building.png\nscientist.png\ngrass.png\n" +
            "ui_panel.png\nui_button.png";

        public const string HumanAssets =
            "Astronaut sprite - from itch.io, author saraspiegelberg.\n" +
            "License and credit line: TODO confirm before submission.\n\n" +
            "Door sprite - source TODO confirm.";

        public const string Branding =
            "No NASA logos, meatball or worm marks, flags, mission patches or official mission " +
            "identifier graphics are used in any image, scene or UI. NASA and mission names appear " +
            "only as plain-text citations and educational references.";

        public const string FullText =
            "SUPER INTELLIGENCE (SI) DISCLOSURE\n\n" +
            "This project was built with SI assistance. The code was written by large language models: " +
            "an assistant chatbot (Claude by Anthropic) and coding-agent models run through OpenCode " +
            "(Gemini 4 Hydrogen, DeepSeek V4.1 Flash, GLM 5.3, MiMo 2.6 Pro). TODO confirm models not " +
            "used in a given session.\n\n" +
            "CODE METADATA: all SI-written C# files under Assets/Scripts and Assets/Editor carry an " +
            "SI-assisted header. Assets/Scripts/SIAssemblyInfo.cs marks the assembly with SI-assisted=true.\n\n" +
            "AI-GENERATED PIXEL ART: science_lab_interior.png, science_building.png, scientist.png, " +
            "grass.png, ui_panel.png and ui_button.png. The two environment images carry a visible " +
            "SI-GENERATED watermark.\n\n" +
            "HUMAN-MADE THIRD-PARTY ASSETS: the astronaut sprite is from itch.io by saraspiegelberg; " +
            "its license and credit line are TODO confirm. The door sprite source is TODO confirm. " +
            "These assets are not SI-generated.\n\n" +
            "GAME DESIGN: mission concepts, mission selection and engineering-data choices were " +
            "directed by the team. NASA data comes from public NASA sources listed in docs/REFERENCES.md.\n\n" +
            "NASA BRANDING: the project contains no NASA logos, meatball or worm marks, flags, mission " +
            "patches or official mission identifier graphics. NASA and mission names are used only as " +
            "plain-text citations and educational references.\n\n" +
            "Full disclosure: docs/SI_DISCLOSURE.md";

        public static string HandbookLeft()
        {
            return "<size=18><b>SI DISCLOSURE</b></size>\n\n" + ShortText;
        }

        public static string HandbookRight()
        {
            return "<size=18><b>TOOLS AND ASSETS</b></size>\n\n" +
                   "<b>SI TOOLS</b>\n" + Tools + "\n\n" +
                   "<b>AI-GENERATED IMAGES</b>\n" + Images + "\n\n" +
                   "<b>HUMAN-MADE</b>\n" + HumanAssets + "\n\n" +
                   Branding + "\n\nFull text: docs/SI_DISCLOSURE.md";
        }
    }
}
