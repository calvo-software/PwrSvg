# PwrSvg Integration Tests

This directory contains Pester-based integration tests for the PwrSvg PowerShell module.

## Overview

The integration tests validate the complete module lifecycle including:

- **Module Build Artifacts**: Verifies that existing build outputs (.NET 8.0 and .NET Framework 4.8) are present and valid
- **Module Layout Creation**: Creates proper deployment structure from existing build artifacts rather than rebuilding
- **Module Import**: Tests module import behavior and dependency handling  
- **Content Validation**: Verifies manifest content and script definitions

## Running Tests

### Using the Convenience Script (Recommended)

```powershell
# Run with detailed output
./Run-PesterTests.ps1

# Run with normal output
./Run-PesterTests.ps1 -OutputFormat Normal

# Generate JUnit XML report for CI/CD integration
./Run-PesterTests.ps1 -TestResultFormat JUnitXml -OutputPath "test-results.xml"

# Get test results object
$results = ./Run-PesterTests.ps1 -PassThru
```

### Direct Pester Invocation

```powershell
# Install Pester if needed
Install-Module -Name Pester -Force -SkipPublisherCheck

# Run tests with modern Pester v5 configuration
$config = New-PesterConfiguration
$config.Run.Path = './PwrSvg.Integration.Tests.ps1'
$config.Output.Verbosity = 'Detailed'
$config.TestResult.Enabled = $true
$config.TestResult.OutputFormat = 'JUnitXml'
$config.TestResult.OutputPath = 'test-results.xml'
Invoke-Pester -Configuration $config
```

## Test Structure

The tests are organized into the following contexts:

1. **Module Build Artifacts**: Tests presence and validity of existing build outputs
2. **Module Layout Creation**: Creates deployment structure from existing build artifacts
3. **Module Import**: Tests import behavior with dependency validation
4. **Module Structure Validation**: Verifies file existence and content
5. **Test SVG Content Processing**: Validates test data integrity

## Efficient Testing Approach

The integration tests are designed to be efficient and test what is actually deployed:

- **No Redundant Building**: Tests use existing build artifacts from the CI/CD pipeline instead of rebuilding
- **Real Deployment Testing**: Creates module layout from the same artifacts that get deployed
- **CI/CD Optimized**: Minimizes build time by reusing previous build outputs

## CI/CD Integration

These tests are automatically run in the GitHub Actions pipeline on all supported platforms. The tests are designed to work in CI environments where dependencies like the Sixel module may not be available.

### Test Reporting

The integration tests generate JUnit XML reports that are automatically processed by GitHub Actions using the `dorny/test-reporter` action. This provides:

- ✅ Visual test result summaries in the GitHub Actions UI
- ✅ Per-test pass/fail status with detailed error messages
- ✅ Integration with GitHub's check runs and pull request status
- ✅ Standardized test reporting format compatible with most CI/CD systems

## Replacing Traditional Integration Tests

These Pester tests replace the traditional `integration-test.ps1` script, providing:

- ✅ Structured test organization with clear pass/fail results
- ✅ Better error reporting and diagnostics
- ✅ Integration with modern PowerShell testing practices
- ✅ Improved CI/CD pipeline integration
- ✅ Easier local development and debugging

## Requirements

- PowerShell 5.1+ or PowerShell Core 7+
- Pester module (automatically installed if needed)
- .NET SDK 8.0+ for building the module