# archilab

[![Build](https://github.com/ksobon/archilab/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/ksobon/archilab/actions/workflows/build.yml)

The official repository for the **archi-lab.net** Dynamo package — a large
collection of nodes extending Dynamo's Revit capabilities, covering views and
sheets, parameters, rooms, filters, Excel and CSV, units, worksets and more.

## Installation

Search for **archi-lab.net** in Dynamo's Package Manager and install. Dynamo
offers the build matching your Revit version automatically.

To install by hand instead, download the zip for your Revit version from the
[releases](https://github.com/ksobon/archilab/releases) and extract it into:

```
%AppData%\Dynamo\Dynamo Revit\<dynamo-version>\packages\archi-lab.net
```

## Supported versions

| Revit | DynamoRevit | DynamoCore | Target framework |
|-------|-------------|------------|------------------|
| 2027  | 27.0        | 4.0        | net10.0-windows  |
| 2026  | 3.6         | 3.6        | net8.0-windows   |
| 2025  | 3.2         | 3.2        | net8.0-windows   |

Revit 2024 and earlier are no longer supported; the last builds for them are in
the commit history.

The two Dynamo columns diverge from Revit 2027 on, where DynamoRevit moved to
year-based versioning. The distinction matters:

- **DynamoRevit** names the packages folder — `Dynamo Revit\27.0\packages`.
- **DynamoCore** is what the `DynamoVisualProgramming.*` NuGet packages track,
  and what `pkg.json`'s `engine_version` refers to.

## Versioning

Package versions are `<year>.<day of year>.<target ordinal>`:

| Version | Meaning | For |
|---------|---------|-----|
| `2026.216.250` | built 2026, day 216 (Aug 4) | Revit 2025 |
| `2026.216.260` | same build | Revit 2026 |
| `2026.216.270` | same build | Revit 2027 |

One build round publishes all three. Nothing is hand-incremented: the version
is derived by the build from the date and the project's `ArchilabTargetOrdinal`
(see `Directory.Build.targets`), and `scripts/package.ps1` reads it back off
the compiled assembly rather than recomputing it. So the stamped assemblies,
`pkg.json`, `node_libraries` and the zip name always carry the same number.

**Why this shape.** All builds publish under the one `archi-lab.net` package
name, and the Dynamo Package Manager refuses any version that moves backwards.
This scheme satisfies that structurally — each segment only ever advances: the
calendar year, then the day within it, then the target ordinal within a single
day's round. A scheme with the target Revit year as the major would not, since
shipping a Revit 2025 fix after a 2027 release would move the major backwards
and be rejected.

**Why an ordinal rather than just the Revit year.** The ordinal is the
two-digit Revit year times ten, plus the Revit update within that year — so
Revit 2027 is `270`. One Revit year can need more than one concurrent build:
if a mid-cycle update such as Revit 2025.5 moved to a different .NET runtime,
users on 2025.0 and on 2025.5 would each need their own package, and both are
"Revit 2025". The ordinal keeps them distinct and correctly ordered:

| Version | Target | Runtime | engine_version |
|---------|--------|---------|----------------|
| `2026.216.250` | Revit 2025.0–2025.4 | net8 | 3.2.1.5366 |
| `2026.216.255` | Revit 2025.5+ | net10 | *its* Dynamo |
| `2026.216.260` | Revit 2026 | net8 | 3.6.1.9895 |

A user on Revit 2025.0 satisfies only `250` and gets it. A user on 2025.5
satisfies both `250` and `255`, and takes the newer `255` — the right build for
them. Adding such a target means a new project pair with its own ordinal, not
an edit to an existing one.

The ordinal tops out at `999` (Revit 2099.9), well inside the 16-bit limit on
assembly version components.

Two releases on the same day would collide. Pass
`-p:ArchilabBuildDate=yyyy-MM-dd` to the build, or use the release workflow's
date input, to stamp a different day.

### How one package serves three Revit versions

Every published version carries its own `engine_version` — the minimum Dynamo
it requires — and Dynamo offers each user the newest version their engine
satisfies:

| Version | engine_version | Offered to |
|---------|----------------|------------|
| `2026.216.250` | 3.2.1.5366 | Revit 2025 and newer |
| `2026.216.260` | 3.6.1.9895 | Revit 2026 and newer |
| `2026.216.270` | 4.0.2.3852 | Revit 2027 |

A Revit 2025 user satisfies only the first, so that is what they get. A Revit
2027 user satisfies all three and takes the newest, which is the 2027 build.
"Newest compatible" and "correct build for my Revit" resolve to the same
version because the target ordinal rises in step with the engine requirement.
Publish a round in ascending ordinal order.

Note that `engine_version` is the exact Dynamo each project builds against, so
a Revit install whose Dynamo predates it will not be offered the update. If
wider reach matters more than binding to the newest API, build against the
oldest Dynamo servicing each Revit release and lower `engine_version` to match.

## Building

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) — it builds
the net8.0 targets too — and Visual Studio 2022 or later for the IDE.

```bash
dotnet build archilabUI2027/archilabUI2027.csproj -c Release
```

Each UI project references its core project, so building the UI project builds
both. Open `archilab.sln` in Visual Studio to build every Revit version at
once; from the command line, target an individual project, because
`dotnet build` does not reliably handle the shared projects (`.shproj`) the
solution contains.

A build deploys into `%AppData%\Dynamo\Dynamo Revit\<version>\packages\archi-lab.net`,
so the package is live the next time Dynamo starts. A **Release** build also
writes an installable zip to `dist/`, the same artifact CI attaches to a
release. Debug builds skip the zip to stay fast, and `dist/` is not committed.

Set `CI=true` to skip both steps.

## Project layout

All node source lives in two shared projects, compiled into every Revit
version:

| Project | Produces | Contains |
|---------|----------|----------|
| `archilabSharedProject` | `archilab.dll` | zero-touch nodes |
| `archilabUISharedProject` | `archilabUI.dll` | NodeModel / UI nodes |

The per-year projects (`archilab2027`, `archilabUI2027`, …) hold no source of
their own. They exist to pin a Revit API version, a Dynamo version and a target
framework, and to define the compilation symbols below.

**Assembly names carry no Revit version.** Every build produces `archilab.dll`
and `archilabUI.dll`; which Revit a build is for is expressed by the package
version and `engine_version`, not the file name. Dynamo records the assembly
name in saved graphs, so version-stamped names meant a graph authored against
`archilab2025.dll` would not resolve its nodes under Revit 2027. A single name
makes graphs portable across Revit versions.

The builds never collide, because each is installed into its own
`Dynamo Revit\<version>\packages` folder.

Also in the repo: `_libs/<dynamorevit-version>/` holds the Dynamo Revit
assemblies, which are not published on NuGet and so are vendored from a Revit
install; `_resources/` holds files shared by every version (the spell-check
dictionaries and the Dynamo customization XML); `_graphics/` holds node icons.

### Version-specific code

Where the Revit API differs between releases, code is guarded with **cumulative**
symbols, which each project defines for every release it satisfies:

```csharp
#if REVIT2026_OR_GREATER
    return (int)id.Value;
#else
    return id.IntegerValue;      // Revit 2025
#endif
```

| Project | Defines |
|---------|---------|
| 2025 | `Revit2025` |
| 2026 | `Revit2026`, `REVIT2026_OR_GREATER` |
| 2027 | `Revit2027`, `REVIT2026_OR_GREATER`, `REVIT2027_OR_GREATER` |

Prefer the `_OR_GREATER` form over an exact-year symbol. An exact-year `#if`
silently selects the older branch as soon as a newer Revit is added, compiling
against an API that no longer exists — which is exactly what happened before
Revit 2027 support was added.

### Adding a new Revit version

1. Copy the newest `_libs/<version>/` from the new Revit's
   `AddIns\DynamoForRevit` folder (`RevitNodes.dll`, `RevitServices.dll`, and
   `nodes\DSRevitNodesUI.dll`).
2. Copy the newest project pair, updating the target framework, the Revit API
   and Dynamo package versions, the `_libs` paths, the deploy folder, the
   `DefineConstants` (year symbol plus every `_OR_GREATER` it satisfies), and
   `ArchilabTargetOrdinal`.
3. Add both projects to `archilab.sln`, including the
   `SharedMSBuildProjectFiles` entries.
4. Add an entry to `$ArchilabTargets` in `scripts/version.ps1` and widen the
   `ValidateSet` in `scripts/package.ps1`.
5. Add the year to the matrix in `.github/workflows/build.yml` and the loops in
   `.github/workflows/release.yml`.
6. Update the supported-versions table above.

Existing guarded code should not need to change unless the new release breaks
an API that is not already guarded.

## Releasing

Push a tag. CI builds all three Revit versions, stamps each assembly from the
build date, and attaches one package zip per version to a GitHub release. The
release workflow can also be run manually, with an optional date override, to
produce artifacts without publishing a release.

Uploading the zips to the Dynamo Package Manager is still manual — it has no
publish API.

## License

MIT — see [LICENSE](LICENSE).

## Support

<p align="center">
  <img width="600" src="https://i2.wp.com/archi-lab.net/wp-content/uploads/2019/04/patreon_archilab.jpg">
</p>

This repo and the package is maintained and supported by Konrad K Sobon. Konrad has a lot of things to look after, so if you like what you see here, like to keep the archi-lab.net package maintained and getting better, please consider supporting Konrad on Patreon: https://www.patreon.com/archilab 
