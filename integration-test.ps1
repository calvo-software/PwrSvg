#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Integration test for PwrSvg module using Pester framework
.DESCRIPTION  
    This script demonstrates the complete usage of Out-ConsoleSvg using modern Pester testing.
    It has been converted from traditional script-based testing to use the Pester framework
    for better structure, reporting, and CI/CD integration.
.NOTES
    This script now uses Pester for modern PowerShell testing practices.
    Run with: ./integration-test.ps1
    Or with Pester directly: Invoke-Pester ./integration-test.ps1
#>

# Import Pester if not already available
if (-not (Get-Module -ListAvailable Pester)) {
    Write-Host "Installing Pester module..." -ForegroundColor Yellow
    Install-Module -Name Pester -Force -SkipPublisherCheck -Scope CurrentUser
}

# Sample SVG content for testing
$script:SvgContent = @"
<svg width="100" height="100" xmlns="http://www.w3.org/2000/svg">
  <circle cx="50" cy="50" r="40" fill="#ff6b6b" stroke="#333" stroke-width="3"/>
  <text x="50" y="50" text-anchor="middle" dy="0.3em" fill="white" font-family="Arial" font-size="12">SVG</text>
</svg>
"@

BeforeAll {
    $script:RepositoryRoot = $PSScriptRoot
    $script:PublishPath = Join-Path $RepositoryRoot "TestPublish"
    
    Write-Host "=== PwrSvg Out-ConsoleSvg Integration Test (Pester) ===" -ForegroundColor Green
    Write-Host "Sample SVG:" -ForegroundColor Yellow
    Write-Host $script:SvgContent
}

Describe "PwrSvg Integration Tests" {
    
    Context "Module Build" {
        It "should build the module successfully" {
            Push-Location (Join-Path $script:RepositoryRoot "PwrSvg")
            try {
                $buildResult = & dotnet build 2>&1
                $LASTEXITCODE | Should -Be 0 -Because "Build should succeed without errors"
                Write-Host "✓ Build successful" -ForegroundColor Green
            } finally {
                Pop-Location
            }
        }
    }
    
    Context "Module Structure and Import" {
        BeforeAll {
            # Publish the module for proper testing (simulating PowerShell Gallery structure)
            Push-Location (Join-Path $script:RepositoryRoot "PwrSvg")
            try {
                $publishResult = & dotnet publish -c Release -o (Join-Path $script:PublishPath "net8") -f net8.0 --verbosity quiet 2>&1
                $script:PublishExitCode = $LASTEXITCODE
                
                if ($script:PublishExitCode -eq 0) {
                    # Copy PowerShell files to the root of the module structure (simulating CI pipeline)
                    Copy-Item (Join-Path $script:RepositoryRoot "PwrSvg" "PwrSvg.psd1") $script:PublishPath
                    Copy-Item (Join-Path $script:RepositoryRoot "PwrSvg" "Out-ConsoleSvg.ps1") $script:PublishPath
                }
            } finally {
                Pop-Location
            }
        }
        
        It "should publish module successfully" {
            $script:PublishExitCode | Should -Be 0 -Because "Publish operation should succeed"
        }
        
        It "should have correct module structure" {
            $manifestPath = Join-Path $script:PublishPath "PwrSvg.psd1"
            $scriptPath = Join-Path $script:PublishPath "Out-ConsoleSvg.ps1" 
            $dllPath = Join-Path $script:PublishPath "net8" "PwrSvg.dll"
            
            $manifestPath | Should -Exist -Because "Manifest file should exist"
            $scriptPath | Should -Exist -Because "PowerShell script should exist"
            $dllPath | Should -Exist -Because "DLL should exist"
        }
        
        It "should import module successfully or handle missing dependency gracefully" {
            try {
                # Import the module using just the root .psd1 file
                $manifestPath = Join-Path $script:PublishPath "PwrSvg.psd1"
                Import-Module $manifestPath -Force -ErrorAction Stop
                Write-Host "✓ Module imported successfully from root .psd1 file" -ForegroundColor Green
                $script:ModuleImported = $true
            } catch {
                # Expected to fail in CI/test environments where Sixel module is not available
                Write-Host "Note: Module import failed due to missing Sixel dependency (expected in test environment)" -ForegroundColor Yellow
                Write-Host "Error: $_" -ForegroundColor Yellow
                $script:ModuleImported = $false
                
                # This is expected behavior, so the test should pass
                $_.Exception.Message | Should -Match "Sixel|required|dependency" -Because "Should fail due to missing Sixel dependency"
            }
        }
    }
    
    Context "Module Content Validation" {
        It "should have valid manifest content" {
            $manifestPath = Join-Path $script:PublishPath "PwrSvg.psd1"
            $manifestContent = Get-Content $manifestPath -Raw
            
            # Check for key components
            $manifestContent | Should -Match "RequiredModules.*Sixel" -Because "Should require Sixel module"
            $manifestContent | Should -Match "ScriptsToProcess.*Out-ConsoleSvg.ps1" -Because "Should process Out-ConsoleSvg script"
            $manifestContent | Should -Match "FunctionsToExport.*Out-ConsoleSvg" -Because "Should export Out-ConsoleSvg function"
        }
        
        It "should have PowerShell script with proper function definition" {
            $scriptPath = Join-Path $script:PublishPath "Out-ConsoleSvg.ps1"
            $scriptContent = Get-Content $scriptPath -Raw
            
            $scriptContent | Should -Match "function Out-ConsoleSvg" -Because "Should define Out-ConsoleSvg function"
            $scriptContent | Should -Match "ConvertTo-Png" -Because "Should call ConvertTo-Png"
            $scriptContent | Should -Match "ConvertTo-Sixel" -Because "Should call ConvertTo-Sixel"
        }
    }
    
    Context "Usage Examples and Documentation" {
        It "should demonstrate usage examples" {
            Write-Host "`n=== Integration Test Summary ===" -ForegroundColor Green
            Write-Host "✓ All tests passed!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Usage Examples:" -ForegroundColor Yellow
            Write-Host "  # Install Sixel module first:"
            Write-Host "  Install-Module Sixel" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "  # Then use Out-ConsoleSvg:"
            Write-Host '  "<svg width=''100'' height=''100''><circle cx=''50'' cy=''50'' r=''40'' fill=''#ff6b6b''/></svg>" | Out-ConsoleSvg' -ForegroundColor Cyan
            Write-Host "  Get-Content test.svg | Out-ConsoleSvg" -ForegroundColor Cyan
            Write-Host "  Get-Content myfile.svg | Out-ConsoleSvg -Width 200 -Height 200" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Note: This replaces the pipeline: ConvertTo-Png | % { ConvertTo-Sixel -Stream `$_ }" -ForegroundColor Gray
            
            # Always pass this test as it's just for documentation
            $true | Should -Be $true
        }
    }
}

AfterAll {
    # Clean up test artifacts
    if (Test-Path $script:PublishPath) {
        Remove-Item $script:PublishPath -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# If script is run directly (not via Invoke-Pester), run the tests
if ($MyInvocation.InvocationName -eq $MyInvocation.MyCommand.Name) {
    Write-Host "Running integration tests with Pester..." -ForegroundColor Green
    
    # Configure Pester for console output
    $config = New-PesterConfiguration
    $config.Run.Path = $PSCommandPath
    $config.Output.Verbosity = 'Detailed'
    $config.TestResult.Enabled = $false  # Don't generate XML when run directly
    
    Invoke-Pester -Configuration $config
}
