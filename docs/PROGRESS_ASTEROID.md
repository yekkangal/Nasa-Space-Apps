# Mission Architect - Asteroid Phase 3 Progress

Spec: [MISSION_SPEC_ASTEROID.md](MISSION_SPEC_ASTEROID.md) (builds on [MISSION_SPEC.md](MISSION_SPEC.md) and [MISSION_SPEC_MARS.md](MISSION_SPEC_MARS.md))
Moon and Mars progress live in PROGRESS.md and PROGRESS_MARS.md - do not edit them.

## Phases
- [x] A0 inspect existing dispatch, Moon and Mars classes, create PROGRESS_ASTEROID.md
- [x] A1 asteroid core: definition, targets, catalog, engineering model, TAG model, simulator + Monte Carlo (Assets/Scripts/Mission/Asteroid/Core)
- [x] A2 terminal pages 1-5 (Assets/Scripts/Mission/Asteroid/UI), same shell, animations and status bar
- [x] A3 dispatch: "Asteroit" opens the asteroid terminal; "Ay" and "Mars" unchanged
- [x] A4 handbook spreads + Sources spread; REFERENCES.md updated; src/game/verify tags in code
- [x] A5 balance tuning, compile pass from Editor.log, final report with "COULD NOT AUTOMATE"
- [ ] A6 (P2) orbit visualizer for page 4, extras

## A0 notes
Inspected:
- `MissionLabBootstrap` already dispatches by mission key: Mars -> `MarsTerminalUI` + `MarsHandbookUI`, otherwise Moon. The lab terminal binds through `IMissionTerminalUI`.
- `MissionTerminal` has `[NonSerialized] IMissionTerminalUI terminalUI`, `Bind`, and a Moon/Mars singleton fallback. Adding an asteroid shell only needs a third singleton fallback.
- Moon and Mars each have isolated Core/UI folders; shared helpers are `MissionUIFactory`, `UIAnimations`, `MissionUIState`, `MissionLog`, `MissionSources`, `MissionStrings`.
- `MarsTerminalUI` is partial: shell + Pages/Surface/Review; `MarsHandbookUI` is a 4-spread book. These are the templates for the asteroid UI (A2/A4).

Dispatch plan for A3 (smallest change): add an `Asteroit` branch in `MissionLabBootstrap` before the Mars branch, creating `AsteroidTerminalUI` + `AsteroidHandbookUI`; add `AsteroidTerminalUI.Instance` to the `MissionTerminal.Interact` fallback. Moon/Mars paths untouched.

Core plan for A1: `Assets/Scripts/Mission/Asteroid/Core` with `AsteroidConstants`, `AsteroidCatalog` (targets, buses, propulsion, arrays, batteries, X-band antennas/RF, samplers, capsules, sites, instruments, launch vehicles), `AsteroidDefinition` (S1-S5), `AsteroidDesign`, `AsteroidReport`, `AsteroidEngineeringModel` (target physics, trajectory, mass, power, comms, TAG, capsule, cost), `AsteroidObjectives`, `AsteroidSimulator` + Monte Carlo, `AsteroidResultStore`. NASA values tagged `// src: [N]`, assumptions `// game`, uncertain values `// verify`, no Moon/Mars edits.

## A1 notes (asteroid core)
Files in `Assets/Scripts/Mission/Asteroid/Core`:
- `AsteroidConstants.cs` - Bennu 490 m / 4.3 h / 168 Mkm / 22 m boulders / 121.6 g returned / 60 g requirement / dry 880 kg / wet 2,110 kg / capsule 46 kg / 3.35 m arm / Atlas V 411 / ~800 M$ + 183.5 M$ (all tagged) plus game values for C3, delta-v, TAG, power and cost.
- `AsteroidCatalog.cs` - 3 targets (Bennu-like, fast rotator, metal-rich), 3 buses, 3 propulsion options, arrays 4-10 m2, batteries, X-band antenna ids reused from the Moon catalog, 3 samplers (TAGSAM/scoop/sticky), 3 capsule classes, 3 sites, 5 instruments (OCAMS/OLA/OVIRS/OTES/REXIS) and 4 launchers with capacity at C3 10/20/30 + interpolation.
- `AsteroidDefinition.cs` - S1-S5 objectives, brief, 8 tips, 10-term glossary.
- `AsteroidDesign.cs` - target, bus, propulsion, array/battery, X-band/RF, sampler, capsule, direct/EGA, vehicle, campaign years, orbit radius, site, contact speed, navigation, attempts.
- `AsteroidReport.cs` + `AsteroidEngineeringModel.cs` - target mass/GM/gravity/escape/orbit period/circular speed/SRP stability, transfer C3/capacity/delta-v, mass chain, 1/r2 power, X-band link + data fraction, TAG expected grams/probability, capsule re-entry survival, cost, S1-S5.
- `AsteroidObjectives.cs` - single live evaluator, rating and auto-tips.
- `AsteroidSimulator.cs` - timeline + allocation-free 1000-run Monte Carlo with launch/EGA/arrival, long-campaign failures, safe modes, TAG attempts (log-normal mass), re-entry and levels.
- `AsteroidResultStore.cs` - shared design/report/MC/timeline state for A2/A3.

