# Testability, Code Coverage & .NET 11 — Product Requirements Document

**Make this repository testable, report coverage to `coverage.mintplayer.com`, and bring the
workspace onto .NET 11 — as one pull request.**

Status: v0.3 — **implemented**. Decision tree resolved 2026-09-18; M0–M7 landed the same day.
Date: 2026-09-18
Owner: pieterjan@2sky.be

> **Implementation changed two decisions.** The M1 spike disproved §3.2's assumption, and the
> exclusion mechanism in §6.1 does not exist on this stack. Both are corrected in place below
> and summarised in §13. The plan is otherwise as executed.

---

## 1. Summary

This started as "add code coverage". Investigation found there is nothing to measure:

> **There is no test framework anywhere in this repository.** A grep across every `*.csproj`,
> `*.props`, `*.targets` and `*.cs` for `xunit|nunit|mstest|Microsoft.NET.Test.Sdk|coverlet|`
> `Microsoft.Testing.Platform|IsTestProject` returns **zero hits**. The `dotnet test` step in all
> four workflows is a **silent no-op that exits 0**, because no project sets `IsTestProject`. CI has
> been reporting a green check for zero executed assertions.

Coverage is therefore the *last* milestone, not the first. The three projects named `*.Test` are not
tests: `ThreeDee.Test` is a 580-line hand-rolled `Program.cs` harness (25 blocks, 115 real
assertions), `QuineMcKluskey.Test` is a `while (true) { … Console.ReadKey(); }` console app with no
assertions at all, and `KarnaughMap.Test` is a `WinExe` calling `Application.Run`.

Coverlet only instruments assemblies **loaded by the test host** — i.e. a test project's
ProjectReference closure. An untested library is therefore *absent* from the report, not 0%. Left
alone, the badge would show ThreeDee's number while six libraries stayed invisible to the metric.
That is the failure this PRD exists to prevent.

Scope grew deliberately during design review and now covers three things in one PR: **testability**
(seams + six test projects), **coverage** (collection + upload + gate config), and **.NET 11**.

---

## 2. Decision log

