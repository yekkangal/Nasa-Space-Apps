# NASA Data Audit - Mission Architect

Initial read-only audit of the Moon, Mars and Asteroid missions against `docs/REFERENCES.md`.
The follow-up tag/value changes are recorded in **Changes made** at the end. Line numbers refer to the current files after the SI disclosure headers.

Tag values in the tables: `src: [N]`, `game`, `verify`, or `NONE` (including the informal `// physics` markers, which are not one of the required tags).

## 1. Scope and method

- Read `docs/MISSION_SPEC.md`, `docs/MISSION_SPEC_MARS.md`, `docs/MISSION_SPEC_ASTEROID.md` and `docs/REFERENCES.md`.
- Walked every constant in `MissionConstants.cs`, `MarsConstants.cs`, `AsteroidConstants.cs`.
- Walked every item in `MissionCatalog.cs`, `MarsCatalog.cs`, `AsteroidCatalog.cs`.
- Walked the transfer, mass, power, comms, TAG, capsule, cost and level formulas in the three engineering models.
- Walked the fixed facts shown by the terminal/handbook pages and the objective definitions.
- Counted `src` / `game` / `verify` occurrences with a raw line search that excludes `///` documentation lines but includes tags inside catalog `Note` strings.

## 2. Tag inventory per file

Only files with at least one tag are listed. **All other C# files in `Assets/Scripts` and `Assets/Editor` have 0 src / 0 game / 0 verify.**

This is the pre-change inventory. Post-change counts are in **Changes made**.

| File | src | game | verify |
|---|---:|---:|---:|
| `Mission/Core/MissionConstants.cs` | 3 | 1 | 0 |
| `Mission/Mars/Core/MarsConstants.cs` | 32 | 46 | 0 |
| `Mission/Mars/Core/MarsCatalog.cs` | 3 | 51 | 0 |
| `Mission/Mars/Core/MarsDefinition.cs` | 7 | 0 | 0 |
| `Mission/Mars/Core/MarsEngineeringModel.cs` | 5 | 7 | 0 |
| `Mission/Mars/UI/MarsTerminalUI.Pages.cs` | 3 | 0 | 0 |
| `Mission/Mars/UI/MarsTerminalUI.Surface.cs` | 2 | 4 | 0 |
| `Mission/Asteroid/Core/AsteroidConstants.cs` | 23 | 48 | 7 |
| `Mission/Asteroid/Core/AsteroidCatalog.cs` | 6 | 34 | 0 |

Notes:

- `MissionConstants.cs` has only 3 `src` tags for 44 numeric constants; `MissionCatalog.cs` has no tags at all.
- `AsteroidDefinition.cs` has **0 tags** even though it contains fixed facts (OSIRIS-REx, Bennu, 121.6 g, 60 g, 12.4 km/s, 800 M$ + 183.5 M$).
- Some `AsteroidCatalog` `src` tags are inside `Note` strings, not code comments; the raw count is therefore higher than the number of commented values.
- `MarsConstants` tags are mostly on NASA values (32 src), but the 46 game tags cover almost all model assumptions.

## 3. Reference integrity

`docs/REFERENCES.md` currently has numbered entries **1-10** (Moon and Mars) plus an asteroid section **A1-A6**.

- Numeric tags used in code: `[1]` through `[10]` (plus multiple tags such as `[4][6]`).
- Every numeric `[N]` has a matching entry 1-10 in `REFERENCES.md`.
- **Asteroid issue:** `AsteroidConstants` and `AsteroidCatalog` use `[1]`-`[6]`, but `REFERENCES.md` lists the asteroid sources as `A1`-`A6`; numeric entries 1-6 in the same document are Mars sources. The asteroid numbering is mission-local and is explained in the asteroid Sources page and the `REFERENCES.md` note, but a reader or automated check can resolve `[1]` to the Mars entry. This is an ambiguity, not a missing entry.
- `[4][6]` on asteroid entry speed: entries A4/A6 list TAG and timeline dates but **do not explicitly list 12.4 km/s**. The tag is therefore present but not fully supported by the listed text.
- Every `TODO confirm` item in the SI disclosure is outside the data tables.

## 4. In-game Sources pages

| Mission | Sources page present | Evidence |
|---|---|---|
| Moon | Yes, page 4 of 5 | `MissionHandbookUI.cs:288-291` branches to `SpreadSourcesLeft`/`Right`; refs use entry 10 |
| Mars | Yes, page 4 of 5 | `MarsHandbookUI.cs:266-269`; refs 1-9 |
| Asteroid | Yes, page 4 of 5 | `AsteroidHandbookUI.cs:265-268`; refs 1-6 in `AsteroidSources.cs` |

All three books now have a fifth SI DISCLOSURE spread after the Sources spread.

---

## 5. Moon mission

### 5.1 Constants, formula parameters and fixed facts

