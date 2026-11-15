# Sources and Credits - Mission Architect (NASA Space Apps Challenge)

Status legend: **OK** = the source page was found and the value read there during development. **CHECK** = a human must open the link and confirm before submission. **TODO** = the team must fill this in. Values marked `game` in the code are design assumptions, not real data.

## 1. Images and art

| File / asset | Origin | SI-generated? | Credit / license |
|---|---|---|---|
| science_lab_interior.png | Generated programmatically (Python/Pillow) by an AI assistant (Claude, Anthropic) | YES, watermarked "SI-GENERATED" | Team-owned SI output. Contains no NASA branding. |
| science_building.png | same | YES, watermarked | same |
| scientist.png | same | YES (PNG metadata) | same |
| grass.png | same | YES (PNG metadata) | same |
| ui_panel.png, ui_button.png | same | YES (PNG metadata) | same |
| Astronaut sprite sheet (walk animation) | itch.io, "Free Pixel Art Character Asset - ASTRONAUT" by saraspiegelberg, <https://saraspiegelberg.itch.io/free-pixel-art-character-asset-astronaut> | NO (human-made) | CHECK the license and credit requirement on the page; credit the author by name. |
| Door sprite | TODO: the source page of the image that was screenshotted (file `Ekran_goruntusu_2026-11-14_162258`) | NO / unknown | TODO: add author, URL and license. If the license is unknown, replace it with an own or SI-generated door only after informing the owner. |
| Pixel font | Pixelify Sans (`Assets/Fonts/PixelifySans-Regular.ttf` and `.asset`) | NO | TODO/CHECK: author, source URL and license (likely OFL; confirm before submission). |
| Background music | `Assets/Resources/Music/bgm` (mp3/ogg/wav) | NO (team-supplied unless stated) | TODO: title, artist, license and URL in `Assets/Scripts/Audio/MusicCredit.cs`. |

No image, sprite or UI element contains NASA logos, flags, mission patches or other NASA design elements.

## 2. Tools, engine and code

| Item | Use | Credit |
|---|---|---|
| Unity 6 (6000.2.5f1), Universal Render Pipeline 2D, Input System, TextMeshPro, 2D Animation | Game engine and packages | Unity Technologies |
| Claude (Anthropic) | Chat assistant: design, code snippets, image generation scripts, specs and prompts | SI tool |
| OpenCode (open-source coding agent), <https://opencode.ai> | Runs the coding models in the project folder | opencode.ai contributors |
| Coding-agent models run through OpenCode: Gemini 4 Hydrogen (Google), DeepSeek V4.1 Flash, GLM 5.3, MiMo 2.6 Pro (via the EVREN platform, T.C. Presidency of Defense Industries) | Wrote and fixed most mission code | SI tools. TODO: list only the models actually used. |
| Git and GitHub, GitHub Desktop | Version control and teamwork | - |

All C# code was written with SI assistance; see [SI_DISCLOSURE.md](SI_DISCLOSURE.md).

## 3. NASA and scientific data, by mission

General note: values tagged `src: [N]` in the code come from these references. Numbers tagged `game` (launch vehicles, costs of parts, failure probabilities, instrument masses and power in the Mars and asteroid missions, balance factors) are NOT NASA data.

### 3.1 Moon (lunar polar orbiter)

| Used for | Source | Status |
|---|---|---|
| Earth-Moon distance 384,400 km, Moon radius 1,737.4 km, gravitational parameter 4,902.8 km3/s2, solar constant 1,361 W/m2, low-orbit period ~118 min | NASA NSSDC, Moon Fact Sheet, <https://nssdc.gsfc.nasa.gov/planetary/factsheet/moonfact.html> | CHECK (standard page; link not opened during development) |
| Orbiter concept, polar orbit, instrument types (camera, laser altimeter, neutron spectrometer) | Inspired by NASA's Lunar Reconnaissance Orbiter (LRO) | TODO: add the LRO NASA mission page and the figures actually used |
| Delta-v budget (TLI, LOI, trim), launch-vehicle capacities, instrument values | Game assumptions based on standard orbital mechanics | `game` |

### 3.2 Mars (surface rover)