| # | Decision | Rationale |
|---|---|---|
| ~~**D1**~~ | ~~**coverlet.collector**, `--collect:"XPlat Code Coverage"`~~ → **superseded by §3.2: Microsoft.Testing.Extensions.CodeCoverage**, still cobertura | What every other MintPlayer repo uses and what the upload action expects. Instruments IL at the test host, so `net*-windows` TFMs are irrelevant to it. Rejected `dotnet-coverage` (tool install, clunkier filters) and AltCover (build-time rewriting collides with `dotnet pack --no-build`). |
| **D2** | Test job on **`windows-latest`** | Everything worth covering is `net*-windows`. `EnableWindowsTargeting` compiles those on Ubuntu; it does not provide a runtime. Build/pack/push stay on `ubuntu-latest`. |
| **D5** | Root **`global.json`** pinning `11.0.100-rc.1.26425.128`, `rollForward: latestFeature`, `allowPrerelease: true` | Copied verbatim from `MintPlayer.AspNetCore.Tools`. Without it, local builds silently use a different major than CI — a defect that repo's PR #31 fixed on its own side. |
| **D6** | One **reusable `workflow_call`** workflow | The four workflows are copy-paste clones; every change here lands in all four. The org already shows this drifting: `Dotnet.Tools` installs only the .NET 11 SDK while `AspNetCore.Tools` correctly installs `10.0.x` alongside. A deviation from org practice, accepted. |
| ~~**D7**~~ | ~~Runsettings: `UseSourceLink=false`, no `ExcludeByAttribute`, `DeterministicReport=false`~~ → **moot**; these are coverlet knobs with no equivalent on MTP coverage (§3.2, §6.1a) | The service's documented constraints override generic best practice — see §6.1. |
| **D8** | Every test project sets **`<IsPackable>false</IsPackable>`** explicitly, plus a filename guard in `Directory.Build.props` | Matches the existing convention (all seven non-packable projects do this). `publish-release.yml` pushes `**/*.nupkg` to nuget.org; a packable test project would be published irreversibly. |
| **D9** | **Statics become instance classes behind interfaces. Breaking changes allowed.** | `PlatformBrowser` and `IconExtractor` are `public static class` — nothing can be injected into them. Follows the existing `IQuineMcCluskeySolver` pattern. Major version bumps expected. |
| **D10** | **Upgrade to .NET 11**; packable libraries multi-target, test/demo projects do not | .NET 11 is STS (EOL 2028-11-09); .NET 10 is LTS (EOL 2028-11-14). Single-targeting `net11.0` would *shorten* the packages' supported life and drop every net10.0 consumer. |
| **D11** | **One abstractions package per library** — `MintPlayer.PlatformBrowser.Abstractions`, `.IconUtils.Abstractions`, `.KarnaughMap.Abstractions` | A shared abstractions package can't express "I only want the browser contract". The implementations drag in real weight (Registry + UWP, `System.Drawing.Common`, WinForms). |
| **D12** | **No MinVer yet.** Explicit versions now; a public-API-hash tool will later compute the correct bump and apply the tag | Defers a fifth novel element out of this PR. An API hash *measures* whether the surface broke, which is strictly better than Conventional Commits guessing at it. |
| **D13** | **Fix `MintPlayer.PlatformBrowser.csproj:37`** and delete the private duplicate `ToFormattedString` | Live defect — see §3.1. |
| **D14** | Test projects **single-target .NET 11**; `MintPlayer.PlatformBrowser.Tests` targets **`net11.0-windows10.0.22621`** | The `#if WINDOWS` sits on the *platform* axis, not the framework axis, so the windows leg is a superset of the plain leg's source. Single-targeting also sidesteps the coverage server's per-TFM union, which is untested where denominators differ per TFM. Targeting plain `net11.0` would silently drop the UWP block from the report. |
| **D15** | **xUnit v3** | Deviation from org (which uses xUnit 2.9.3). Accepted for native `[STAFact]` as an escape hatch. **Gated by the M1 spike** — see §3.2. |
| **D16** | **Delete `Microsoft.SourceLink.GitHub 1.1.1`** from `IconUtils` and `PlatformBrowser`; add `PublishRepositoryUrl` + `EmbedUntrackedSources` | SourceLink has shipped in the SDK since .NET 8. But the SDK gives the *mapping*, not the metadata — neither property is set anywhere today, so the repo URL never lands in the nupkg. |
| **D17** | `MintPlayer.ObservableCollection` **7.0.1 → 11.0.0-rc.1**; `System.Drawing.Common` to the 11 line | Four majors behind the org's own package. NU5104 never fires because our packages are themselves `-rc`. **A second round is required after .NET 11 GA (2026-11-10)**, sequenced behind `MintPlayer.ObservableCollection` publishing stable 11.0.0. |
| **D18** | Three build files: **`versions.props`** (data only, tool-owned), **`Directory.Build.props`** (defaults), **`Directory.Build.targets`** (anything conditioned on project-declared properties) | `Directory.Build.props` is imported *before* the project body, so `$(OutputType)` / `$(IsPackable)` are unset there and conditions on them silently never match. |
| **D19** | `ObservableCollection` **leaks into** `MintPlayer.KarnaughMap.Abstractions` for now | Accepted coupling; revisit once the API-hash tool can classify the change. |
| **D20** | **Release** configuration for build, test, pack and publish, matching the org | Shipping Debug-configured assemblies to nuget.org is a defect independent of coverage. `--no-restore` does **not** imply `--no-build`. |
| **D21** | **MVP** for `MintPlayer.KarnaughMap` — `IKarnaughMapView` + presenter + model | Moves toggle cycling, minterm mapping and solver orchestration into a class instantiable without a message loop. |
| **D22** | The view contract is a **render model** — the presenter produces an immutable description of what to draw; `Paint` is a dumb walker | The only shape where tests assert a returned data structure rather than verifying calls on their own mock. Captures the cell-text ladder and the both-ones-and-zeros hatching, which are currently unreachable inside a `PaintEventArgs` handler. |
| **D23** | **Same MVP treatment for `MintPlayer.BrowserDialog`** | `BrowserDialog_Load` is 88 of its 144 lines and is the one place both new seams (`IPlatformBrowser`, `IIconExtractor`) are exercised against a realistic caller. ~60 lines remain uncoverable; leaving it alone would leave all 144. |
| **D24** | **`coverage.yml` with explicit targets, `blocking: false`** | Baseline is unknown and R5's ceilings differ wildly per assembly, so a solution-wide blocking number is the wrong instrument. Targets make the trend visible without blocking a merge. |

### Withdrawn
- **D3** (xUnit v3 pending spike) — superseded by D15; the spike survives as the M1 gate.
- **D4** (a single `Directory.Build.props`) — superseded by D18's three-file split.