| Mission | Item | Value | file:line | Tag | What it is based on |
|---|---|---|---|---|---|
| Moon | Standard gravity g0 | 9.80665 m/s2 | `MissionConstants.cs:10` | NONE | Physics constant |
| Moon | Solar constant | 1361 W/m2 | `MissionConstants.cs:11` | NONE | Earth solar constant; NASA-like but untagged |
| Moon | Boltzmann constant | -228.6 dBW/K/Hz | `MissionConstants.cs:12` | NONE | Physics constant |
| Moon | Speed of light | 299792.458 km/s | `MissionConstants.cs:13` | NONE | Physics constant |
| Moon | Moon GM | 4902.8 km3/s2 | `MissionConstants.cs:14` | src: [10] | NASA NSSDC Moon Fact Sheet |
| Moon | Moon radius | 1737.4 km | `MissionConstants.cs:15` | src: [10] | NASA NSSDC Moon Fact Sheet |
| Moon | Earth-Moon distance | 384400 km | `MissionConstants.cs:16` | src: [10] | NASA NSSDC Moon Fact Sheet |
| Moon | Science orbit altitude | 100 km | `MissionConstants.cs:17` | game | Mission design choice |
| Moon | Eclipse fractions | 0.30 average / 0.40 worst | `MissionConstants.cs:19-20` | NONE | Lunar-orbit geometry assumption; LRO-like but untagged |
| Moon | Battery specific energy / DoD | 150 Wh/kg / 0.60 | `MissionConstants.cs:22-23` | NONE | Spacecraft engineering assumption |
| Moon | Tank / structure / mass margin / capacity limit | 0.08 / 0.18 / 0.15 / 0.95 | `MissionConstants.cs:25-28` | NONE | Game mass model |
| Moon | Bus / thermal / comms chain | 60 W / 30 W / 0.35 | `MissionConstants.cs:30-32` | NONE | Game load model |
| Moon | Array W per m2 / cos factor / degradation | 0.28 x 1361 / 0.90 / 0.98 per year | `MissionConstants.cs:34-36` | NONE | Game power model |
| Moon | Chemical delta-v components | 0.05 / 0.90 / 0.05 / 0.15 per year / 0.05 km/s | `MissionConstants.cs:38-42` | NONE | Lunar mission delta-v budget; LRO-like but untagged |
| Moon | Chemical margin / electric spiral | 1.10 / 7.5 km/s | `MissionConstants.cs:43-44` | NONE | Game mission design |
| Moon | Hall efficiency | 0.60 | `MissionConstants.cs:46` | NONE | Game SEP model |
| Moon | Radiation dose / belt dose | 3 krad/year / 8 krad/month | `MissionConstants.cs:47-48` | NONE | Lunar radiation assumption; LRO/CRaTER-like but untagged |
| Moon | Ops cost / contingency / mission length | 0.8 $M/month / 0.20 / 12 months | `MissionConstants.cs:50-52` | NONE | Program cost model |
| Moon | Power / link thresholds | 0.15 / 3.0 / 2.5 / 6.0 dB | `MissionConstants.cs:54-57` | NONE | Game link model |
| Moon | Full / partial point thresholds | 95 / 50 points | `MissionConstants.cs:59-60` | NONE | Game rating |
| Moon | TLI / LOI / spiral success | 0.97 / 0.97 / 0.97 | `MissionConstants.cs:62-64` | NONE | Game reliability |
| Moon | Orbit period formula | 2 pi sqrt(a^3/mu), a = R+100 km, ~117.8 min | `MissionConstants.cs:69-74` | NONE | Derived from tagged R and mu |
| Moon | One-way light time formula | distance / c, ~1.28 s | `MissionConstants.cs:76` | NONE | Derived from tagged distance and c |
| Moon | Ground contact factor | 0.5 of visibility | `EngineeringModel.cs:192` | NONE | Game occultation assumption |
| Moon | Link formula constants | 0.55 aperture efficiency, -1.5 dB EIRP, -2.5 dB misc, +228.6, 6 dB required | `EngineeringModel.cs:180-190` | NONE | Game link budget |
| Moon | Data compression / overhead | 2:1 compression, 0.9 overhead | `EngineeringModel.cs:213-214` | NONE | Game data model |
| Moon | Radiation / cost formulas | 3 krad/yr + 8 krad/mo belt; 0.8 $M/mo; 20% contingency; 12 months | `EngineeringModel.cs:227-254` | NONE | Game model on constants |
| Moon | Mission facts: distance | 384400 km | `MissionDefinition.cs:91`, `MissionTerminalUI.cs:407` | src: [10] via constant | Moon Fact Sheet |
| Moon | Mission facts: light time | 1.28 s | `MissionDefinition.cs:92`, `MissionTerminalUI.cs:408` | NONE (derived) | Derived from tagged values |
| Moon | Mission facts: transfer | 3-5 days | `MissionDefinition.cs:93`, `MissionTerminalUI.cs:409` | NONE | Mission design statement |
| Moon | Mission facts: orbit period | ~118 min | `MissionDefinition.cs:94`, `MissionTerminalUI.cs:411` | NONE (derived) | Derived from tagged R/mu |
| Moon | Mission facts: lunar night | 14.75 Earth days | `MissionDefinition.cs:95`, `MissionTerminalUI.cs:412` | NONE | Real lunar fact, no tag |
| Moon | Mission facts: mission life | 12 months | `MissionDefinition.cs:99`, `MissionTerminalUI.cs:413` | NONE | Mission requirement |
| Moon | Mission facts: delta-v budget | 1.32 km/s | `MissionDefinition.cs:96`, `MissionTerminalUI.cs:415` | NONE | Derived/game budget |
| Moon | Mission facts: budget cap | 350 $M | `MissionDefinition.cs:97`, `MissionTerminalUI.cs:416` | NONE | Game budget |
| Moon | Mission facts: mass limit | 95% of capacity | `MissionDefinition.cs:98`, `MissionTerminalUI.cs:417` | NONE | Game requirement |
| Moon | Objective points O1/O4/O5 | 30 / 10 / 10 | `MissionDefinition.cs:102-125` | NONE | Game scoring |
| Moon | Rating full/partial | 95 points + O1/O4/O6; 50 + O1 | `MissionConstants.cs:59-60`, `MissionObjectives.cs` | NONE | Game scoring |

### 5.2 Catalog items

| Mission | Item | Value | file:line | Tag | What it is based on |
|---|---|---|---|---|---|
| Moon | Launch vehicle Kestrel-1 | LEO 300 kg, TLI 0, $8M, rel 0.93 | `MissionCatalog.cs:107` | NONE | Game catalog |
| Moon | Launch vehicle Condor-5 | LEO 5500 kg, TLI 1600 kg, $55M, rel 0.95 | `MissionCatalog.cs:108` | NONE | Game catalog |
| Moon | Launch vehicle Condor-9 | LEO 17000 kg, TLI 5500 kg, $95M, rel 0.97 | `MissionCatalog.cs:109` | NONE | Game catalog |
| Moon | Launch vehicle Titan-H | LEO 50000 kg, TLI 18000 kg, $240M, rel 0.96 | `MissionCatalog.cs:110` | NONE | Game catalog |
| Moon | Propulsion Monopropellant | Isp 225 s, 12 kg, 20 W, $6M | `MissionCatalog.cs:115` | NONE | Game catalog |
| Moon | Propulsion Bipropellant | Isp 315 s, 28 kg, 30 W, $14M | `MissionCatalog.cs:116` | NONE | Game catalog |
| Moon | Propulsion Electric Hall | Isp 1800 s, 35 kg, 1500 W, $22M | `MissionCatalog.cs:117` | NONE | Game catalog |
| Moon | Solar arrays | 2 / 4 / 6 / 8 m2; 5 kg/m2; 1.5 $M/m2 | `MissionCatalog.cs:122-125` | NONE | Game catalog |
| Moon | Batteries | 200 / 400 / 800 / 1600 / 3200 Wh | `MissionCatalog.cs:130-134` | NONE | Game catalog |
| Moon | LGA S-band | 6 dBi, 1 kg, $1M, 2280 MHz, 2 Mbps cap | `MissionCatalog.cs:139` | NONE | Game catalog |
| Moon | X-band dish 0.5 m | 3 kg, $3M, 8450 MHz, 25 Mbps cap | `MissionCatalog.cs:140` | NONE | Game catalog |
| Moon | X-band dish 1.0 m | 6 kg, $6M, 8450 MHz, 25 Mbps cap | `MissionCatalog.cs:141` | NONE | Game catalog |
| Moon | X-band dish 1.5 m | 11 kg, $10M, 8450 MHz, 25 Mbps cap | `MissionCatalog.cs:142` | NONE | Game catalog |
| Moon | Ka-band dish 1.0 m | 8 kg, $12M, 32000 MHz, 150 Mbps cap | `MissionCatalog.cs:143` | NONE | Game catalog |
| Moon | RF powers | 10 / 25 / 50 / 100 W; 3 / 5 / 8 / 12 kg; $2 / 3 / 5 / 8M | `MissionCatalog.cs:148-151` | NONE | Game catalog |
| Moon | University ground station | 10 m, 100 K, 6 h/day, 0.3 $M/month | `MissionCatalog.cs:156` | NONE | Game catalog; DSN-like but untagged |
| Moon | Deep Space ground station | 34 m, 30 K, 10 h/day, 2.0 $M/month | `MissionCatalog.cs:157` | NONE | Game catalog; DSN-like but untagged |
| Moon | Radiation COTS / Mil / Rad-hard | 10 / 30 / 100 krad; cost x1.0 / 1.6 / 2.8; mass x1.0 / 1.08 / 1.15 | `MissionCatalog.cs:162-164` | NONE | Game catalog |
| Moon | Instrument narrow-angle camera | 20 kg, 30 W, 30 GB/day, $25M, 25 pts | `MissionCatalog.cs:169` | NONE | Game catalog |
| Moon | Instrument multispectral imager | 12 kg, 15 W, 12 GB/day, $18M, 15 pts | `MissionCatalog.cs:170` | NONE | Game catalog |
| Moon | Instrument laser altimeter | 14 kg, 25 W, 1 GB/day, $20M, 15 pts | `MissionCatalog.cs:171` | NONE | Game catalog |
| Moon | Instrument neutron spectrometer | 8 kg, 10 W, 0.1 GB/day, $10M, 25 pts | `MissionCatalog.cs:172` | NONE | Game catalog |
| Moon | Instrument radar sounder | 35 kg, 60 W, 8 GB/day, $45M, 20 pts | `MissionCatalog.cs:173` | NONE | Game catalog |
| Moon | Instrument magnetometer | 3 kg, 4 W, 0.02 GB/day, $4M, 8 pts | `MissionCatalog.cs:174` | NONE | Game catalog |

