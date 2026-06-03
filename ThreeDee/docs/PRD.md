# ThreeDee — Product Requirements Document

**A SketchUp-style 3D modeling application built from scratch in C# / WinForms (.NET 10), with no third-party libraries.**

Status: Draft v0.1
Date: 2026-06-03
Owner: pieterjan@2sky.be

---

## 1. Vision

ThreeDee is a demonstrative-but-extensive direct-manipulation 3D modeler in the spirit of
Trimble SketchUp. The user draws 2D shapes on surfaces and "pushes/pulls" them into 3D
solids, orbits/pans/zooms a camera, and uses an inference (snapping) engine to draw
precisely. The project is intentionally **dependency-free** — every layer (math, mesh,
renderer, tools, UI) is implemented by hand on top of the .NET BCL (`System.Numerics`,
`System.Drawing`/GDI+, WinForms). The goal is breadth of features and a clean, teachable
architecture, not production polish.

### Non-goals (for v1)
- **Textures / materials beyond flat color** — explicitly postponed.
- Third-party rendering, math, or geometry libraries (OpenTK, SharpDX, Vortice, helix,
  Assimp, etc.). BCL only.
- Photorealism, ray-traced shadows, global illumination.
- Multi-document / collaboration / cloud.
- Import/export of foreign formats (STL/OBJ are *stretch* goals, see §8).

---

## 2. Target user & platform

- Windows 11, .NET 10 desktop runtime.
- Single-window WinForms desktop app, mouse + keyboard driven.
- A developer/enthusiast audience evaluating "how would I build a 3D modeler from scratch?"

---

## 3. Key technical forks (to be resolved by spikes)

These are the decisions that shape the whole codebase. Each is assigned a spike in §9; the
PRD will be updated with the chosen path once spike results land.

| # | Fork | Options | Decision driver |
|---|------|---------|-----------------|
| F1 | **Render backend** | (a) Hand-written **software rasterizer** drawing into a GDI+ `Bitmap`; (b) **WPF `Viewport3D`** hosted via `ElementHost` (WPF ships with .NET, so not "third party") | Interactive FPS at realistic triangle counts; control/teachability; how much of the engine we get to write |
| F2 | **Hidden-surface removal** | Painter's algorithm (sort faces) vs per-pixel **Z-buffer** in software | Correctness on intersecting geometry vs speed |
| F3 | **Mesh topology** | Half-edge vs winged-edge vs face-vertex list | Cost/robustness of push/pull, edge dissolve, face splitting |
| F4 | **Picking** | CPU ray-cast (Möller–Trumbore) vs color-ID buffer pick | Accuracy for edges/vertices vs simplicity |

The renderer fork (F1) is foundational and gated first.

---

## 4. Core concepts / domain model

- **Vector / Matrix math** — `System.Numerics.Vector3`, `Matrix4x4`, `Quaternion`.
- **Scene graph** — flat list of *entities* (Edges, Faces, Groups) in world space for v1;
  groups/components as a stretch goal.
- **Mesh** — vertices, edges, faces with adjacency (topology TBD by F3). Faces are planar
  polygons (triangulated only at render time).
- **Camera** — orbit target, eye position, up vector, perspective FOV (with optional
  parallel/orthographic projection toggle, SketchUp-style).
- **Tool** — a state machine handling mouse/keyboard for one operation (Line, Rectangle,
  Arc, Push/Pull, Orbit, etc.). Exactly one active tool at a time.
- **Inference** — context the active tool queries to snap the cursor to meaningful points
  (endpoints, midpoints, on-edge, on-face, axis directions, parallel/perpendicular).

---

## 5. Feature list

### 5.1 Camera / navigation (P0)
- **Orbit** — rotate camera about the model (MMB drag, SketchUp default).
- **Pan** — translate view (Shift+MMB).
- **Zoom** — dolly via mouse wheel, zoom-to-cursor.
- **Zoom Extents** — frame the whole model.
- **Standard views** — Top/Front/Right/Iso hotkeys.
- **Projection toggle** — perspective ↔ parallel/orthographic.

### 5.2 Drawing tools (P0/P1)
- **Line** (P0) — chained edges; closes a loop to auto-create a **face**.
- **Rectangle** (P0) — drawn on a face or ground plane.
- **Circle / Polygon** (P1) — N-sided, tessellated.
- **Arc** (P1) — 2-point + bulge, and 3-point.
- **Freehand / Bézier curve** (P2) — cubic Bézier, tessellated to a polyline.