Validation with a standalone Roslyn harness:
- Bennu-like target: mass 7.33e10 kg, GM 4.9 m3/s2, surface escape 0.200 m/s, 1.75 km orbit period 57.8 h, circular speed 0.053 m/s, SRP/gravity 2.1% (stable).
- Default OSIRIS-REx-like design: EGA C3 26, delta-v 2.17 km/s, wet 1,716 kg / 1,824 kg limit, array 2,019 W vs 461 W load, 54% data returned, TAG 93% / 124 g expected, capsule 96%, cost 1,018 / 1,050 $M, 127 pts. Monte Carlo in 0.78 ms: FULL 60.2%, PARTIAL 10.6%, FAIL 29.2% -> **70.8% success** (target 60-80).
- Direct transfer, metal target and rad-hard designs fail mass or budget; light capsule drops full success; max-everything fails budget.
- Compile: Unity imported all 9 core files and rebuilt `Assembly-CSharp.dll` (18:09) with **0 errors, 0 warnings**.

## A2 notes (asteroid terminal pages)
Files in `Assets/Scripts/Mission/Asteroid/UI`:
- `AsteroidTerminalUI.cs` - shell: 1920x1080 overlay canvas, in-code EventSystem, CRT status bar (budget, wet mass/capacity, delta-v, expected grams, capsule, success), 5 tabs with keys 1-5, event console, Esc/[X], shared `UIAnimations` open/close.
- `AsteroidTerminalUI.Pages.cs` - page 1 TARGET (three target choices, live diameter/rotation/orbit/mass/GM/gravity/escape, distance/close approach/light time, science/richness/hazard/C3/delta-v trade); page 2 SPACECRAFT (bus, hydrazine/biprop/electric, array/battery, X-band antenna/RF, TAGSAM/scoop/sticky, capsule classes, five instruments, mass/power/data summary); page 3 TRAJECTORY (direct vs EGA, vehicle capacity at C3, delta-v breakdown, mass/capacity bar, cruise/survey/return timeline).
- `AsteroidTerminalUI.Ops.cs` - page 4 PROX-OPS (orbit-radius slider with live period/circular/escape/SRP/stability bar, campaign slider, site choice, contact-speed slider, optical navigation, 1-3 attempts, expected grams, mapping data vs downlink); page 5 REVIEW (readiness, Monte Carlo, LAUNCH + animated timeline/overlay, debrief with S1-S5 levels, grams, tips, Redesign/Close).
- `AsteroidStrings.cs` - English labels.
- Reuses `MarsUIWidgets` for runtime sliders, `MissionUIFactory`, `UIAnimations`, `MissionUIState`, `MissionLog`; no Moon or Mars files changed.
- Compile: Unity imported all 4 UI files and rebuilt `Assembly-CSharp.dll` (18:19) with **0 errors, 0 warnings**. The shell is not instantiated yet - A3 adds the dispatch branch and terminal binding.

## A3 notes (dispatch)
- `MissionLabBootstrap` gained one branch before Mars: `MissionSelection.Key == AsteroidDefinition.Key` creates `AsteroidTerminalUI` + `AsteroidHandbookUI`, binds the lab terminal, logs and returns. The existing Mars and Moon branches are byte-for-byte unchanged.
- `MissionTerminal.Interact` fallback gained a third check for `AsteroidTerminalUI.Instance`; Moon and Mars fallbacks are unchanged.
- No scene edits, no `MissionNPC` edit, no changes to Moon/Mars terminal or handbook classes.

