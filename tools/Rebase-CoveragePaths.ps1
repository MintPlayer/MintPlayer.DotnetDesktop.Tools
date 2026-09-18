<#
.SYNOPSIS
    Rewrites absolute Windows paths in Cobertura reports to repository-relative,
    forward-slash paths.

.DESCRIPTION
    coverage.mintplayer.com resolves a report's files by suffix-matching their paths
    against `git ls-files`, which yields forward-slash paths relative to the repository
    root:

        ThreeDee/MintPlayer.ThreeDee/Geometry/Mesh.cs

    Microsoft.Testing.Extensions.CodeCoverage writes absolute paths in the host's native
    form, which on a Windows runner means:

        D:\a\MintPlayer.DotnetDesktop.Tools\MintPlayer.DotnetDesktop.Tools\ThreeDee\...\Mesh.cs

    Those never suffix-match, so the server drops every file. It does this SILENTLY: the
    upload is still accepted, the build is still created, and the report page is simply
    empty. Nothing in the workflow goes red.

    Every other MintPlayer repository runs its tests on Ubuntu, where paths are already
    forward-slashed, which is why no other repository needs this step.

.PARAMETER Path
    Directory containing the .cobertura.xml files. Searched recursively.

.PARAMETER WorkspaceRoot
    Repository root to make paths relative to. Defaults to the current directory.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Path,

    [string] $WorkspaceRoot = (Get-Location).Path
)

$ErrorActionPreference = 'Stop'

# Normalise the root the same way we will normalise the report paths, so the prefix
# comparison is like-for-like.
$root = $WorkspaceRoot.Replace('\', '/').TrimEnd('/')
$rootPrefix = "$root/"

$reports = Get-ChildItem -Path $Path -Filter '*.cobertura.xml' -Recurse -File
if ($reports.Count -eq 0) {
    Write-Error "No .cobertura.xml files found under '$Path'."
}

$totalRebased = 0

foreach ($report in $reports) {
    $content = Get-Content -LiteralPath $report.FullName -Raw
    $rebasedInFile = 0

    $updated = [regex]::Replace($content, 'filename="([^"]*)"', {
        param($match)

        $file = $match.Groups[1].Value.Replace('\', '/')

        if ($file.StartsWith($rootPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            $file = $file.Substring($rootPrefix.Length)
            $script:rebasedInFile++
        }

        # Attribute values are XML-escaped on the way in; nothing here introduces a
        # character that needs escaping, so the original quoting is safe to reuse.
        return 'filename="' + $file + '"'
    })

    if ($updated -ne $content) {
        Set-Content -LiteralPath $report.FullName -Value $updated -NoNewline -Encoding UTF8
    }

    Write-Host "  $($report.Name): $rebasedInFile path(s) rebased"
    $totalRebased += $rebasedInFile
}

Write-Host "Rebased $totalRebased path(s) across $($reports.Count) report(s) against $root"

if ($totalRebased -eq 0) {
    # Worth failing on: it means the paths did not look the way this script expects, and
    # the consequence downstream is an empty report page rather than an error.
    Write-Error "No paths were rebased. The reports do not start with '$rootPrefix'; the upload would silently produce an empty report."
}