### 5.3 Classification

**A. NASA-sourced and correctly tagged**

- Moon GM 4902.8 km3/s2 (`src: [10]`).
- Moon radius 1737.4 km (`src: [10]`).
- Earth-Moon distance 384400 km (`src: [10]`).
- Derived light time and orbit period use those tagged values; the derived UI facts are not themselves tagged.

**B. Looks real but no source tag, or tag not supported**

- Solar constant 1361 W/m2 - real physical/NASA value, no tag.
- Chemical delta-v components 0.05 / 0.90 / 0.05 / 0.15 / 0.05 km/s - real lunar mission design (LRO-like), no tag.
- Eclipse fractions 0.30 / 0.40 - real lunar-orbit geometry, no tag.
- Radiation 3 krad/year and belt 8 krad/month - LRO/CRaTER-like, no tag.
- Lunar night 14.75 Earth days - real fact, no tag.
- Transfer "3-5 days" - real design statement, no tag.
- DSN ground-station diameters, system temperatures, visibility and cost - plausible real DSN/University station data, no tag.
- Antenna efficiency 0.55, EIRP -1.5 dB, misc 2.5 dB, Boltzmann 228.6 - standard link-budget values, no tag.
- Array efficiency 0.28, cos 0.90, degradation 0.98 - engineering assumptions, no tag.
- All Moon launch-vehicle capacities, costs, reliabilities and instrument masses/power/data are spec starter values with no source tag; they are not claimed as real data.
- `[10]` is the only Moon references entry; no unsupported `[N]` exists.

**C1. Game assumptions that could plausibly use real NASA mission data**

- Launch vehicle LEO/TLI capacities, cost and reliability -> LRO/Atlas V 401/551 performance, cost, reliability.
- Instrument masses, power, data rates and points -> LRO payload (LROC, LOLA, Diviner, LAMP, CRaTER, Mini-RF).
- Chemical delta-v components and stationkeeping -> LRO mission design.
- Eclipse fraction and radiation dose -> LRO orbit/CRaTER measurements.
- Ground station G/T, Tsys and contact hours -> NASA DSN station specifications.
- Solar array performance and battery sizing -> LRO power subsystem.

**C2. Legitimately game design**

- Point values and FULL/PARTIAL thresholds.
- 350 $M budget cap and 95% mass limit.
- 0.08 tank, 0.18 structure, 0.15 mass margin and 5% launch reserve.
- Array 0.28 efficiency, 0.90 cos factor, 0.98/year degradation.
- 0.97 TLI/LOI/spiral success probabilities and all Monte Carlo event rates.
- Ground-station cost model and 0.8 $M/month ops.
- The catalog's exact costs/names and the "Kestrel/Condor/Titan" vehicle branding.

---

## 6. Mars mission

### 6.1 Constants, formula parameters and fixed facts

| Mission | Item | Value | file:line | Tag | What it is based on |
|---|---|---|---|---|---|
| Mars | Surface gravity / escape / radius / GM | 3.71 m/s2 / 5.03 km/s / 3396.2 km / 42828 km3/s2 | `MarsConstants.cs:15-18` | src: [1] | NASA NSSDC Mars Fact Sheet |
| Mars | Solar irradiance | 588 W/m2 (586-589) | `MarsConstants.cs:19` | src: [1] | Mars Fact Sheet |
| Mars | Earth / Mars semi-major axes | 1.000 / 1.524 AU | `MarsConstants.cs:20-21` | src: [1] | Mars Fact Sheet / orbit data |
| Mars | Hohmann departure v-infinity / C3 / arrival v-infinity / TOF | 2.94 km/s / 8.6 km2/s2 / 2.65 km/s / 259 d | `MarsConstants.cs:23-26` | src: [1] derived | Hohmann from orbit radii |
| Mars | Perseverance mass / MMRTG power / trip distance | 1025 kg / 110 W / 471 Mkm | `MarsConstants.cs:28-30` | src: [2] | Mars 2020 quick facts |
| Mars | Reference build / operations cost | 2400 / 300 $M | `MarsConstants.cs:31-32` | src: [2] | Mars 2020 cost references |
| Mars | Prime mission / window | 687 d / 668 sols / 26 months | `MarsConstants.cs:34-36` | src: [3] | Mars year and launch period |
| Mars | Surface pressure / average temperature | 1/100 Earth / -81 F | `MarsConstants.cs:38-39` | src: [4] / [9] | InSight press kit / NASA Mars poster |
| Mars | Cruise / surface dose | 1.8 / 0.67 mSv/day | `MarsConstants.cs:41-42` | src: [8] | Curiosity RAD |
| Mars | Relay max / pass / typical | 2 Mbps / 8 min / 0.5 Mbps | `MarsConstants.cs:44-46` | src: [5] / [7] | Mars Relay Network |
| Mars | DTE range / distance range | 160-500 bps / 55-400 Mkm | `MarsConstants.cs:47-50` | src: [6] / [1] | Perseverance comms / Mars fact sheet |
| Mars | Sol / daylight / array sun hours | 24.66 h / 12 h / 5 h | `MarsConstants.cs:53-55` | game | Mars sol and power model |
| Mars | Array efficiency / battery / MMRTG decay | 0.28 / 150 Wh/kg / 0.96 per year | `MarsConstants.cs:56-59` | game | Game power model |
| Mars | Night fraction / housekeeping / drive | 0.55 / 120 Wh/sol / 40 m/h / 0.30 Wh/m | `MarsConstants.cs:60-64` | game | Game surface model |
| Mars | Drill / sample data / sample success | 60 h/sample / 40 Wh / 1000 Mb / 0.90 | `MarsConstants.cs:65-68` | game | Game planner model |
| Mars | Comm / rover mass margin / capacity margin | 10 Wh/pass / 1.15 / 0.95 | `MarsConstants.cs:69-72` | game | Game model |
| Mars | Contingency / ops / ground / DTE | 0.20 / 0.45 $M/mo / 22 mo / 30 $M / 6 h / 300 bps | `MarsConstants.cs:73-78` | game | Game program model; DTE within [6] |
| Mars | Conjunction / dust storms | 14 d / 3%/mo / 20-60 sols / 0.20-0.50 | `MarsConstants.cs:79-88` | game | Game environment model |
| Mars | Failures / SPE / data loss | 0.6%/mo / 2%/mo / 0.30 / 0.08 / 0.01 / 0.02 | `MarsConstants.cs:90-96` | game | Game reliability model |
| Mars | Level points / sample target / science threshold | 30 / 30 / 15 / 10 / 10 / 0.60 | `MarsConstants.cs:98-103` | game | Game scoring |
| Mars | Budget cap | 2700 $M | `MarsConstants.cs:104` | src: [2] + game cap | 2.4 B$ build+launch + 0.3 B$ ops |
| Mars | C3 curve | 8.6 + 0.004 d^2 | `MarsEngineeringModel.cs:14-17` | game | Game curve anchored on Hohmann C3 |
| Mars | Cruise model | 205 + 0.25 abs(d) days | `MarsEngineeringModel.cs:21-22` | game | Anchored on Perseverance 203 d |
| Mars | Distance model | 160 + 1.2 abs(d) Mkm | `MarsEngineeringModel.cs:26-27` | game | Game distance curve |
| Mars | Capacity model | cap10 x (1 - 0.045(C3-10)), 40% floor | `MarsEngineeringModel.cs:31-38` | game | Game launch table |
| Mars | Mass chain | structure 18%, rover margin 15%, entry hardware, 95% limit | `MarsEngineeringModel.cs:102-110` | game | Game mass model |
| Mars | EDL model | base x mass factor x site + TRN, shield/chute | `MarsEngineeringModel.cs:136-158` | game (sky crane citations tagged [2]) | Heritage-inspired game model |
| Mars | Power model | solar area x 0.28 x 588 x 5 h x season x dust; MMRTG 110 W x 24.66 h | `MarsEngineeringModel.cs:164-210` | game | Game power model |
| Mars | Data model | relay rate x 8 min x passes + DTE; 2:1 science; conjunction loss | `MarsEngineeringModel.cs:219-243` | game | Game comms model |
| Mars | Radiation model | 1.8 mSv/day cruise + 0.67 surface; /10000 to krad | `MarsEngineeringModel.cs:272-285` | src [8] dose / game conversion | RAD data plus game conversion |
| Mars | Cost model | parts + instruments + launch + ground + ops + 20% | `MarsEngineeringModel.cs:289-319` | game | Game program model |
| Mars | Levels and points | L1/L2/L3/L4/budget | `MarsEngineeringModel.cs:336-360` | game | Game scoring |
| Mars | Brief: 1 Mars year / 687 d / 668 sols | 687 d / 668 sols | `MarsDefinition.cs:48-51` | src: [3] | Mars year |
| Mars | Budget cap / distance / temp facts | 2700 $M / 55-400 Mkm / -81 F | `MarsDefinition.cs:53-58` | src: [2]/[1]/[9] | References |
| Mars | Objective points L1-L4/B | 30 / 0 / 30 / 15 / 10 | `MarsDefinition.cs:66-85` | NONE | Game scoring |
| Mars | Handbook key numbers | 687/668, 26 mo, 55-400 Mkm, 3-22 min, -81 F, 2700 $M, 10 samples | `MarsHandbookUI.cs:328-337` | src via constants / game | Mixed |
| Mars | Pressure fact in UI | under 1/100 Earth | `MarsTerminalUI.Pages.cs:106`, `:342` | src: [4] | InSight press kit |
| Mars | MMRTG decay UI fact | about 4%/year | `MarsTerminalUI.Surface.cs:150` | game | Game assumption |

