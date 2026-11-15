# Mission Architect - Progress

Spec: [MISSION_SPEC.md](MISSION_SPEC.md)

## Phases
- [x] P0 inspect folders, create PROGRESS.md
- [x] P1 core model: MissionDefinition, catalog, design state, engineering calculations, simulator + Monte Carlo (Assets/Scripts/Mission/Core)
- [x] P2 terminal UI (Assets/Scripts/Mission/UI) including its open/close animation
- [x] P2b handbook UI (B key): live objectives, glossary and tips pages, open and page-flip animations, shared UIAnimations helper (Assets/Scripts/Mission/UI)
- [x] P3 scene wiring: MissionTerminal, MissionLabBootstrap, MissionNPC edit, Editor setup script
- [x] P4 launch timeline + debrief
- [x] P5 extras, balance tuning, compile-error pass from Editor.log

## P0 notes
- Scenes: `Assets/Scenes/Interior.unity`, `Assets/Scenes/Outdoor.unity` (no SampleScene).
- Sprites: `Environment/science_lab_interior.png`, `Environment/scientist.png`, `UI/ui_panel.png`, `UI/ui_button.png`, `Astronaut/Astronaut.png`.
- Pixel TMP font: `Assets/Fonts/PixelifySans-Regular.asset` (+ .ttf).
- Existing scripts are global-namespace in `Assets/Scripts` (`MissionNPC`, `GameState`, `SceneFader`, `Door`, `UIPopup`, `PlayerInteractor`).

## P1 notes (core model)
Files in `Assets/Scripts/Mission/Core`:
- `MissionConstants.cs` - g0, solar, Boltzmann, c, mu/R Moon, margins, 100 km orbit period.
- `MissionCatalog.cs` - launchers, propulsion, arrays, batteries, antennas, RF, ground stations, radiation classes, 6 instruments.
- `MissionDefinition.cs` - data-driven Moon ("Ay") plus Mars/Asteroit placeholders (offline notice); objectives O1-O6.
- `MissionDesign.cs` - editable design state (ids + instrument flags + Version counter).
- `EngineeringModel.cs` - mass (structure 18%, tank 8%, margin 15%), rocket equation, delta-v budget (1.32 km/s chemical / 8.25 km/s electric), power by 4 modes + eclipse battery, link budget, data budget, radiation, cost, reliability.
- `EngineeringReport.cs` - typed results consumed by both UIs.
- `MissionObjectives.cs` - the single live objective evaluator (BRIEFING + handbook), rating and auto-tips.
- `MissionSimulator.cs` - one-run timeline and 1000-run Monte Carlo (allocation-free xorshift loop).
- `MissionResultStore.cs` / `MissionLog.cs` / `MissionStrings.cs` - shared state, 8-line console buffer, English UI strings.

