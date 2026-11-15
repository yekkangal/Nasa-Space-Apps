# MISSION ARCHITECT - Phase 2 spec: MARS mission + NASA data and sources

Read `docs/MISSION_SPEC.md` first (section 0 working rules, section 1 project description, section 6/6b UI style, section 8 extras). Everything there still applies. This file ADDS the Mars mission. The Moon mission must keep working exactly as before.

## 0. Rules for this task
- Progress file: `docs/PROGRESS_MARS.md` (not PROGRESS.md, a teammate may edit that one). Create it first, tick phases as you go, continue from it in later sessions.
- Do not run git commands. Do not delete, rename or rewrite files you did not create in this task. Prefer adding new classes over rewriting Moon classes; if you must touch a shared class (terminal shell, handbook, MissionDefinition, MissionNPC dispatch), make the smallest change and keep Moon behavior identical.
- No web research. The NASA numbers you need are in section 7. Tag every NASA-derived constant in code with a comment `// src: [N]` (N = reference number in section 8). Values that are game assumptions (not NASA data) are tagged `// game`.
- You cannot run Unity: after writing code read `%LOCALAPPDATA%\Unity\Editor\Editor.log`, fix `error CS` lines. If Unity did not recompile, say "click the Unity window" and stop.
- Same code rules as docs/MISSION_SPEC.md section 0 (new Input System, namespace `MissionArchitect`, no UnityEditor in runtime code, Point filter, UI text in English).

## 1. What is different from the Moon mission (important)
Moon = ORBITER, linear tabs (Briefing / Spacecraft / Launch / Analysis / Launch control), one static analysis.
Mars = SURFACE ROVER, built around PHASES and decisions in time. Same terminal look, status bar, log, B-key handbook and animations, but different pages and different gameplay:
1. **Launch window is a decision** (a date slider), not a fixed vehicle check.
2. **Entry, descent and landing (EDL) is its own design page** with a landing-site trade-off.
3. **Surface operations are planned sol by sol** with an energy and data budget (a daily activity planner).
4. **Comms are relay-based** with orbiter passes, direct-to-Earth fallback and a solar-conjunction blackout.
5. **Power is MMRTG vs solar** with dust.
6. Success has levels (landing / baseline science / sample caching / extended) like NASA mission success criteria.

## 2. Mission: MARS ASTROBIOLOGY ROVER ("Mars")
Brief: land a rover in a Jezero-like crater, search for signs of ancient microbial life, cache rock samples for a future return mission. Prime mission: 1 Mars year (687 Earth days, about 668 sols) [3].
Mission levels and points (tune):
- L1 Safe landing (EDL success) - mandatory - 30
- L2 Baseline science: each installed instrument gives points x fraction of its data returned (cap 1)
- L3 Sample caching: samples cached (target >= 10) - up to 30
- L4 Extended: power still positive and rover alive at sol 668 - 15
- Budget cap 2,700 $M (reference: Perseverance cost about 2.4 B$ to build and launch plus about 300 M$ for prime operations [2]). Meeting it - 10
Rating: FULL = L1 + L2 >= 60% + L3 >= 10 samples + budget met; PARTIAL = L1 + at least 1 instrument returning data; else FAIL. Tune so a balanced Perseverance-like design succeeds 65-85% and "max everything" fails mass, EDL or budget.

## 3. Terminal pages for Mars (6 tabs, keys 1-6)
Status bar (always): BUDGET | LAUNCH MASS vs capacity | C3 | EDL success % | POWER (W/Wh per sol) | DATA per sol | SUCCESS %.
1. **WINDOW**: slider "launch date" over +/-60 days around the optimal date (the window repeats every ~26 months [3]). Shows C3 (km2/s2), cruise days, Earth-Mars distance at launch, arrival v-infinity. Pick launch vehicle here (capacity shown at the current C3).
2. **ROVER**: choose rover bus size (small / medium / Perseverance-class), mobility (wheels), robotic arm, power system (MMRTG ~110 W continuous [2] or solar array m2 + battery Wh), thermal (heaters), instruments (see 6).
3. **EDL**: choose entry system: airbags, skycrane (heritage of Curiosity/Perseverance), propulsive legs; heat shield size; parachute; terrain-relative navigation on/off; landing site: Safe Plain (low hazard, low science) / Delta Crater (high science, high hazard) / Polar Ice Region (medium). Shows EDL success probability and the landed-mass limit.
4. **SURFACE**: the sol planner. Each sol has an energy budget (Wh) and data budget (Mb). The player sets a daily policy with sliders: drive %, science %, drilling/sampling %, comms %, rest %. A mini "day bar" shows sleeping at night, heaters, comm passes. Shows expected samples, distance, data per sol, and power margin per season (dust).
5. **COMMS**: choose UHF relay (orbiter passes, up to 2 Mbps for ~8 min per pass [5][6]) and direct-to-Earth X-band (160-500 bps [6]). Shows one-way light time (computed from distance, 3 to 22 min), data volume per sol, and the solar conjunction blackout (about 2 weeks, game rule).
6. **REVIEW**: phase checklist (Launch, Cruise, EDL, Surface), Monte Carlo (1000 runs) of the whole timeline, LAUNCH button, phase-by-phase animated timeline (cruise progress bar, "seven minutes of terror"-style EDL countdown with typed log lines, then sols passing), DEBRIEF with level results and tips. Buttons Redesign / Close.
Handbook (B key) for Mars: spread 1 objectives (live L1-L4), spread 2 glossary (C3, v-infinity, EDL, sol, MMRTG, UHF relay, conjunction, TRN), spread 3 tips, **spread 4 DATA SOURCES** (list of references in section 8 with the values each one supplies). Moon handbook gets the same Sources spread (see section 6).

