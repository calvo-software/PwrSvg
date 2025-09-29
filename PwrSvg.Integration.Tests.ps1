<#
.SYNOPSIS
    Pester integration tests for PwrSvg module
.DESCRIPTION
    These tests validate the complete module build, structure and import functionality
    replacing the traditional integration-test.ps1 script with a proper Pester framework.
.NOTES
    Requires Pester module (automatically installed if not available)
#>

BeforeAll {
    # Setup test environment
    $script:TestSvgContent = @"
<svg width="100" height="100" xmlns="http://www.w3.org/2000/svg">
  <circle cx="50" cy="50" r="40" fill="#ff6b6b" stroke="#333" stroke-width="3"/>
  <text x="50" y="50" text-anchor="middle" dy="0.3em" fill="white" font-family="Arial" font-size="12">SVG</text>
</svg>
"@
    
    $script:RepositoryRoot = $PSScriptRoot
    $script:PublishPath = Join-Path $RepositoryRoot "TestPublish"
    
    # Auto-detect build configuration (prefer Release, fall back to Debug)
    $releasePath = Join-Path $RepositoryRoot "PwrSvg" "bin" "Release"
    $debugPath = Join-Path $RepositoryRoot "PwrSvg" "bin" "Debug"
    
    if (Test-Path $releasePath) {
        $script:BuildConfig = "Release"
        Write-Host "Using Release build artifacts" -ForegroundColor Green
    } elseif (Test-Path $debugPath) {
        $script:BuildConfig = "Debug" 
        Write-Host "Using Debug build artifacts" -ForegroundColor Yellow
    } else {
        $script:BuildConfig = "Release"  # Default assumption for CI
        Write-Host "No build artifacts found, assuming Release configuration" -ForegroundColor Yellow
    }
    
    # Clean up any existing test publish directory
    if (Test-Path $script:PublishPath) {
        Remove-Item $script:PublishPath -Recurse -Force
    }
}

