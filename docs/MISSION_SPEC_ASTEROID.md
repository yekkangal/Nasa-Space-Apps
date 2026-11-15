# MISSION ARCHITECT - Phase 3 spec: ASTEROID sample-return mission + NASA data and sources

Read `docs/MISSION_SPEC.md` first (section 0 working rules, section 6/6b UI style, section 8 extras), then `docs/MISSION_SPEC_MARS.md` (how the mission dispatch, mission-specific terminal pages, handbook Sources spread and docs/REFERENCES.md were built). Everything there still applies. This file ADDS the Asteroid mission ("Asteroit"). Moon and Mars must keep working exactly as before.

## 0. Rules for this task
- Progress file: `docs/PROGRESS_ASTEROID.md` (do not touch PROGRESS.md or PROGRESS_MARS.md). Create it first, tick phases as you go, continue from it in later sessions.
- Do not run git commands. Do not delete, rename or rewrite files you did not create in this task. Prefer adding new classes (folder `Assets/Scripts/Mission/Asteroid/`) over rewriting Moon/Mars classes; if you touch a shared class (dispatch, handbook, MissionDefinition), make the smallest change and keep Moon and Mars behavior identical. Reuse the existing dispatch pattern from the Mars phase.
- No web research. NASA numbers are in section 7. Tag every NASA-derived constant in code with `// src: [N]` (N = reference in section 8); mark game assumptions with `// game`; mark values that need a human check with `// verify`.
- You cannot run Unity: after writing code read `%LOCALAPPDATA%\Unity\Editor\Editor.log`, fix `error CS` lines. If Unity did not recompile, say "click the Unity window" and stop.
- Same code rules as docs/MISSION_SPEC.md section 0. UI text in English.
- SCOPE CONTROL (the work may be interrupted by a quota): build in this order and make each step playable before the next: 1 TARGET, 2 SPACECRAFT, 3 TRAJECTORY, 4 PROX-OPS, 5 REVIEW. The orbit visualizer and extras are P2, last.

## 1. What is different from Moon and Mars
Moon = orbiter, static design analysis. Mars = surface rover, phases, sol planner.
Asteroid = **sample return from a tiny low-gravity world** (OSIRIS-REx / Bennu inspired). Its own ideas:
1. **Target choice** with a different science case and different physics per asteroid.
2. **Earth gravity assist** vs direct trajectory (trade of launch energy, time and delta-v).
3. **Proximity operations in micro-gravity**: orbit radius selection, orbital period, escape velocity of a few tens of cm/s, survey phases.
4. **Touch-and-go (TAG) sampling**: contact speed, site hazards (boulders), number of attempts, sample mass in grams.
5. **Return capsule**: sample return capsule mass, re-entry, recovery.
Success is measured in **grams of sample returned**, not in orbit insertion or sols.

## 2. Mission: ASTEROID SAMPLE RETURN ("Asteroit")
Brief: rendezvous with a carbon-rich near-Earth asteroid, map it, pick a sampling site, collect regolith with a touch-and-go arm, return the sample capsule to Earth. Reference mission: OSIRIS-REx / Bennu.
Targets (player picks one on page 1):
- **Bennu-like (NASA data)**: ~490 m top-shaped rubble pile, 4.3 h rotation, ~1.2 year orbit, mean distance from Sun ~168 million km [3]. Boulders up to ~22 m [3]. Medium accessibility, high science.
- **Fast-rotator (game)**: ~150 m, 20 minute rotation, almost no regolith, very low gravity: hard to sample, cheap to reach, low sample probability.
- **Metal-rich body (game)**: about 25 km wide, high density, a much deeper gravity well (orbits are fast, escape velocity of several m/s), hard to reach (large delta-v), very high science value, low sample mass (little loose regolith).
Levels and points (tune):
- S1 Arrive and match orbit with the target - mandatory - 20
- S2 Global mapping + spectroscopy: each instrument gives its points x fraction of its data returned (cap 1)
- S3 Sample acquisition: collected mass in g. 60 g is the minimum success requirement [4]; the real mission returned 121.6 g [3]. Points: 0 below 20 g, partial 20-60 g, full at >= 60 g, bonus at >= 120 g - up to 40
- S4 Safe return: capsule survives re-entry and is recovered - 25
- S5 Budget cap 1,050 M$ (reference: about 800 M$ mission cost plus ~183.5 M$ for the Atlas V 411 launch [5] `// verify`). Meeting it - 10
Rating: FULL = S1 + S4 + sample >= 60 g + S5; PARTIAL = S1 and (S4 with sample > 0 or S2 >= 50%); else FAIL. Tune so a balanced OSIRIS-REx-like design succeeds 60-80% and extreme designs fail mass, budget or TAG.

