<#
.SYNOPSIS
    Runs the PwrSvg Pester integration tests against a module layout.
.DESCRIPTION
    Runs PwrSvg.Integration.Tests.ps1 in the PowerShell that invokes this script
    and writes JUnit XML results for the CI test reporter. The results file is
    named after the running edition (Desktop or Core) so a Windows run under
    both PowerShells keeps both reports.
.PARAMETER ModulePath
    The PwrSvg module folder produced by build/New-ModuleLayout.ps1.
#>
param(
    [Parameter(Mandatory)]
    [string]$ModulePath
)

$ErrorActionPreference = 'Stop'

$pester = Get-Module -ListAvailable Pester | Where-Object { $_.Version -ge [version]'5.0' } | Select-Object -First 1
if (-not $pester) {
    Install-Module -Name Pester -MinimumVersion 5.0 -Force -SkipPublisherCheck -Scope CurrentUser
}
Import-Module Pester -MinimumVersion 5.0

$repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$edition = if ($PSVersionTable.PSEdition) { $PSVersionTable.PSEdition } else { 'Desktop' }

$container = New-PesterContainer -Path (Join-Path $repositoryRoot 'PwrSvg.Integration.Tests.ps1') -Data @{ ModulePath = $ModulePath }

$config = New-PesterConfiguration
$config.Run.Container = $container
$config.Run.Exit = $true
$config.TestResult.Enabled = $true
$config.TestResult.OutputFormat = 'JUnitXml'
$config.TestResult.OutputPath = Join-Path $repositoryRoot "pester-results-$edition.xml"
$config.Output.Verbosity = 'Detailed'

Invoke-Pester -Configuration $config
