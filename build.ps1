#!/usr/bin/env pwsh
#
# Build script for PwrSvg PowerShell module
#

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [Parameter()]
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'

# Get the module directory
$ModuleRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$OutputPath = Join-Path $ModuleRoot "bin/$Configuration/net6.0"

Write-Host "Building PwrSvg module..." -ForegroundColor Green
Write-Host "Module Root: $ModuleRoot" -ForegroundColor Gray
Write-Host "Configuration: $Configuration" -ForegroundColor Gray

try {
    # Clean if requested
    if ($Clean) {
        Write-Host "Cleaning previous build artifacts..." -ForegroundColor Yellow
        if (Test-Path (Join-Path $ModuleRoot "bin")) {
            Remove-Item (Join-Path $ModuleRoot "bin") -Recurse -Force
        }
        if (Test-Path (Join-Path $ModuleRoot "obj")) {
            Remove-Item (Join-Path $ModuleRoot "obj") -Recurse -Force
        }
    }

    # Check for .NET SDK
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw ".NET SDK not found. Please install .NET 6.0 or later."
    }
    Write-Host "Using .NET SDK version: $dotnetVersion" -ForegroundColor Gray

    # Restore NuGet packages
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore $ModuleRoot
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed"
    }

    # Build the project
    Write-Host "Building project..." -ForegroundColor Yellow
    dotnet build $ModuleRoot --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed"
    }

    # Copy the built DLL to the module root
    $SourceDll = Join-Path $OutputPath "PwrSvg.dll"
    $TargetDll = Join-Path $ModuleRoot "PwrSvg.dll"
    
    if (Test-Path $SourceDll) {
        Copy-Item $SourceDll $TargetDll -Force
        Write-Host "Copied PwrSvg.dll to module root" -ForegroundColor Green
    } else {
        throw "Built DLL not found at: $SourceDll"
    }

    # Copy dependent assemblies
    $DependentAssemblies = @(
        "SkiaSharp.dll",
        "Svg.Skia.dll",
        "ExCSS.dll",
        "Fizzler.dll",
        "HarfBuzzSharp.dll",
        "ShimSkiaSharp.dll",
        "SkiaSharp.HarfBuzz.dll",
        "Svg.Custom.dll",
        "Svg.Model.dll"
    )

    foreach ($Assembly in $DependentAssemblies) {
        $SourcePath = Join-Path $OutputPath $Assembly
        $TargetPath = Join-Path $ModuleRoot $Assembly
        
        if (Test-Path $SourcePath) {
            Copy-Item $SourcePath $TargetPath -Force
            Write-Host "Copied $Assembly to module root" -ForegroundColor Green
        } else {
            Write-Warning "Dependent assembly not found: $SourcePath"
        }
    }

    # Copy runtime native libraries
    $RuntimesPath = Join-Path $OutputPath "runtimes"
    $TargetRuntimesPath = Join-Path $ModuleRoot "runtimes"
    
    if (Test-Path $RuntimesPath) {
        if (Test-Path $TargetRuntimesPath) {
            Remove-Item $TargetRuntimesPath -Recurse -Force
        }
        Copy-Item $RuntimesPath $TargetRuntimesPath -Recurse -Force
        Write-Host "Copied runtime native libraries to module root" -ForegroundColor Green
        
        # Copy the appropriate native library for the current platform to the module root
        $Platform = if ($IsWindows) { "win-x64" } elseif ($IsMacOS) { "osx" } else { "linux-x64" }
        $NativeLibPath = Join-Path $TargetRuntimesPath "$Platform/native"
        
        if (Test-Path $NativeLibPath) {
            Get-ChildItem $NativeLibPath | ForEach-Object {
                $TargetNativeLib = Join-Path $ModuleRoot $_.Name
                Copy-Item $_.FullName $TargetNativeLib -Force
                Write-Host "Copied $($_.Name) to module root for current platform" -ForegroundColor Green
            }
        }
    }

    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Module files are ready in: $ModuleRoot" -ForegroundColor Gray

} catch {
    Write-Error "Build failed: $_"
    exit 1
}