### 6.2 Catalog items

| Mission | Item | Value | file:line | Tag | What it is based on |
|---|---|---|---|---|---|
| Mars | Kestrel-1 | cap10 0 kg, $8M, rel 0.93 | `MarsCatalog.cs:114` | game | Game catalog |
| Mars | Condor-5 | cap10 1100 kg, $55M, rel 0.95 | `MarsCatalog.cs:115` | game | Game catalog |
| Mars | Condor-9 | cap10 3900 kg, $95M, rel 0.97 | `MarsCatalog.cs:116` | src: [2] + game capacity | Atlas V 541 class; 3900 kg is game |
| Mars | Titan-H | cap10 12000 kg, $240M, rel 0.96 | `MarsCatalog.cs:117` | game | Game catalog |
| Mars | Small rover bus | 120 kg, $180M | `MarsCatalog.cs:122` | game | Game catalog |
| Mars | Medium rover bus | 260 kg, $450M | `MarsCatalog.cs:123` | game | Game catalog |
| Mars | Perseverance-class bus | 400 kg, $650M | `MarsCatalog.cs:124` | game | Named after Perseverance, numbers game |
| Mars | Standard wheels | 60 kg, $40M | `MarsCatalog.cs:129` | game | Game catalog |
| Mars | Heavy wheels | 120 kg, $90M | `MarsCatalog.cs:130` | game | Game catalog |
| Mars | MMRTG | 45 kg, 110 W from constants, $150M | `MarsCatalog.cs:134` | game | Mass/cost game; power [2] |
| Mars | Solar arrays | 2 / 4 / 6 / 8 m2 | `MarsCatalog.cs:138-141` | game | Game catalog |
| Mars | Batteries | 0 / 500 / 1000 / 2000 / 3200 Wh | `MarsCatalog.cs:146-150` | game | Game catalog |
| Mars | Passive / active thermal | 10 kg / 400 Wh/sol, $20M; 25 kg / 700 Wh/sol, $40M | `MarsCatalog.cs:155-156` | game | Game catalog |
| Mars | Airbags | base 0.80, landed fraction 0.40, limit 600 kg, $60M | `MarsCatalog.cs:161` | game | Game EDL |
| Mars | Sky crane | base 0.92, fraction 0.26, limit 1200 kg, $100M | `MarsCatalog.cs:162` | src: [2] + game values | Heritage inspired |
| Mars | Propulsive legs | base 0.85, fraction 0.35, limit 1000 kg, $80M | `MarsCatalog.cs:163` | game | Game EDL |
| Mars | Small / large heat shield | 3.8 m / 70 kg / $25M; 4.5 m / 110 kg / $35M | `MarsCatalog.cs:168-169` | game | Game EDL |
| Mars | Safe / Delta / Polar sites | hazard 1.00/0.88/0.93; science 0.85/1.15/1.00 | `MarsCatalog.cs:174-176` | game | Game site trade |
| Mars | Mastcam-Z | 5 kg, 10 W, 30 Mb/h, $40M, 8 pts | `MarsCatalog.cs:181` | game | Named NASA instrument; values game |
| Mars | SuperCam | 8 kg, 12 W, 20 Mb/h, $80M, 15 pts | `MarsCatalog.cs:182` | game | Named NASA instrument; values game |
| Mars | PIXL | 5 kg, 10 W, 15 Mb/h, $60M, 12 pts | `MarsCatalog.cs:183` | game | Named NASA instrument; values game |
| Mars | SHERLOC | 4 kg, 8 W, 12 Mb/h, $50M, 10 pts | `MarsCatalog.cs:184` | game | Named NASA instrument; values game |
| Mars | MOXIE | 18 kg, 300 W, 2 Mb/h, $70M, 6 pts | `MarsCatalog.cs:185` | game | Named NASA instrument; values game |
| Mars | RIMFAX | 3 kg, 5 W, 8 Mb/h, $25M, 8 pts | `MarsCatalog.cs:186` | game | Named NASA instrument; values game |
| Mars | MEDA | 5 kg, 3 W, 1 Mb/h, $30M, 4 pts | `MarsCatalog.cs:187` | game | Named NASA instrument; values game |
| Mars | Parachute / TRN / relay / DTE mass-cost | 60 kg/15M; 3 kg/10M; 15 kg/50M; 20 kg/50M | `MarsCatalog.cs:190-198` | game | Game hardware |
| Mars | Ground station | no selectable station; fixed 30 $M ground/DSN cost | `MarsConstants.cs:76` | game | Game programme model |

### 6.3 Classification

**A. NASA-sourced and correctly tagged**

- Mars gravity, escape, radius, GM, irradiance, orbit radii, Hohmann values (`src: [1]`).
- Perseverance mass, MMRTG power, trip distance, reference build/ops cost (`src: [2]`).
- 687 d / 668 sols / 26-month window (`src: [3]`).
- 1/100 surface pressure (`src: [4]`).
- Relay 2 Mbps / 8 min (`src: [5]`), DTE 160-500 bps (`src: [6]`), typical relay rate (`src: [7]`).
- Radiation 1.8 / 0.67 mSv/day (`src: [8]`).
- -81 F average temperature (`src: [9]`).
- 55-400 Mkm distance range (`src: [1]`).
- Budget cap 2700 $M is a tagged derivation from [2] plus a game cap.

**B. Plausible real data with no tag or an over-strong tag**