## A4 notes (handbook + sources)
- `Assets/Scripts/Mission/Asteroid/UI/AsteroidHandbookUI.cs` - 4-spread book: live S1-S5 objectives (current values and points), key numbers and current design, 10-term glossary, 8 tips, and DATA SOURCES (asteroid refs 1-6) plus the expanded NASA credit and verification note. Same slide/cover open, page-flip and B/Esc/[X] handling.
- `Assets/Scripts/Mission/Asteroid/Core/AsteroidSources.cs` - six asteroid references (citation, URL, values supplied) using the shared `MissionSource` type.
- `docs/REFERENCES.md` - appended an "Asteroid mission sources" section (A1-A6) without removing the Moon/Mars refs 1-10; the expanded NASA/JPL-Caltech/Goddard/University of Arizona credit line is included.
- Tags: asteroid constants already carry `src` / `game` / `verify`; catalog game options were tagged `// game` and the target strings no longer leak tags into the UI.
- Compile: Unity imported the handbook, sources and dispatch changes and rebuilt `Assembly-CSharp.dll` (18:24) with **0 errors, 0 warnings**.

## A5 notes (balance + final report)
Balance changes:
- S1 now requires a stable proximity orbit (`Target.OrbitPass`) in the deterministic report, Monte Carlo and timeline. An orbit outside 1.2-12 radii or with SRP/gravity above 5% fails S1, so the page-4 slider is a real constraint.
- Long-campaign reliability uses 0.5%/month spacecraft failure plus safe modes; this brings the OSIRIS-REx-like design into the 60-80% target instead of ~92%.
- Data rates and X-band scale were tuned so the link limits mapping: default 54% data returned, x05/10 W only 6%, x10/50 W 24-27%.
- Catalog/cost tuning: Condor-9 2,400 kg at C3 20 / 1,600 kg at C3 30, larger bus cost, 1,000 Wh default battery, 0.5 h TAG peak load, COTS default radiation. Default cost is 1,018 / 1,050 $M.

Balance sweep (1,000 runs each):
- Default OSIRIS-REx-like design: wet 1,716 / 1,824 kg, TAG 124 g expected, capsule 96%, cost 1,018/1,050 $M -> **FULL 60%, PARTIAL 11%, success 71%, expected 89 g** (target 60-80).
- Safe Plain 72% success / FULL 46% (80 g); Boulder Field 69% / FULL 56%; 3 TAG attempts FULL 70% / success 72% with 1,789 kg wet.
- Direct transfer (2,123 kg), metal target (2,033 kg), rad-hard (2,066 $M) and max-everything (2,238 $M) all fail mass or budget; unstable high orbit -> 0%.
- Light capsule drops FULL to 19%; scoop/sticky samplers fall to 45/15 g; campaign 1 year costs 1,035 $M and still passes, 2 years fails mass/budget.

Compile: Unity rebuilt `Assembly-CSharp.dll` (18:29) with **0 errors, 0 warnings** after the balance changes.

## Final report
Status: A0-A5 complete. The asteroid mission now has a full core model, five terminal pages, dispatch from the NPC, a four-spread handbook with Sources, and a documented reference list. The Moon and Mars terminals and handbooks are unchanged; "Ay" and "Mars" keep their original branches.

Balance target met: the balanced OSIRIS-REx-like design succeeds 71% (FULL 60%, PARTIAL 11%), while direct/metal/rad-hard/max designs fail mass or budget and unstable orbits fail S1. `docs/REFERENCES.md` covers Moon, Mars and asteroid sources; NASA constants are tagged `src`, game values `game`, uncertain values `verify`.

## COULD NOT AUTOMATE
- Play-mode end-to-end verification of the asteroid mission: selecting "Asteroit" at the NPC, opening the terminal, every page/slider, TAG feedback, launch timeline, debrief and B-key handbook. Unity play mode cannot be driven from this environment.
- Regression play-through of the Moon and Mars missions after the shared dispatch change (static review and compile only).
- Human verification of every NASA reference link before submission.
- A6 orbit visualizer and remaining extras (P2) are not implemented.



