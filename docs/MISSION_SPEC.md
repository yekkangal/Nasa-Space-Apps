# MISSION ARCHITECT - Phase 1 spec: Moon mission lab + terminal

## 0. How to work (read first)
- Project: Unity 6 (6000.2.5f1), 2D URP, NEW Input System only (use `Keyboard.current`, `Mouse.current`; never `UnityEngine.Input`). cwd = Unity project root.
- Be efficient: write many files per step, do not re-read files you just wrote, no web research, keep chat replies to a few lines.
- FIRST create `docs/PROGRESS.md` with the phases of section 9 as checkboxes. Update it after every phase. If it already exists, continue from the first unchecked item.
- Work autonomously. Do not ask the user questions. At the very end print a short list titled "COULD NOT AUTOMATE" (only things that truly need a human).
- You cannot run Unity. After writing code, read the Unity log (`%LOCALAPPDATA%\Unity\Editor\Editor.log`, tail ~300 lines) and fix any `error CS` lines. If the log shows no fresh compile output, say "click the Unity window so it recompiles" and stop.
- Code rules: namespace `MissionArchitect`; no third-party packages (TextMeshPro and UGUI are installed); runtime scripts must not reference `UnityEditor`; use `FindFirstObjectByType` (not `FindObjectOfType`); `Rigidbody2D.linearVelocity`; every Texture2D/Sprite you create in code uses `FilterMode.Point`.
- Only modify existing files listed in section 2. Never delete the user's assets.

## 1. Existing project (inspect folders first, do not assume names)
`Assets/Scripts`: `IInteractable` (string Prompt; void Interact()), `GameState` (static string SelectedMission = "Ay"|"Mars"|"Asteroit"; static bool InputLocked), `PlayerController`, `PlayerInteractor` (shows prompt inside the trigger of an IInteractable, E calls Interact), `PlayerAnimator`, `Door` (sceneName, calls `SceneFader.LoadScene`), `MissionNPC`, `UIPopup`, `IdleBob`, `CameraFollow`, `SceneFader` (static LoadScene(string), black fade, auto-created).
`Assets/Scenes`: outside scene (may be named SampleScene or Outside) and `Interior` (lab room image, player, canvas with prompt box, scientist NPC with MissionNPC, exit Door).
Sprites: `Assets/Sprites/Environment` (science_lab_interior.png, scientist.png), `Assets/Sprites/UI` (ui_panel.png, ui_button.png, 9-slice border 4, PPU 16), `Assets/Sprites/Astronaut`. Style: pixel art.

## 2. Flow and allowed edits to existing files
- Edit `MissionNPC.SelectMission`: keep current behavior, then after ~1.2 s call `SceneFader.LoadScene("MissionLab")`.
- New scene `Assets/Scenes/MissionLab.unity` (section 7). The player walks to a TERMINAL object; E opens a full-screen terminal UI; Esc or an [X] button closes it. While open: `GameState.InputLocked = true`.
- Only the Moon mission ("Ay") is fully playable. "Mars" and "Asteroit" must have data-driven `MissionDefinition` entries (placeholder values) and the terminal shows "DATA LINK OFFLINE - mission data arrives in Phase 2" for them.
- UI language: English (international competition). Keep all strings in one static class so translation is easy.

## 3. Mission: LUNAR POLAR ORBITER (Moon)
Brief: put a science orbiter in a 100 km polar lunar orbit, map the surface and detect water ice at the poles, return the data to Earth, within budget and launch-vehicle limits. Mission life after arrival: 12 months.
Facts to show in the terminal: Earth-Moon distance 384,400 km; one-way light time 1.28 s; chemical transfer 3-5 days; low orbit period ~118 min; lunar night ~14.75 Earth days (relevant to future lander phases).
Objectives (id, requirement, points):
- O1 Reach 100 km polar orbit (launch + TLI + LOI all succeed) - mandatory - 30 pts
- O2 Science return: each instrument gives its points x fraction of its data actually downlinked (cap 1)
- O3 Water-ice detection requires the neutron spectrometer installed and returning data (included in its points)
- O4 Budget <= 350 $M total - 10 pts
- O5 Power margin >= 15% in every mode - 10 pts
- O6 Mass <= 95% of launch vehicle capacity to its target orbit - mandatory
Rating: FULL >= 95 pts and O1,O4,O6 met; PARTIAL >= 50 pts and O1 met; otherwise FAIL. Tune thresholds so a balanced design (Condor-5 or Condor-9, bipropellant, 3-4 instruments, 1.0 m X-band dish, 50 W, 4-6 m2 array) reaches 70-90% success, while "max everything" fails mass or budget.