## 4. Engineering model for Mars (pure C#)
Constants [1]: Mars surface gravity 3.71 m/s2, escape velocity 5.03 km/s, radius 3396.2 km, GM 42,828 km3/s2, solar irradiance at Mars ~586-589 W/m2 (0.431 of Earth).
1. **Transfer**: Hohmann reference from standard orbit radii (a_Earth 1.000 AU, a_Mars 1.524 AU): departure v-infinity ~2.94 km/s, C3 = v_inf^2 ~8.6 km2/s2, arrival v-infinity ~2.65 km/s, time of flight ~259 days. Real missions fly 200-210 days (Perseverance ~203 days, 471 million km [2]). Model: launch date offset d (days): C3(d) = 8.6 + 0.004*d^2 (game), cruise days = 205 + 0.25*|d|, Earth-Mars distance at launch grows with |d|.
2. **Launch vehicle capacity to Mars**: capacity(C3) = cap10 * (1 - 0.045*(C3 - 10)), min 40% of cap10. cap10: Kestrel-1 0 (cannot), Condor-5 1,100 kg, Condor-9 3,900 kg (Atlas V 541 class [2]), Titan-H 12,000 kg. Costs and reliability as in docs/MISSION_SPEC.md section 5.
3. **Mass chain**: launch mass = cruise stage + entry system + landed system. Landed mass (rover + descent hardware that lands) as a fraction of launch mass: skycrane ~0.26, airbags ~0.40 but max landed 600 kg, propulsive legs ~0.35 (game). Rover reference: 1,025 kg [2]. Launch mass must be <= capacity(C3) with 5% margin.
4. **EDL success**: base by system (airbags 0.80, skycrane 0.92, legs 0.85, game values) x factor for landed mass vs the system limit x site hazard (Safe 1.0, Delta 0.88, Polar 0.93) with TRN raising it by +0.06 on hazardous sites; thin atmosphere: surface pressure under 1/100 of Earth's [4] makes landing hard (explain in the tooltip).
5. **Power on the surface**: MMRTG 110 W continuous [2] with ~4%/year decay (Pu-238 half-life 87.7 years); solar: output = area * 0.28 * 588 W/m2 * 5 sun-hours equivalent per sol * seasonal factor (0.7 to 1.0) * dust factor. Dust factor falls with a random dust-storm event (monthly 3% chance, lasts 20-60 sols, solar output x0.2-0.5, MMRTG unaffected). Night heater need grows with cold: average temperature about -81 F (about -63 C) [9]. Battery sizing as in the Moon model.
6. **Radiation** [8]: dose-equivalent in cruise 1.8 mSv/day, on the surface 0.67 mSv/day. Use the ratio (surface = 0.37 x cruise) to scale the electronics dose model from the Moon spec (parts classes COTS / Mil / Rad-hard).
7. **Comms** [5][6][7]: relay: up to 2 Mbps during a pass of ~8 min, 1-2 passes per sol; typical network rates 0.1-0.5 Mbps at the greatest distance [7]; direct-to-Earth X-band 160-500 bps. Daily data volume = relay rate x pass time x passes (limited by orbiter relay load) + DTE bits. Light time = distance / c (distance 55-400 million km [1]). Conjunction blackout: about 14 days with no commands.
8. **Operations**: 668 sols, each sol consumes energy per activity (drive 30 Wh/100 m, drill 40 Wh, instrument activities per instrument power x hours, comm 10 Wh/pass, heaters at night) and produces data per instrument. Samples cached per drilling activity with a failure chance. Distances and samples accumulate.
9. **Cost**: parts + instruments + launch vehicle + ground/DSN + ops 0.45 $M/sol-month x 22 months + 20% contingency. A Perseverance-class design must land near 2.4-2.7 B$ [2].
10. **Monte Carlo** (1000 runs, under 100 ms): launch roll -> cruise (radiation, TCM) -> EDL roll -> 668 sols with dust storms, failures, safe modes, conjunction -> levels L1-L4 -> P(FULL), P(PARTIAL), P(FAIL).

