# CI/CD Pipeline Documentation

This repository has two GitHub Actions workflows: `ci-cd.yml` builds and tests every push and pull request, and `release.yml` releases and publishes the PwrSvg PowerShell module.

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

### 2. Release (`release.yml`)
- **Triggers**: Push to `main`

Releases are made by [release-please](https://github.com/googleapis/release-please) through the shared release chain in `shoji-labs/infrastructure`. In one run it:

1. opens or updates the release PR, which bumps `ModuleVersion` in `PwrSvg/PwrSvg.psd1` and `.release-please-manifest.json` and updates `CHANGELOG.md`;
2. builds and tests the release PR's branch on Windows and Linux;
3. squash-merges the release PR;
4. creates the `vX.Y.Z` tag and the GitHub release.

The `publish` job then builds the module layout with `build/New-ModuleLayout.ps1` on Windows and publishes it to the PowerShell Gallery when the version on `main` is tagged and not on the Gallery yet.

Versions come from the PR titles merged into `main`, which must follow [Conventional Commits](https://www.conventionalcommits.org/): `fix:` bumps the patch version, `feat:` the minor version, and `feat!:` or a `BREAKING CHANGE:` footer the major version. `docs:`, `test:`, `ci:` and `chore:` do not release.

## Setup Requirements

- Repository secret `POWERSHELL_GALLERY_API_KEY`: a PowerShell Gallery API key with push rights for PwrSvg.
- Actions must be allowed to create and approve pull requests (repository or organization setting), so the release chain can open and merge the release PR.

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
