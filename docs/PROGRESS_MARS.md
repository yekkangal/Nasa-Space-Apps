# Mission Architect - Mars Phase 2 Progress

Spec: [MISSION_SPEC_MARS.md](MISSION_SPEC_MARS.md) (builds on [MISSION_SPEC.md](MISSION_SPEC.md))
Moon progress is tracked separately in [PROGRESS.md](PROGRESS.md) - do not edit it.

## Phases
- [x] M0 inspect existing Moon engine and UI classes, create PROGRESS_MARS.md, decide the smallest dispatch change (mission -> page set)
- [x] M1 Mars core: definition, catalog, engineering model, simulator + Monte Carlo (Assets/Scripts/Mission/Mars/Core)
- [x] M2 Mars terminal pages 1-6 and the planner/sliders (Assets/Scripts/Mission/Mars/UI) with the same animations and status bar
- [x] M3 dispatch: selecting "Mars" at the NPC opens the Mars terminal in MissionLab; "Ay" still opens the Moon terminal; "Asteroit" still shows OFFLINE
- [x] M4 Mars handbook spreads + Sources spread for both missions; docs/REFERENCES.md; src tags in code
- [x] M5 balance tuning, compile pass from Editor.log, final report with "COULD NOT AUTOMATE"

## M0 notes
Inspected:
- `Assets/Scripts/Mission/Core` - Moon constants, catalog, design, `EngineeringModel`, `EngineeringReport`, `MissionObjectives`, `MissionSimulator` (timeline + MC), `MissionResultStore`, `MissionStrings`, `MissionLog`.
- `Assets/Scripts/Mission/UI` - `MissionUIFactory`, `UIAnimations`, `MissionTerminalUI`, `MissionHandbookUI`, `MissionUIState`, `UIPointerHint`; all runtime-built, no prefabs.
- `MissionLabBootstrap` creates both Moon UIs and binds `MissionTerminal.terminalUI` (concrete `MissionTerminalUI`); `MissionTerminal.Interact` opens it.
- `MissionNPC.SelectMission` already loads `MissionLab` for every mission; `MissionSelection.Key` carries "Ay"/"Mars"/"Asteroit".

Dispatch decision (smallest change, Moon untouched):
1. Add `IMissionTerminalUI { void Open(); void Close(); void Toggle(); }`.
2. Make `MissionTerminalUI` implement it (declaration only) and change `MissionTerminal`'s field/bind parameter to the interface.
3. `MissionLabBootstrap` branches on `MissionSelection.Key`: "Mars" -> create `MarsTerminalUI` + `MarsHandbookUI` (new classes under `Mission/Mars/UI`), otherwise the existing Moon classes. No scene edits, no Inspector wiring.
4. `MarsTerminalUI` reuses `MissionUIFactory`, `UIAnimations`, `MissionUIState`, `MissionLog` so the look/animations/input lock stay shared.
5. "Asteroit" keeps the Moon UI and its OFFLINE briefing.

Core plan for M1: new namespace-consistent classes prefixed `Mars` under `Assets/Scripts/Mission/Mars/Core` (constants, catalog, definition, design, report, engineering model, objectives, simulator, result store). Moon classes are only read, never rewritten. NASA constants tagged `// src: [N]`, assumptions `// game`.

## M1 notes (Mars core)
Files in `Assets/Scripts/Mission/Mars/Core`:
- `MarsConstants.cs` - NASA constants tagged `// src: [1..9]` (gravity 3.71, escape 5.03, radius 3396.2, GM 42,828, irradiance 588, Hohmann 8.6/2.65/259 d, Perseverance 1,025 kg, MMRTG 110 W, 687 d/668 sols, 26-month window, 1/100 pressure, -81 F, 1.8/0.67 mSv/day, relay/DTE rates, 55-400 Mkm) and `// game` assumptions (sol length, dust storms, heater load, EDL factors where not NASA).
- `MarsCatalog.cs` - 4 launchers with cap10 (Kestrel 0, Condor-5 1,100, Condor-9 3,900 Atlas V 541 class, Titan-H 12,000), 3 rover buses, 2 mobility sets, MMRTG/solar/battery, thermal, 3 EDL systems, 2 heat shields, parachute/TRN, 3 sites, 7 instruments, relay/DTE hardware, arm. Moon radiation classes reused read-only.
- `MarsDefinition.cs` - L1-L4 + budget objectives, brief, 8 tips, 8-term glossary.
- `MarsDesign.cs` - launch offset, vehicle, bus, mobility, arm, MMRTG/solar, array/battery, thermal, TRN, instruments, EDL/heat shield/parachute/site, relay passes/DTE, radiation class, daily policy weights.
- `MarsReport.cs` + `MarsEngineeringModel.cs` - transfer C3/cruise/distance/v-infinity/light time, capacity(C3) with 40% floor, mass chain (landed fraction + 15% rover margin + entry hardware + 5% reserve), EDL (base x mass x site + TRN, shield/chute), surface power (MMRTG decay or solar x season x dust + battery), relay/DTE data and conjunction loss, radiation, operations/cost with 20% contingency, levels and points.
- `MarsObjectives.cs` - single live evaluator for the Review page and handbook.
- `MarsSimulator.cs` - phase timeline (window, launch, cruise, six EDL beats, 22 surface months) and allocation-free 1000-run Monte Carlo with dust storms, safe modes, failures, conjunction and level scoring.
- `MarsResultStore.cs` - shared design/report/MC/timeline state for M2/M3.