## 5. Catalog (starter, tune)
Rover bus: small 120 kg / medium 260 kg / Perseverance-class 400 kg (structure+avionics), costs scale. MMRTG 45 kg [2-ish], 110 W. Solar array 2/4/6 m2. Battery Wh. Wheels (mobility) 60/120 kg, robotic arm 45 kg (turret, [2]), TRN +3 kg.
Instruments inspired by the NASA rover payload (names only; masses, power, data are game-scaled approximations, tag them `// game`): Mastcam-Z (cameras), SuperCam (remote laser spectrometer), PIXL (X-ray lithochemistry), SHERLOC (organics Raman), MOXIE (oxygen from CO2, needs lots of power when running), RIMFAX (ground-penetrating radar), MEDA (weather station). Each has mass, power, hours of use, Mb per use, points.

## 6. Moon retrofit: NASA sources
- Add the same Sources spread to the Moon handbook, listing the Moon references (section 8, entry [10]).
- Moon constants in code that come from NASA fact sheets (Earth-Moon distance 384,400 km, Moon radius 1,737.4 km, mu 4,902.8 km3/s2, gravity) get `// src: [10]` comments. Do not change their values.

## 7. NASA data table (use these, do not invent others)
| Value | Source |
|---|---|
| Mars gravity 3.71 m/s2; escape velocity 5.03 km/s; radius 3396.2 km; solar irradiance ~586-589 W/m2 | [1] |
| Earth-Mars distance about 55 to 400 million km (light time 3 to 22 min, computed) | [1] |
| Launch window every ~26 months | [3] |
| Perseverance rover mass 1,025 kg; MMRTG ~110 W; Atlas V 541; 471 million km trip (July 30 launch) | [2] |
| Mission cost ~2.4 B$ build+launch, ~300 M$ prime-mission operations; prime mission 1 Mars year | [2] |
| Surface pressure under 1/100 of Earth's | [4] |
| Average temperature about -81 F | [9] |
| Radiation dose equivalent: 1.8 mSv/day in cruise, 0.67 mSv/day on the surface | [8] |
| Relay up to 2 Mbps, ~8 min passes; DTE X-band 160-500 bps; typical MRN rates 0.1-0.5 Mbps at max distance | [5][6][7] |
| Hohmann values: derived from planetary orbit radii (standard orbital mechanics) | [1] and orbit data |

## 8. References (put in docs/REFERENCES.md and in the in-game Sources pages; a human must verify every link before the final submission)
1. NASA NSSDC, Mars Fact Sheet. https://nssdc.gsfc.nasa.gov/planetary/factsheet/marsfact.html
2. NASA/JPL, Mars 2020 Perseverance quick facts. https://www.jpl.nasa.gov/news/press_kits/mars_2020/launch/quick_facts/ and https://www.jpl.nasa.gov/news/press_kits/mars_2020/landing/quick_facts
3. NASA/JPL, Mars 2020 launch period and window. https://www.jpl.nasa.gov/news/press_kits/mars_2020/launch/mission
4. NASA/JPL, Mars at a Glance (InSight press kit). https://www.jpl.nasa.gov/news/press_kits/insight/launch/facts/mars-at-a-glance
5. NASA, Mars Relay Network. https://science.nasa.gov/mars/mars-relay-network-update/
6. NASA NTRS, Mars Perseverance Communications (2021). https://ntrs.nasa.gov/api/citations/20210010148/downloads/Mars%20Perseverance%20Communications%202021%20revised.pdf
7. NASA JPL IPN Progress Report 42-246 (Mars Relay Network data rates). https://ipnpr.jpl.nasa.gov/progress_report/42-246/42-246A.pdf
8. Zeitlin et al. / Hassler et al., Curiosity RAD radiation measurements (Science, 2013-2014); SwRI release. https://www.swri.org/newsroom/press-releases/swri-led-team-calculates-the-radiation-exposure-associated-trip-mars
9. NASA, Mars poster (average temperature). https://www.nasa.gov/pdf/173749main_Mars_Poster.pdf
10. NASA NSSDC, Moon Fact Sheet (Moon constants). https://nssdc.gsfc.nasa.gov/planetary/factsheet/moonfact.html
In `docs/REFERENCES.md` also add the line: "Images and data credit: NASA/JPL-Caltech where applicable. Values marked `game` are design assumptions, not NASA data."

## 9. Phases (tick in docs/PROGRESS_MARS.md)
- [ ] M0 inspect existing Moon engine and UI classes, create PROGRESS_MARS.md, decide the smallest dispatch change (mission -> page set)
- [ ] M1 Mars core: definition, catalog, engineering model, simulator + Monte Carlo (Assets/Scripts/Mission/Mars/Core)
- [ ] M2 Mars terminal pages 1-6 and the planner/sliders (Assets/Scripts/Mission/Mars/UI) with the same animations and status bar
- [ ] M3 dispatch: selecting "Mars" at the NPC opens the Mars terminal in MissionLab; "Ay" still opens the Moon terminal; "Asteroit" still shows OFFLINE
- [ ] M4 Mars handbook spreads + Sources spread for both missions; docs/REFERENCES.md; src tags in code
- [ ] M5 balance tuning, compile pass from Editor.log, final report with "COULD NOT AUTOMATE"
Done = zero compile errors, Moon unchanged, Mars playable end to end with a debrief, handbook Sources page present for both missions, docs/REFERENCES.md written.