---

## 3. Findings that change the work

### 3.1 A live defect in `MintPlayer.PlatformBrowser` (D13)

```xml
<!-- MintPlayer.PlatformBrowser.csproj:37 -->
<Compile Remove="Extensions\PackageVersionExtensions.cs"
         Condition=" $(TargetFramework.StartsWith('net7.0-windows')) == 'false' " />
```

The project targets `net10.0;net10.0-windows10.0.22621`. Neither starts with `net7.0-windows`, so
the condition is true for **every** leg and the file is excluded from **every** build. Its public
`ToFormattedString` / `ToPackageVersion` API ships in no assembly.

Someone then worked around it without noticing: `PlatformBrowser.cs:191` declares a **private
duplicate** `ToFormattedString` inside `#if WINDOWS`. Fix the condition, delete the duplicate. Until
this lands, the 54 lines of `PackageVersionExtensions` are uncoverable — you cannot test code that
is in no assembly.

### 3.2 xUnit v3 vs the coverage collector — SUPERSEDED BY THE M1 SPIKE

This section predicted that xUnit v3 merely *defaults* to Microsoft.Testing.Platform, leaving a
VSTest path available for coverlet. **That is wrong, and the spike proved it:**

```
error : Testing with VSTest target is no longer supported by Microsoft.Testing.Platform
on .NET 10 SDK and later.
```

The VSTest bridge is **removed** on SDK 10+, so there is no "keep the VSTest path" option, and D1
(coverlet.collector) is unreachable while D15 (xUnit v3) stands. What was actually built:

- Coverage comes from **Microsoft.Testing.Extensions.CodeCoverage**, driven by MTP command-line
  arguments (`--coverage --coverage-output-format cobertura`). Still cobertura, so the upload
  action is unaffected.
- There is **no `coverlet.runsettings`**. D7's `UseSourceLink` / `DeterministicReport` /
  `ExcludeByAttribute` guidance describes coverlet knobs that have no equivalent here, and §6.1
  below is superseded by §6.1a.
- `dotnet test` must be opted into the MTP runner, and the opt-in lives in **`global.json`**
  (`"test": { "runner": "Microsoft.Testing.Platform" }`), not in a `dotnet.config`. The SDK target
  that raises the error keys off `_SupportsGlobalJsonTestRunner`.
- xUnit v3 requires `<OutputType>Exe</OutputType>` to be set **explicitly**; it does not infer it.
  D18's symbol-package guard still excludes test projects, but only because we set it.

**The failure mode is silent.** A test host that rejects an option reports `Zero tests ran` with a
handshake failure, not an error naming the option. That is R1 in practice, and it bit twice during
implementation.

### 3.3 `net11.0-windows` is viable — verified locally

A WinForms `UserControl` multi-targeting `net10.0-windows;net11.0-windows;net10.0-windows10.0.22621;net11.0-windows10.0.22621`
builds **0 warnings, 0 errors** on SDK `11.0.100-rc.1.26425.128`, producing all four legs. The only
diagnostic is the informational `NETSDK1057` (preview SDK), which will not trip
`TreatWarningsAsErrors`. `EnableWindowsTargeting` and `UseWindowsForms` carry over unchanged.

This proves **compilation only**. Nothing exercised the WinForms designer, `System.Drawing.Common`,
or the UWP `PackageManager` path on .NET 11 — and **no `net*-windows` project anywhere in the
MintPlayer org has been moved to .NET 11**, so there is no precedent to fall back on.

### 3.4 `Width`/`Height` are assigned inside `KarnaughMap_Paint`

`KarnaughMap.cs:289-290` sets the control's size during a paint pass, which can re-trigger layout
and repaint. The computation `(columnCount + 1) * gridSize + 1 + Font.Height` is pure and moves to
the presenter as a `PreferredSize` calculation under D21.

---

## 4. Current state (verified 2026-09-18)

14 projects, one classic `.sln`. **Absent:** `Directory.Build.props`, `Directory.Build.targets`,
`Directory.Packages.props`, `global.json`, `.runsettings`, root `.editorconfig`. **Present:**
`nuget.config`.

Only five external packages in the entire repo:

```
MintPlayer.ObservableCollection   7.0.1    <- org shipped 11.0.0-rc.1
Microsoft.Win32.Registry          5.0.0
Microsoft.WinForms.Designer.SDK   1.6.0
System.Drawing.Common             8.0.0
Microsoft.SourceLink.GitHub       1.1.1    <- deleted by D16
```

