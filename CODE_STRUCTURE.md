# Code Structure

All gameplay scripts live under `Assets/Scripts/`, grouped by responsibility.
None of them use C# namespaces, so the folders are purely for organisation — any
class can reference any other.

```
Assets/Scripts/
├── Core/          cross-cutting infrastructure used everywhere
├── Player/        the first-person controller and how it interacts
├── Interaction/   things in the world you can look at + press E
├── Dialogue/      the dialogue box system and NPC conversations
├── Missions/      the learning mini-game engine and its content
├── UI/            shared, code-built user-interface pieces
└── Editor/        editor-only tools (not part of the build)
```

### How a play session flows
1. **Player** walks around (`Player/`) and aims the crosshair.
2. **`PlayerInteraction`** raycasts; whatever it hits that is an **`IInteractable`**
   (`Core/`) shows a `[E]` hint and runs on key press.
3. Hitting an **NPC** (`Interaction/NPCInteraction`) starts a conversation
   (`Dialogue/`) or, for a mission-giver, hands off to **`GameFlowManager`**
   (`Core/`) which loads a **mission scene**.
4. In the mission scene a **`MissionStation`** (`Interaction/`) opens the
   **`MissionMiniGame`** (`Missions/`), which runs the quiz/activity. On success
   `GameFlowManager` records completion and toasts, but you stay in the room; you
   return through the **`ReturnPortal`** door whenever you like (and can revisit).
5. **`InfoObject`**s (`Interaction/`) let you inspect props via **`InfoPopup`**
   (`UI/`). The crosshair, prompts and toasts come from **`ScreenPrompt`** (`UI/`).

---

## Core/  — infrastructure
Foundational pieces with no dependency on any specific feature.

- **`IInteractable.cs`** — the one-method contract (`Prompt` + `Interact()`) for
  anything the crosshair can activate. NPCs, mission stations, exit doors and
  info objects all implement it, so `PlayerInteraction` treats them uniformly.
- **`UIBlocker.cs`** — a tiny global counter. UIs call `Push()`/`Pop()` when they
  open/close; gameplay scripts check `IsBlocked` to freeze movement and
  interaction while a panel is up. `GameFlowManager` resets it on every scene load
  so the game can never get stuck "blocked".
- **`GameFlowManager.cs`** — the persistent (`DontDestroyOnLoad`, auto-created)
  director of the leave-island → do-mission → return loop. Remembers where you
  stood, loads the mission scene, records completed missions (which stay
  re-enterable), and repositions you when you choose to return.

## Player/  — the avatar
- **`Movement.cs`** (class `PlayerMovement`) — the first-person `CharacterController`
  walk/look/jump/sprint controller. Freezes itself while `UIBlocker.IsBlocked`.
- **`PlayerInteraction.cs`** — the single interaction driver. Each frame it
  raycasts from screen centre; if it hits an `IInteractable` in range it shows the
  hint and calls `Interact()` on E / left-click. Also ensures the HUD exists.

## Interaction/  — things you interact with
All implement `IInteractable`.

- **`NPCInteraction.cs`** — marks a character as talkable. On interact it either
  hands off to a sibling `MissionGiver` (mission) or shows the NPC's inline
  `lines` (typed straight into the Inspector — no assets).
- **`MissionGiver.cs`** — sits next to `NPCInteraction`. Shows an intro line; on
  Accept it calls `GameFlowManager.StartMission(scene, id)`. Greets you differently
  once the mission is done.
- **`MissionStation.cs`** — the activity spot inside a mission scene (stove, plinth,
  map table). On interact it runs the mini-game built by the sibling
  `IMissionContent`; finishing records completion + toasts but keeps you in the
  room, and it can be replayed.
- **`ReturnPortal.cs`** — the exit door inside a mission scene; press E to return
  to the island whenever you want.
- **`InfoObject.cs`** — drop on any prop (food, painting…) to make it inspectable:
  press E to open an `InfoPopup` with its name + description. Auto-adds a fitted
  collider if the prop has none.

## Dialogue/  — the dialogue box
- **`DialogueManager.cs`** — a persistent singleton that builds its own dialogue
  box in code and shows **paged text under a speaker name** (Enter advances, Esc
  dismisses). The final page can carry an "Accept" action — that's how
  `MissionGiver` offers to start a mission. Freezes the player while open. Used by
  both `MissionGiver` (one page + Accept) and `NPCInteraction` (plain `lines`).

## Missions/  — the learning mini-games
- **`MissionStep.cs`** — the data model. A `MissionStep` is one screen: `Info`,
  `SingleChoice`, `MultiSelect` or `Ordering`, built with the static helpers
  (`Info`, `Single`, `Multi`, `Order`, `WithWrongFeedback`). Also defines
  `MissionOption` and the `IMissionContent` interface (a mission's `Title` +
  `BuildSteps()`).
- **`MissionMiniGame.cs`** — the self-building UI engine that runs a list of
  `MissionStep`s, handles answering/retrying/ordering, and reports success or
  abort. Used by the mission stations in each mission scene.
- **`CuisineMission.cs` / `HistoryMission.cs` / `GeographyMission.cs` /
  `MusicMission.cs`** — the actual content (steps) for each mission, implementing
  `IMissionContent`. Edit the text here; this is where the Romanian-culture
  material lives.

## UI/  — shared interface building blocks
- **`UIKit.cs`** — static factory helpers (`Canvas`, `Panel`, `Label`, `Rect`,
  `EnsureEventSystem`) plus **`UITheme`** (the shared colour palette). Every
  code-built UI uses these, so styling is consistent and not copy-pasted.
- **`ScreenPrompt.cs`** — the always-on HUD: crosshair (hidden while a panel is up),
  the `[E] <verb>` interaction hint, and timed toast messages.
- **`InfoPopup.cs`** — the self-building "read about this object" panel shown by
  `InfoObject`.

## Editor/  — tooling (excluded from builds)
- **`MissionSceneBuilder.cs`** — menu commands under **Tools ▸ Romania Game** that
  generate each mission room (floor, walls, light, first-person player, station,
  exit door, inspectable exhibits) and register all scenes in Build Settings.
  Because it lives in an `Editor/` folder it compiles only in the editor.

---

### Note on file vs class names
`Movement.cs` contains the class `PlayerMovement` (historical name). Every other
file matches its primary class name.
