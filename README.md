# Mission Architect

A Unity 6 mission-design game for the NASA Space Apps Challenge. The player picks one of three mission profiles at the lab NPC and uses a runtime-built retro terminal to design it:

- **Moon** - a polar science orbiter with launch, power, link and budget trade-offs.
- **Mars** - an astrobiology rover with a launch window, EDL, a sol-by-sol planner and relay comms.
- **Asteroid** - a sample-return mission with target physics, direct vs gravity-assist transfer, proximity operations and TAG sampling.

The result is scored with deterministic engineering models and a 1000-run Monte Carlo, so a balanced design succeeds more often than a maximal one.

## Run

1. Open the project in Unity **6000.2.5f1**.
2. Open `Assets/Scenes/Interior.unity` and press Play (or use the built `MissionLab` scene).
3. Talk to the scientist, pick a mission, walk to the terminal and press **E**.

Controls: **WASD** move, **E** interact, **B** handbook, **F1** SI disclosure, **Esc** close, **1-6** terminal tabs.

## SI disclosure

This project was built with **SI (Super Intelligence) assistance**. Large language models wrote the runtime and editor C# code, and the environment/UI pixel art was generated with AI. Human-made third-party assets are credited in the full disclosure.

- Full details, tool list, asset credits and branding audit: [docs/SI_DISCLOSURE.md](docs/SI_DISCLOSURE.md)
- Sources, asset licenses and verification status: [docs/SOURCES_AND_CREDITS.md](docs/SOURCES_AND_CREDITS.md)
- NASA data sources: [docs/REFERENCES.md](docs/REFERENCES.md)

The project contains no NASA logos, flags, mission patches or official mission identifier graphics. NASA and mission names appear only as plain-text citations and educational references.

## Credits

Data credit: NASA, NASA/JPL-Caltech, NASA Goddard, University of Arizona, Southwest Research Institute where applicable. Values marked `game` in code are design assumptions, not NASA data. This project is not affiliated with or endorsed by NASA. See [SOURCES_AND_CREDITS.md](SOURCES_AND_CREDITS.md) for asset licenses and verification status.
