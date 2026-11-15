# SI Disclosure - Mission Architect

This project was built with **SI (Super Intelligence) assistance** for the NASA Space Apps Challenge. This page records where and how SI tools were used, which assets are AI-generated and which are human-made, and the NASA branding audit.

## Summary

- The runtime and editor C# code was written by large language models.
- The environment and UI pixel art was generated with AI, programmatically.
- Human-made third-party assets are limited to the astronaut sprite and the door sprite; they are credited below with TODO items for final license confirmation.
- Game design, mission concepts and engineering-data selection were directed by the team.
- NASA data comes from public NASA sources listed in [REFERENCES.md](REFERENCES.md).
- **The project contains no NASA logos, flags, mission patches or official mission identifier graphics.** NASA and mission names appear only as plain-text citations and educational references.

## SI tools used

Session records indicate the following tools/models were used. The current implementation session used **DeepSeek V4.1 Flash** through OpenCode. Any model that cannot be independently confirmed from the session records is marked `TODO confirm` rather than claimed.

| Tool | Use | Confirmation |
|---|---|---|
| Claude by Anthropic | Assistant chatbot: planning, code review, writing support | TODO confirm exact sessions |
| Gemini 4 Hydrogen | Coding-agent model through OpenCode | TODO confirm |
| DeepSeek V4.1 Flash | Coding-agent model through OpenCode; core, UI, documentation and this disclosure | Confirmed in the current session |
| GLM 5.3 | Coding-agent model through OpenCode | TODO confirm |
| MiMo 2.6 Pro | Coding-agent model through OpenCode | TODO confirm |

No web research was used for the NASA values; the numbers come from the public sources in [REFERENCES.md](REFERENCES.md).

## How SI was used

- **Code:** LLM coding agents wrote the C# scripts under `Assets/Scripts` and `Assets/Editor`, including the Moon, Mars and Asteroid engineering models, the 1000-run Monte Carlo, the runtime terminal UI, the handbooks, the scene bootstrap and the scene setup editor script.
- **Documentation:** SI assistants helped write the design notes, progress files, references table and this disclosure. Human team direction set the mission concepts, gameplay priorities, scope and balance targets.
- **Pixel art:** the environment and UI sprites were generated programmatically by SI image/code tools. The two environment images visibly carry an `SI-GENERATED` watermark.
- **No SI-generated NASA imagery:** no NASA image, logo, patch or flag was generated or embedded.

## AI-generated pixel art

These files under `Assets/Sprites` were generated with AI:

- `Environment/science_lab_interior.png`
- `Environment/science_building.png`
- `Environment/scientist.png`
- `Environment/grass.png`
- `UI/ui_panel.png`
- `UI/ui_button.png`

`science_lab_interior.png` and `science_building.png` include a visible `SI-GENERATED` watermark. The astronaut and door sprites are **not** on this list.

## Human-made / third-party assets (NOT SI)

- **Astronaut sprite:** from itch.io, "Free Pixel Art Character Asset - ASTRONAUT" by **saraspiegelberg**, <https://saraspiegelberg.itch.io/free-pixel-art-character-asset-astronaut>. Credit: "Astronaut sprite by saraspiegelberg (itch.io)." License: **CHECK/TODO confirm** and add the exact license line before submission.
- **Pixel font:** Pixelify Sans, local files `Assets/Fonts/PixelifySans-Regular.ttf` and `.asset`. Author, source URL and license: **TODO/CHECK** (likely OFL; confirm).
- **Door sprite:** source **TODO confirm**. The image used by the outdoor door appears to be `Assets/Sprites/Ekran_görüntüsü_2026-11-14_162258-removebg-preview.png`; confirm the original author and license. This is a generic pixel-art door, not a NASA asset.
- Full asset source and licensing status: [SOURCES_AND_CREDITS.md](SOURCES_AND_CREDITS.md).
- **Background music:** `Assets/Resources/Music/bgm` is team-supplied and human-made unless the team says otherwise. Title, artist, license and URL: **TODO** in `Assets/Scripts/Audio/MusicCredit.cs`.
- These third-party sprites are used as game art only and are not SI-generated.

