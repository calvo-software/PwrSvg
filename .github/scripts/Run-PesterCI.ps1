#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Helper script for running Pester integration tests in CI/CD pipeline
.DESCRIPTION
    This script encapsulates the common Pester test execution logic to avoid duplication
    in the GitHub Actions workflow. It handles installation, configuration, execution, 
    and result reporting for both .NET Framework 4.8 and .NET 8 tests.
.PARAMETER TargetFramework
    Target framework for the tests (net48 or net8)
.PARAMETER PowerShellEdition
    PowerShell edition name for display purposes
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("net48", "net8")]
    [string]$TargetFramework,
    
    [Parameter(Mandatory=$true)]
    [string]$PowerShellEdition
)

Write-Host "🧪 Running Pester Integration Tests for $PowerShellEdition" -ForegroundColor Green

# Ensure Pester is available
if (-not (Get-Module -ListAvailable Pester)) {
    Write-Host "Installing Pester module..." -ForegroundColor Yellow
    Install-Module -Name Pester -Force -SkipPublisherCheck -Scope CurrentUser
}

$pesterVersion = (Get-Module -ListAvailable Pester | Select-Object -First 1).Version
Write-Host "Using Pester version: $pesterVersion with $PowerShellEdition ($($PSVersionTable.PSVersion))" -ForegroundColor Green

# Configure Pester for xUnit XML output
$config = New-PesterConfiguration
$config.TestResult.Enabled = $true
$config.TestResult.OutputFormat = 'JUnitXml'
$config.TestResult.OutputPath = "pester-test-results-$TargetFramework.xml"
$config.Run.Path = './PwrSvg.Integration.Tests.ps1'
$config.Output.Verbosity = 'Detailed'

# Run Pester with configuration and capture results
try {
    Write-Host "Executing Pester tests for $PowerShellEdition..." -ForegroundColor Yellow
    $testResults = Invoke-Pester -Configuration $config
    Write-Host "Pester execution completed for $PowerShellEdition" -ForegroundColor Yellow
    
    if ($testResults.FailedCount -gt 0) {
        Write-Host "❌ Pester integration tests failed: $($testResults.FailedCount) failed out of $($testResults.TotalCount) tests" -ForegroundColor Red
        # Don't exit immediately - let the test reporter process the results
    } else {
        Write-Host "✅ All Pester integration tests passed: $($testResults.PassedCount)/$($testResults.TotalCount)" -ForegroundColor Green
    }
    
    # Verify test results file exists
    if (Test-Path "pester-test-results-$TargetFramework.xml") {
        Write-Host "✅ Test results file generated successfully" -ForegroundColor Green
        $fileSize = (Get-Item "pester-test-results-$TargetFramework.xml").Length
        Write-Host "File size: $fileSize bytes" -ForegroundColor Yellow
    } else {
        Write-Host "⚠️  Test results file not found, creating fallback results file" -ForegroundColor Yellow
        $fallbackXml = '<?xml version="1.0" encoding="utf-8"?><testsuites name="Pester" tests="0" errors="1" failures="0" time="0"><testsuite name="Pipeline Error" tests="1" errors="1" failures="0" time="0"><testcase name="Test execution failed" classname="Pipeline" time="0"><error message="Pester test execution failed - no results generated"/></testcase></testsuite></testsuites>'
        $fallbackXml | Out-File -FilePath "pester-test-results-$TargetFramework.xml" -Encoding UTF8
    }
    
    # Exit with failure code if tests failed, but after generating the report
    if ($testResults.FailedCount -gt 0) {
        exit 1
    }
} catch {
    Write-Host "❌ Error running Pester tests: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
    
    # Create error report for test reporter
    $errorMessage = $_.Exception.Message -replace '"', '&quot;'
    $errorXml = "<?xml version=`"1.0`" encoding=`"utf-8`"?><testsuites name=`"Pester`" tests=`"0`" errors=`"1`" failures=`"0`" time=`"0`"><testsuite name=`"Pipeline Error`" tests=`"1`" errors=`"1`" failures=`"0`" time=`"0`"><testcase name=`"Pester execution error`" classname=`"Pipeline`" time=`"0`"><error message=`"$errorMessage`"/></testcase></testsuite></testsuites>"
    $errorXml | Out-File -FilePath "pester-test-results-$TargetFramework.xml" -Encoding UTF8
    exit 1
}
