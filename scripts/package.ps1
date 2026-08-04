<#
.SYNOPSIS
    Stages a built archilab version as a Dynamo package and zips it.

.DESCRIPTION
    Copies from the archilabUI<year> output directory, which is a superset of
    the core project's output (CopyLocalLockFileAssemblies plus the
    ProjectReference), into the pkg.json / bin / extra layout Dynamo expects.

    Assemblies are chosen by allowlist, never by exclusion: the build output
    also contains RevitAPI.dll, RevitAPIUI.dll, AdWindows.dll and the whole
    Dynamo assembly set, none of which may ship in a package.

.EXAMPLE
    ./scripts/package.ps1 -RevitYear 2027
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet(2025, 2026, 2027)][int]$RevitYear,
    [string]$Configuration = 'Release',
    [string]$OutDir = 'dist'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $PSScriptRoot 'version.ps1')

$target = $ArchilabTargets[$RevitYear]

$src = Join-Path $root "archilabUI$RevitYear\bin\$Configuration\$($target.Tfm)"
if (-not (Test-Path $src)) {
    throw "Build output not found: $src. Build archilabUI$RevitYear in $Configuration first."
}

# Take the version from the compiled assembly rather than recomputing it, so
# the stamped binaries, pkg.json and the zip name cannot drift apart. The
# build derives it in Directory.Build.targets; pass -p:ArchilabBuildDate there
# to reproduce an earlier release.
$assemblyVersion = (Get-Item (Join-Path $src 'archilab.dll')).VersionInfo.FileVersion
$Version = ([version]$assemblyVersion).ToString(3)

# Ship these. A missing one is a packaging bug, so fail rather than ship a
# package that loads with pieces silently absent.
$required = @(
    'archilab'
    'archilabUI'
    'ClosedXML'
    'CommunityToolkit.Mvvm'
    'DocumentFormat.OpenXml'
    'EPPlus'
    'ExcelNumberFormat'
    'FastMember.Signed'
    'HtmlAgilityPack'
    'itextsharp'
    'LumenWorks.Framework.IO'
    'Microsoft.Xaml.Behaviors'
    'RestSharp'
    'WeCantSpell.Hunspell'
    'Xceed.Wpf.Toolkit'
)

# Inbox on net8/net10, so normally absent from the output. Ship them only if
# the build actually produced them.
$optional = @(
    'System.Buffers'
    'System.Drawing.Common'
    'System.IO.Packaging'
    'System.Memory'
    'System.Numerics.Vectors'
    'System.Runtime.CompilerServices.Unsafe'
)

$outPath = Join-Path $root $OutDir
$stage = Join-Path $outPath "stage-$RevitYear"
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path (Join-Path $stage 'bin'), (Join-Path $stage 'extra') | Out-Null

$binDir = Join-Path $stage 'bin'

foreach ($name in $required) {
    Copy-Item (Join-Path $src "$name.dll") $binDir
}

$skipped = @()
foreach ($name in $optional) {
    $dll = Join-Path $src "$name.dll"
    if (Test-Path $dll) { Copy-Item $dll $binDir } else { $skipped += $name }
}
if ($skipped) { Write-Host "  note: not in build output, skipped: $($skipped -join ', ')" }

# XML docs drive Dynamo's node tooltips; the customization file maps
# namespaces to library categories. Both must sit alongside the assemblies.
foreach ($name in @('archilab', 'archilabUI')) {
    $xml = Join-Path $src "$name.xml"
    if (Test-Path $xml) { Copy-Item $xml $binDir }
}
Copy-Item (Join-Path $src 'archilab_DynamoCustomization.xml') $binDir

# Hunspell dictionaries for the spell-check nodes.
Copy-Item (Join-Path $src 'en_US.aff'), (Join-Path $src 'en_US.dic') (Join-Path $stage 'extra')

$pkg = Get-Content (Join-Path $PSScriptRoot 'pkg.template.json') -Raw
$pkg = $pkg -replace '\{\{VERSION\}\}', $Version `
            -replace '\{\{ENGINE_VERSION\}\}', $target.Engine `
            -replace '\{\{ASSEMBLY_VERSION\}\}', $assemblyVersion

# Every package published carries this license, so make sure the claim is
# actually backed by a LICENSE file in the repository.
$license = ($pkg | ConvertFrom-Json).license
if (-not $license) {
    Write-Warning "pkg.template.json has no license set; every published package will claim none."
} elseif (-not (Test-Path (Join-Path $root 'LICENSE'))) {
    Write-Warning "pkg.template.json claims the $license license but the repository has no LICENSE file."
}

# WriteAllText with an explicit BOM-less encoder: Set-Content -Encoding utf8
# emits a BOM under Windows PowerShell 5.1, which trips strict JSON parsers.
[System.IO.File]::WriteAllText(
    (Join-Path $stage 'pkg.json'), $pkg, (New-Object System.Text.UTF8Encoding($false)))

$zip = Join-Path $outPath "archi-lab.net_${Version}_Revit${RevitYear}_Dynamo$($target.Dynamo).zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip
Remove-Item $stage -Recurse -Force

$size = [math]::Round((Get-Item $zip).Length / 1MB, 2)
Write-Host "Packaged Revit $RevitYear (Dynamo $($target.Dynamo)) -> $zip [$size MB]"

# Each project's PostBuild copies binaries into the local Dynamo packages
# folder but writes no manifest, and Dynamo will not load a package folder
# without pkg.json. Keep it in step with the build so a local Release build is
# immediately loadable.
if ($env:CI -ne 'true') {
    $deployed = Join-Path $env:AppData "Dynamo\Dynamo Revit\$($target.Dynamo)\packages\archi-lab.net"
    if (Test-Path $deployed) {
        [System.IO.File]::WriteAllText(
            (Join-Path $deployed 'pkg.json'), $pkg, (New-Object System.Text.UTF8Encoding($false)))
        Write-Host "  refreshed pkg.json in $deployed"
    }
}