- Condor-9 3900 kg at C3 10: tagged `[2]` but [2] states Atlas V 541 class, not a 3900 kg capacity; capacity is game.
- Sky crane 0.92 / 0.26 / 1200 kg: tagged `[2]` but [2] is heritage, not these numeric values.
- MMRTG mass 45 kg is tagged game; [2] gives 110 W but not the 45 kg mass.
- DTE 300 bps is tagged game but sits inside the [6] 160-500 bps range; could be tagged as a chosen value from [6].
- MMRTG 4%/year decay is tagged game; the physical Pu-238 half-life is 87.7 years, so 4%/year is a game simplification, not real data.
- "Jezero-like" in the brief is a real place reference with no tag.
- Instrument names are NASA mission instruments but masses, power and data are game; the `// game` tag is correct, although a reader may assume the names imply real values.
- Perseverance-class bus label uses the NASA mission name for a 400 kg game mass.
- No unsupported numeric `[N]` exists; all 1-9 are in REFERENCES.

**C1. Game assumptions that could plausibly use real NASA mission data**

- Launch vehicle capacities/costs/reliability -> Mars 2020/Atlas V 541, real performance and cost.
- EDL base probabilities, mass factors and landed limits -> Mars 2020 EDL.
- Rover bus, wheels, arm, thermal, TRN and instrument masses/power/data -> Perseverance.
- MMRTG mass/output/decay -> Perseverance MMRTG.
- Solar array/dust/seasonal power -> Mars 2020/InSight power.
- Relay/DTE rates, passes and ground station -> MRO/MAVEN relay and DSN.
- Surface temperature, pressure and radiation are already [4][8][9]; radiation conversion to krad is game.
- Dust storm frequency/duration and safe-mode rates -> Mars 2020/InSight operations.

**C2. Legitimately game design**

- Level points/thresholds (L1-L4/B) and sample target 10.
- 2700 $M budget cap and 95% mass limit.
- Structure 18%, rover margin 15%, tank 8%, battery 150 Wh/kg/0.6 DoD.
- C3/cruise/distance curves 0.004, 205/0.25, 160/1.2 and capacity 0.045.
- EDL base/factors and site multipliers.
- Dust/season factors, heater loads, drive/drill/sample energy.
- SPE/failure/safe-mode probabilities and conjunction loss.
- Catalog costs and the "Condor/Titan" vehicle branding.

---

## 7. Asteroid mission

### 7.1 Constants, formula parameters and fixed facts

| Mission | Item | Value | file:line | Tag | What it is based on |
|---|---|---|---|---|---|
| Asteroid | Gravitational constant | 6.67430e-11 | `AsteroidConstants.cs:14` | NONE (tagged `physics`) | Physics constant |
| Asteroid | Bennu diameter / rotation / orbit / mean distance | 490 m / 4.3 h / 1.2 yr / 168 Mkm | `AsteroidConstants.cs:16-19` | src: [3] | Bennu facts |
| Asteroid | Bennu close approach / boulders / returned sample | 299000 km / 22 m / 121.6 g | `AsteroidConstants.cs:20-22` | src: [3] | Bennu facts |
| Asteroid | Sample requirement | 60 g | `AsteroidConstants.cs:23` | src: [4] | TAG requirement |
| Asteroid | Bennu bulk density | 1190 kg/m3 | `AsteroidConstants.cs:24` | src: [3] + verify | Reference says verify; [3] does not list density explicitly |
| Asteroid | TAG contact / back-away speed | 10 / 40 cm/s | `AsteroidConstants.cs:25-26` | src: [4] + verify | TAG pages; not explicit in A4 |
| Asteroid | OSIRIS dry / wet / capsule / arm | 880 kg / 2110 kg / 46 kg / 3.35 m | `AsteroidConstants.cs:28-33` | src: [1] | OSIRIS-REx press kit |
| Asteroid | Solar power reference | 1226-3000 W | `AsteroidConstants.cs:31-32` | src: [1] | Press kit |
| Asteroid | Reference mission / launch cost | 800 / 183.5 $M | `AsteroidConstants.cs:34-35` | src: [5] + verify | Beshore/NASA figures; verify |
| Asteroid | Orbit radius / arrival speed | 1750 m / 19 km/s | `AsteroidConstants.cs:36-37` | src: [6] / [4][6] + verify | Mission timeline |
| Asteroid | Capsule entry speed | 12.4 km/s | `AsteroidConstants.cs:38` | src: [4][6] + verify | Not explicitly listed in A4/A6 |
| Asteroid | AU / solar constant | 149597870.7 km / 1361 W/m2 | `AsteroidConstants.cs:41-42` | NONE (tagged `physics`) | Physics |
| Asteroid | Array efficiency / cos / degradation | 0.28 / 0.90 / 0.98 | `AsteroidConstants.cs:43-45` | game | Game power |
| Asteroid | SRP pressure / reflectivity / stability limit | 4.56e-6 N/m2 / 1.2 / 0.05 | `AsteroidConstants.cs:46-48` | game | Game SRP model |
| Asteroid | Direct / EGA C3 | 32 / 26 km2/s2 | `AsteroidConstants.cs:51-52` | game | Game trajectory |
| Asteroid | Direct / EGA arrival | 1.4 / 2.2 years | `AsteroidConstants.cs:53-54` | game | Game trajectory |
| Asteroid | Rendezvous / prox / departure / trim | 1.4 / 0.9 / 0.15 per year / 0.9 / 0.05 km/s | `AsteroidConstants.cs:55-59` | game | Game delta-v |
| Asteroid | TAG maneuver / margin / return cruise | 0.05 km/s per attempt / 1.10 / 1.0 year | `AsteroidConstants.cs:60-62` | game | Game trajectory |
| Asteroid | Structure / mass margin / tank / capacity | 0.18 / 1.15 / 0.08 / 0.95 | `AsteroidConstants.cs:65-68` | game | Game mass model |
| Asteroid | Battery / peak load | 150 Wh/kg / 0.6 DoD / 0.5 h | `AsteroidConstants.cs:69-71` | game | Game power |
| Asteroid | Housekeeping / thermal / sampler / comms | 60 W / 30 W / 20 W / 0.35 | `AsteroidConstants.cs:74-77` | game | Game loads |
| Asteroid | Contact hours / downlink overhead | 8 h/day / 0.90 | `AsteroidConstants.cs:78-79` | game | Game comms |
| Asteroid | TAG optimum speed / sigma / lognormal sigma / max attempts | 10 cm/s / 12 / 0.45 / 3 | `AsteroidConstants.cs:82-85` | game | Game TAG |
| Asteroid | Budget cap | 1050 $M | `AsteroidConstants.cs:88` | src: [5] + game cap | Reference cost + game cap |
| Asteroid | Ops cost / contingency | 0.8 $M/month / 0.20 | `AsteroidConstants.cs:89-90` | game | Game cost |
| Asteroid | Level points S1/S3/S4/S5 | 20 / 40 / 25 / 10 | `AsteroidConstants.cs:91-94` | game | Game scoring |
| Asteroid | Cruise/EGA/arrival success | 0.97 / 0.97 / 0.98 | `AsteroidConstants.cs:97-99` | game | Game reliability |
| Asteroid | Power/spacecraft/safe-mode failure | 1% / 0.5% / 2% per month | `AsteroidConstants.cs:100-102` | game | Game reliability |
| Asteroid | Target mass formula | 4/3 pi r3 x density | `AsteroidEngineeringModel.cs:47-48` | game/physics | Sphere approximation |
| Asteroid | GM / surface gravity / escape | G x mass; GM/r2; sqrt(2GM/r) | `AsteroidEngineeringModel.cs:49-51` | game/physics | Physics |
| Asteroid | Orbit clamp / period / circular speed | 1.2-12 radii; 2 pi sqrt(a3/GM); sqrt(GM/a) | `AsteroidEngineeringModel.cs:54-56` | game | Game orbit model |
| Asteroid | SRP stability | SRP pressure / au2 x Cr x area/mass; ratio to gravity | `AsteroidEngineeringModel.cs:190-199` | game | Game SRP model |
| Asteroid | Trajectory model | EGA/direct C3, arrival, rendezvous, prox, TAG, departure, 10% margin | `AsteroidEngineeringModel.cs:82-93` | game | Game trajectory |
| Asteroid | Mass model | base + structure 18%, 15% margin, tank 8%, rocket equation | `AsteroidEngineeringModel.cs:124-135` | game | Game mass model |
| Asteroid | Power model | area x 0.28 x 1361 / au2 x 0.90 x 0.98^years | `AsteroidEngineeringModel.cs:163-185` | game | Game power model |
| Asteroid | Comms model | X-band link; 8 h/day; 2:1 science; data fraction | `AsteroidEngineeringModel.cs:207-242` | game | Game link model |
| Asteroid | TAG model | contact factor, site hazard, nav bonus, target penalty | `AsteroidEngineeringModel.cs:246-260` | game | Game TAG model |
| Asteroid | Capsule model | entry 12.4 km/s, class limit, survival x0.35 if over limit | `AsteroidEngineeringModel.cs:279-293` | src entry speed + game survival | Mixed |
| Asteroid | Cost model | parts + instruments + launch + ground + ops + 20% | `AsteroidEngineeringModel.cs:297-311` | game | Game program model |
| Asteroid | Levels and rating | S1-S5 formulas | `AsteroidEngineeringModel.cs:338-365` | game | Game scoring |
| Asteroid | Brief: OSIRIS-REx/Bennu/TAG/121.6 g | 20 Oct 2020, 121.6 g | `AsteroidDefinition.cs:45-48` | NONE | Real mission reference, no tag |
| Asteroid | Objective points S1-S5 | 20 / 0 / 40 / 25 / 10 | `AsteroidDefinition.cs:59-78` | NONE | Game scoring |
| Asteroid | UI target facts | 490 m, 4.3 h, 1.123 AU, 9.3 min, escape 0.200 m/s, 22 m | `AsteroidTerminalUI.Pages.cs:76-89` | derived from constants | Bennu facts |
| Asteroid | UI sample/capsule/arm facts | 60 g, 121.6 g, 46 kg, 3.35 m | `AsteroidHandbookUI.cs:332-335` | src via constants | OSIRIS/Bennu |

