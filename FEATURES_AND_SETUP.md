# Primii Pași în România — Feature Expansion

This document covers everything added in the expansion & polishing phase: the new
game systems, how to turn them on (a couple of one-click editor steps), the default
controls, and where things live. It complements [SETUP_MISSIONS.md](SETUP_MISSIONS.md)
and [CODE_STRUCTURE.md](CODE_STRUCTURE.md).

> The **kitchen / Cuisine** scene was intentionally left untouched (a colleague owns it).
> Everything below either adds new files or generates the *other* scenes.

---

## 1. One-time setup (two clicks)

Everything is driven from the Unity menu bar, same as before.

1. **Tools ▸ Romania Game ▸ Build EVERYTHING (Menu + Environments + Finale)**
   Generates and registers, in Build Settings (in boot order):
   `MenuScene → MainScene → CuisineScene → GeographyScene (Train Station) →
   HistoryScene (Castle Museum) → MusicScene (Theatre) → FinaleScene`.
   *GeographyScene, HistoryScene and MusicScene are rebuilt as rich themed rooms;
   CuisineScene is never touched.*

2. Open **MainScene**, then **Tools ▸ Romania Game ▸ World ▸ Add or Fit Boundary
   In Open Scene**, and **save the scene** (Ctrl-S). This fences the city so the
   player can't walk off the island, and adds a fall-guard.

Press **Play** from `MenuScene` (or just play — the menu is build index 0).

> Individual builders also exist under **Tools ▸ Romania Game ▸ Environments / App**
> if you want to rebuild just one scene.

---

## 2. Default controls

| Action     | Default key | Rebind in |
|------------|-------------|-----------|
| Move       | W A S D     | Settings ▸ Controls |
| Sprint     | Left-Shift  | Settings ▸ Controls |
| Jump       | Space       | Settings ▸ Controls |
| Interact   | E (or LMB)  | Settings ▸ Controls |
| Inventory  | I           | Settings ▸ Controls |
| Pause/Menu | Esc         | Settings ▸ Controls |

All controls are rebindable and every setting is saved automatically.

---

## 3. The five systems

### 3.1 World boundaries  → `Assets/Scripts/World/WorldBounds.cs`
Builds four tall **invisible wall colliders** around a configurable box on Start, so
the CharacterController slides along them smoothly (no teleporting). A **fall-guard**
respawns the player at the `SpawnPoint` if they drop below the world. The editor
fitter auto-sizes the box to the scene. Every generated room also gets its own
boundary automatically.

### 3.2 Themed environments  → `Assets/Scripts/Editor/RomaniaGameTools.cs`
Three new procedurally-built, themed rooms (own materials, lighting & ambience):

- **Geography → Train Station**: platform & canopy, rails + sleepers, a standing
  CFR train, departure board, network map, ticket machines, an info desk that starts
  the mission, and inspectable geography exhibits.
- **History → Castle Museum**: tall stone hall with columns and brazier lights,
  the Romanian tricolor banners, a timeline wall, artifact plinths (crown, Trajan's
  column, sword) and a museum lectern that starts the mission.
- **Music → Theatre**: a stage with proscenium arch & red curtains, spotlights, an
  orchestra (piano + chairs + stands), **playable instruments** (click to hear a
  synthesised tone), audience seating, a **jukebox**, and a conductor's podium.

### 3.3 Settings menu  → `Assets/Scripts/UI/SettingsMenu.cs`, `Core/GameSettings.cs`
Tabbed: **Controls** (rebind every key — click a key, press the new one),
**Mouse** (sensitivity slider + invert-Y), **Audio** (master / music / effects),
**Graphics** (quality preset + FPS toggle). Plus **Reset to Defaults**. Persisted to
`PlayerPrefs` and applied live. Open it from the pause menu or the main menu.

### 3.4 Save system  → `Assets/Scripts/Core/SaveSystem.cs`, `Core/GameProgress.cs`
Saves completed missions, collected items, unlocked areas, final-quest status and the
player's position to JSON in `Application.persistentDataPath/primii-pasi-save.json`,
with a timestamp. **Auto-saves after every mission** and on returning to the island.
The **main menu** offers **Continue** (shows the save time) and **New Game** (asks
before overwriting). The pause menu has a manual **Save Game**.

### 3.5 Inventory & rewards  → `Core/Collectibles.cs`, `UI/InventoryUI.cs`, `Core/FinaleNpcManager.cs`
Each mission grants one collectible (Cuisine→Dicționar, Geography→Busolă,
History→Hrisov, Music→Notă; Transport→Bilet & Accessibility→Insignă are defined for
future missions). The inventory (key **I**) shows collected vs. missing items and a
**progress %**. When all required items are gathered, a glowing **final-guide NPC**
appears in the city offering the **Final Quest** (`FinaleScene`) — a mixed capstone
quiz. No scene editing needed; the NPC is spawned by code only when earned.

---

## 4. Music / audio

No copyrighted audio is bundled. Drop your own legally-owned files into
**`Assets/Resources/Music/`** (see the README there) and they appear in the theatre
**jukebox**, looked up by file name — e.g. `anthem`, `folk`, `manea`, `pop`, `zamfir`,
`ballad`. The main menu plays `anthem` if present. The Music mission's instrument
recognition uses **runtime-synthesised tones**, so it works with zero audio files.

---

## 5. New files at a glance

```
Assets/Scripts/
├── Core/      SaveSystem · GameProgress · GameSettings · AudioManager · Collectibles · FinaleNpcManager
├── UI/        SettingsMenu · PauseMenu · InventoryUI · MainMenuController   (+ FPS in ScreenPrompt)
├── World/     WorldBounds
├── Interaction/  SoundProp · MusicJukebox · FinaleGiver · FinaleStation
├── Missions/  FinaleMission
└── Editor/    RomaniaGameTools   (MissionSceneBuilder left as-is)
Assets/Resources/Music/   (drop your audio here)
```

Refactored: `GameFlowManager` (menu/continue/new-game + rewards), `Movement` &
`PlayerInteraction` (read rebindable keys + sensitivity), `UIKit` (Button/Slider
factories), `ScreenPrompt` (FPS counter).

Both the runtime and editor assemblies were verified to compile cleanly against the
Unity 2022.3.62 reference assemblies.