Validation with a standalone Roslyn harness (Core + Mars Core):
- Default (Perseverance-class, MMRTG, sky crane, Delta Crater, 6 instruments): rover 852 kg, launch 3,446 kg of 3,938 kg limit; EDL 87%; power +130%; data 98%; cost 2,663 $M; 12 samples; 149 pts. Monte Carlo 1000 runs in 0.45 ms: FULL 72.8%, PARTIAL 10.4%, FAIL 16.8% -> **83.2% success** (target 65-85%).
- Max everything (Titan-H, rad-hard, all 7 instruments): mass passes, cost 4,391 $M -> budget fail.
- Solar 4 m2 passes worst-case power, 2 m2 fails; off-optimal +45 d cuts capacity to 2,588 kg -> mass fail; DTE-only returns 2% data (PARTIAL only); no instruments -> FAIL; Kestrel-1 -> no Mars capability.
- Light time 8.9 min at the modelled 160 Mkm launch distance (range 3-22 min with distance).
- Compile: Unity imported all 9 files and rebuilt `Assembly-CSharp.dll` (15:40) with **0 errors, 0 warnings**.

## M2 notes (Mars terminal UI)
Files in `Assets/Scripts/Mission/Mars/UI`:
- `MarsTerminalUI.cs` - shell: 1920x1080 overlay canvas, canvas scaler, in-code EventSystem, CRT status bar (budget, launch mass/capacity, C3, EDL %, power, data, success), 6-tab nav with keys 1-6, event console, Esc/[X] close, CRT open/close through `UIAnimations`.
- `MarsTerminalUI.Pages.cs` - pages 1-3:
  - WINDOW: -60..+60 day slider, C3 / cruise / distance / v-infinity / light-time cards, window note, vehicle list with capacity recomputed at the current C3, capacity-vs-mass bar.
  - ROVER: bus, mobility, arm, thermal, MMRTG/solar switch, solar area (disabled while MMRTG), battery, 7 instruments with mass/power/data/points, mass-power summary.
  - EDL: airbags/sky crane/legs, heat shield, parachute, TRN, three landing sites, big EDL probability, landed-mass vs limit bar, factor breakdown.
- `MarsTerminalUI.Surface.cs` - page 4 planner (drive/science/drill/comms/rest sliders, normalise button, coloured mini-day bar, power budget, expected distance/data/samples, nominal/winter/dust-storm margins) and page 5 comms (relay toggle, 1-2 passes, DTE fallback, returned-data bar, volume breakdown, light time and conjunction blackout).
- `MarsTerminalUI.Review.cs` - page 6: phase chips (Launch/Cruise/EDL/Surface), readiness checklist, 1000-run Monte Carlo, tips, LAUNCH + animated overlay (window, launch, cruise, six EDL beats, 22 surface months, Space skip, progress bar), DEBRIEF with L1-L4 results, points, failure reason, tips, Redesign / Close.
- `MarsUIWidgets.cs` - runtime UGUI sliders (track, fill, handle, pointer hit area) and segmented bar.
- `MarsStrings.cs` - English Mars UI labels.

Notes: all changes are new Mars UI files plus a small `MarsSimulator.Finalize` fix so the debrief objective points use the actual run's data fraction and sample count instead of the design-time estimate. No Moon or shared class was edited. The Mars UI is not instantiated yet - M3 adds the bootstrap dispatch and terminal binding. Compile: Unity imported all 6 UI files and rebuilt `Assembly-CSharp.dll` (15:48) with **0 errors, 0 warnings**.

