# Mission Architect - Polish Progress

Spec: [MISSION_SPEC.md](MISSION_SPEC.md)
Previous mission progress: [PROGRESS.md](PROGRESS.md), [PROGRESS_MARS.md](PROGRESS_MARS.md), [PROGRESS_ASTEROID.md](PROGRESS_ASTEROID.md)

## Tasks
- [x] 1. Background music (`MusicPlayer`, import settings, credit, F1 line)
- [x] 2. Readable text (`UITextScale` / theme, F2 Small-Normal-Large, contrast, layout)
- [x] 3. Controls hint HUD (context aware, H toggle, B pulse)
- [x] 4. Turkish to English (player-visible strings, internal ids preserved)
- [x] 5. Interior exit door + scene polish (`PolishSetup`)
- [x] Final compile pass and test checklist

## What changed

### 1. Background music
- `Assets/Scripts/Audio/MusicPlayer.cs`: created before the first scene, `DontDestroyOnLoad`,
  loads `Resources/Music/bgm`, looping 2D source, base volume 0.4, 2 s fade-in, never restarts on
  scene change, ducks to 0.25 while `MissionUIState.AnyOpen`, `M` toggles mute (PlayerPrefs).
  One warning and no exception when the clip is missing.
- `Assets/Editor/MusicImportSettings.cs`: streaming load + Vorbis for audio under
  `Assets/Resources/Music`.
- `Assets/Scripts/Audio/MusicCredit.cs`: `TITLE` / `ARTIST` / `LICENSE` / `URL` (all `TODO`,
  replace when the team supplies the track). The F1 panel appends the `Music:` line.
- Credits placeholder added to `SOURCES_AND_CREDITS.md` and `SI_DISCLOSURE.md`.

### 2. Readable text
- `Assets/Scripts/Mission/UI/UITextScale.cs` is the single text theme: every runtime TMP text is
  registered by `MissionUIFactory`, mapped to a readable base size, and scaled by the
  Small / Normal / Large setting (F2, PlayerPrefs, default Large).
- Base sizes (1920x1080 reference, multiples of 4): log/small 26, tables 28, body 30,
  section headings 36, page headings 40. Small = 0.85x, Normal = 1.0x, Large = 1.15x.
- Contrast: `TextMain` is now `#EAF6FF` and `TextDim` `#A9C4D6` on opaque dark panels, with
  about +10% leading. (A TMP outline was tried but removed: setting it while a page is still
  inactive throws inside TextMeshProUGUI before its CanvasRenderer exists, which aborted terminal
  and handbook construction. Bright text on opaque panels carries the contrast instead.)
- Layout safety: TMP auto-sizing is enabled for every scaled text, so a box that is too small
  shrinks its text instead of spilling or overlapping its neighbours; the floor is 75% of the
  requested readable size. The densest handbook pages therefore settle near 28 px.
- Font: `MissionUIFactory.ResolveFont` loads `Resources/Fonts/PixelifySans-Regular` first, so
  startup UI uses Pixelify Sans instead of LiberationSans. The scene text was already Pixelify.
- Inline rich-text headings in the handbooks, the asteroid terminal and the F1 panel were raised
  from 17-19 px to 36-40 px, so they stay larger than the enlarged body text.

### 3. Controls hint HUD
- `Assets/Scripts/UI/ControlsHint.cs`: persistent bottom-centre key-cap chips, stepped slide/fade
  in through `UIAnimations`. Canvas sorting order 9000 (below `SceneFader` at 9999), no raycasts.
- World/Interior: WASD Move, E Interact, M Music, F2 Text size, H Hide hints.
- Mission Lab adds B Handbook; the B chip pulses until the handbook is opened once
  (`MA_HandbookSeen`).
- Terminal open: `1-N` Tabs (N from `MissionUIState.TerminalTabs`: 5 Moon/Asteroid, 6 Mars) and
  Esc Close. Handbook open: `< >` Page and `B/Esc` Close.
- H hides the row (saved as `MA_HintsOn`) and leaves a single `H - Show hints` chip.
- Normal rows sit above the bottom-left SI label; the two-chip terminal/handbook rows are
  centred and narrow, so they never overlap it.

