# PwrSvg Integration Tests

`PwrSvg.Integration.Tests.ps1` holds Pester tests that import the module from
the same layout that is published to the PowerShell Gallery and exercise its
commands.

## The module layout

`build/New-ModuleLayout.ps1` builds `artifacts/PwrSvg`:

```
artifacts/PwrSvg/
  PwrSvg.psd1
  PwrSvg.psm1      loads the assembly for the running PowerShell
  Out-ConsoleSvg.ps1
  net8/     PwrSvg.dll for PowerShell 7
  net48/    PwrSvg.dll for Windows PowerShell (built on Windows only)
```

PwrSvg.psm1 loads the assembly for the PowerShell that imports it, so the
tests take only the module path and run unchanged under Windows PowerShell and
PowerShell 7. CI tests this layout, and the release packages the same one.

## Running the tests

```powershell
Install-Module Sixel -Scope CurrentUser   # required by the module
./integration-test.ps1                    # build the layout, then test it
./integration-test.ps1 -SkipBuild         # test the existing layout
```

Run it from `powershell` to test the net48 assembly and from `pwsh` to test
the net8 one.

## What is tested

- The module loads from the layout under test and exposes `ConvertTo-Png`
  and `Out-ConsoleSvg`.
- `ConvertTo-Png` renders a file and pipeline markup to a PNG stream, writes a
  PNG file with `-OutFile`, and honours `-Width` and `-Height`.
- `Out-ConsoleSvg` emits Sixel output.

## CI

`.github/scripts/Run-PesterCI.ps1 -ModulePath <layout>` runs the tests and
writes JUnit XML to `pester-results-<edition>.xml`, which
`dorny/test-reporter` shows on the run. On Windows the tests run under both
Windows PowerShell and PowerShell 7; on Linux and macOS under PowerShell 7.

## Requirements

- Windows PowerShell 5.1 or PowerShell 7
- Pester 5 (installed by the runner if missing)
- The Sixel module
- .NET SDK 8.0 or later to build the layout