## M3 notes (dispatch)
- `Assets/Scripts/Mission/UI/IMissionTerminalUI.cs` - `IsOpen` + `Open/Close/Toggle`; implemented by both terminal shells.
- `MissionTerminal.terminalUI` / `Bind` now use the interface; `Interact` falls back to whichever singleton exists. The scene field was always null (bootstrap binds at runtime), so the MissionLab scene is unchanged.
- `MissionLabBootstrap.Start` branches on `MissionSelection.Key == MarsDefinition.Key`:
  - "Mars" -> `MarsTerminalUI.Create` + `MarsHandbookUI.Create`, bind the Mars terminal.
  - "Ay" / "Asteroit" / empty -> existing `MissionTerminalUI` + `MissionHandbookUI`, unchanged.
- No `MissionNPC` edit needed (it already loads MissionLab for every selection). "Asteroit" still renders the Moon briefing OFFLINE notice.

## M4 notes (handbooks + sources)
- `Assets/Scripts/Mission/Core/MissionSources.cs` - shared reference data for refs 1-10 (citation, URL, values supplied), credit line and verification note.
- `Assets/Scripts/Mission/Mars/UI/MarsHandbookUI.cs` - Mars book, 4 spreads: live L1-L4 + budget objectives, key numbers/current design, glossary, tips, DATA SOURCES (refs 1-9). Same pixel book, B/Esc/[X] handling, slide/cover open and page-flip animations.
- `MissionHandbookUI` (Moon) - updated to 4 spreads and added the DATA SOURCES spread with ref [10] plus credit; navigation/labels now /4. Moon mission content otherwise unchanged.
- `docs/REFERENCES.md` - all 10 references with the required credit line and "human must verify" note.
- src tags: Moon `EarthMoonDistanceKm`, `MoonRadiusKm`, `MuMoon` tagged `// src: [10]`; Mars constants/definition/catalog were already tagged in M1. UI source pages list reference ids without showing raw tags to the player.
- Compile: Unity imported the new interface, sources and handbook files and rebuilt `Assembly-CSharp.dll` (15:55) with **0 errors, 0 warnings**.

## M5 notes (balance + final pass)
Balance changes:
- Added a monthly solar-particle-event roll to the Mars Monte Carlo and timeline. COTS parts fail an SPE 30% of the time, Mil 8%, Rad-hard 1% (`// game`). This makes the radiation class a real choice instead of a pure cost penalty and keeps the hardened options meaningful.
- Re-ran the balance sweep with 1000 runs per design. Key results:
  - Perseverance-like default (large bus, heavy wheels, arm, MMRTG, sky crane, Delta Crater, TRN, chute, Mil, 6 instruments): launch 3,446 kg / 3,938 kg limit, P(EDL) 87%, power +130%, data 98%, cost 2,663 / 2,700 $M, 12 samples, 149 pts. Monte Carlo: FULL 71%, PARTIAL 12%, FAIL 17% -> **83% success** (target 65-85).
  - Safe Plain variant: 88% success (above the band, but it trades 15% science value for hazard); Delta is the balanced science choice.
  - COTS variant: 1,989 $M, FULL 66% (cheaper but less reliable in SPEs); Mil is the balanced pick; Rad-hard 4,009 $M -> budget fail.
  - Small airbag rover: 924 $M, 76% success; +30 d launch offset -> capacity 2,588 kg, mass fail; all-in "max everything" 4,391 $M -> budget fail.
  - Moon regression unchanged: default 78.7% success, S-band 0% FULL / 78.7% PARTIAL, max-everything over budget.
- Final compile: Unity rebuilt `Assembly-CSharp.dll` at 15:58 with **0 errors, 0 warnings**.

## Final report
Status: all M0-M5 phases complete. The Moon mission keeps its original terminal, handbook, objectives and balance (78.7% default success). Selecting Mars at the NPC now opens the Mars terminal (window, rover, EDL, surface planner, comms, review + debrief), B opens the Mars handbook with live L1-L4 levels, and Asteroit still shows the Moon OFFLINE briefing. Sources spreads and `docs/REFERENCES.md` exist for both missions, with NASA constants tagged `// src: [N]`.

Verification available without Unity: standalone Roslyn builds of the model and UI against the Unity 6 assemblies, a 1000-run balance sweep, and the Editor.log compile pass (0 errors / 0 warnings).

## COULD NOT AUTOMATE
- Play-mode end-to-end walkthrough and visual QA: NPC selection -> MissionLab -> Mars terminal pages, slider dragging, CRT/timeline animations, debrief, Mars/Moon handbook page flips, and the Moon regression. Unity play mode cannot be driven from this environment.
- Human verification of every reference link in `docs/REFERENCES.md` (required by the spec before submission).
- Final visual/audio polish on the real display and any glyph coverage issues beyond the known Pixelify bracket limitation.