### 4. Turkish to English
- Player-visible strings are English: `E - Enter` (outdoor door), `E - Talk` (Mark), `E - Exit`
  (interior/lab exit), `Moon Mission` / `Mars Mission` / `Asteroid Mission`,
  `Selected mission: Moon`, and the Mark line
  "Hi, I'm Mark! Which mission will you take on today?".
- Internal ids stay `Ay`, `Mars`, `Asteroit`. `GameState.NormalizeMission` also accepts `Moon` and
  `Asteroid`; `GameState.DisplayName` returns Moon / Mars / Asteroid and is used for the label.
  `MissionDefinition.Get` also accepts the English ids.
- Comments in the old scripts were translated to English.

### 5. Interior exit door + scene polish
- `Assets/Editor/PolishSetup.cs`: idempotent, `[InitializeOnLoadMethod]` + `delayCall`, menu
  `Tools/Mission Architect/Apply polish`, skips play mode / compiling / prefab stage, restores
  the previously active scene and closes scenes it opened.
- Interior: `ExitDoor` at `(0, -5.7)`, `BoxCollider2D` trigger 2.8 x 1.6, `Door.sceneName` set to
  the build scene whose door points at Interior (`Outdoor`), `promptText` `E - Exit`. The door is
  slightly below the requested `-5.4` so the player does not spawn inside its trigger.
- The stale `E to Interact` label is cleared and renamed `PromptText` instead of deleted, because
  it is the `PlayerInteractor.promptText` target; prompt box/text wiring is re-checked.
- Dialogue panel and mission buttons translated and enlarged (NPC 40, buttons 36, buttons widened
  to 380 x 60, dialogue panel alpha 0.95). The same text cleanup ran on `MissionLab`, which had
  copied the Turkish strings.
- The Mission Lab exit door still returns to Interior; `PolishSetup` logs
  `MissionLab exit door returns to Interior: OK.`
- Optional spawn-point feature was skipped: no spawn-point component or spawn id was added.

## Final compile
Unity recompiled after the polish pass with **0 `error CS`** lines in
`%LOCALAPPDATA%\Unity\Editor\Editor.log`. `PolishSetup` ran automatically and saved both scenes.

### Runtime crash fixed
The first play test of the Mars lab threw:

`NullReferenceException -> TMPro.TextMeshProUGUI.SetOutlineThickness -> TMP_Text.set_outlineWidth -> UITextScale.Apply`

`outlineWidth` needs a live `CanvasRenderer`, but terminal pages are built while inactive, so it
threw and aborted `MarsTerminalUI.Create` before the terminal was bound and before the handbook was
created. The outline assignment was removed (bright `#EAF6FF` on opaque panels keeps the contrast);
`UITextScale.Apply` now only sets safe TMP fields. The log shows no exception after the fix.

## Manual test checklist
1. Outer scene: walk to the building door - prompt shows `E - Enter`; press E.
2. Interior: walk to Mark - prompt shows `E - Talk`; press E; buttons read
   Moon / Mars / Asteroid Mission; the label reads `Selected mission: Moon` (or the chosen one).
3. Choose a mission: Mission Lab loads; the B Handbook chip is pulsing; press B; the handbook
   opens (English, larger text) and the pulse stops; press B/Esc to close.
4. Walk to the terminal, press E; use `1-5` (`1-6` on Mars) and Esc; the hint row switches to
   Tabs / Close.
5. Play one terminal page in each mission to confirm nothing regressed (Moon, Mars, Asteroid).
6. Interior: walk down to the bottom doorway - prompt shows `E - Exit`; press E; you return to the
   outdoor scene.
7. Press F2 three times (Small / Normal / Large); text stays readable and does not overlap; the
   choice survives a scene change and an editor restart.
8. Press M - music mutes/unmutes; it fades in on start, keeps playing across scenes, and gets
   quieter while the terminal or handbook is open.
9. Press H - hints hide; press H again - they return; the choice survives a restart.
10. Press F1 - the SI panel includes the `Music: TODO` line.
