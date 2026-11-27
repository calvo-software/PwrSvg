#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds the PwrSvg module layout that is tested and published.
.DESCRIPTION
    Publishes PwrSvg into <OutputPath>/PwrSvg with the manifest and scripts at
    the root and one subdirectory per target framework. The manifest picks the
    assembly for the running PowerShell. The net48 build needs Windows, so it is
    produced only there.
.PARAMETER OutputPath
    Directory that receives the PwrSvg module folder.
.PARAMETER Configuration
    Build configuration passed to dotnet publish.
.OUTPUTS
    System.IO.DirectoryInfo of the module folder.
#>
[CmdletBinding()]
param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '..' 'artifacts'),
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$sourceDir = Join-Path $PSScriptRoot '..' 'PwrSvg'
$projectPath = Join-Path $sourceDir 'PwrSvg.csproj'
$moduleDir = Join-Path $OutputPath 'PwrSvg'

if (Test-Path $moduleDir) {
    Remove-Item $moduleDir -Recurse -Force
}
New-Item -ItemType Directory -Path $moduleDir -Force | Out-Null

$frameworks = [ordered]@{ 'net8.0' = 'net8' }
if ($IsWindows -or $PSVersionTable.PSEdition -eq 'Desktop') {
    $frameworks['net48'] = 'net48'
}

foreach ($framework in $frameworks.Keys) {
    $frameworkDir = Join-Path $moduleDir $frameworks[$framework]
    Write-Verbose "Publishing $framework to $frameworkDir"
    & dotnet publish $projectPath --configuration $Configuration --framework $framework --output $frameworkDir --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $framework with exit code $LASTEXITCODE"
    }
}

Copy-Item (Join-Path $sourceDir 'PwrSvg.psd1') $moduleDir
Copy-Item (Join-Path $sourceDir 'Out-ConsoleSvg.ps1') $moduleDir

Get-Item $moduleDir