### 7.2 Catalog items

| Mission | Item | Value | file:line | Tag | What it is based on |
|---|---|---|---|---|---|
| Asteroid | Bennu-like target | 490 m, 1190 kg/m3, 4.3 h, 1.2 yr, 1.123 AU, 299000 km, 22 m | `AsteroidCatalog.cs:123-136` | src: [3] via constants | Bennu facts |
| Asteroid | Fast rotator target | 150 m, 1500 kg/m3, 0.333 h, 0.8 yr, 0.9 AU, 50000000 km, 2 m | `AsteroidCatalog.cs:137-144` | game | Fictional target |
| Asteroid | Metal-rich target | 25000 m, 7000 kg/m3, 6 h, 3.0 yr, 2.0 AU, 400000000 km, 10 m | `AsteroidCatalog.cs:145-152` | game | Fictional target |
| Asteroid | Small / medium / large bus | 160/260/340 kg; $150/260/400M | `AsteroidCatalog.cs:157-159` | game | Game catalog |
| Asteroid | Hydrazine / biprop / electric | Isp 225/315/1800 s; 20/35/40 kg; 20/30/1500 W; $12/20/30M | `AsteroidCatalog.cs:164-166` | game | Game catalog |
| Asteroid | Solar arrays | 4 / 6 / 8 / 10 m2 | `AsteroidCatalog.cs:171-174` | game | Game catalog |
| Asteroid | Batteries | 0 / 500 / 1000 / 2000 Wh | `AsteroidCatalog.cs:179-182` | game | Game catalog |
| Asteroid | Antennas | reuses Moon X-band 0.5 / 1.0 / 1.5 m; default 1.5 m | `AsteroidDesign.cs:16`, `MissionCatalog.cs:140-142` | NONE | Reused Moon catalog |
| Asteroid | RF power | reuses Moon 10 / 25 / 50 / 100 W; default 100 W | `AsteroidDesign.cs:17`, `MissionCatalog.cs:148-151` | NONE | Reused Moon catalog |
| Asteroid | Ground station | reuses Moon University 10 m and Deep Space 34 m; default DSS | `AsteroidDesign.cs:18`, `MissionCatalog.cs:156-157` | NONE | Reused Moon catalog |
| Asteroid | TAGSAM-like arm | 40 kg, $90M, median 100 g, sigma 0.45, failure 0.08 | `AsteroidCatalog.cs:189` | src: [1][3] in Note + game values | OSIRIS sampler; distribution game |
| Asteroid | Simple scoop | 15 kg, $30M, median 35 g, sigma 0.70, failure 0.18 | `AsteroidCatalog.cs:190` | game | Game sampler |
| Asteroid | Sticky pad | 8 kg, $20M, median 12 g, sigma 0.90, failure 0.32 | `AsteroidCatalog.cs:191` | game | Game sampler |
| Asteroid | Light capsule | 30 kg, $25M, survival 0.85, limit 11.5 km/s | `AsteroidCatalog.cs:196` | game | Game capsule |
| Asteroid | Standard capsule | 46 kg, $40M, survival 0.96, limit 12.6 km/s | `AsteroidCatalog.cs:197` | src: [1] mass + game survival | OSIRIS capsule 46 kg |
| Asteroid | Heavy capsule | 70 kg, $60M, survival 0.985, limit 13.5 km/s | `AsteroidCatalog.cs:198` | game | Game capsule |
| Asteroid | Safe / Nightingale / Boulder sites | hazard 0.05/0.18/0.35; sample 0.70/1.15/1.30 | `AsteroidCatalog.cs:203-205` | game; Nightingale [4] in Note | Game site trade |
| Asteroid | OCAMS / OLA / OVIRS / OTES / REXIS | 12/8/10/8/6 kg; 20/15/12/10/8 W; 1.8/0.7/0.9/0.5/0.1 GB/day; 25/15/18/15/12 $M; 20/10/12/10/8 pts | `AsteroidCatalog.cs:210-214` | game | Named NASA instruments; game values |
| Asteroid | Kestrel / Condor-5 / Condor-9 / Titan-H | cap10/20/30 = 0; 900/500/250; 3400/2400/1600; 11000/8000/5500 kg | `AsteroidCatalog.cs:219-222` | game; Condor-9 [1] in Note | Game launcher table |
| Asteroid | Condor-9 cost/reliability | $95M, 0.97 | `AsteroidCatalog.cs:221` | game | Game catalog |
| Asteroid | Other launcher costs/reliability | $8/55/95/240M; 0.93/0.95/0.97/0.96 | `AsteroidCatalog.cs:219-222` | game | Game catalog |

### 7.3 Classification

**A. NASA-sourced and correctly tagged**

- Bennu diameter, rotation, orbit, mean distance, close approach, boulders, returned sample (`src: [3]`).
- 60 g requirement (`src: [4]`).
- OSIRIS dry/wet/capsule/arm/solar power (`src: [1]`).
- 800 M$ / 183.5 M$ (`src: [5]`, with verify).
- 1.75 km orbit / 19 km/s arrival (`src: [6]` / `[4][6]`).
- Standard capsule mass 46 kg uses `ReferenceCapsuleKg` (`src: [1]`).
- `AsteroidSources` page lists 1-6.

**B. Plausible real data with no tag or an unsupported tag**

