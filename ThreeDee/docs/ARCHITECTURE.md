# ThreeDee — Architecture & Spike Synthesis

**Status:** Decided v1.0
**Date:** 2026-06-03
**Last updated:** 2026-08-06 — added §1 "Rotation representation"; M9 row corrected (Follow-Me shipped); assertion count 109 → 114.
**Owner:** pieterjan@2sky.be
**Supersedes:** PRD §3 forks (now resolved), refines PRD §6 (modules) and §10 (open questions).
**Companion documents:** [PRD.md](PRD.md) (requirements) · [quaternions.md](quaternions.md) (rotation theory reference)

This document resolves the four technical forks from PRD §3 using the **measured** results of spikes S1–S7, fixes the final module/namespace breakdown with the `IRenderer` abstraction, lays out a milestone roadmap that maps every PRD feature to a shippable increment, and answers the PRD §10 open questions.

---

## 1. Fork resolutions (decided)

### F1 — Render backend: **Hand-written software rasterizer (GDI+ `Bitmap`)** ✅

**Decision: F1(a), the GDI+ software rasterizer. WPF `Viewport3D` (F1(b)) is kept only as a documented, un-built fallback.**

Rationale, citing measured numbers:

- **S1 clears PRD §7 with 4–8× headroom, single-threaded.** Z-buffer + LockBits path: **118.5 FPS @ 9,600 tris** (8.44 ms) and **140.2 FPS @ 1,760 tris** (7.13 ms), in Release at 1280×720. PRD §7 demands ≥30 FPS @ 2k and ≥15 FPS @ 10k. We beat the 10k target by ~8× and the 2k target by ~4×. The render hot loop parallelizes trivially if we ever want more.
- **S2 cannot even confirm WPF on its merits.** The WPF spike could only measure `RenderTargetBitmap`, which forces WPF's *software* 3D path plus a heavy GPU→CPU readback: **~8–12 FPS flat** across a 5× triangle increase (2k: 84–102 ms/frame; 10k: 88–122 ms/frame). Per-frame time is dominated by fixed composite+readback cost, not triangle throughput — so these numbers neither confirm nor refute on-screen GPU FPS (hence S2's PARTIAL verdict). Adopting WPF would have required a *further* on-screen ElementHost spike before committing.
- **Teachability is the decisive tie-breaker** (the stated project goal, PRD §1). WPF makes the renderer a black box and deletes exactly what the project exists to teach: the rasterizer, the **entire F2 Z-buffer decision**, projection/clipping, and per-face flat shading. S2 also enumerated real integration tax: ElementHost airspace (all 2D HUD must move into WPF Visuals), DPI unit mismatch (DIP vs physical px), routed-event/keyboard-focus interop, and per-vertex Gouraud lighting that needs vertex duplication to fake flat shading.
- **Zero third-party dependency** is preserved either way, but the software path also keeps us off the WPF interop surface entirely.