Validation (standalone Roslyn build of Core, C# 9): default design = Condor-5 / biprop / 4 m2 / 800 Wh / X 1.0 m / 50 W / University / Mil / camera+multispectral+neutron:
- wet 334 kg (cap 1600 TLI), cost 245.9 $M, link 25 Mbps / +11.6 dB, data 100%, dose 3/30 krad.
- 1000-run MC in ~0.5 ms: FULL 78.7%, PARTIAL 0%, FAIL 21.3% (target 70-90% success).
- S-band variant: 0% FULL / 78.7% PARTIAL (data-link education works).
- Max-everything: 563 $M -> budget fail; electric 8 m2: 58 krad (rad-hard needed), cost fail.
- Kestrel-1: O1 NOT MET (no TLI capability).

Compile check: Unity log shows a fresh Assembly-CSharp build with zero `error CS` lines. Mission content text is data in `MissionDefinition`; UI labels live in `MissionStrings` (move content there if full localization is needed later).

## P2 / P2b notes (UI)
Files in `Assets/Scripts/Mission/UI`:
- `UIAnimations.cs` - hidden runner + coroutines: CRT on/off, handbook slide/cover open + close, page flip, typewriter. Unscaled time, stepped frames, exact reset at both ends, re-entrancy safe (caller stops the previous routine).
- `MissionUIFactory.cs` - runtime factory: 1920x1080 overlay canvas, InputSystemUIInputModule event system (legacy module removed), 2 px bordered panels using the sliced `ui_panel` sprite when supplied, buttons, bars, pixel-font fallback, palette (#0B1410 background, green/amber, brown/paper book tones).
- `MissionTerminalUI.cs` - full-screen terminal: status bar (mission, budget, mass, power, delta-v, link, success rate), left tabs 1-5 with key shortcuts, 8-line event console, [X] and Esc close, CRT open/close with typed boot text.
  - BRIEFING: live O1-O6 table from `MissionObjectives` plus mission facts.
  - SPACECRAFT: propulsion / array / battery / RF / antenna / ground / radiation / redundancy / instrument toggles; every click recomputes report + Monte Carlo and refreshes the status bar.
  - LAUNCH: vehicle list with capacity vs wet-mass bar, reliability, cost, window note.
  - ANALYSIS: seven PASS/WARN/FAIL tables (mass, power by mode, delta-v, link, data, radiation, cost) each with a "why this matters" line.
  - LAUNCH CONTROL: readiness checklist, 1000-run Monte Carlo, launch button, debrief with rating, points per objective and weakest-constraint tips, Redesign / Close. Timeline is printed to the event console now; the stepped timeline animation is P4.
- `MissionHandbookUI.cs` - B-key book: 3 spreads (live objectives + key numbers, glossary, engineer's tips), 1/3 indicator, arrow keys and page buttons, page-flip animation, Esc / [X] / B close, HUD hint "[B] Handbook", B ignored while the terminal is open, `GameState.InputLocked` held while open.
- `MissionUIState.cs` - terminal/handbook open flags and input-lock sync.

Compile check: Unity rebuilt `Assembly-CSharp.dll` (14:53) with no `error CS`. Not yet instantiated in a scene - P3 adds MissionTerminal + MissionLabBootstrap wiring.

## P3 notes (scene wiring)
- `Assets/Scripts/Mission/MissionTerminal.cs` - `IInteractable` with prompt "E - Open Terminal", trigger BoxCollider2D, opens `MissionTerminalUI.Instance`, and generates a 32x28 pixel-art CRT monitor sprite at runtime (FilterMode.Point, PPU 16) when the SpriteRenderer has none.
- `Assets/Scripts/Mission/MissionLabBootstrap.cs` - scene entry point: ensures the EventSystem, syncs `MissionSelection.Key` from `GameState.SelectedMission`, creates the terminal + handbook UIs, binds the terminal, seeds the event log.
- `Assets/Editor/MissionLabSetup.cs` - idempotent builder:
  - copies `Assets/Scenes/Interior.unity` to `Assets/Scenes/MissionLab.unity` when missing (source found by scanning `Assets/Scenes`);
  - removes the `MissionNPC` object and hides the orphaned DialoguePanel / SelectedMissionText;
  - sets the exit `Door` to `Interior` (creates `ExitDoor` at (0,-5.2) in the room's bottom-wall gap because Interior had no Door);
  - creates `TERMINAL` at (0,2.4) with SpriteRenderer + trigger BoxCollider2D + `MissionTerminal`;
  - creates `MissionLabBootstrap` and assigns `ui_panel` / `ui_button` sprites and the Pixelify Sans TMP font via `AssetDatabase.LoadAssetAtPath`;
  - saves the scene and appends it to `EditorBuildSettings.scenes`;
  - runs once on load when the scene is missing and from `Tools/Mission Architect/Rebuild Mission Lab scene`.
- `Assets/Scripts/MissionNPC.cs` - `SelectMission` keeps its behavior (label, dialogue close, input unlock) and then loads `MissionLab` through `SceneFader` after 1.2 s.

Generated scene verified: `ExitDoor` (sceneName Interior), `TERMINAL` at (0,2.4), `MissionLabBootstrap` with sprite/font/terminal references, no MissionNPC component, DialoguePanel + SelectedMissionText inactive. Build settings order: Outdoor, Interior, MissionLab. Editor and runtime assemblies rebuilt with zero `error CS`.

## P4 notes (launch timeline + debrief)
- `MissionTerminalUI` now builds a full-content `LaunchOverlay` above the pages: title, phase line, 20-line event stream, month progress bar (00-12) and final status line.
- LAUNCH starts `LaunchSequenceRoutine` (unscaled time): prints launch/TLI/LOI/spiral/month events one by one, colors alarms red and nominal events green, advances the month bar, can be skipped with Space, and always resets to the exact end state.
- After the sequence the overlay closes and the LAUNCH CONTROL page shows the DEBRIEF: rating, points, orbit result, per-objective points, failure reason, weakest-constraint tips, plus Redesign / Close Terminal.
- Input is locked during the sequence (tabs/Esc ignored, overlay blocks clicks); Esc/close still works and safely ends the sequence.
- Random events come from `MissionSimulator.RunTimeline`; the same data feeds the debrief and the event console.

## P5 notes (extras, balance, polish)
- ANALYSIS hover: transparent pointer targets on every section update a bottom hint line with the full "why this matters" text (`UIPointerHint`).
- Color legend (PASS / WARN / FAIL) added to the ANALYSIS header; design-tips panel, defaults, static `MissionResultStore`, Esc/[X]/B closing and null-safe sprite/font fallbacks were already in place.
- Play-test log analysis: runtime worked (MissionLab loaded repeatedly, no exceptions), but Pixelify Sans has no `[` or `]` glyphs, so every bracketed label was replaced with plain text (`B HANDBOOK`, `ESC CLOSE / 1-5 TABS`, `ON/OFF`, `MET / NOT MET / PENDING`, `PREV / NEXT`, no `>` selection markers, launch phases without brackets).
- Balance re-checked with the standalone core harness: default design 334 kg / 245.9 $M / 100% data / 78.7% success; max-everything 563 $M (budget fail); S-band 0% FULL and 78.7% PARTIAL.
- Compile: Roslyn build of Core + UI + runtime wiring against the Unity 6 assemblies is clean (0 errors, 0 warnings). The running Unity editor was in play mode / unavailable for refresh at handoff, so the P4/P5 scripts are not yet in its Assembly-CSharp - click the Unity window (or stop play mode) to import and compile; the previous Editor.log build had zero `error CS`.