- Bennu bulk density 1190 kg/m3: tagged `[3] + verify`, but A3 does not explicitly list density.
- TAG contact/back-away 10/40 cm/s: tagged `[4] + verify`, but A4 does not list these values.
- Capsule entry 12.4 km/s: tagged `[4][6] + verify`, but A4/A6 do not list 12.4 km/s.
- `AsteroidDefinition` brief names OSIRIS-REx/Bennu and 121.6 g with **no tag**.
- `ReferenceMissionCost`/`ReferenceLaunchCost` are tagged `[5] + verify`; the verify flag is correct but they are presented as exact.
- Condor-9 `[1]` note refers to Atlas V 411, but the capacity 3400/2400/1600 kg and $95M are game.
- TAGSAM-like arm `[1][3]` tag is inside a `Note` string, not a code comment; the 40 kg/90M/median/sigma/failure are game.
- Numeric asteroid refs `[1]-[6]` collide with Mars refs 1-6 in `REFERENCES.md`; asteroid entries are A1-A6.
- `ReferenceDryMassKg`/`ReferenceWetMassKg` are calibration references, not used by the mass model.
- `TagLognormalSigma`, `TagBackAwaySpeedCmS` and `ReferenceArrayMinW`/`MaxW` are defined but not used by the model; `BennuOrbitYears` and `ReferenceArrivalSpeedKmS` are display/calibration values.

**C1. Game assumptions that could plausibly use real NASA mission data**

- Bus/propulsion/array/battery masses and costs -> OSIRIS-REx spacecraft.
- Sampler mass/median/failure and site factors -> OSIRIS-REx TAGSAM and Bennu TAG site.
- Capsule mass/survival/heat-shield limits -> OSIRIS-REx sample return capsule.
- Instrument masses/power/data -> OSIRIS-REx OCAMS/OLA/OVIRS/OTES/REXIS.
- Launch capacity/cost -> Atlas V 411 at the actual C3.
- Bennu density and TAG contact speed -> Bennu/OSIRIS TAG data.
- Mission cost and launch cost -> NASA/Beshore references.

**C2. Legitimately game design**

- Level points/rating and 60 g threshold (threshold is real [4], scoring is game).
- 1050 $M budget cap and 95% mass limit.
- Structure/tank/mass margin, array efficiency/degradation, battery, SRP stability 5%.
- Direct/EGA C3, arrival years, delta-v components, TAG maneuver 0.05 km/s/attempt, 10% margin.
- Fast rotator and metal-rich targets, their C3/dv penalties, science/richness/hazard multipliers.
- Site hazard/sample factors, sampler median/sigma/failure, capsule survival multipliers.
- Monte Carlo success/failure/safe-mode rates.
- Catalog costs and "Condor/Titan" vehicle branding.

---

## 8. Moon constants with no source

`MissionConstants.cs` contains 44 numeric constants. Only **3** carry `src: [10]`; **1** (`OrbitAltitudeKm`) carries `game`; **40 have no source tag**.

Grouped list of the 40:

- Physics/model: `G0`, `SolarConstant`, `BoltzmannDbw`, `LightSpeedKmS`.
- Orbit/power: `EclipseFractionAverage`, `EclipseFractionWorst`, `BatteryWhPerKg`, `BatteryUsableDod`, `ArrayWPerM2`, `ArrayCosFactor`, `ArrayDegradationPerYear`.
- Mass: `TankFraction`, `StructureFraction`, `MassMargin`, `CapacityMassLimit`.
- Loads: `BusPowerW`, `ThermalPowerW`, `CommsChainEfficiency`.
- Delta-v: `ChemicalMidcourseKmS`, `ChemicalLoiKmS`, `ChemicalTrimKmS`, `ChemicalStationKeepKmSPerYear`, `ChemicalDesatKmS`, `ChemicalMarginFactor`, `ElectricSpiralKmS`, `HallEfficiency`.
- Environment/reliability: `RadiationKradPerYear`, `BeltDoseKradPerMonth`, `TliSuccessProbability`, `LoiSuccessProbability`, `SpiralSuccessProbability`.
- Program: `OpsCostPerMonth`, `ContingencyFactor`, `MissionMonths`, `MinPowerMargin`, `LinkThresholdDb`, `LinkMiscDb`, `LinkRequiredEbNoDb`, `PointsFull`, `PointsPartial`.

Some of these are physics (`G0`, `c`, Boltzmann) and do not need a NASA citation; others (`SolarConstant`, delta-v, eclipse, radiation, DSN-like link values) look like real data and should be tagged `src` or explicitly `game`.

---

## 9. Top 10 places to bring in real NASA data

1. **Moon launch vehicles**: replace LEO/TLI capacities, cost and reliability with real LRO/Atlas V 401/551 performance (`MissionCatalog.cs:107-110`).
2. **Moon delta-v budget**: replace 1.32 km/s components and stationkeeping with LRO mission-design values (`MissionConstants.cs:38-43`, `MissionDefinition.cs:96`).
3. **Moon power**: replace array efficiency, eclipse fractions, battery and load values with LRO power-subsystem data (`MissionConstants.cs:19-36`, `EngineeringModel.cs:103-157`).
4. **Moon instruments**: replace camera/altimeter/spectrometer/radar masses, power, data and points with LRO LROC/LOLA/Diviner/LAMP/CRaTER/Mini-RF values (`MissionCatalog.cs:169-174`).
5. **Moon radiation and link**: use LRO/CRaTER dose and real DSN G/T, Tsys and contact hours (`MissionConstants.cs:47-48`, `MissionCatalog.cs:156-157`, `EngineeringModel.cs:180-190`).
6. **Mars EDL**: replace airbags/sky-crane/legs base probabilities, landed fractions and limits with Mars 2020/Curiosity EDL data (`MarsCatalog.cs:161-163`, `MarsEngineeringModel.cs:136-158`).
7. **Mars rover and instruments**: replace bus/wheels/arm/thermal/TRN and Mastcam-Z/SuperCam/PIXL/SHERLOC/MOXIE/RIMFAX/MEDA masses, power and data with Perseverance data (`MarsCatalog.cs:122-187`).
8. **Mars power and dust**: replace MMRTG mass/decay, solar/dust/season factors and heater loads with Mars 2020/InSight values (`MarsConstants.cs:56-88`, `MarsEngineeringModel.cs:164-210`).
9. **Mars relay and ground**: replace 8-minute/pass, 0.5 Mbps, 1-2 passes, DTE rate and DSN contact with MRO/MAVEN/DSN data (`MarsConstants.cs:44-50`, `MarsEngineeringModel.cs:219-243`).
10. **Asteroid OSIRIS-REx chain**: replace bus/prop/array/sampler/capsule/instruments, Atlas V 411 capacity and Bennu density/TAG data with OSIRIS-REx and Bennu values (`AsteroidCatalog.cs:157-222`, `AsteroidConstants.cs:16-38`, `AsteroidEngineeringModel.cs:246-293`).

---

## 10. NEEDS HUMAN RESEARCH

Exact data points that should be sourced before submission. Units are explicit.

### Moon

- Mission: **LRO / Atlas V**: launch vehicle TLI capacity in kg, LEO capacity in kg, launch cost in M$, launch reliability as a fraction.
- Mission: **LRO**: delta-v for midcourse, LOI, orbit trim, stationkeeping in km/s per year, desaturation reserve in km/s.
- Mission: **LRO**: solar-array area in m2, beginning/end-of-life power in W, battery capacity in Wh, bus and thermal loads in W.
- Mission: **LRO**: eclipse fraction at the chosen lunar orbit in percent, average and worst case.
- Mission: **LRO / CRaTER**: radiation dose in krad per year and part tolerance in krad.
- Mission: **LRO**: LROC, LOLA, Diviner, LAMP, CRaTER and Mini-RF mass in kg, power in W, data rate in GB/day and cost in M$.
- Mission: **DSN**: ground station diameter in m, system temperature in K, G/T in dB/K, contact hours per day and cost per month in M$.

