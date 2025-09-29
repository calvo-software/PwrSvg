#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs Pester integration tests for PwrSvg module
.DESCRIPTION
    This script provides a convenient way to run Pester integration tests locally
    or in CI environments. It installs Pester if needed and runs the tests with
    appropriate output formatting.
.PARAMETER OutputFormat
    Output format for Pester results. Default is 'Detailed'
.PARAMETER PassThru
    Return the Pester test results object
.EXAMPLE
    ./Run-PesterTests.ps1
    Runs tests with detailed output
.EXAMPLE
    ./Run-PesterTests.ps1 -OutputFormat Normal
    Runs tests with normal output
#>

param(
    [ValidateSet('None', 'Normal', 'Detailed', 'Diagnostic')]
    [string]$OutputFormat = 'Detailed',
    [switch]$PassThru
)

# Ensure we're in the correct directory
if (-not (Test-Path './PwrSvg.Integration.Tests.ps1')) {
    Write-Error "PwrSvg.Integration.Tests.ps1 not found. Please run this script from the repository root."
    exit 1
}

Write-Host "=== PwrSvg Pester Integration Tests ===" -ForegroundColor Green
Write-Host "Running integration tests using Pester framework..." -ForegroundColor Yellow

# Check if Pester is available
try {
    $pesterModule = Get-Module -ListAvailable Pester | Select-Object -First 1
    if ($null -eq $pesterModule) {
        Write-Host "Installing Pester module..." -ForegroundColor Yellow
        Install-Module -Name Pester -Force -SkipPublisherCheck -Scope CurrentUser
    } else {
        Write-Host "Using Pester version $($pesterModule.Version)" -ForegroundColor Green
    }
} catch {
    Write-Error "Failed to install or import Pester: $_"
    exit 1
}

# Run the tests
try {
    $testParams = @{
        Path = './PwrSvg.Integration.Tests.ps1'
        Output = $OutputFormat
    }
    
    if ($PassThru) {
        $testParams.PassThru = $true
    }
    
    $testResults = Invoke-Pester @testParams
    
    if ($testResults -and $testResults.FailedCount -gt 0) {
        Write-Host "`n❌ Tests Failed: $($testResults.FailedCount) out of $($testResults.TotalCount)" -ForegroundColor Red
        exit 1
    } else {
        $passedCount = if ($testResults) { $testResults.PassedCount } else { "All" }
        $totalCount = if ($testResults) { $testResults.TotalCount } else { "tests" }
        Write-Host "`n✅ All Tests Passed: $passedCount/$totalCount" -ForegroundColor Green
    }
} catch {
    Write-Error "Failed to run Pester tests: $_"
    exit 1
}

Write-Host "`nPester integration tests completed successfully!" -ForegroundColor Green