## Game design and NASA data

Mission concepts, mission selection, engineering-data choices and balance decisions were directed by the team. The engineering model uses public NASA data from the sources in [REFERENCES.md](REFERENCES.md). NASA text is used as a citation, not as branding.

## Code metadata

- Every SI-written C# file under `Assets/Scripts` and `Assets/Editor` starts with:
  `// SI-assisted: written with AI tools (LLM-generated code). See docs/SI_DISCLOSURE.md.`
- `Assets/Scripts/SIAssemblyInfo.cs` adds `AssemblyMetadata("SI-assisted", "true")` and an assembly description.
- The in-game disclosure is created at startup and persists across scenes: a bottom-left label plus an F1 panel.
- All three B-key handbooks end with an SI DISCLOSURE spread.

## Polish-pass files (AI-written)

Added in the final polish pass:

- `Assets/Scripts/Audio/MusicPlayer.cs` - persistent looping background music, fade-in, ducking and mute.
- `Assets/Scripts/Audio/MusicCredit.cs` - title / artist / license / URL constants shown in the F1 panel.
- `Assets/Editor/MusicImportSettings.cs` - streaming Vorbis import settings for `Assets/Resources/Music`.
- `Assets/Scripts/Mission/UI/UITextScale.cs` - central readable text scale and the F2 size setting.
- `Assets/Scripts/UI/ControlsHint.cs` - contextual key-cap hint HUD (H to hide).
- `Assets/Editor/PolishSetup.cs` - idempotent scene polish: English dialogue and buttons, larger dialogue text, Interior exit door, Mission Lab door check.
- `Assets/Resources/Fonts/PixelifySans-Regular.asset` - runtime-loadable copy of the Pixelify Sans font asset, so UI created before a scene loads still uses the game font.

Scene text and component values in `Assets/Scenes/Interior.unity` and `Assets/Scenes/MissionLab.unity` (English labels, font sizes, exit door on the Interior scene) were changed by `PolishSetup.cs`, not by hand.

## Branding check

Scope: all `.cs`, `.unity`, `.asset`, `.md` and `.txt` files under `Assets` and `docs`, image filenames, the scene UI strings, and a visual review of the image assets.

Findings:

- **No NASA logos, meatball or worm marks, flags, mission patches or official mission identifier graphics** were found in any image, scene or UI.
- `NASA` appears only as plain text in citations, source references and comments (for example in `docs/REFERENCES.md` and the in-game Sources pages).
- Mission names such as OSIRIS-REx and Perseverance appear only as plain-text educational references in source lists and explanatory text; no mission logos, patches or identifier graphics are used.
- The environment images carry an `SI-GENERATED` watermark; that is an AI-origin label, not a NASA mark.
- The astronaut sprite is a generic white/blue pixel-art suit with no visible NASA logo; the door sprite is a generic wooden door.
- The only asset names containing "Logo" are TextMesh Pro example materials under `Assets/TextMesh Pro/Examples & Extras`; these are generic TMP samples, not NASA logos.
- No NASA flag, insignia, mission patch or official identifier appears in the game UI or assets.

**Conclusion: the project contains no NASA logos, flags, mission patches or official mission identifier graphics.** NASA and mission names are used only as text citations and educational references. If a reviewer treats a plain-text mission name as an identifier, replace that text before submission; no such graphics exist to remove.

## Verification TODO

- Confirm every NASA link in [REFERENCES.md](REFERENCES.md) and the asteroid section.
- Confirm the astronaut sprite license/credit wording with saraspiegelberg.
- Confirm the door sprite author/source and license.
- Confirm which SI models were used in each session and remove the TODO markers.