`IncludeSymbols` + `SymbolPackageFormat=snupkg` are already set on all seven packable projects. The
`.snupkg` push already works — `dotnet nuget push "**/*.nupkg"` pushes a matching `.snupkg`
automatically, which is exactly why `PushGithub` passes `--no-symbols` (GPR rejects symbol
packages). **Both lines are correct; do not "fix" them.**

**CI today:** four cloned workflows, all `runs-on: ubuntu-latest`, `setup-dotnet@v4`,
`dotnet-version: 10.0.100`, build **Debug**, `dotnet test --no-restore --verbosity normal` (no-op),
`dotnet pack --no-build --configuration Debug`. No concurrency block, no `permissions` block except
`packages: write` on publish, no coverage of any kind.

---

## 5. Target architecture

### 5.1 Packages after this PR — ten packable projects in six version groups

| Version group | Projects |
|---|---|
| ThreeDee | `MintPlayer.ThreeDee` |
| BrowserDialog | `MintPlayer.BrowserDialog` |
| IconUtils | `MintPlayer.IconUtils` + `.Abstractions` |
| KarnaughMap | `MintPlayer.KarnaughMap` + `.Abstractions` |
| PlatformBrowser | `MintPlayer.PlatformBrowser` + `.Abstractions` |
| QuineMcCluskey | `MintPlayer.QuineMcCluskey` + `.Abstractions` |

`MintPlayer.DeMorgan` is **not** packable (`DeMorgan.csproj:9`) — it is a standalone app and stays
excluded. `MintPlayer.QuineMcCluskey.Abstractions` is three files of interfaces and an enum: **no
executable code**, so it has no sequence points and needs no test project.

### 5.2 Six test projects (D14: all single-target)

| Test project | TFM | Covers |
|---|---|---|
| `MintPlayer.ThreeDee.Tests` | `net11.0-windows` | ported harness — 25 blocks / 115 assertions |
| `MintPlayer.QuineMcCluskey.Tests` | `net11.0` | new; solver |
| `MintPlayer.KarnaughMap.Tests` | `net11.0-windows` | presenter + model + render model |
| `MintPlayer.PlatformBrowser.Tests` | **`net11.0-windows10.0.22621`** | detection logic against a fake source |
| `MintPlayer.IconUtils.Tests` | `net11.0-windows` | extractor logic against a faked Win32 layer |
| `MintPlayer.BrowserDialog.Tests` | `net11.0-windows` | presenter against both fakes |

### 5.3 Seams (D9, D21–D23)

- `PlatformBrowser` — static → `IPlatformBrowser` + instance implementation; the registry/UWP source
  injected so the ~342 lines of parsing become testable.
- `IconExtractor` — static → `IIconExtractor`; the `Kernel32`/`PsApi` P/Invoke layer behind its own
  interface.
- `KarnaughMap` — MVP: `IKarnaughMapView` + presenter + model; the presenter emits an **immutable
  render model** (cells with text + fill style, axis labels, focus rect, preferred size) and `Paint`
  walks it. The render model is contract surface in `.Abstractions`.
- `BrowserDialog` — presenter consuming `IPlatformBrowser` + `IIconExtractor`.

---

## 6. Configuration artifacts

### 6.1 `coverlet.runsettings` (repo root) — NOT BUILT, see §6.1a

> Kept for the reasoning behind each filter, which still explains *what* has to be excluded
> and why. The mechanism does not apply: coverlet cannot attach on this stack (§3.2).

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Format>cobertura</Format>
          <Include>[MintPlayer.*]*</Include>
          <Exclude>[*.Demo]*,[*.Test]*,[*.Tests]*,[DeMorgan]*</Exclude>
          <ExcludeByFile>**/*.Designer.cs,**/obj/**/*.g.cs,**/obj/**/*.g.i.cs,**/*.GlobalUsings.g.cs,**/AssemblyInfo.cs</ExcludeByFile>
          <SkipAutoProps>true</SkipAutoProps>
          <!-- D7: MUST stay false. The server suffix-matches report paths against
               `git ls-files`; SourceLink rewrites them to raw.githubusercontent URLs,
               which makes duplicate basenames ambiguous and the server drops those
               files SILENTLY. Symptom: coverage looks inexplicably low, no error. -->
          <UseSourceLink>false</UseSourceLink>
          <!-- D7: false while ContinuousIntegrationBuild is unset during the TEST
               build. Move that property to the build step and paths become /_/… and
               this must flip to true. -->
          <DeterministicReport>false</DeterministicReport>
          <!-- D7: ExcludeByAttribute intentionally ABSENT. Measured upstream:
               Obsolete drops `readonly ref struct` types; CompilerGenerated drops
               every async method body. Exclusion is by file glob instead. -->
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

