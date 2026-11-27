#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds the PwrSvg module layout and runs the Pester integration tests on it.
.DESCRIPTION
    Local equivalent of the CI test step. Run it from Windows PowerShell or
    PowerShell 7: the manifest loads the assembly for the PowerShell that runs
    the tests. Requires the Sixel module (Install-Module Sixel).
.PARAMETER SkipBuild
    Test the existing layout in ./artifacts instead of rebuilding it.
#>
[CmdletBinding()]
param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

$artifactsPath = Join-Path $PSScriptRoot 'artifacts'
if (-not $SkipBuild) {
    & (Join-Path (Join-Path $PSScriptRoot 'build') 'New-ModuleLayout.ps1') -OutputPath $artifactsPath | Out-Null
}

& (Join-Path (Join-Path (Join-Path $PSScriptRoot '.github') 'scripts') 'Run-PesterCI.ps1') -ModulePath (Join-Path $artifactsPath 'PwrSvg')