**Hard build requirements (from S1):** target `net10.0-windows` with `UseWindowsForms=true` (System.Drawing/GDI+ is Windows-only on .NET 10; plain `net10.0` won't resolve `Bitmap`/`Graphics`, and **no NuGet is needed**). Enable `AllowUnsafeBlocks` for the LockBits blit.

### F2 — Hidden-surface removal: **Per-pixel Z-buffer (+ LockBits)** ✅

**Decision: per-pixel Z-buffer with a LockBits framebuffer. Painter's algorithm is rejected.**

Rationale, citing measured numbers:

- **Painter's only wins where it doesn't matter and loses where it does.** Painter's (sort + `Graphics.FillPolygon`) is faster at low tri counts — **347.6 FPS @ 1,760** vs Z-buffer's 140.2 — but that gap is pure overdraw/GDI+ acceleration and **vanishes by 10k**: painter's 123.9 FPS @ 9,600 vs Z-buffer's 118.5 FPS, a ~4% delta. Both clear PRD §7 comfortably, so the speed argument is a wash exactly where scenes get big.
- **Painter's average-depth sort is provably wrong** for the interpenetrating and large-overlapping-small geometry that a push/pull modeler produces every session. Z-buffer is per-pixel correct (S1 `sample_zbuf_9600.png` confirms correct occlusion, flat directional shading, and back-face culling).
- **LockBits gives the pixel-level control later features need:** color-ID picking (if ever wanted), hidden-line / wireframe styles (PRD §5.6), and back-face tint (SketchUp blue/white). Painter's with AA also leaves visible seams between adjacent triangles (AA had to be disabled for a fair comparison).

**Implementation notes (from S1):** the blit must copy **row-by-row using `BitmapData.Stride`** (do not assume `stride == width*4`). Cull sign is subtle — `CreateLookAt` is right-handed and the viewport Y-flip inverts screen winding, so a front-facing CCW world triangle yields **negative** signed screen area; cull rule is `area >= 0 discards back faces`, and mesh winding (S5) must match. Guard `clip.W <= 0` (behind camera) before the perspective divide.

### F3 — Mesh topology: **Half-edge** ✅

**Decision: a compact half-edge mesh (`Vertex` / `HalfEdge` / `Face`). Winged-edge and face-vertex lists are rejected.**

Rationale, citing measured results:

- **S5 proves half-edge makes push/pull correct and O(1).** Walking a face loop (`h.Next`) and crossing an edge to the neighbour (`h.Twin.Face`) are O(1), which maps directly onto extrude: walk the source boundary loop in order, emit one consistently-wound side quad per edge. Verified results: quad→box gives **V=8, E=12, F=6, Euler=2, 0 boundary half-edges, 0 untwinned half-edges, 0 bad normals**; triangle→prism gives **V=6, E=9, F=5, Euler=2**, same clean invariants. All 14 assertions PASS.
- **The alternatives are winding-bug magnets.** Winged-edge forces a cw/ccw direction test on every hop; face-vertex lists carry no adjacency, so every edit rebuilds edge→face maps. Half-edge also directly enables the **edge-dissolve (coplanar merge)** and **face-split (draw edge on face)** primitives we need later.
- **The critical invariant is captured:** during extrude the **source/base face must be flipped in place** — its original normal points *into* the resulting solid. The first S5 run failed with exactly 2 inverted cap normals + 16 untwinned half-edges until the flip was added. Topology *counts* alone don't catch this; the outward-normal and manifold checks do — so both must be permanent unit tests in `Geometry/`.

**Scaling already designed (S5):** faces-with-holes = one outer loop + N inner loops wound the opposite sense (Newell's normal handles multi-loop unchanged); coplanar merge = edge dissolve (needs a `Prev` accessor + non-manifold guard); drawing an edge on a face = face split. Note the convex-only outward-normal test `dot(faceNormal, centroid-meshCenter) > 0` must be replaced by local edge-orientation consistency + one global sign (signed volume / ray test) once concave (L-shaped) pulls land.

### F4 — Picking: **CPU ray-cast (Möller–Trumbore + ray–segment)** ✅

**Decision: CPU ray-cast picking. Color-ID buffer is rejected.**

Rationale, citing measured numbers:

- **S4 is ~100× under budget with brute force.** Nearest-of-10,000-triangles pick measured at **0.150 ms/pick** (~6,650 picks/sec) in Release — vs the 16 ms PRD §7 picking budget. Linear scaling puts the 16 ms threshold near **~1M triangles**, so **no BVH/grid is needed for v1**; defer spatial acceleration until scenes exceed ~100k–200k tris or picking runs many times per frame. 13/13 assertions PASS.
- **Color-ID structurally cannot do what the modeler needs.** Möller–Trumbore returns exact **barycentric** hit points (needed for on-face inference, S6), and ray–segment closest-approach returns analytic **edge distance** (needed for edge/midpoint hover and snap). A color-ID buffer gives neither — it identifies a face but not *where* on it or *which edge/vertex* is near.
- It consumes the S3 unproject ray directly and is **backend-agnostic** — it does not depend on F1, so the same picker works no matter the renderer.

**Implementation notes (from S4):** back-face culling for picking must default **OFF** (you must click faces seen from behind) but stay a flag. Edge-pick tolerance must be a **screen-space pixel radius back-projected to world units at the hit depth** via the S3 camera — not a fixed world radius. Keep the model near origin (epsilon is world-space 1e-7) or switch to a relative epsilon. `NearestTriangle` needs a face-id tie-break for coincident faces.

### Foundational, non-fork spikes (adopted verbatim)

- **S3 camera math — adopt as-is.** Project→unproject round-trip max error **1.776e-4** world units (tol 1e-3); orbit over 13,067 orientations had **no NaN, all matrices invertible**, no gimbal lock; zoom-to-cursor drift **7.6e-5 px**. Single precision is ample. Carry over the isolated `NdcToScreen`/`ScreenToNdc` Y-flip helpers, `SetCameraFromEyeTarget` inverse, ±89° elevation clamp, and remember `CreatePerspectiveFieldOfView` uses **D3D depth [0,1]** (unproject near at z=0, far at z=1).
- **S6 inference — adopt as-is.** 7/7 PASS; priority-ordered short-circuit engine at **~1.7 µs/call**. F-independent. Feed it pixel-radius tolerances unprojected at candidate depth (S3) so far geometry doesn't snap too eagerly. AxisLock projects onto the axis line, so downstream typed-length must read `InferenceResult.WorldPoint`, not re-derive from the ray.
- **S7 curves — adopt as-is.** 9/9 PASS; adaptive Bézier flattening (worst chord dev 6.29e-4 @ tol 1e-3), arc radius error ~1e-6, ~4.2 µs/flatten. One unified arc tuple `(center, radius, u, v, startAngle, sweep)` drives 3-point arc, bulge arc, full circle, and N-gon; store the tuple and re-tessellate on zoom against a back-projected pixel tolerance.

### Rotation representation — deliberately not uniform

There is no single rotation type used throughout. Each site picks the representation that is
actually right for it, and the differences are load-bearing rather than historical:

| Site | Representation | Why |
|------|----------------|-----|
| `Camera` orbit | spherical `(azimuth, elevation, distance)` + `Up = UnitY` | **Structurally forbids roll**, which is what a CAD orbit camera needs. A freely-orientable camera would make roll a free parameter that must be continually projected back out. It is also exactly what the UI edits and what `SceneIO` serializes. The ±89° elevation clamp (S3) is the cost, and it is cheap. |
| `Camera` view/projection | `Matrix4x4` | What the projection pipeline consumes; `Vector4.Transform` per vertex. |
| `RotateCommand` / `RotateTool` | `Matrix4x4.CreateFromAxisAngle` | One rotation applied to **many** vertices. A matrix-vector multiply is fewer flops per vertex than a quaternion sandwich product, and the build cost amortizes across the selection. Axis-angle input means there is no gimbal lock to avoid here — that is a defect of *Euler-angle* parameterizations, not of matrices. |
| `Tessellation` circles/arcs | `cos`/`sin` against an orthonormal `(u,v)` basis | Every point is computed independently, so nothing accumulates. Incrementally rotating a start vector would drift and be slower. |
| `FollowMeCommand` sweep | `Quaternion` | The one place rotations are **composed and accumulated** along a path. Renormalizing a single quaternion restores an exactly-orthonormal frame; three drifted basis vectors cannot be fixed as cheaply. See PRD §11.1 (R11.1-1, R11.1-3). |

The full reasoning, including which rotation edge cases a representation change genuinely removes
and which are topologically forced to stay, is in **[quaternions.md](quaternions.md)**.

---

## 2. Final module / namespace breakdown (refines PRD §6)

Root namespace `ThreeDee`. Backend is behind `IRenderer` so F1 stays swappable and everything above `Rendering/` is backend-agnostic.

```
ThreeDee/                               // net10.0-windows; UseWindowsForms=true; AllowUnsafeBlocks=true
├─ Program.cs                           // top-level entry (must precede type decls — keep types in other files)
├─ App/
│   ├─ MainForm                         // single window: menu, toolbar, status bar, VCB box
│   ├─ ViewportControl                  // owns the IRenderer target Bitmap, routes mouse/keyboard to ToolContext
│   └─ Hud                              // 2D overlay: axis tripod, inference glyphs, rubber-band, dimensions
│
├─ Math/                                // thin layer over System.Numerics (BCL only)
│   ├─ Ray, Plane
│   ├─ ProjectMath                      // NdcToScreen/ScreenToNdc (Y-flip), project, unproject  [S3]
│   └─ Tessellation                     // Bezier adaptive flatten, Arc (3-pt/bulge), Circle, N-gon  [S7]
│
├─ Geometry/                            // HALF-EDGE topology (F3)  [S5]
│   ├─ Mesh, Vertex, HalfEdge, Face
│   ├─ MeshBuilder                      // build face/loop, twinning by vertex identity
│   └─ Ops/                             // Extrude(face,dist), EdgeDissolve (coplanar merge), FaceSplit
│
├─ Scene/
│   ├─ Scene                           // single global mesh + entity list (see §4 open Q)
│   ├─ Entity (Edge | Face | Group)
│   ├─ Selection
│   └─ BoundingBox
│
├─ Rendering/                           // F1 = software rasterizer; WPF fallback un-built
│   ├─ Camera                          // OrbitCamera: az/el/distance about Target, ±89° clamp  [S3]
│   ├─ IRenderer                        // the swap point (see §3)
│   ├─ SoftwareRasterizer : IRenderer   // GDI+ Bitmap + LockBits + per-pixel Z-buffer (F1a/F2)  [S1]
│   │   (— WpfViewportRenderer : IRenderer — documented fallback, not built —)
│   ├─ Shading                          // flat per-face directional "sun"; front/back-face tint
│   └─ Picker                           // Möller–Trumbore + ray–segment, cull flag OFF (F4)  [S4]
│
├─ Inference/                           // F-independent snapping  [S6]
│   ├─ InferenceEngine                  // priority-ordered short-circuit scan
│   └─ InferenceResult { Type, WorldPoint, Reference }
│
├─ Tools/                               // one state machine per tool; exactly one active
│   ├─ ITool, ToolContext               // ctx exposes Scene, Camera, Picker, Inference, History
│   ├─ OrbitTool, PanTool, ZoomTool
│   ├─ LineTool, RectTool, CircleTool, ArcTool, BezierTool
│   ├─ PushPullTool, MoveTool, RotateTool, OffsetTool, EraserTool
│   └─ SelectTool
│
└─ Commands/                            // undo/redo command pattern
    ├─ ICommand, History
    └─ (one ICommand per mutating tool action)
```

### 3. The `IRenderer` abstraction

`IRenderer` isolates the F1 outcome. The rasterizer writes into a GDI+ `Bitmap` exposed to `ViewportControl`; a WPF backend (fallback) would implement the same surface contract.

```csharp
public interface IRenderer
{
    // Resize backing framebuffer + Z-buffer (LockBits-friendly, stride-aware).
    void Resize(int widthPx, int heightPx);

    // Clear color + depth, then rasterize the scene from the camera.
    // Returns the frame the ViewportControl blits (Bitmap for the software path).
    Bitmap RenderFrame(Scene scene, Camera camera, RenderStyle style);

    // Renderer-owned options that touch the pixel pipeline.
    RenderStyle Style { get; set; }     // Wireframe | HiddenLine | Shaded | ShadedEdges
    Vector3 SunDirection { get; set; }  // flat per-face directional light
}
```

Picking is intentionally **outside** `IRenderer` (it is CPU ray-cast, F4) so it is identical across backends and never depends on a color-ID framebuffer. The camera (S3) is likewise backend-agnostic and shared by both renderer and picker.

---

## 4. Implementation roadmap (ordered, each milestone shippable)

Each milestone builds, runs, and demos something. PRD feature IDs in brackets.

| Milestone | Goal (shippable) | PRD features delivered |
|-----------|------------------|------------------------|
| **M1 — Viewport + Camera** ✅ done | WinForms window hosting a `ViewportControl`; `SoftwareRasterizer` (Z-buffer + LockBits) draws hard-coded test meshes behind `IRenderer`; `Camera` with orbit/pan/zoom-to-cursor, Zoom Extents, standard views, perspective/ortho toggle; all 4 render styles (Wireframe/HiddenLine/Shaded/ShadedEdges) + grid + axis tripod. **Proved F1+F2+S3 on screen.** | §5.1 (all), §5.6 shaded + flat sun shading + all styles |
| **M2 — Half-edge geometry + Scene** ✅ done | `Geometry/` half-edge mesh (Vertex/HalfEdge/Face/Mesh/MeshBuilder) + validated Extrude kernel; `Scene` single global mesh emitting one RenderMesh/face; renderer draws the real `Scene`; back-face tint on single-sided faces; hand-rolled regression harness in `tests/` (24 asserts: Euler/manifold/normals + extrude box/prism + scene emission). | §5.6 render styles + back-face tint, ground plane + tripod + grid |
| **M3 — Picking + Selection** ✅ done | `Picker` (Möller–Trumbore + ray–segment, lifted from S4) on the S3 unproject ray; edge-vs-face priority with back-projected pixel tolerance; `Selection` (faces+edges); hover highlight, LMB click-select, Shift/Ctrl toggle, drag window/crossing box-select, double-click face+edges, Esc/Ctrl+A; GDI+ `SelectionOverlay` (translucent fills via alpha). Known limit: overlay not yet depth-tested. | §5.5 (all), picking < 16 ms (S4: 0.15 ms) |
| **M4 — Line + Rectangle + Commands** ✅ done | `Commands/` undo/redo (`ICommand`+`History`, `CreateFaceCommand`); `Tools/` framework (`ITool`/`ToolBase`/`ToolContext` with vertex-snap + face/ground-plane inference); `SelectTool` (M3 logic refactored in), `LineTool` (chained points → closed face), `RectangleTool`; navigation stays tool-independent; menus reorganized (Edit/Draw) + bare-key tool switching (Space/L/R). | §5.2 Line+Rect (P0), §5.7 undo/redo |
| **M5 — Push/Pull (headline)** ✅ done | `Mesh.Extrude`/`UnExtrude` (undoable) + `PushPullCommand` with two paths: extrude a free face into a solid, or stretch a solid's face by translating shared vertices; `PushPullTool` (hover highlight, click→move→click, cursor projected onto pull axis, live preview). Selection pruned on undo/redo. Numeric distance entry deferred to M6 (VCB). | §5.3 Push/Pull (P0) |
| **M6 — Inference + VCB** ✅ done | `Inference/InferenceEngine` (endpoint/midpoint/on-edge/on-face + axis lock, lifted from S6) feeding Line/Rectangle; back-projected pixel tolerance; snap indicators + red/green/blue on-axis rubber band; `ToolStripTextBox` VCB with type-to-start + live measurement, driving Line length, Rectangle w;d, Push/Pull distance. Parallel/perp + intersection snaps deferred. | §5.4 (most), §5.7 status bar/VCB |
| **M7 — Curves + Move/Rotate/Eraser** ✅ done | `Math/Tessellation` (S7: adaptive Bézier, 3-point arc, circle/N-gon); `CircleTool` (→ N-gon face), `ArcTool` (3-point → wire), `BezierTool` (4-pt → wire); `MoveTool`/`RotateTool` (on selection, with VCB length/angle), `EraserTool` + Delete key; `EditCommands` (Move/Rotate/Delete/AddPolyline, all undoable). | §5.2 Circle/Polygon/Arc/Bézier, §5.3 Move/Rotate/Eraser |
| **M8 — Save/Load** ✅ done | `Persistence/SceneIO` — native `.3dee` JSON (System.Text.Json): vertices + faces (index loops + color) + wires + camera; `Mesh.Clear`/`History.Clear`; File menu (New/Open/Save/Save As, Ctrl+N/O/S) with dialogs + title tracking. | §5.7 save/load |
| **M9 — Polish & stretch** ✅ done (subset) | `MeshExport` Wavefront **OBJ** + ASCII **STL** (invariant decimals); `TapeMeasureTool` (distance readout + dimension overlay); **Follow-Me** (`FollowMeCommand`/`FollowMeTool`, later corrected per PRD §11.1). Offset, section planes, and components/instances remain documented future work. | §8 export (done), §5.3 Follow-Me (done), §5.3 Offset + §5.6 sections (deferred) |

Critical-path ordering rationale: M1 ships the gated F1/F2/S3 result on screen first; M2–M3 lay the topology + picking spine; M4 introduces commands *before* mutation tools so undo exists from the first edit; M5 (push/pull) is intentionally early as the headline; inference (M6) follows because it most improves *existing* drawing/edit tools.

**Status (all milestones M1–M9 complete, plus follow-up features).** The app is a working SketchUp-style modeler: orbit/pan/zoom + standard views/projection; Line/Rectangle/Circle/Arc/Bézier drawing with inference snapping + a VCB for exact dimensions; **selectable construction plane (XZ/XY/YZ)** so shapes can be drawn on vertical planes; **auto-fill of closed planar wire loops** (draw an arc, close it with a line → face); Push/Pull; **Follow-Me** (sweep a profile along a path via parallel-transport frames, or an exact surface of revolution when the path is planar and closed — see PRD §11.1); Move/Rotate/Eraser; selection + undo/redo; 4 render styles; native `.3dee` save/load; OBJ/STL export; tape measure. Hand-rolled BCL-only regression harness (`tests/ThreeDee.Tests`, 114 assertions). Deferred for the future: face-split when drawing *across* an existing face, coplanar-edge dissolve/merge, Offset, section planes, components/instances, depth-tested selection highlight, and textures (an explicit v1 non-goal).

---

## 5. PRD §10 open questions — answers

1. **Default mouse mapping: SketchUp vs CAD?** → **SketchUp.** Orbit on MMB-drag, Pan on Shift+MMB, Zoom on wheel with **zoom-to-cursor** (S3 confirmed zoom-to-cursor at 7.6e-5 px drift — the SketchUp-feel behaviour is *measured to work* with our camera, and the correct algorithm is "move eye toward picked point **and** translate Target by the same delta", not a pure dolly). Matches the target audience's mental model. Make it a remappable table, not hard-coded.

2. **Single global mesh vs per-entity meshes for v1?** → **Single global scene mesh.** The half-edge mesh (F3/S5) *requires* shared vertex/edge topology for coplanar merge (edge dissolve) and for push/pull to stitch into neighbours; per-entity meshes would re-introduce the edge→face rebuild that made face-vertex lists lose the F3 fork. Entities (Edge/Face/Group) become *views/handles* into the one mesh. Defer multi-mesh until components/instances (M9 stretch).

3. **Ortho vs perspective default?** → **Perspective, with an easy toggle** (matches SketchUp). S3 validated perspective project/unproject and explicitly flagged the D3D `[0,1]` depth range, so perspective is the de-risked default; ortho is a one-matrix swap (`CreateOrthographic*`) on the same camera and exposed as a hotkey toggle per §5.1.

---

## 6. Spike scorecard

| Spike | Verdict | Key measured number |
|-------|---------|---------------------|
| **S1 render-gdi** (F1a/F2) | PASS | Z-buffer **118.5 FPS @ 9,600 tris** (8.44 ms) — ~8× over §7's 15 FPS @ 10k |
| **S2 render-wpf** (F1b) | PARTIAL | Software-path lower bound only: **~8–12 FPS** flat across 2k→10k; cannot confirm on-screen GPU FPS |
| **S3 camera-math** | PASS | Project→unproject round-trip max error **1.776e-4** (tol 1e-3); 13,067 orbits, no NaN/gimbal |
| **S4 picking** (F4) | PASS | **0.150 ms/pick** over 10k tris (~100× under 16 ms budget); 13/13 asserts |
| **S5 mesh-pushpull** (F3) | PASS | Quad→box **V=8 E=12 F=6, Euler=2**, 0 untwinned, 0 bad normals; 14/14 asserts |
| **S6 inference** | PASS | **~1.7 µs/call**; 7/7 snap-classification asserts |
| **S7 curves** | PASS | Bézier max chord dev **6.29e-4** @ tol 1e-3; arc radius error ~1e-6; 9/9 asserts |

All seven spikes build and run. The two gating spikes (S1/S2) resolve F1 in favour of the software rasterizer; the remaining five de-risk every downstream layer.