### Mars

- Mission: **Mars 2020 / Perseverance**: EDL success probability as a fraction, landed mass fraction, entry-system mass limits in kg, heat-shield mass in kg.
- Mission: **Perseverance**: rover bus, wheels, arm, thermal, TRN and instrument masses in kg; instrument power in W and data in Mb/sol.
- Mission: **Perseverance**: MMRTG mass in kg, output in W and decay per year as a fraction.
- Mission: **Mars 2020 / InSight**: solar array output in W, dust-storm probability per month, storm duration in sols, solar output factor during storms.
- Mission: **Perseverance**: cruise and surface radiation in mSv/day (already [8]), and electronics dose conversion in krad.
- Mission: **MRO / MAVEN / DSN**: relay rate in Mbps, pass duration in min, passes per sol, direct-to-Earth rate in bps, station contact hours per day.
- Mission: **Mars 2020**: launch vehicle C3 capacity in kg at the chosen C3 and launch cost in M$.

### Asteroid

- Mission: **Bennu**: bulk density in kg/m3, diameter in m, rotation in h, close-approach distance in km, boulder size in m.
- Mission: **OSIRIS-REx**: dry mass kg, wet mass kg, capsule mass kg, arm length m, solar array power W, bus/prop/array/battery masses kg.
- Mission: **OSIRIS-REx TAG**: contact speed cm/s, back-away speed cm/s, sample median g, sample distribution, failure probability per attempt.
- Mission: **OSIRIS-REx capsule**: entry speed km/s, heat-shield mass kg, survival probability as a fraction.
- Mission: **OSIRIS-REx / Atlas V 411**: launch capacity kg at the mission C3, launch cost M$, launch reliability as a fraction.
- Mission: **OSIRIS-REx**: mission cost M$ without launch and with launch; 800/183.5 figures need verification.
- Mission: **OSIRIS-REx instruments**: OCAMS/OLA/OVIRS/OTES/REXIS mass kg, power W, data Gb/day.
- Mission: **Bennu / OSIRIS-REx**: orbit radius 1.75 km, orbital period h, escape velocity m/s, arrival speed km/s, departure/return dates.

### Non-data human checks

- Confirm the astronaut sprite license and credit with saraspiegelberg on itch.io.
- Confirm the door sprite author/source and license.
- Confirm the SI model list per session and remove the `TODO confirm` markers.

## 11. Audit conclusion

- **Moon:** three NASA constants are correctly tagged [10]; 40 constants have no source tag, including real-looking delta-v, eclipse, radiation, link and power values. The catalog is entirely untagged game data.
- **Mars:** the NASA constants and definitions are generally well tagged; catalog capacities, EDL success numbers, MMRTG mass/decay and several game values need either real data or stronger game labels. The Condor-9 and sky-crane `[2]` tags overstate what the reference supplies.
- **Asteroid:** many OSIRIS-REx/Bennu values are correctly tagged, but density, TAG contact speed, entry speed and the brief need verification; asteroid refs 1-6 collide with Mars refs 1-6.
- **All three:** the in-game Sources pages exist in every handbook; all numeric `[N]` values have some entry in `REFERENCES.md`, with the asteroid mission-scope ambiguity noted above.

## 12. Changes made (follow-up)

**No numeric values were changed.** This pass only changed tags, comments, reference bookkeeping and documentation, so the engineering results and balance are unchanged.

### Moon

- `docs/MISSION_SPEC_MOON_NASA.md` and `docs/NASA_DATA.md` do not exist, so no external Moon data table could be applied.
- Tagged the 40 previously untagged `MissionConstants` values:
  - `verify`: G0, solar constant, Boltzmann, light speed, both eclipse fractions, the chemical delta-v components and margin, radiation and belt dose.
  - `game`: battery, tank/structure/margin/capacity, bus/thermal/comms, array efficiency/cos/degradation, electric spiral, Hall efficiency, ops/contingency/months, power/link thresholds, points and success probabilities.
- Tagged all 36 Moon catalog items: 34 `game`, 2 ground stations `verify`.
- Added `src: [10] derived` to the distance/light-time/orbit-period definition fields; `verify` to transfer, lunar night and the 1.32 km/s budget; `game` to budget/mass/month facts and the scoring block.
- Added the missing Moon data to `docs/NASA_TODO.md`.

### Mars

- **Condor-9 capacity:** changed from `src: [2]` to `game`, because reference [2] only establishes the Atlas V 541 class, not a 3900 kg capacity.
- **Sky-crane EDL values:** changed from `src: [2]` to `game`, because [2] is heritage context, not these probabilities/fractions/limits.
- **Arm mass note:** removed the `[2]` claim; it is game-scaled.
- No numerical value changed. Other Mars tags were already consistent; the ground cost and MMRTG decay remain honestly `game` and are listed in `docs/NASA_TODO.md`.

### Asteroid

- **Bennu density:** removed `src: [3]`; it is now `verify`, because A3 does not list 1.19 g/cm3.
- **TAG contact/back-away speeds:** removed `src: [4]`; now `verify`, because A4 does not list them.
- **Capsule entry speed:** removed `src: [4][6]`; now `verify`, because A4/A6 do not list 12.4 km/s.
- **Brief:** added `src: [3][4]` for the Bennu/OSIRIS/TAG/121.6 g facts.
- **Catalog tags inside `Note` strings:** moved to code comments and marked `game` where the values are game (sampler masses/distributions, capsule survival, site hazards, launcher capacities); the 46 kg capsule mass remains sourced to [1].
- **Objectives:** marked the scoring block `game`.
- Added the missing asteroid quantities to `docs/NASA_TODO.md`.

### Reference and Sources checks

- No new numeric reference numbers were introduced; `[10]` and `[3][4]` already exist in `docs/REFERENCES.md`.
- The Moon Sources page lists [10]; the Asteroid Sources page lists 1-6, including [3] and [4]. Mars pages are unchanged.
- The asteroid refs 1-6 versus A1-A6 ambiguity is documented but was not renumbered, to avoid breaking existing tags and pages.

### Balance re-check after changes

Tags do not affect arithmetic. Re-running the standalone harnesses confirms the original results:

| Mission | Default design success | Spec target | Status |
|---|---:|---|---|
| Moon | 78.7% (FULL 78.7%, PARTIAL 0.0%) | 70-90% | Unchanged |
| Mars | 83.2% (FULL 70.9%, PARTIAL 12.3%) | 65-85% | Unchanged |
| Asteroid | 70.8% (FULL 60.2%, PARTIAL 10.6%) | 60-80% | Unchanged |

### Post-change tag inventory

Raw line counts; includes tag-like text in catalog `Note` strings and excludes `///` summary lines. All other C# files: 0 / 0 / 0.

| File | src | game | verify |
|---|---:|---:|---:|
| `Mission/Core/MissionConstants.cs` | 3 | 27 | 14 |
| `Mission/Core/MissionCatalog.cs` | 0 | 34 | 2 |
| `Mission/Core/MissionDefinition.cs` | 3 | 4 | 3 |
| `Mission/Mars/Core/MarsConstants.cs` | 32 | 47 | 0 |
| `Mission/Mars/Core/MarsCatalog.cs` | 0 | 51 | 0 |
| `Mission/Mars/Core/MarsDefinition.cs` | 7 | 0 | 0 |
| `Mission/Asteroid/Core/AsteroidConstants.cs` | 19 | 49 | 7 |
| `Mission/Asteroid/Core/AsteroidCatalog.cs` | 3 | 37 | 1 |
| `Mission/Asteroid/Core/AsteroidDefinition.cs` | 1 | 1 | 0 |

### Build

The changes are comments/tags only. Unity rebuilt `Assembly-CSharp.dll` at 19:02 with **0 errors, 0 warnings**.

