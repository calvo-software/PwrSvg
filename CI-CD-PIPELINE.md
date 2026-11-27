# CI/CD Pipeline Documentation

This repository includes a CI/CD pipeline that builds, tests, and publishes the PwrSvg PowerShell module.

## Pipeline Overview

The CI/CD pipeline is configured with GitHub Actions and includes the following stages:

### 1. Build and Test (`test` job)
- **Triggers**: Push to `main`/`develop` branches, Pull Requests to `main`
- **Platforms**: Ubuntu, Windows, macOS
- **Framework Targets**: .NET 8.0 (all platforms), .NET Framework 4.8 (Windows only)

**Steps:**
- Set up .NET 8.0 SDK
- Restore NuGet packages and build the solution
- Run unit tests (xUnit)
- Build the module layout with `build/New-ModuleLayout.ps1`
- Run the Pester integration tests against that layout: under PowerShell 7 everywhere, and also under Windows PowerShell on Windows

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

### Unit Tests (xUnit)
- **Coverage**: Cmdlet attribute validation, property configuration, output type verification, parameter validation
- **Command**: `dotnet test`

### Integration Tests (Pester)
- **Coverage**: Importing the published module layout and running `ConvertTo-Png` and `Out-ConsoleSvg` on it
- **Approach**: The tests take only the path of the module layout. The manifest loads the assembly for the PowerShell that imports it, so the same tests cover the net48 assembly under Windows PowerShell and the net8 assembly under PowerShell 7
- **Reporting**: JUnit XML per PowerShell edition, shown on the run by `dorny/test-reporter`
- **Local run**: `./integration-test.ps1` (see [INTEGRATION-TESTS.md](INTEGRATION-TESTS.md))

## Build Matrix

| Platform | .NET 8.0 | .NET Framework 4.8 | PowerShell 7 | Windows PowerShell |
|----------|----------|---------------------|--------------|--------------------|
| Ubuntu   | ✅       | ❌                  | ✅           | ❌                 |
| Windows  | ✅       | ✅                  | ✅           | ✅                 |
| macOS    | ✅       | ❌                  | ✅           | ❌                 |

## Local Development

```powershell
dotnet build -c Release
dotnet test -c Release
./integration-test.ps1
```