## 3. Terminal pages (5 tabs, keys 1-5; same shell, status bar, log, animations as the other missions)
Status bar (always): BUDGET | WET MASS vs launch capacity | DELTA-V margin | EXPECTED SAMPLE (g) | CAPSULE margin | SUCCESS %.
1. **TARGET**: pick the asteroid; show size, rotation, gravity, escape velocity, accessibility, hazards, science value, mean distance, Earth close-approach distance, light time to Earth at typical distance.
2. **SPACECRAFT**: bus size, propulsion (hydrazine / biprop; electric optional), solar array (power falls with distance: P ~ 1/r^2), battery, X-band antenna + RF power, **sampling system** (TAGSAM-like arm 3.35 m [1] / simple scoop / sticky pad: each with sample-mass distribution and failure chance), **sample return capsule** (mass 46 kg reference [1], heat shield class), instruments (see 5).
3. **TRAJECTORY**: launch vehicle and **trajectory type**: Direct vs Earth gravity assist (EGA; OSIRIS-REx orbited the Sun for a year, then used Earth's gravity [2]). Show C3, launch capacity at that C3, delta-v budget (rendezvous, proximity ops, departure), cruise time, key dates, and the **sample return window**.
4. **PROX-OPS**: orbit radius slider around the asteroid with live orbital period, circular speed, escape velocity and solar-radiation-pressure stability meter; survey campaign length; mapping data volume vs downlink; **site selection** (safe site low sample / hazardous site high sample, boulders); **TAG panel**: contact speed (reference ~10 cm/s contact, ~40 cm/s back-away [4] `// verify`), approach navigation (optical natural-feature tracking on/off), number of attempts; the panel shows probabilities and expected grams.
5. **REVIEW**: readiness checklist, Monte Carlo (1000 runs), LAUNCH button, animated timeline (launch -> cruise -> EGA -> arrival -> survey -> TAG countdown -> departure -> re-entry), DEBRIEF with level results, sample grams and tips. Buttons Redesign / Close.
Handbook (B key) for the asteroid: spread 1 objectives (live S1-S5), spread 2 glossary (rendezvous, proximity operations, TAG, TAGSAM, gravity assist, escape velocity, regolith, rubble pile, solar radiation pressure, re-entry), spread 3 tips, spread 4 DATA SOURCES. Dispatch: selecting "Asteroit" at the NPC opens this terminal; "Ay" and "Mars" unchanged.

## 4. Engineering model (pure C#)
1. **Target physics**: mass from diameter and density (Bennu-like: diameter ~490 m [3], bulk density ~1.19 g/cm3 `// verify`), GM = G*M (about 4.9 m3/s2), escape velocity v_esc = sqrt(2*GM/r) (about 0.2 m/s at the surface), orbital period T = 2*pi*sqrt(a^3/GM) (about 2.4 days at 1.75 km, the radius where OSIRIS-REx entered orbit [6]). Show these live on page 4.
2. **Solar radiation pressure** acts on small bodies: stability meter = f(orbit radius, spacecraft area/mass, distance from Sun). Orbits that are too large or too small are unstable (game).
3. **Power**: array output scales with 1/r^2 from the Sun. NASA reference: two panels give 1,226 W at the far end up to ~3,000 W near Earth [1]. Calibrate the model to those two numbers.
4. **Mass**: reference spacecraft dry mass 880 kg, wet mass 2,110 kg, sample return capsule 46 kg [1]. Dry mass = subsystems + instruments + sampler + capsule + structure + 15% margin. Wet = dry + propellant (rocket equation, Isp 225 s hydrazine / 315 s biprop). With Isp 225 s the NASA wet/dry masses imply about 1.9-2.0 km/s total delta-v; use that as a calibration check `// derived`.
5. **Trajectory**: Direct: C3 ~32 km2/s2, arrival in ~1.4 years, rendezvous delta-v ~1.4 km/s. EGA: C3 ~26 km2/s2, arrival in ~2.2 years, rendezvous delta-v ~0.9 km/s (all `// game`, qualitatively from [2]). Prox-ops 0.15 km/s per year, departure 0.9 km/s, return trim 0.05 km/s, +10% margin. Launch capacity table (kg at C3 10 / 20 / 30, linear interpolation, `// game`): Kestrel-1 0/0/0; Condor-5 900/500/250; Condor-9 3,400/2,300/1,500 (Atlas V 411 class [1]); Titan-H 11,000/8,000/5,500. Costs and reliability as in docs/MISSION_SPEC.md section 5.
6. **Comms**: X-band link budget as in the Moon model but with the Sun-Earth-asteroid distance (0.05 to 2 AU, light time up to ~17 min, computed from distance / c). Downlink volume limits the mapping campaign.
7. **TAG model**: contact outcome by sampler type x site hazard x navigation x contact speed. Sample mass is log-normal (median 100 g for the TAGSAM-like arm on a regolith-rich site, much lower for the others `// game`); failure chances: boulder collision, surface too hard, bounce. Number of attempts multiplies chances (each uses propellant). Reference outcome: the real TAG on 20 October 2020 [4].
8. **Return**: return window and delta-v, capsule re-entry (very high speed, heat shield class decides survival), parachute, recovery. Probability of capsule survival from heat shield class and design margin.
9. **Cost**: parts + instruments + launch + ground + ops (monthly cost x campaign length) + 20% contingency; reference ~800 M$ + ~183.5 M$ launch [5].
10. **Monte Carlo** (1000 runs, under 100 ms): launch -> cruise (EGA success) -> rendezvous -> survey (power, data) -> TAG attempts -> departure -> re-entry -> sample grams and levels S1-S5 -> P(FULL), P(PARTIAL), P(FAIL), expected grams.

## 5. Catalog (starter, tune)
Instruments inspired by the NASA payload (names only; masses, power, data are game-scaled, tag `// game`): OCAMS (camera suite), OLA (laser altimeter), OVIRS (visible/IR spectrometer), OTES (thermal emission spectrometer), REXIS (X-ray imaging spectrometer). Sampler options: TAGSAM-like arm (3.35 m [1]), simple scoop, sticky pad. Capsule heat shield classes (light / standard / heavy). Reuse bus, power, antenna and RF catalog items from the Moon catalog where possible.

## 6. Retrofit: Sources
Add a DATA SOURCES spread to the asteroid handbook and append the asteroid references to `docs/REFERENCES.md` (do not remove the Moon and Mars references).

## 7. NASA data table (use these, do not invent others)
| Value | Source |
|---|---|
| Spacecraft dry mass 880 kg; wet mass 2,110 kg; sample return capsule 46 kg; solar power 1,226 W to ~3,000 W; sampling arm 3.35 m; Atlas V 411 | [1] |
| Launch period; orbit the Sun for a year, then use Earth gravity | [2] |
| Bennu about half a kilometer wide; rotation 4.3 h; orbit 1.2 years; mean distance from the Sun ~168 million km; closest approach to Earth ~299,000 km every ~6 years; boulders up to ~22 m; returned sample 121.6 g | [3] |
| TAG on 20 October 2020; mission requirement at least 60 g; 70.3 g found on the outside of the sampler head | [4] |
| Mission cost ~800 M$ (without launch vehicle), Atlas V ~183.5 M$ | [5] `// verify` |
| Arrival 3 December 2018 matching speed at ~19 km; orbit at ~1.75 km on 31 December 2018; departure 10 May 2021; capsule return 24 September 2023 | [4][6] `// verify` |
| Bulk density 1.19 g/cm3; contact speed ~10 cm/s; sample arm sinks ~half a metre | `// verify` against NASA TAG pages |

## 8. References (put in docs/REFERENCES.md and the in-game Sources pages; a human must verify every link before the final submission)
1. NASA, OSIRIS-REx Press Kit (August 2016). https://www.nasa.gov/wp-content/uploads/2016/06/osiris-rex_press_kit_0.pdf
2. NASA, OSIRIS-REx Fact Sheet. https://www.nasa.gov/wp-content/uploads/2016/06/osiris_rex_factsheet5-25.pdf
3. NASA, Bennu Facts. https://science.nasa.gov/solar-system/asteroids/101955-bennu/facts/ and https://solarsystem.nasa.gov/small-bodies/asteroids/101955-bennu/in-depth
4. NASA, OSIRIS-REx mission page (TAG, sample timeline). https://science.nasa.gov/mission/osiris-rex/
5. Beshore et al., "OSIRIS-REx: Sample Return from Asteroid (101955) Bennu", IEEE Aerospace 2015 (NASA Goddard). https://ael.gsfc.nasa.gov/sed/content/uploadFiles/publication_files/Beshore%20et%20al%20IEEE%202015.pdf (cost-cap ~850 M$ plus launch vehicle) and NASA release figures for mission cost.
6. NASA, OSIRIS-REx mission timeline (arrival, orbit, return). https://science.nasa.gov/mission/osiris-rex/
In `docs/REFERENCES.md` keep the line: "Images and data credit: NASA/JPL-Caltech/Goddard/University of Arizona where applicable. Values marked `game` are design assumptions, not NASA data."

## 9. Phases (tick in docs/PROGRESS_ASTEROID.md)
- [ ] A0 inspect existing dispatch, Moon and Mars classes, create PROGRESS_ASTEROID.md
- [ ] A1 asteroid core: definition, targets, catalog, engineering model, TAG model, simulator + Monte Carlo (Assets/Scripts/Mission/Asteroid/Core)
- [ ] A2 terminal pages 1-5 (Assets/Scripts/Mission/Asteroid/UI), same shell, animations and status bar
- [ ] A3 dispatch: "Asteroit" opens the asteroid terminal; "Ay" and "Mars" unchanged
- [ ] A4 handbook spreads + Sources spread; REFERENCES.md updated; src/game/verify tags in code
- [ ] A5 balance tuning, compile pass from Editor.log, final report with "COULD NOT AUTOMATE"
- [ ] A6 (P2) orbit visualizer for page 4, extras
Done = zero compile errors, Moon and Mars unchanged, asteroid playable end to end with a debrief and a grams-of-sample result, Sources page present in all three handbooks, docs/REFERENCES.md covers all three missions.