Why each filter here specifically:
- **`**/*.Designer.cs` is load-bearing.** Six designer files exist. With `ExcludeByAttribute` off
  (D7), this glob is the *only* thing excluding WinForms `InitializeComponent`.
- **`[DeMorgan]*`** — standalone app, not production library code.
- **`[*.Demo]*`** — `ThreeDee.Test` references `ThreeDee.Demo` for `DemoScene.Build()`.
- **`SkipAutoProps=true`** — abstractions projects are near-pure auto-properties and would
  otherwise be free 100%.
- Glob separators must be forward slashes. **No source generators exist in this repo**
  (`Microsoft.WinForms.Designer.SDK` is designer tooling, not a generator), so the `obj/**` globs
  are hygiene, not a live problem.

### 6.1a Exclusions as built — SUPERSEDES §6.1

§6.1's `coverlet.runsettings` was never created: coverlet cannot attach at all (§3.2). The
Microsoft collector takes its configuration through `--coverage-settings`, and **that option is
rejected at startup by version 18.11.2** — the option name exists in the extension assembly, but
passing it produces `Zero tests ran` and a handshake failure regardless of path form or schema.

Both exclusions the report actually needed have a better home than a settings file anyway:

| Needed exclusion | How it is done |
|---|---|
| `MintPlayer.ThreeDee.Demo` (18 files) | **Removed the dependency.** It was only referenced for `DemoScene.Build()` — 20 lines with no WinForms dependency — which now lives in the test project as `TestScene`. The tests no longer depend on an application. |
| WinForms `*.Designer.cs` | `[ExcludeFromCodeCoverage]` on the designer partial classes, which the Microsoft collector honours. |

Everything else §6.1 filtered for turned out not to need filtering: an assembly with no test
project is never loaded, so it is absent from the report rather than measured. `DeMorgan`,
`MintPlayer.BrowserDialog.Demo` and `MintPlayer.PlatformBrowser.Demo` never appear.

Verified after the change: `MintPlayer.ThreeDee.Demo` is gone, `MintPlayer.BrowserDialog` drops
from 14 measured files to 10 and `MintPlayer.KarnaughMap` from 18 to 16. The one remaining file
matching `*Designer*` is `KarnaughMapDesigner.cs`, which is hand-written support code and is
correctly measured.

### 6.2 Build files (D18)

```
versions.props            # six <XxxVersion> properties. Data only. The API-hash tool owns this file.
Directory.Build.props     # imports versions.props; LangVersion 15, Nullable, ImplicitUsings,
                          # PublishRepositoryUrl, EmbedUntrackedSources,
                          # <IsPackable Condition="$(MSBuildProjectName.EndsWith('.Tests'))">false</IsPackable>
Directory.Build.targets   # <PropertyGroup Condition="'$(OutputType)' == 'Library' And '$(IsPackable)' != 'false'">
                          #   <IncludeSymbols>true</IncludeSymbols>
                          #   <SymbolPackageFormat>snupkg</SymbolPackageFormat>
```

xUnit v3 test projects are `OutputType=Exe` (v3 tests are self-executing), so the `Library`
condition excludes all six automatically — D15 and D18 agree without extra work. Each csproj keeps a
comment pointing at `versions.props`, since `grep "<Version>" --include=*.csproj` will find nothing
in this repo.

### 6.3 `global.json` (D5)

```json
{
  "sdk": {
    "version": "11.0.100-rc.1.26425.128",
    "rollForward": "latestFeature",
    "allowPrerelease": true
  }
}
```

### 6.4 `coverage.yml` (D24)

```yaml
gate:
  projectMode: auto
  projectBasis: scoped      # NOT projection — see R6
  patchTarget: 80
  patchThreshold: 5
  blocking: false
```

---

## 7. Workflow shape (D6, D2, D20)

One reusable `workflow_call` holds restore/build/test/coverage; the four entry workflows supply
triggers and publish steps. Test job on `windows-latest`; build/pack/push stay on `ubuntu-latest`.