### 5.3 Modify tools (P0/P1)
- **Push/Pull** (P0) — extrude a planar face along its normal; the headline feature.
- **Move** (P1) — move vertices/edges/faces/selection with inference.
- **Rotate** (P1) — rotate selection about an axis.
- **Offset** (P2) — offset a face's edge loop inward/outward.
- **Eraser** (P1) — delete edges/faces.
- **Follow Me** (P2, stretch) — sweep a profile along a path (SketchUp's lathe/extrude).

### 5.4 Inference / precision (P1)
- Endpoint, midpoint, on-edge, on-face, center snaps.
- Axis inference (snap to red/green/blue X/Y/Z) with on-axis color feedback.
- Parallel / perpendicular / from-point inference.
- **Typed length** — type a number to set the exact length/dimension of the current op.
- Length/angle locking (arrow keys to lock an axis, SketchUp-style).

### 5.5 Selection & editing (P1)
- Click select, box select (window/crossing), add/remove with Ctrl/Shift.
- Hover highlight of edge/face under cursor.
- Soft selection of connected geometry; double-click face selects bounded face.

### 5.6 Display / view (P1/P2)
- Render styles: wireframe, hidden-line, shaded, shaded+edges.
- Flat per-face shading from a single directional "sun" light (no textures).
- Back-face shading distinct from front-face (SketchUp blue/white).
- Ground plane + 3-axis tripod + adaptive grid.
- Section planes (P2, stretch).

### 5.7 App-level (P1)
- Undo/redo (command pattern, unbounded within session).
- Save/Load native scene format (JSON via `System.Text.Json`).
- Measurements / dimensions overlay (P2).
- Status bar (current tool, inference hint, typed-length VCB box).

---

## 6. Architecture (proposed, pending spikes)

```
ThreeDee/
├─ Program.cs, MainForm                 // shell, menu, toolbar, status bar, VCB
├─ Math/                                // thin helpers over System.Numerics
│   └─ Ray, Plane, Transform helpers, Tessellation (arc/bezier/circle)
├─ Geometry/                            // mesh topology (F3), boolean-ish edits
│   └─ Mesh, Vertex, HalfEdge/Edge, Face, extrude/split/merge ops
├─ Scene/
│   └─ Scene, Entity, Selection, BoundingBox
├─ Rendering/                           // backend chosen by F1
│   └─ Camera, IRenderer, SoftwareRasterizer (or WpfViewportRenderer), Shading, Picker (F4)
├─ Tools/                               // one state machine per tool
│   └─ ITool, ToolContext, LineTool, RectTool, PushPullTool, OrbitTool, ...
├─ Inference/                           // snapping engine (F-independent)
│   └─ InferenceEngine, InferenceResult
└─ Commands/                            // undo/redo command pattern
    └─ ICommand, History
```

Rendering is behind an `IRenderer` interface so F1's outcome is swappable and the rest of
the app is backend-agnostic.

---

## 7. Performance targets (acceptance for the render spike)

- ≥ 30 FPS interactive orbit at **2,000 triangles** on the chosen backend.
- ≥ 15 FPS at **10,000 triangles** (degraded-but-usable) for the "extensive" demo scenes.
- Picking response < 16 ms for a 10k-triangle scene.

(If the software rasterizer cannot hit these, F1 falls back to hosted WPF `Viewport3D`.)

---

## 8. Stretch goals
- OBJ / STL export.
- Components/instances (reused geometry).
- Follow-Me & Offset tools.
- Section planes and scenes/animation.

---

## 9. Spike plan (de-risking — run by the investigation team)

Each spike is a small throwaway program under `spikes/<name>/` that **builds and runs**,
prints measured results, and ends with a PASS/FAIL verdict and a recommendation.

| Spike | Question it answers | Success metric |
|-------|--------------------|----------------|
| **S1 render-gdi** | Can a hand-written GDI+ software rasterizer hit the §7 FPS targets? | FPS at 2k & 10k tris, with Z-buffer vs painter's (F2) |
| **S2 render-wpf** | Does hosted WPF `Viewport3D` (no 3rd-party) clear §7 easily, and at what integration cost? | FPS + a list of WinForms-interop gotchas |
| **S3 camera-math** | Orbit/pan/zoom + perspective projection + screen→world **unproject** using `System.Numerics` | Round-trip project/unproject error < 1e-3; orbit has no gimbal issues |
| **S4 picking** | Ray–triangle (Möller–Trumbore) and ray–segment picking for faces/edges (F4) | Correct hit + barycentric/param on known cases |
| **S5 mesh-pushpull** | A mesh topology (F3) that supports extruding a face along its normal and stitching side walls | Extrude a quad → closed solid with correct face count & normals |
| **S6 inference** | Snapping: endpoint/midpoint/on-edge/on-face + axis lock from a cursor ray | Correct snap classification on a fixture scene |
| **S7 curves** | Arc (3-pt & bulge) and cubic Bézier tessellation to 3D polylines | Tessellation within tolerance of analytic curve |

**Gating:** S1 and S2 results decide F1 before heavy engine work begins.

---

## 10. Open questions
- Default mouse mapping: SketchUp (orbit on MMB) vs CAD conventions? → propose SketchUp.
- Single global mesh vs per-entity meshes for v1? → propose single scene mesh.
- Ortho vs perspective default? → propose perspective with easy toggle.

---

## 11. Follow-Me revolution & curve selection (enhancements)

Two spikes (`spikes/followme-revolve`, `spikes/curve-select`) investigated defects/gaps in
the shipped M9 Follow-Me and M3 selection features. Both returned **PASS** with proven,
drop-in algorithms. This section captures the corrected behaviour as implementation-ready
requirements. It refines §5.3 (Follow Me) and §5.5 (Selection); it does not change the
fork decisions or module layout in ARCHITECTURE.md.

### 11.1 Follow-Me: preserve profile offset / revolve about a planar path axis

**Problem being fixed.** `FollowMeCommand.Do()` recenters every cross-section ring onto its
path station, so it cannot revolve a profile that is radially offset from the path. The
profile is encoded relative to **its own centroid** (`Vector3 c = profile.Centroid()`, then
`a[j]=Dot(prof[j]-c,u0); b[j]=Dot(prof[j]-c,v0)`) and each ring is placed at
`pts[i] + a[j]*u[i] + b[j]*v[i]`. The path-point→centroid offset (the revolution radius R) is
discarded, so a semicircle profile offset from a circular path collapses onto the path circle
— producing a thin torus-like tube, not a sphere. (Despite the parallel-transport framing,
only the in-plane `u,v` basis is transported and the radial offset is dropped.) Spike measured
the current command at sphere-fit **Rfit=1.273, maxRelErr=15.85%** — not a sphere.

**R11.1-1 — Profile offset must be preserved (moving-frame sweep, baseline).**
Replace the centroid-relative `(a,b)` encoding + `pts[i]+a*u+b*v` placement with a full
moving-frame (parallel-transport) sweep that carries each profile vertex's START-frame local
coordinates, including the tangential and radial offset:
1. Build a START frame at `pts[0]`: `T0 = Tangent(0)`, and `(U0,V0)` = the profile-plane basis
   `(u0,v0)` rotated by `ShortestArc(profileNormal, T0)`.
2. For each profile vertex `p_j`, store three locals against the full frame:
   `l1 = Dot(p_j-pts[0], T0)`, `l2 = Dot(p_j-pts[0], U0)`, `l3 = Dot(p_j-pts[0], V0)`.
3. Parallel-transport the **whole** frame `(T,U,V)` station-to-station with the existing
   `ShortestArc(Tangent(i-1), Tangent(i))` quaternions (currently only `u,v` are transported;
   `T` must be transported too).
4. Place each vertex at `pts[i] + l1*T[i] + l2*U[i] + l3*V[i]`.

This is spike candidate **B (MOVING-FRAME SWEEP)** and is the minimal correct fix. It
generalizes to open and non-planar paths (the spike's non-planar helix probe ran fine, 1617
verts), where the revolution branch below is undefined.

**R11.1-2 — Exact revolution for planar+closed paths (refinement).**
When the path is planar **and** closed, snap to exact surface-of-revolution
(spike candidate **C**) for a numerically perfect lathe/sphere: `Center = centroid(path)`,
`axis = Newell-normal(path)`, `angle_i =` signed angle about `axis` from `(P0-Center)` to
`(P_i-Center)` (`atan2(r_i·r0perp, r_i·r0n)`), place
`q_j(i) = Center + Rot(axis, angle_i)·(p_j - Center)`. Production rule: **if path is planar and
closed → revolve (C); else moving-frame sweep (B).**

**Acceptance criteria (from spike, bar = 1e-2 sphericity).**
Fixture: 64-gon circular path (R=1) + 33-point semicircle profile (R=1), ~2112 surface verts,
algebraic sphere fit, metric = `max|dist-to-fitted-center − Rfit| / Rfit`.
- Current command: Rfit=1.273, maxRelErr=**1.585E-1** → FAIL (must no longer ship).
- After fix (B sweep): Rfit=1.0000, maxRelErr=**3.58E-7** → PASS.
- After fix (C revolve): Rfit=1.0000, maxRelErr=**1.79E-7** → PASS.
Acceptance: the semicircle+circle fixture must produce a sphere with **maxRelErr ≤ 1e-2**
(both proven paths clear this by ~5 orders of magnitude). B is sufficient on its own; C is an
optional accuracy/robustness upgrade for the common planar lathe case.

**Scope / deferred.** No data-model or serialization change. C is only defined for
planar+closed paths (planarity residual must be small; the helix probe at residual 6.42E-1 is
correctly routed to B). Capping/twin logic and the open-vs-closed cap handling in
`FollowMeCommand.Do()` are unchanged.

### 11.2 Whole-curve / connected-edge-path selection

**Problem being fixed.** An arc/circle/bezier is committed as N independent wire edges with no
grouping (`ArcTool` tessellates to 32 segments → `AddPolylineCommand`, which welds points via
`Mesh.FindOrAddVertex` and emits each segment as a separate `Mesh.Wires` entry). Selection is a
flat `HashSet<EdgeKey>`; a click selects one segment and there is no notion of "the whole arc".
`SelectTool.OnDoubleClick` is currently **face-only** and ignores edges. The connectivity needed
to recover a curve already exists implicitly: welded segments share the **same Vertex by
reference**, and `EdgeKey` equality is by reference identity — so traversal is possible with no
data-model change.

**R11.2-1 — Connected-edge flood-fill (spike approach (a)).**
Add `Mesh.ConnectedEdgePath(EdgeKey seed) -> IEnumerable<EdgeKey>` next to `UniqueEdges()`:
build a `Dictionary<Vertex, List<EdgeKey>>` adjacency map from `UniqueEdges()` (which already
yields deduped pairs over both half-edges **and** wires), then iterative-stack flood from the
seed (edge → shared vertex → incident edges), collecting every reachable `EdgeKey`. Because
welded vertices are shared by reference and `EdgeKey` equality is by reference, traversal
terminates exactly at the curve's connectivity boundary. This is the spike's verbatim
`ConnectedWirePath(mesh, seed)` algorithm.

**R11.2-2 — Double-click selects the whole connected path.**
In `SelectTool.OnDoubleClick`, before the existing face path, do an edge pick: if an edge is
hit, clear the selection unless Shift/Ctrl is held, then
`foreach (var e in ctx.Scene.Mesh.ConnectedEdgePath(hitEdge)) ctx.Selection.AddEdge(e);`,
invalidate and return; otherwise fall through to the existing face-loop behaviour. No change to
`Selection.cs` is required (it already stores `HashSet<EdgeKey>` and prunes by vertex
membership).

**R11.2-3 — Optional junction cap.**
To stop the flood bleeding across a junction where a curve meets a face or another curve,
optionally cap traversal at vertices with incident degree > 2 (treat valence-2 chains as one
curve, as SketchUp does). The spike's unrelated-geometry case already confirms separate
components never merge even without the cap.

**Acceptance criteria (from spike, exit 0, all asserts pass).**
Fixture: 32-segment arc (33 welded points, 32 wire edges) + 9 unrelated wire edges = 41 wires;
`UniqueEdges()` = 41.
- Flood from arc segment #15 → **32 edges**, exactly the arc set, **0** unrelated edges grabbed.
- Flood from arc endpoint #0 → **32 edges** (open-curve endpoints recover the full chain).
- Flood from a lone edge → **1 edge** (isolated edges and closed loops handled).

**Interaction with Follow-Me (positive, zero-cost).** `FollowMeTool.OnMouseDown` calls
`OrderPath(ctx.Selection)`, which reorders `Selection.Edges` into a polyline and tolerates
arbitrary edge order. Double-click flood-fill simply fills `Selection.Edges` with the full
chain in one gesture, so the user can double-click the whole tessellated path then click the
profile, instead of shift-clicking 32 segments. `OrderPath` consumes the flood-filled set
unchanged.

**Scope / deferred.** Adopt approach (a) for v1. Defer approach (b) — persistent curve grouping
(a `CurveId` on wires + Selection group expansion), the eventual SketchUp-faithful model that
survives across faces and enables curve-aware ops — because it requires touching
`Mesh.Wires` storage, `AddPolylineCommand`, `SceneIO` persistence, and `Selection`. Approach (a)
delivers the UX with one self-contained method and no data-model/serialization migration.