## 4. Engineering model (pure C#, deterministic functions + Monte Carlo)
Constants: g0 = 9.80665 m/s2; solar constant 1361 W/m2; Boltzmann -228.6 dBW/K/Hz; c = 299,792.458 km/s; mu_moon = 4902.8 km3/s2; R_moon = 1737.4 km.
1. Rocket equation: dv = Isp*g0*ln(m0/mf); propellant = m_dry*(exp(dv/(Isp*g0)) - 1); tank mass = 8% of propellant (counted in dry mass).
2. Delta-v budget from TLI (chemical): midcourse 0.05 + LOI to 100 km 0.90 + orbit trim 0.05 + stationkeeping 0.15 km/s per year + desaturation reserve 0.05, then +10% margin (~1.32 km/s).
3. Electric option (Hall thruster, SEP): launches to LEO capacity, needs dv ~7.5 km/s spiral; thrust F = 2*eta*P/(Isp*g0), eta 0.6; spiral time = dv / (F/m_avg) in days; during the spiral add radiation dose (belt crossing, ~8 krad per month) and ops cost; needs the array power.
4. Mass: dry = sum(subsystems + instruments) + structure (18% of that sum) ; add 15% mass margin; wet = dry + propellant. Compare wet with the launch vehicle capacity to TLI (chemical) or LEO (electric), 5% margin.
5. Power: array output P = area * 0.28 * 1361 * cos factor (0.90) * (0.98^years degradation). Eclipse fraction 0.30 average, 0.40 worst case (orbit period T = 2*pi*sqrt(a^3/mu), a = R_moon + 100). Battery: 150 Wh/kg, usable DoD 0.6; must carry the eclipse load. Loads by mode: bus 60 W + thermal 30 W + comms DC (RF/0.35) + active instruments. Modes: cruise, science, science+downlink, safe. Margin = (generation - load)/load.
6. Comms link: tx antenna gain G = 10log10(0.55*(pi*D/lambda)^2); EIRP = 10log10(P_rf) + G - 1.5 dB; FSPL = 20log10(d_km) + 20log10(f_MHz) + 32.44; Eb/N0 = EIRP + G/T - FSPL - 2.5 dB (misc/pointing) + 228.6 - 10log10(R). Required Eb/N0 3 dB + 3 dB margin. Max data rate R = value where the margin is exactly 3 dB, capped by band allocation (S 2 Mbps, X 25 Mbps, Ka 150 Mbps). Ground station G/T computed from dish diameter, frequency, system temperature (University 10 m, Tsys 100 K, 6 h/day Moon visibility, 0.3 $M/month; Deep Space station 34 m, Tsys 30 K, 10 h/day, 2.0 $M/month). Contact time per day = visibility hours * 0.5 (Moon occultation in polar orbit). Downlink volume/day = R * contact * 0.9 overhead. Science data/day = sum of instrument rates / 2 (2:1 compression).
7. Radiation: dose rate ~3 krad/yr in lunar orbit plus belt dose during electric spiral. Part classes: COTS 10 krad (cheap), Mil 30 krad, Rad-hard 100 krad (expensive, +mass). If cumulative dose exceeds tolerance, monthly failure probability jumps.
8. Reliability: launch vehicle reliability; bus reliability 0.98/yr, redundancy option raises it to 0.995 for +8% mass and +10% cost. Monthly random events: solar particle event 2% (damages low-hardness parts), reaction wheel failure 1% (redundancy mitigates), safe mode 3% (loses 1 week of data).
9. Cost = parts + instruments + launch vehicle + ground (cost/month x 12) + ops team 0.8 $M/month x (12 + spiral months) + 20% contingency on development cost.
10. Monte Carlo: 1000 runs of the mission timeline (launch roll -> TLI -> LOI roll, success 0.97 only if propellant covers dv, else 0 -> 12 monthly steps with events, dose, power, data) -> P(FULL), P(PARTIAL), P(FAIL), expected points. Success rate shown = P(FULL) + P(PARTIAL). Must run in well under 100 ms (plain arithmetic, no allocations in the loop).

## 5. Catalog (starter values, you may add more but keep consistent; data in static C# classes, no ScriptableObjects, nothing created by hand)
Launch vehicles (LEO kg / TLI kg / cost $M / reliability): Kestrel-1 300/0/8/0.93 (cannot reach the Moon); Condor-5 5500/1600/55/0.95; Condor-9 17000/5500/95/0.97; Titan-H 50000/18000/240/0.96.
Propulsion (Isp s / mass kg / power W / cost $M): Monoprop 225/12/20/6; Biprop 315/28/30/14; Electric Hall 1800/35/1500/22.
Solar array area 2/4/6/8 m2 (5 kg/m2, 1.5 $M/m2). Battery sized in Wh (0.1 $M/kg).
Antennas: LGA S-band (6 dBi, 1 kg, 1 $M); X-band dish 0.5 / 1.0 / 1.5 m (3 / 6 / 11 kg, 3 / 6 / 10 $M); Ka dish 1.0 m (8 kg, 12 $M). RF power 10/25/50/100 W (3/5/8/12 kg).
Bus avionics 40 kg (8 $M). Radiation class and redundancy options as above.
Instruments (mass kg / power W / raw GB/day / cost $M / points): Narrow-angle camera 20/30/30/25/25; Multispectral imager 12/15/12/18/15; Laser altimeter 14/25/1/20/15; Neutron spectrometer 8/10/0.1/10/25; Radar sounder 35/60/8/45/20; Magnetometer 3/4/0.02/4/8.

