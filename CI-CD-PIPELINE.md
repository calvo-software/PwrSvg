# CI/CD Pipeline Documentation

This repository includes a comprehensive CI/CD pipeline that builds, tests, and publishes the PwrSvg PowerShell module.

## Pipeline Overview

The CI/CD pipeline is configured with GitHub Actions and includes the following stages:

### 1. Build and Test (`test` job)
- **Triggers**: Push to `main`/`develop` branches, Pull Requests to `main`
- **Platforms**: Ubuntu, Windows, macOS  
- **Framework Targets**: .NET 8.0 (all platforms), .NET Framework 4.8 (Windows only)

**Steps:**
- Set up .NET 8.0 SDK
- Install .NET Framework targeting pack (Windows only)
- Restore NuGet packages
- Build solution for both target frameworks
- Run unit tests (xUnit)
- Run integration tests (Pester)
- Test PowerShell module import on each platform

### 2. Create Packages (`package` job)
- **Triggers**: Release published
- **Platform**: Windows (required for .NET Framework 4.8 builds)

**Steps:**
- Build and publish both .NET 8.0 and .NET Framework 4.8 versions
- Update module manifest version from release tag
- Create PowerShell Gallery compatible package structure
- Upload build artifacts

### 3. Publish to PowerShell Gallery (`publish` job)
- **Triggers**: Release published
- **Platform**: Windows

**Steps:**
- Download packaged module
- Publish to PowerShell Gallery using `POWERSHELL_GALLERY_API_KEY` secret

## Setup Requirements

### Repository Secrets

Add the following secret to your GitHub repository:

- `POWERSHELL_GALLERY_API_KEY`: Your PowerShell Gallery API key
  - Get this from [PowerShell Gallery](https://www.powershellgallery.com/account/apikeys)

### Creating a Release

To trigger the packaging and publishing pipeline:

1. Create a new tag: `git tag v1.0.0`
2. Push the tag: `git push origin v1.0.0`  
3. Create a release from the tag on GitHub

The pipeline will:
- Build packages for both .NET 8.0 and .NET Framework 4.8
- Update the module version to match the release tag
- Publish to PowerShell Gallery

## Test Coverage

The pipeline includes comprehensive testing at multiple levels:

### Unit Tests (xUnit)
- **Framework**: xUnit (.NET testing framework)
- **Coverage**: Cmdlet attribute validation, property configuration, output type verification, parameter validation
- **Command**: `dotnet test`

### Integration Tests (Pester)
- **Framework**: Pester (PowerShell testing framework)
- **Coverage**: Module build artifact validation, module layout creation from existing builds, import testing, file existence checks, error handling
- **Approach**: Tests existing build artifacts rather than rebuilding, ensuring we test what we deploy
- **Configuration Detection**: Automatically detects build configuration (Release in CI, Debug locally)
- **Reporting**: Generates JUnit XML reports compatible with GitHub Actions and other CI/CD systems
- **Command**: `pwsh -c "Invoke-Pester ./PwrSvg.Integration.Tests.ps1"`
- **Convenience Script**: `./Run-PesterTests.ps1`
- **CI/CD Integration**: Uses `dorny/test-reporter` action for proper test result visualization in GitHub

### End-to-End Tests
- **Coverage**: PowerShell integration testing, ConvertTo-Png functionality validation
- **Runs on**: All supported platforms (.NET 8.0 cross-platform, .NET Framework 4.8 on Windows)

## Build Matrix

| Platform | .NET 8.0 | .NET Framework 4.8 | PowerShell Core | Windows PowerShell |
|----------|----------|---------------------|------------------|-------------------|
| Ubuntu   | ✅       | ❌                  | ✅               | ❌                |
| Windows  | ✅       | ✅                  | ✅               | ✅                |
| macOS    | ✅       | ❌                  | ✅               | ❌                |

## Package Outputs

The pipeline produces three artifacts:

1. **PwrSvg-net8.0**: .NET 8.0 compiled module with dependencies
2. **PwrSvg-net48**: .NET Framework 4.8 compiled module with dependencies  
3. **PwrSvg-PSGallery**: PowerShell Gallery compatible package

## Local Development

To run the build and tests locally:

```bash
# Restore packages
dotnet restore

# Build solution  
dotnet build -c Release

# Run tests
dotnet test -c Release

# Run integration tests
./Run-PesterTests.ps1

# Test module import
dotnet publish PwrSvg/PwrSvg.csproj -c Release -f net8.0 -o ./publish
pwsh -c "Import-Module ./publish/PwrSvg.dll; Get-Command -Module PwrSvg"
```