```yaml
      - uses: actions/setup-dotnet@v5
        with:
          # The .NET 11 SDK builds everything (global.json pins it). The .NET 10 SDK is
          # installed for its runtime. Pinned exactly rather than 11.0.x: while .NET 11 is
          # RC, a floating version would let an RC2 change CI behaviour without a commit.
          dotnet-version: |
            10.0.x
            11.0.100-rc.1.26425.128
          source-url: https://nuget.pkg.github.com/${{ github.repository_owner }}/index.json

      - name: Test
        # --no-restore does NOT imply --no-build; without it the solution is built a second
        # time and coverage is measured against a build nothing else uses.
        # --collect writes coverage.cobertura.xml into a GUID-named subdirectory of
        # --results-directory, one per test project — hence the ** in the glob below.
        run: >
          dotnet test --no-restore --no-build --configuration Release
          --settings coverlet.runsettings
          --collect:"XPlat Code Coverage" --results-directory coverage
```

Upload on **master**:

```yaml
      - name: Upload coverage
        if: always() && hashFiles('coverage/**/coverage.cobertura.xml') != ''
        uses: MintPlayer/MintPlayer.Spark/apps/CodeCoverage/action@coverage-upload-v1
        with:
          url: https://coverage.mintplayer.com
          token: ${{ secrets.COVERAGE_TOKEN }}
          files: |
            coverage/**/coverage.cobertura.xml
          # With search on, a glob matching nothing silently falls back to auto-detection
          # and would upload stray unparsable reports.
          disable-search: true
          finish: true
          # A coverage outage or missing token must not block a NuGet release.
          fail-ci-if-error: false
```

Upload on **pull_request** — same, plus `base-sha: ${{ github.event.pull_request.base.sha }}`,
`continue-on-error: true`, and a fork guard
(`github.event.pull_request.head.repo.full_name == github.repository`), because fork PRs get no
secrets and must not go red over it.

Pack/publish keeps both existing push lines unchanged:

```yaml
      - run: dotnet pack --no-build --configuration Release /p:ContinuousIntegrationBuild=true
      - run: dotnet nuget push "**/*.nupkg" --source https://api.nuget.org/v3/index.json --api-key ${{ secrets.PUBLISH_TO_NUGET_ORG }} --skip-duplicate
      - run: dotnet nuget push "**/*.nupkg" --no-symbols --skip-duplicate
```

Also add `concurrency: { group: ${{ github.workflow }}-${{ github.head_ref || github.ref }}, cancel-in-progress: true }`
and explicit `permissions: contents: read`.

---

## 8. Milestones — commit order within the single PR

Ordering is deliberate: the PR carries a framework upgrade, a breaking refactor, six new test
projects and coverage wiring. If CI goes red, "was it the refactor or the TFM change?" must remain
answerable, so each concern lands in its own commit and `git bisect` still works.

**M0 — Prerequisite (manual, blocks M6).** Create the `COVERAGE_TOKEN` repository secret from this
repo's page on `coverage.mintplayer.com`. The `coverageproduction` GitHub App is already installed
org-wide with `repository_selection: all`, so this repo is in scope automatically. Confirm the
installation still has **Checks: Read & write** and **Pull requests: Read & write** *accepted* —
until a permission change is accepted, the check runs silently never appear. Alternative: OIDC via
the action's `use-oidc` input plus `permissions: id-token: write`, which avoids a stored secret but
cannot work on fork PRs.

**M1 — Spike: prove v3 + coverage (gates everything).** Add `global.json`, `versions.props`,
`Directory.Build.props`, `Directory.Build.targets`. Create **one** xUnit v3 test project and prove
`--collect:"XPlat Code Coverage"` emits a non-empty `coverage.cobertura.xml`. Do not proceed until
this passes (§3.2).

**M2 — .NET 11 upgrade.** Retarget packable libraries to `net10.0-windows;net11.0-windows`
(`PlatformBrowser` keeps its four legs), demos/apps to `net11.0-windows`, `LangVersion` to 15. Bump
`MintPlayer.ObservableCollection` → `11.0.0-rc.1` and `System.Drawing.Common` (D17). Delete the
SourceLink references, add `PublishRepositoryUrl` + `EmbedUntrackedSources` (D16). Set every version
group to `11.0.0-rc.1` in `versions.props` — **required**: `--skip-duplicate` means a PR that
retargets everything and forgets the version publishes **nothing, silently, green**. Grep for
hardcoded `net10.0` strings outside csproj.

**M3 — Fix D13.** Correct the `net7.0-windows` condition, delete the private duplicate
`ToFormattedString`.

