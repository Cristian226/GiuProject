# Performance Report — Primii Pași în România

This document supports items **9–11** of the project plan. The numbers must be
captured by **running the game** with the in-game performance overlay; they are left
blank on purpose (they cannot be measured without running the build).

## How to capture data

The overlay is provided by `FpsMonitor` (auto-loads in every scene):

| Key | Action |
|-----|--------|
| **F3** | Show / hide the performance overlay (current / avg / lowest / highest FPS, frame time, memory) |
| **F4** | Append a benchmark row to `benchmarks.csv` |
| **F5** | Reset the running stats (do this after entering a scene and moving around a bit) |

`benchmarks.csv` is written to `Application.persistentDataPath`
(`%USERPROFILE%\AppData\LocalLow\<Company>\<Product>\benchmarks.csv` on Windows).
Columns: `timestamp, scene, avg_fps, lowest_fps, highest_fps, frame_ms, memory_mb, quality`.

**Draw calls / batches / tris** are read from Unity's **Game view ▸ Stats** panel
(or the Frame Debugger / Profiler) — they are not available to a runtime script in a
player build, so note them by hand.

### Suggested procedure
1. Build/run **before** optimisation. For each scene: enter, F5 to reset, walk a fixed
   loop for ~30 s, F4 to save. Record draw calls from the Stats panel.
2. Apply the item-10 optimisations.
3. Repeat the same loop **after** and F4 again.
4. Fill in the table below from the two CSV runs.

## Results

### Whole game (representative scene: ____________)

| Metric | Before | After |
|--------------|--------|-------|
| Average FPS  |        |       |
| Lowest FPS   |        |       |
| Highest FPS  |        |       |
| Frame Time   |        |       |
| Draw Calls   |        |       |
| Memory Usage |        |       |

### Per scene (optional)

| Scene | Avg FPS before | Avg FPS after | Draw calls before | Draw calls after |
|-------|----------------|---------------|-------------------|------------------|
| MainScene      |  |  |  |  |
| GeographyScene |  |  |  |  |
| HistoryScene   |  |  |  |  |
| MusicScene     |  |  |  |  |
| FinaleScene    |  |  |  |  |

## Evidence
- Screenshots: place `before_*.png` / `after_*.png` next to this file (overlay visible).
- Graphs: import `benchmarks.csv` into a spreadsheet and chart avg/lowest FPS before vs after.

## Explanation of improvements
_(Fill in once item 10 is applied — which technique helped which metric and why, e.g.
static batching ↓ draw calls, baked lighting ↓ frame time, object pooling ↓ GC spikes.)_

- Frustum / occlusion culling: …
- LOD / static batching / material sharing: …
- Lighting (baked) / particle limits: …
- Script (cached refs, fewer `Update()`s, fewer allocations): …
