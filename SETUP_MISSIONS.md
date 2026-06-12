# Romania Serious Game — Mission System

This adds a reusable **"talk to an NPC → travel to a themed room → learn → return"**
loop. Three missions are included: **Cuisine**, **History**, and **Geography & Legends**.

Everything is driven by small C# scripts under `Assets/Scripts/`. The mission rooms
are generated for you by an editor tool, and each room's learning activity is a UI
mini-game built entirely in code (no prefab wiring needed).

---

## 1. Generate the mission scenes (1 click)

In the Unity menu bar:

> **Tools ▸ Romania Game ▸ Build ALL Mission Scenes**

This creates and saves three walkable rooms in `Assets/Scenes/`:

| Scene             | Mission script (auto-added) | Suggested NPC theme       |
|-------------------|-----------------------------|---------------------------|
| `CuisineScene`    | `CuisineMission`            | Cook / market vendor      |
| `HistoryScene`    | `HistoryMission`            | Historian / museum guide  |
| `GeographyScene`  | `GeographyMission`          | Traveller / cartographer  |
| `MusicScene`      | `MusicMission`              | Musician / theatre usher  |

Each generated scene already contains: a room (floor + walls + light), a
**first-person Player**, a **mission station** (walk up, press **E** to start the
mini-game), an **exit door** (press **E** to return to the island whenever you
like), an EventSystem, and a safety `GameFlowManager`. It also registers all
scenes (including `MainScene`) in
**File ▸ Build Settings** — required for scene loading to work.

> Note: *Register Scenes In Build Settings* overwrites the build list with
> MainScene + the four mission scenes. If you add more scenes later, re-add them
> in Build Settings (or extend `MissionSceneBuilder.RegisterScenes`).

---

## 2. Turn an island NPC into a mission giver

For each NPC on the island that should start a mission:

1. Select the NPC in `MainScene`. It should already have an **`NPCInteraction`**
   component; set its **Npc Name** (e.g. `Bunica`) — that name is used everywhere.
2. Click **Add Component ▸ Mission Giver**.
3. Fill in the fields:
   - **Mission Scene Name** – `CuisineScene` (must match exactly)
   - **Mission Id** – `cuisine` (any unique string; used to remember completion)
   - **Intro Text** – what the NPC says before sending you in
   - **Already Done Text** – shown when revisiting a finished mission

That's it. When the player looks at the NPC and presses **E / Left-click**, the
intro appears; pressing **Enter** loads the mission scene. Finishing the mini-game
marks the mission complete but leaves the player in the room to explore; they
return through the **exit door** (which drops them **at the exact spot they left**).
Completed missions can be re-entered any time.

Recommended values:

| NPC theme | Mission Scene Name | Mission Id   |
|-----------|--------------------|--------------|
| Cuisine   | `CuisineScene`     | `cuisine`    |
| History   | `HistoryScene`     | `history`    |
| Geography | `GeographyScene`   | `geography`  |
| Music     | `MusicScene`       | `music`      |

> Every NPC works the same way: `NPCInteraction` checks for a `MissionGiver`
> first; if there isn't one, it just says its **Lines** (typed into the NPC's
> Inspector — see below). No dialogue assets are involved.

### Plain talking NPCs (no mission)

For an NPC that should only say something, leave off the `MissionGiver` and fill
the **Lines** list on its `NPCInteraction`: each entry is one page, shown in order
(Enter advances, Esc skips). That's the whole dialogue system now.

---

## 3. Test it

1. Open `MainScene` and press **Play**.
2. Walk to a mission-giver NPC, press **E**, then **Enter** to accept.
3. You load into the room. Walk to the station block, press **E**.
4. Answer the mini-game (multiple choice, pick-the-ingredients, or order-the-timeline).
   Finish it → "Mission complete!" toast; you stay in the room to explore.
5. Walk to the **exit door**, press **E** → back on the island where you were standing.
6. Talk to the NPC again → they greet you with the "welcome back" line and you can
   re-enter the mission.

Movement is automatically frozen while any dialogue or mini-game is open
(handled by `UIBlocker`).

---

## 4. Decorate the rooms (optional but recommended)

The generated rooms are plain grey boxes — functional, not pretty. Make them
thematic with assets you already own, e.g. the **Low-Poly Medieval Market** pack
(`Assets/Low-Poly Medieval Market/Prefabs/`):

- **Kitchen** → `table_with_meat_chease`, `round_bread`, `round_cheese`,
  vegetables, `wooden_bowl_01`, etc.
- **Museum** → tables, racks, `Table_with_weapon`, banners.
- **Map room** → tables, scrolls, bottles.

Just drag prefabs into the scene and arrange them. Keep the **Player**, the
**station** object, and the **ReturnDoor** — those carry the logic.

---

## 4b. Inspectable objects (food, props, paintings…)

Any static object can show an info panel. Select the object (e.g. a bread prefab),
**Add Component ▸ Info Object**, and fill:

- **Object Name** – e.g. `Covrigi`
- **Info** – the description text (multi-line)
- **Prompt Verb** – defaults to `Inspect`

Look at it and press **E** → a readable popup appears; press **E / Esc / Enter**
or click **Close** to dismiss. If the prop has no collider, a fitted one is added
automatically so the crosshair can hit it.

The generated mission rooms already contain **three inspectable exhibits each**
(real Romanian-culture facts) so you can see the feature working — replace the
placeholder cubes with real props but keep the `InfoObject` component.

> Interaction is now unified: looking at **any** NPC, station, door or info object
> shows a `[E] <verb>` hint via the crosshair, and only the object you aim at is
> triggered.

---

## 5. Add a fourth mission later (template)

1. Create a script in `Assets/Scripts/Missions/` that implements `IMissionContent`
   (copy `CuisineMission.cs` and change the steps). Step builders available:
   - `MissionStep.Info(title, body)`
   - `MissionStep.Single(prompt, explanation, O.Right("..."), O.Wrong("..."), ...)`
   - `MissionStep.Multi(prompt, explanation, O.Right("..."), O.Wrong("..."), ...)`
   - `MissionStep.Order(prompt, explanation, "first", "second", "third", ...)`
2. Add a `Build <Name> Scene` menu entry in
   `Assets/Scripts/Editor/MissionSceneBuilder.cs` (copy an existing one, pass your
   new content type), or build a room by hand and drop your content script +
   `MissionStation` on a cube.
3. Wire an NPC with a `MissionGiver` pointing at the new scene name + id.

Ideas not yet built: **Traditions** (Mărțișor, Dragobete, painted eggs, colinde),
**Language** (expanded vocabulary matching), **Sports/People** (Nadia Comăneci,
Hagi, Simona Halep).

---

## UI & quality-of-life

- **All UI text enlarged** for readability (dialogue, mini-games, prompts, info
  popups) and canvases match width/height evenly across aspect ratios.
- **Unified crosshair + interaction prompts** in every scene (provided by the HUD).
- **Movement freezes** while any dialogue / mini-game / info popup is open.
- **"Mission complete!" toast** shown in the room when you finish (you then leave
  via the door; completed missions stay re-enterable).
- NPCs now show a `[E] Talk to …` hint when you look at them.

To use a nicer font everywhere, assign a TMP Font Asset to the **Dialogue Font**
field on the DialogueManager (it already supports this).

## Code layout

The scripts are organised into folders under `Assets/Scripts/`
(`Core/`, `Player/`, `Interaction/`, `Dialogue/`, `Missions/`, `UI/`, `Editor/`).

See **[CODE_STRUCTURE.md](CODE_STRUCTURE.md)** for a description of every folder
and file and how they fit together.