**M4 — Seams.** `PlatformBrowser` and `IconExtractor` static → instance + interface; three
`.Abstractions` projects; MVP for `KarnaughMap` (presenter + render model) and `BrowserDialog`.

**M5 — Tests.** Port `ThreeDee.Test` (25 blocks → `[Fact]`/`[Theory]`; mechanical). Rewrite
`QuineMcCluskey.Test` — **delete the `while (true)` / `Console.ReadKey()` loop**; on a runner with
redirected stdin `ReadKey` throws, turning the infinite loop into a tight exception loop. Add the
remaining four suites. Rename `MintPlayer.KarnaughMap.Test` → `.Demo` so it stops reading as a test
project, and fix the `QuineMcKluskey` folder typo.

**M6 — Coverage + CI.** `coverlet.runsettings`, `coverage.yml`, the reusable workflow, Release
throughout, `setup-dotnet@v5`, dual SDK install, upload steps.

**M7 — Visibility.** Badge at the top of `README.md`:

```markdown
[![Coverage](https://coverage.mintplayer.com/badge/MintPlayer/MintPlayer.DotnetDesktop.Tools.svg)](https://coverage.mintplayer.com/r/MintPlayer/MintPlayer.DotnetDesktop.Tools)
```

The server renders the SVG itself (not shields.io). It never 404s — an unknown repo returns a grey
`unknown` badge at HTTP 200, and `unknown` ≠ 0%.

---

## 9. Risks

- **R1 — An empty coverage report is indistinguishable from a passing one.** Today's no-op
  `dotnet test` exits 0; MTP-mode v3 (§3.2) would too. The `hashFiles(…)` guard makes that a skipped
  upload, not a red build. M6 should assert a minimum discovered-test count.
- **R2 — Ubuntu cannot run any of it.** `EnableWindowsTargeting` compiles `net*-windows`; it does
  not provide the Windows Desktop runtime.
- **R3 — First `net11.0-windows` in the org.** §3.3 proves compilation only. The WinForms designer,
  `System.Drawing.Common` and the UWP `PackageManager` path on .NET 11 are unexercised, and there is
  no sibling repo to copy a fix from.
- **R4 — The MVP repaint hazard (D21/D22).** `Invalidate()` is currently called *inline* inside
  `SetValue`, `ToggleNumber` and the `OutputVariable` setter. Once state moves to the model,
  repainting becomes event-driven, and any path that forgets to raise produces a control that
  silently stops redrawing. **No test in this PR would catch it** — tests assert the render model,
  not pixels. `MintPlayer.KarnaughMap.Demo` is the only thing that would reveal it, and only if a
  human looks.
- **R5 — A realistic per-assembly ceiling.** `IconUtils` is P/Invoke into Win32 icon/resource APIs;
  `BrowserDialog` keeps ~60 uncoverable lines after D23; `KarnaughMap` keeps ~60 lines of GDI+
  walking after D22 — and that is *correct*. Judge per assembly, never against one solution-wide
  percentage.
- **R6 — `coverage.yml` is read from the base ref**, so the config added in this PR does **not**
  apply to this PR; it takes effect on the next one. Expecting to see it work here will look broken.
  `projectBasis: scoped` matters because assemblies enter the report as tests are written — a
  `projection` basis reads every newly-tested assembly appearing at 30% as a "drop".
- **R7 — `async void` at the UI boundary (D23).** Pulling `BrowserDialog_Load` into a presenter
  turns `async void` into an awaitable the dialog fires-and-forgets. Today a throw inside `Load`
  crashes visibly on the UI thread; via a forgotten task it can vanish silently — and the tests
  won't see it, because they await the presenter properly.
- **R8 — Fork PRs get no secrets.** The PR upload step is guarded so a contributor's PR skips the
  upload rather than failing.
- **R9 — `/p:ContinuousIntegrationBuild=true` at pack time with `--no-build` is largely decorative.**
  That property does its work during *compilation*, which already happened. Inherited from the org
  template; it keeps D7's `DeterministicReport=false` correct, but SourceLink path normalisation may
  not actually be applied to the shipped symbols.
- **R10 — Warning counts double under multi-targeting**, purely because there are two inner builds.
  Expect it; don't chase it.

---

## 10. Open items

1. **M0: secret or OIDC?**
2. **M1's outcome** — VSTest path or MTP-native coverage (§3.2).
3. **Post-GA round.** After 2026-11-10, bump everything `-rc` → stable, sequenced behind
   `MintPlayer.ObservableCollection` shipping stable 11.0.0. Cross-repo sequencing, not a separate PR.