| Used for | Source | Status |
|---|---|---|
| Mars gravity 3.71 m/s2, escape velocity 5.03 km/s, radius 3,396.2 km, solar irradiance ~586-589 W/m2 | [1] NASA NSSDC, Mars Fact Sheet, <https://nssdc.gsfc.nasa.gov/planetary/factsheet/marsfact.html> | OK |
| Earth-Mars distance range ~55 to 400 million km (light time 3 to 22 min, computed) | [1] same sheet, distance rows | CHECK (the numbers were first read on a university course page) |
| Rover mass 1,025 kg; MMRTG ~110 W; Atlas V 541; 471 million km trip; cost ~2.4 B$ + ~300 M$ operations; prime mission 1 Mars year | [2] NASA/JPL, Mars 2020 quick facts, <https://www.jpl.nasa.gov/news/press_kits/mars_2020/launch/quick_facts/> and <https://www.jpl.nasa.gov/news/press_kits/mars_2020/landing/quick_facts> | OK |
| Launch window every ~26 months | [3] NASA/JPL, Mars 2020 launch period, <https://www.jpl.nasa.gov/news/press_kits/mars_2020/launch/mission> | OK |
| Surface pressure under 1/100 of Earth's | [4] NASA/JPL, Mars at a Glance (InSight press kit), <https://www.jpl.nasa.gov/news/press_kits/insight/launch/facts/mars-at-a-glance> | OK |
| Relay up to 2 Mbps | [5] NASA, Mars Relay Network, <https://science.nasa.gov/mars/mars-relay-network-update/> | OK |
| Direct-to-Earth X-band 160-500 bps; relay geometry | [6] NASA NTRS, Mars Perseverance Communications (2021), <https://ntrs.nasa.gov/api/citations/20210010148/downloads/Mars%20Perseverance%20Communications%202021%20revised.pdf> | OK (check the exact wording of the pass duration: "about 8 minutes per day") |
| Typical relay network rates 0.1-0.5 Mbps at maximum distance | [7] NASA JPL IPN Progress Report 42-246, <https://ipnpr.jpl.nasa.gov/progress_report/42-246/42-246A.pdf> | OK |
| Radiation: ~1.8 mSv/day in cruise, ~0.67 mSv/day on the surface | [8] Curiosity RAD results (Zeitlin et al., Hassler et al., Science 2013-2014); SwRI release, <https://www.swri.org/newsroom/press-releases/swri-led-team-calculates-the-radiation-exposure-associated-trip-mars> | OK |
| Average temperature about -81 F | [9] NASA Mars poster, <https://www.nasa.gov/pdf/173749main_Mars_Poster.pdf> | OK |
| Hohmann transfer values (C3 ~8.6, ~259 days) | Derived from planetary orbit data with standard orbital mechanics | derived |
| MMRTG mass 45 kg, plutonium-238 half-life 87.7 years, cruise ~203 days | Secondary sources (encyclopedia and space news sites) | CHECK against NASA/JPL pages |

### 3.3 Asteroid (sample return, OSIRIS-REx / Bennu)

| Used for | Source | Status |
|---|---|---|
| Spacecraft dry mass 880 kg, wet mass 2,110 kg, sample return capsule 46 kg, solar power 1,226 W up to ~3,000 W, sampling arm 3.35 m, Atlas V 411 | [A1] NASA, OSIRIS-REx Press Kit (Aug 2016), <https://www.nasa.gov/wp-content/uploads/2016/06/osiris-rex_press_kit_0.pdf> | OK |
| Launch period; one year around the Sun then Earth gravity assist | [A2] NASA, OSIRIS-REx Fact Sheet, <https://www.nasa.gov/wp-content/uploads/2016/06/osiris_rex_factsheet5-25.pdf> | OK |
| Bennu ~0.5 km wide, 4.3 h rotation, 1.2 year orbit, mean Sun distance ~168 million km, close approach ~299,000 km every ~6 years, boulders up to ~22 m, returned sample 121.6 g | [A3] NASA, Bennu Facts, <https://science.nasa.gov/solar-system/asteroids/101955-bennu/facts/> and <https://solarsystem.nasa.gov/small-bodies/asteroids/101955-bennu/in-depth> | OK |
| Touch-and-go on 20 Oct 2020, minimum requirement 60 g, 70.3 g found outside the sampler head | [A4] NASA, OSIRIS-REx mission page, <https://science.nasa.gov/mission/osiris-rex/> | OK |
| Cost-cap ~850 M$ plus launch vehicle | [A5] Beshore et al., IEEE Aerospace 2015 (NASA Goddard), <https://ael.gsfc.nasa.gov/sed/content/uploadFiles/publication_files/Beshore%20et%20al%20IEEE%202015.pdf> | OK |
| Mission cost ~800 M$ and Atlas V ~183.5 M$; arrival 3 Dec 2018, orbit at ~1.75 km, departure 10 May 2021, capsule return 24 Sep 2023 | Encyclopedia summaries of NASA releases | CHECK against NASA pages |
| Bulk density 1.19 g/cm3, TAG contact speed ~10 cm/s, arm sinking ~0.5 m | Secondary sources | CHECK against NASA TAG pages (code tagged `verify`) |

## 4. How to read this in the game

The in-game DATA SOURCES page (handbook) and [REFERENCES.md](REFERENCES.md) list the same references. Every NASA-derived constant in the code has a comment `// src: [N]`, assumptions have `// game`, and uncertain values have `// verify`.

Credit line: "Data credit: NASA, NASA/JPL-Caltech, NASA Goddard, University of Arizona, Southwest Research Institute where applicable. This project is not affiliated with or endorsed by NASA."

## 5. Team to-do before submission

1. Open every link above and confirm the value. Fix the status column.
2. Fill the TODO rows: door sprite source, astronaut sprite license, font license, models actually used, LRO figures for the Moon.
3. Replace any asset whose source or license cannot be confirmed. **Do not replace an asset silently if it may have been made by the owner; inform the owner after the work instead.**