## 6. Terminal UI (built entirely in code at runtime, no Inspector wiring)
- Canvas ScreenSpaceOverlay, CanvasScaler 1920x1080, EventSystem with `InputSystemUIInputModule` created in code if missing (never StandaloneInputModule).
- Look: retro CRT terminal, dark background (#0B1410), green/amber text, 2 px borders, panels use the `ui_panel` sprite (sliced) if the bootstrap has it assigned, otherwise plain colored Images. Use the project's pixel TMP font asset if one exists in `Assets/Fonts`, else TMP default. Use `UIPopup`-style stepped open animation (pixel-art feel) when the terminal opens.
- Do NOT use ScrollRect. Design pages to fit 1920x1080, use pagination buttons if a list is long.
- Always visible top status bar: MISSION | BUDGET used/350 $M | MASS used/capacity | POWER margin | DELTA-V margin | LINK margin | SUCCESS RATE %. Green/amber/red by value.
- Left navigation tabs: 1 BRIEFING, 2 SPACECRAFT, 3 LAUNCH, 4 ANALYSIS, 5 LAUNCH CONTROL. Keys 1-5 switch tabs. Bottom: event/log console (last ~8 lines).
- BRIEFING: objectives table (id, objective, metric, requirement, current value, status, points) plus the mission facts (distance, light time, delta-v budget).
- SPACECRAFT: choose propulsion, array area, battery Wh, antenna, RF power, ground station, radiation class, redundancy, toggle instruments. Every change recomputes everything instantly.
- LAUNCH: pick a vehicle; show capacity vs wet mass bar, cost, reliability, launch-window note.
- ANALYSIS: tables for mass, power by mode, propellant/delta-v, link budget, data budget, radiation, cost. Each row PASS / WARN / FAIL (colored) plus a one-line "why this matters" explanation (this is the educational part).
- LAUNCH CONTROL: readiness checklist, Monte Carlo result (P(FULL), P(PARTIAL), P(FAIL)), LAUNCH button (allowed even with warnings). On launch: time-accelerated timeline printed like a terminal (phases, month progress bar, random events), then a DEBRIEF: rating, points per objective, auto-generated tips pointing to the weakest constraint, buttons Redesign / Close terminal.
- P2 (only after everything else works): a command line at the bottom (`help`, `status`, `tab <n>`, `set <part>`, `analyze`, `launch`) that mirrors the clicks.

## 7. Scene setup automation (Editor)
Create `Assets/Editor/MissionLabSetup.cs` that, idempotently:
1. If `Assets/Scenes/MissionLab.unity` does not exist, `AssetDatabase.CopyAsset` the Interior scene to it (find the right source by scanning `Assets/Scenes`).
2. Opens it, removes the NPC object that has `MissionNPC`, sets the exit `Door.sceneName` to the Interior scene name, adds a TERMINAL GameObject near the top wall (about (0, 2.4)) with `MissionTerminal` (IInteractable, Prompt "E - Open Terminal", trigger BoxCollider2D, builds its own pixel-art monitor sprite in `Awake` if the SpriteRenderer has none), and a `MissionLabBootstrap` GameObject (assigns sprite fields loaded with `AssetDatabase.LoadAssetAtPath`, creates the terminal UI and EventSystem at runtime).
3. Saves the scene and appends it to `EditorBuildSettings.scenes` if missing.
4. Runs once automatically (`[InitializeOnLoadMethod]` + `EditorApplication.delayCall`, never during play mode or compilation) and is also available as menu `Tools/Mission Architect/Rebuild Mission Lab scene`.

## 8. Extras you should add without being asked
Tooltips/hint line on hover, color legend, a "design tips" panel, sensible defaults so the first open already shows a flyable (but not optimal) design, results stored in a static `MissionResultStore`, `Esc` and [X] closing, no null-reference crashes if a sprite or font is missing.

## 9. Phases (tick them off in docs/PROGRESS.md)
- [ ] P0 inspect folders, create PROGRESS.md
- [ ] P1 core model: MissionDefinition, catalog, design state, engineering calculations, simulator + Monte Carlo (Assets/Scripts/Mission/Core)
- [ ] P2 terminal UI (Assets/Scripts/Mission/UI)
- [ ] P3 scene wiring: MissionTerminal, MissionLabBootstrap, MissionNPC edit, Editor setup script
- [ ] P4 launch timeline + debrief
- [ ] P5 extras, balance tuning, compile-error pass from Editor.log
Done = zero compile errors, MissionLab scene exists and is in build settings, selecting the Moon at the NPC leads to the lab, the terminal opens and closes, all five tabs work, launching shows a debrief.
