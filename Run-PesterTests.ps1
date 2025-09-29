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
.PARAMETER TestResultFormat
    Test result output format for CI/CD integration. Default is 'JUnitXml'
.PARAMETER OutputPath
    Path where to save the test result report. If not specified, defaults to 'pester-test-results.xml' when TestResultFormat is used
.EXAMPLE
    ./Run-PesterTests.ps1 -TestResultFormat JUnitXml -OutputPath "test-results.xml"
    Runs tests and generates JUnit XML report for CI/CD integration
#>

param(
    [ValidateSet('None', 'Normal', 'Detailed', 'Diagnostic')]
    [string]$OutputFormat = 'Detailed',
    [switch]$PassThru,
    [string]$OutputPath,
    [ValidateSet('NUnitXml', 'NUnit2.5', 'NUnit3', 'JUnitXml')]
    [string]$TestResultFormat = 'JUnitXml'
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
    # Create Pester configuration
    $config = New-PesterConfiguration
    $config.Run.Path = './PwrSvg.Integration.Tests.ps1'
    $config.Output.Verbosity = $OutputFormat
    
    # Configure test result output if requested
    if ($TestResultFormat -and ($OutputPath -or $TestResultFormat -eq 'JUnitXml')) {
        $config.TestResult.Enabled = $true
        $config.TestResult.OutputFormat = $TestResultFormat
        $config.TestResult.OutputPath = if ($OutputPath) { $OutputPath } else { 'pester-test-results.xml' }
        Write-Host "Test results will be saved to: $($config.TestResult.OutputPath) in $TestResultFormat format" -ForegroundColor Yellow
    }
    
    if ($PassThru) {
        $testResults = Invoke-Pester -Configuration $config
    } else {
        $testResults = Invoke-Pester -Configuration $config
    }
    
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