4. **The API-hash tool** (D12) is out of scope here; MinVer is revisited once it exists.
5. **D19 revisit** — narrowing `ObservableCollection` out of the `.Abstractions` contract is exactly
   the kind of change that tool is meant to classify.
6. **Exact path of `coverage.yml`** (root vs `.github/`) is documented only as "read from the base
   ref", and no MintPlayer repo has one to copy. The repository page UI sets the same values.

---

## 11. Acceptance criteria

- [ ] `dotnet test` discovers and runs a non-zero number of tests, and a deliberately broken
      assertion turns CI red.
- [ ] A non-empty `coverage/**/coverage.cobertura.xml` exists after a CI test run, containing every
      `MintPlayer.*` production assembly that has tests — and **no** `*.Demo`, `*.Test*`, `DeMorgan`
      or `*.Designer.cs` entry.
- [ ] `https://coverage.mintplayer.com/r/MintPlayer/MintPlayer.DotnetDesktop.Tools` shows the build
      for the merge commit **with source files rendered** — i.e. no silent path-matching drop (D7).
- [ ] A PR shows `coverage/project` and `coverage/patch` check runs and the sticky comment.
- [ ] The README badge renders a real percentage, not grey `unknown`.
- [ ] A fork PR completes green with the upload step skipped.
- [ ] All ten packable projects produce `.nupkg` + `.snupkg` at `11.0.0-rc.1`; no test project
      produces a package.
- [ ] `MintPlayer.KarnaughMap.Demo` still renders and responds to input (the only check for R4).

---

## 13. What implementation changed

Everything in §2 held except the two below. Both were found by building the thing.

**§3.2 — coverlet is unreachable, so coverage is collected differently.** The plan assumed xUnit v3
merely *preferred* Microsoft.Testing.Platform. In fact MTP's VSTest bridge is removed on SDK 10+,
so coverlet's data collector cannot attach at all. Coverage comes from
Microsoft.Testing.Extensions.CodeCoverage instead — still cobertura, so the upload contract is
unchanged, but D1 and D7 no longer describe what is built, and there is no `coverlet.runsettings`.
This is the one place this repository diverges from every other MintPlayer repo, and it is forced,
not chosen.

**§6.1a — exclusions live in source, not in a settings file.** `--coverage-settings` is rejected at
startup by extension version 18.11.2. The two exclusions that were actually needed turned out to
have better homes: the demo dependency was removed outright (its 20-line scene builder moved into
the test project), and the WinForms designer classes carry `[ExcludeFromCodeCoverage]`. Everything
else §6.1 filtered for never needed filtering, because an assembly with no test project is never
loaded and is therefore absent rather than measured.

**Third delta, found after the first uploads: Windows paths had to be rebased.** The report page
was empty for commits whose upload the service had *accepted*. The collector writes absolute paths
in the host's native form — `D:\a\<repo>\<repo>\ThreeDee\…\Mesh.cs` — while the server resolves
files by suffix-matching against `git ls-files`, which yields `ThreeDee/…/Mesh.cs`. Those never
match, so every file was dropped. Silently: upload accepted, build created, page blank, nothing
red anywhere.

This was written down as a risk in M1 and then not acted on, which is the worst of both. It is now
`tools/Rebase-CoveragePaths.ps1`, run before the upload, and it **fails the build** when no path
matches — turning the silent case into a loud one. Every other MintPlayer repo runs its tests on
Ubuntu, where paths are already forward-slashed, so no other repo needs this.

**All three failures were silent**, which is R1 exactly: a test host that rejects an option reports
`Zero tests ran` with a handshake failure, not an error naming the option. Anyone changing the
coverage invocation should assume a green run proves nothing until a report file is confirmed on
disk.

Two smaller corrections: `dotnet sln add` is broken in SDK `11.0.100-rc.1.26425.128` (it reports
success and writes nothing, so solution entries were written directly), and xUnit v3 requires
`<OutputType>Exe</OutputType>` to be set explicitly rather than inferring it.

### Outcome against §12

All acceptance criteria are met except the three that need the live service, which cannot be
verified until the token exists (M0) and the branch is pushed: the report page rendering source
files, the check runs appearing on a pull request, and the badge showing a real percentage.

`dotnet test` runs **146 tests across six projects**, up from zero executed assertions.
