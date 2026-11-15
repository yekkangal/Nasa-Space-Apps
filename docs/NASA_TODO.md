# NASA Data TODO

Values that are currently `verify` or `game` and still need a defensible NASA source. No numbers are invented here. This list is the follow-up to `docs/NASA_DATA_AUDIT.md`.

`docs/MISSION_SPEC_MOON_NASA.md` and `docs/NASA_DATA.md` do **not** exist in this project, so the Moon rows below are the missing Moon data list required by the audit instructions.

## Moon

| Mission | Quantity | Unit | Suggested NASA source |
|---|---|---|---|
| Moon | Launch vehicle LEO capacity | kg | NASA LRO mission / Atlas V 401 or 551 performance data |
| Moon | Launch vehicle TLI capacity | kg | NASA LRO mission / Atlas V 401 or 551 performance data |
| Moon | Launch cost | M$ | NASA LRO launch cost / launch services data |
| Moon | Launch reliability | fraction | Launch vehicle mission history / NASA risk data |
| Moon | Midcourse delta-v | km/s | LRO mission design |
| Moon | LOI delta-v | km/s | LRO mission design |
| Moon | Orbit trim delta-v | km/s | LRO mission design |
| Moon | Stationkeeping delta-v | km/s per year | LRO mission design |
| Moon | Desaturation reserve | km/s | LRO mission design |
| Moon | Lunar eclipse fraction (average) | fraction | LRO orbit geometry / NASA orbit data |
| Moon | Lunar eclipse fraction (worst case) | fraction | LRO orbit geometry / NASA orbit data |
| Moon | Radiation dose rate | krad per year | LRO / CRaTER radiation measurements |
| Moon | Van Allen belt dose during electric spiral | krad per month | NASA radiation-belt models |
| Moon | Solar array area | m2 | LRO solar array data |
| Moon | Solar array power at beginning of life | W | LRO power subsystem |
| Moon | Solar array degradation | fraction per year | LRO power subsystem |
| Moon | Battery capacity | Wh | LRO power subsystem |
| Moon | Bus load | W | LRO power subsystem |
| Moon | Thermal load | W | LRO thermal subsystem |
| Moon | LROC mass | kg | NASA LRO LROC instrument page |
| Moon | LROC power | W | NASA LRO LROC instrument page |
| Moon | LROC data rate | GB/day | NASA LRO LROC instrument page |
| Moon | LOLA mass | kg | NASA LRO LOLA instrument page |
| Moon | LOLA power | W | NASA LRO LOLA instrument page |
| Moon | LOLA data rate | GB/day | NASA LRO LOLA instrument page |
| Moon | Diviner mass / power / data | kg / W / GB per day | NASA LRO Diviner instrument page |
| Moon | LAMP mass / power / data | kg / W / GB per day | NASA LRO LAMP instrument page |
| Moon | CRaTER mass / power / data | kg / W / GB per day | NASA LRO CRaTER instrument page |
| Moon | Mini-RF mass / power / data | kg / W / GB per day | NASA LRO Mini-RF instrument page |
| Moon | Ground station diameter | m | NASA Deep Space Network station data |
| Moon | Ground station system temperature | K | NASA DSN station data |
| Moon | Ground station G/T | dB/K | NASA DSN station data |
| Moon | Ground station contact time | h per day | NASA DSN scheduling data |
| Moon | Ground station cost | M$ per month | NASA DSN cost data |
| Moon | Lunar night duration | Earth days | NASA Moon Fact Sheet / LRO |
| Moon | Chemical transfer duration | days | NASA LRO mission timeline |

## Mars