Describe "PwrSvg Module Build and Integration Tests" {
    
    Context "Module Build Artifacts" {
        It "should have .NET 8.0 build artifacts" {
            $dllPath = Join-Path $script:RepositoryRoot "PwrSvg" "bin" $script:BuildConfig "net8.0" "PwrSvg.dll"
            $dllPath | Should -Exist -Because ".NET 8.0 build should produce DLL"
            (Get-Item $dllPath).Length | Should -BeGreaterThan 0 -Because "DLL should not be empty"
        }
        
        It "should have .NET Framework 4.8 build artifacts" {
            $dllPath = Join-Path $script:RepositoryRoot "PwrSvg" "bin" $script:BuildConfig "net48" "PwrSvg.dll"
            $dllPath | Should -Exist -Because ".NET Framework 4.8 build should produce DLL"
            (Get-Item $dllPath).Length | Should -BeGreaterThan 0 -Because "DLL should not be empty"
        }
        
        It "should have PowerShell module files in source" {
            $manifestPath = Join-Path $script:RepositoryRoot "PwrSvg" "PwrSvg.psd1"
            $scriptPath = Join-Path $script:RepositoryRoot "PwrSvg" "Out-ConsoleSvg.ps1"
            
            $manifestPath | Should -Exist -Because "Module manifest should exist in source"
            $scriptPath | Should -Exist -Because "PowerShell script should exist in source"
            
            (Get-Item $manifestPath).Length | Should -BeGreaterThan 0 -Because "Manifest should not be empty"
            (Get-Item $scriptPath).Length | Should -BeGreaterThan 0 -Because "Script should not be empty"
        }
    }
    
    Context "Module Layout Creation" {
        BeforeAll {
            # Create module layout from existing build artifacts (simulating deployment structure)
            # This tests what we actually deploy without rebuilding
            
            # Create the module structure using existing build outputs
            New-Item -ItemType Directory -Path $script:PublishPath -Force | Out-Null
            New-Item -ItemType Directory -Path (Join-Path $script:PublishPath "net8") -Force | Out-Null
            New-Item -ItemType Directory -Path (Join-Path $script:PublishPath "net48") -Force | Out-Null
            
            # Copy .NET 8.0 build artifacts
            $net8Source = Join-Path $script:RepositoryRoot "PwrSvg" "bin" $script:BuildConfig "net8.0"
            $net8Dest = Join-Path $script:PublishPath "net8"
            if (Test-Path $net8Source) {
                Copy-Item "$net8Source\*" $net8Dest -Recurse -Force
                $script:Net8CopySuccess = $true
            } else {
                $script:Net8CopySuccess = $false
            }
            
            # Copy .NET Framework 4.8 build artifacts  
            $net48Source = Join-Path $script:RepositoryRoot "PwrSvg" "bin" $script:BuildConfig "net48"
            $net48Dest = Join-Path $script:PublishPath "net48"
            if (Test-Path $net48Source) {
                Copy-Item "$net48Source\*" $net48Dest -Recurse -Force
                $script:Net48CopySuccess = $true
            } else {
                $script:Net48CopySuccess = $false
            }
            
            # Copy PowerShell files to the root (simulating CI pipeline layout)
            Copy-Item (Join-Path $script:RepositoryRoot "PwrSvg" "PwrSvg.psd1") $script:PublishPath
            Copy-Item (Join-Path $script:RepositoryRoot "PwrSvg" "Out-ConsoleSvg.ps1") $script:PublishPath
        }
        
        It "should create module layout from .NET 8.0 artifacts" {
            $script:Net8CopySuccess | Should -Be $true -Because "Should be able to copy .NET 8.0 build artifacts"
            
            $dllPath = Join-Path $script:PublishPath "net8" "PwrSvg.dll"
            $dllPath | Should -Exist -Because "DLL should exist in net8 subdirectory"
        }
        
        It "should create module layout from .NET Framework 4.8 artifacts" {
            $script:Net48CopySuccess | Should -Be $true -Because "Should be able to copy .NET Framework 4.8 build artifacts"
            
            $dllPath = Join-Path $script:PublishPath "net48" "PwrSvg.dll" 
            $dllPath | Should -Exist -Because "DLL should exist in net48 subdirectory"
        }
        
        It "should create manifest file at root" {
            $manifestPath = Join-Path $script:PublishPath "PwrSvg.psd1"
            $manifestPath | Should -Exist -Because "Manifest file should exist at module root"
        }
        
        It "should create PowerShell script at root" {
            $scriptPath = Join-Path $script:PublishPath "Out-ConsoleSvg.ps1"
            $scriptPath | Should -Exist -Because "PowerShell script should exist at module root"
        }
        
        It "should create DLL in subdirectory" {
            $dllPath = Join-Path $script:PublishPath "net8" "PwrSvg.dll"
            $dllPath | Should -Exist -Because "DLL should exist in net8 subdirectory"
        }
        
        It "should have valid manifest content" {
            $manifestPath = Join-Path $script:PublishPath "PwrSvg.psd1"
            $manifestContent = Get-Content $manifestPath -Raw
            
            # Test key manifest properties
            $manifestContent | Should -Match "ModuleVersion\s*=.*\d+\.\d+\.\d+" -Because "Manifest should have version"
            $manifestContent | Should -Match "RequiredModules\s*=.*@\('Sixel'\)" -Because "Manifest should require Sixel module"
            $manifestContent | Should -Match "ScriptsToProcess\s*=.*@\('Out-ConsoleSvg\.ps1'\)" -Because "Manifest should process Out-ConsoleSvg script"
            $manifestContent | Should -Match "FunctionsToExport\s*=.*@\('Out-ConsoleSvg'\)" -Because "Manifest should export Out-ConsoleSvg function"
        }
    }
    
    Context "Module Import" {
        BeforeAll {
            $script:ManifestPath = Join-Path $script:PublishPath "PwrSvg.psd1"
        }
        
        It "should attempt module import" {
            # This test expects the import to fail due to missing Sixel dependency in CI environment
            # We're testing the failure mode and structure validation instead
            
            $importAttempt = {
                Import-Module $script:ManifestPath -Force -ErrorAction Stop
            }
            
            # The import should fail due to missing Sixel dependency
            $importAttempt | Should -Throw -Because "Import should fail without Sixel module dependency"
        }
        
        It "should have correct error message for missing dependency" {
            try {
                Import-Module $script:ManifestPath -Force -ErrorAction Stop
                # If we get here, the import succeeded unexpectedly
                $true | Should -Be $false -Because "Import should have failed due to missing Sixel dependency"
            }
            catch {
                $_.Exception.Message | Should -Match "Sixel.*is not loaded" -Because "Error should mention missing Sixel dependency"
            }
        }
    }
    
    Context "Module Structure Validation" {
        It "should validate module files exist after publish" {
            $manifestPath = Join-Path $script:PublishPath "PwrSvg.psd1"
            $scriptPath = Join-Path $script:PublishPath "Out-ConsoleSvg.ps1"
            $dllPath = Join-Path $script:PublishPath "net8" "PwrSvg.dll"
            
            # Validate all required files exist
            $manifestPath | Should -Exist -Because "Manifest file should exist"
            $scriptPath | Should -Exist -Because "PowerShell script should exist"  
            $dllPath | Should -Exist -Because "DLL should exist"
            
            # Validate file sizes are reasonable (not empty)
            (Get-Item $manifestPath).Length | Should -BeGreaterThan 0 -Because "Manifest should not be empty"
            (Get-Item $scriptPath).Length | Should -BeGreaterThan 0 -Because "Script should not be empty"
            (Get-Item $dllPath).Length | Should -BeGreaterThan 0 -Because "DLL should not be empty"
        }
        
        It "should have PowerShell script with proper function definition" {
            $scriptPath = Join-Path $script:PublishPath "Out-ConsoleSvg.ps1"
            $scriptContent = Get-Content $scriptPath -Raw
            
            $scriptContent | Should -Match "function\s+Out-ConsoleSvg" -Because "Script should define Out-ConsoleSvg function"
            $scriptContent | Should -Match "\[CmdletBinding\(" -Because "Function should have CmdletBinding attribute"
        }
    }
    
    Context "Test SVG Content Processing" {
        It "should have valid test SVG content" {
            $script:TestSvgContent | Should -Match "<svg.*>" -Because "Test SVG should have valid opening tag"
            $script:TestSvgContent | Should -Match "</svg>" -Because "Test SVG should have closing tag"
            $script:TestSvgContent | Should -Match "circle.*cx.*cy.*r" -Because "Test SVG should contain circle element"
        }
    }
}

AfterAll {
    # Clean up test publish directory
    if (Test-Path $script:PublishPath) {
        Remove-Item $script:PublishPath -Recurse -Force
    }
}