# Mission Architect

Mission Architect is a Unity 6 mission-design game built for the NASA Space Apps Challenge. You walk around a science lab, talk to Mark, pick a mission, and design it on a retro mission-control terminal: the game scores every choice with deterministic engineering models and a 1000-run Monte Carlo, so a balanced design succeeds more often than a maximal one.

Three missions are playable:

- **Moon** - a polar science orbiter with launch, power, link and budget trade-offs.
- **Mars** - an astrobiology rover with a launch window, EDL, a sol-by-sol planner and relay communications.
- **Asteroid** - a sample-return mission with target physics, direct vs gravity-assist transfer, proximity operations and TAG sampling.

## HOW TO PLAY

1. Open the [Releases page](https://github.com/yekkangal/Nasa-Space-Apps/releases).
2. Download **MissionArchitect_Windows_v1.0.0.zip**.
3. Right-click the zip and choose **Extract All**, then open the extracted folder.
4. Run **MissionArchitect.exe**.
5. If Windows SmartScreen appears, click **More info**, then **Run anyway**.

The game needs **no account, no registration and no password**. It is a single-player offline build.

## Requirements

- Windows 10 or Windows 11, 64-bit.
- A DirectX 11/12 capable GPU. The game runs windowed at 1920x1080 and the window is resizable.

## Controls

| Key | Action |
|---|---|
| **WASD** | Move |
| **E** | Interact / open the terminal |
| **B** | Handbook (in the lab) |
| **M** | Music on/off |
| **F2** | Text size (Small / Normal / Large) |
| **H** | Hide / show the control hints |
| **F1** | SI disclosure panel |
| **Esc** | Close the terminal or the handbook |
| **1-6** | Terminal tabs |

## SI disclosure

This project was built with **SI (Super Intelligence) assistance**. Large language models wrote the runtime and editor C# code, and the environment/UI pixel art was generated with AI. Human-made third-party assets are credited in the full disclosure.

- Full disclosure, tool list, asset credits and branding audit: [docs/SI_DISCLOSURE.md](docs/SI_DISCLOSURE.md)
- Sources and asset licenses: [docs/SOURCES_AND_CREDITS.md](docs/SOURCES_AND_CREDITS.md)

## NASA sources

Engineering values and mission background come from public NASA sources listed in [docs/REFERENCES.md](docs/REFERENCES.md). NASA and mission names appear only as plain-text citations and educational references; the project contains no NASA logos, flags, mission patches or official mission identifier graphics.

**Not affiliated with or endorsed by NASA.**

## Building from source

1. Open the project in Unity **6000.2.5f1**.
2. Use **Tools > Mission Architect > Rebuild Mission Lab scene** if the lab scene is missing.
3. For a release build, run Unity in batch mode with
   `-executeMethod BuildScript.BuildWindows`; the player is written to `Builds/Windows/`.

Data credit: NASA, NASA/JPL-Caltech, NASA Goddard, University of Arizona and Southwest Research Institute where applicable. Values marked `game` in code are design assumptions, not NASA data.