| Mission | Quantity | Unit | Suggested NASA source |
|---|---|---|---|
| Mars | Condor-9 capacity at C3 20 and 30 | kg | Mars 2020 / Atlas V 541 performance data |
| Mars | Atlas V 541 launch cost | M$ | Mars 2020 launch cost data |
| Mars | Atlas V 541 reliability | fraction | Launch vehicle mission history / NASA risk data |
| Mars | Airbag EDL success probability | fraction | NASA Mars Pathfinder / MER EDL data |
| Mars | Sky-crane EDL success probability | fraction | Mars 2020 / Curiosity EDL data |
| Mars | Propulsive-legs EDL success probability | fraction | NASA lander EDL data |
| Mars | Landed-mass fraction for each EDL system | fraction | Mars 2020 / MER / Phoenix data |
| Mars | Landed-mass limits | kg | Mars 2020 / MER / Phoenix data |
| Mars | Heat-shield mass and diameter | kg / m | Mars 2020 aeroshell data |
| Mars | Parachute mass | kg | Mars 2020 parachute data |
| Mars | Rover bus mass | kg | Perseverance rover mass breakdown |
| Mars | Wheel mass | kg | Perseverance mobility data |
| Mars | Robotic arm mass | kg | Perseverance arm/turret data |
| Mars | Thermal subsystem mass | kg | Perseverance thermal data |
| Mars | TRN mass | kg | Perseverance landing navigation data |
| Mars | Mastcam-Z mass / power / data | kg / W / Mb per sol | NASA Perseverance Mastcam-Z page |
| Mars | SuperCam mass / power / data | kg / W / Mb per sol | NASA Perseverance SuperCam page |
| Mars | PIXL mass / power / data | kg / W / Mb per sol | NASA Perseverance PIXL page |
| Mars | SHERLOC mass / power / data | kg / W / Mb per sol | NASA Perseverance SHERLOC page |
| Mars | MOXIE mass / power / data | kg / W / Mb per sol | NASA Perseverance MOXIE page |
| Mars | RIMFAX mass / power / data | kg / W / Mb per sol | NASA Perseverance RIMFAX page |
| Mars | MEDA mass / power / data | kg / W / Mb per sol | NASA Perseverance MEDA page |
| Mars | MMRTG mass | kg | NASA MMRTG fact sheet |
| Mars | MMRTG decay per year | fraction | NASA MMRTG / Pu-238 data |
| Mars | Dust-storm occurrence | fraction per month | Mars 2020 / InSight environmental data |
| Mars | Dust-storm duration | sols | Mars 2020 / InSight environmental data |
| Mars | Solar output during a storm | fraction | Mars 2020 / InSight power data |
| Mars | Seasonal solar factor | fraction | Mars 2020 / InSight power data |
| Mars | Surface heater load | Wh per sol | Perseverance thermal/power data |
| Mars | Direct-to-Earth rate | bps | NASA Perseverance communications data |
| Mars | Relay passes per sol and pass duration | passes / min | MRO / MAVEN relay data |
| Mars | Ground/DSN cost | M$ per month | NASA DSN cost data |
| Mars | Surface failure rate | fraction per month | Mars 2020 / InSight reliability data |

## Asteroid

| Mission | Quantity | Unit | Suggested NASA source |
|---|---|---|---|
| Asteroid | Bennu bulk density | kg/m3 | NASA Bennu fact page / OSIRIS-REx science |
| Asteroid | TAG contact speed | cm/s | NASA OSIRIS-REx TAG data |
| Asteroid | TAG back-away speed | cm/s | NASA OSIRIS-REx TAG data |
| Asteroid | Capsule re-entry speed | km/s | NASA OSIRIS-REx capsule data |
| Asteroid | Spacecraft dry mass | kg | OSIRIS-REx press kit |
| Asteroid | Spacecraft wet mass | kg | OSIRIS-REx press kit |
| Asteroid | Sample return capsule mass | kg | OSIRIS-REx press kit |
| Asteroid | Sampling arm length | m | OSIRIS-REx press kit |
| Asteroid | Solar array power | W | OSIRIS-REx press kit |
| Asteroid | Bus / propulsion / array / battery masses | kg | OSIRIS-REx mass breakdown |
| Asteroid | Sampler mass | kg | OSIRIS-REx TAGSAM data |
| Asteroid | Sample median / distribution | g | OSIRIS-REx TAG results |
| Asteroid | TAG failure probability per attempt | fraction | OSIRIS-REx TAG data |
| Asteroid | Capsule survival probability | fraction | OSIRIS-REx re-entry data |
| Asteroid | Atlas V 411 capacity at mission C3 | kg | NASA/ULA Atlas V 411 performance data |
| Asteroid | Atlas V 411 launch cost | M$ | NASA OSIRIS-REx launch cost data |
| Asteroid | Atlas V 411 reliability | fraction | Launch vehicle mission history |
| Asteroid | Mission cost without launch | M$ | Beshore et al. / NASA cost data |
| Asteroid | OCAMS / OLA / OVIRS / OTES / REXIS mass | kg | OSIRIS-REx instrument pages |
| Asteroid | OCAMS / OLA / OVIRS / OTES / REXIS power | W | OSIRIS-REx instrument pages |
| Asteroid | OCAMS / OLA / OVIRS / OTES / REXIS data | Gb per day | OSIRIS-REx instrument pages |

## Notes

- `docs/MISSION_SPEC_MOON_NASA.md` and `docs/NASA_DATA.md` are absent; the Moon rows above are therefore the complete missing-data list.
- No value in this file should be used as NASA data until a human verifies the source.
- Asset license TODO items are tracked separately in `docs/SI_DISCLOSURE.md`.
