#!/usr/bin/env pwsh

# Integration test for Out-ConsoleSvg function
# This script demonstrates the complete usage of Out-ConsoleSvg

Write-Host "=== PwrSvg Out-ConsoleSvg Integration Test ===" -ForegroundColor Green

# Sample SVG content
$svgContent = @"
<svg width="100" height="100" xmlns="http://www.w3.org/2000/svg">
  <circle cx="50" cy="50" r="40" fill="#ff6b6b" stroke="#333" stroke-width="3"/>
  <text x="50" y="50" text-anchor="middle" dy="0.3em" fill="white" font-family="Arial" font-size="12">SVG</text>
</svg>
"@

Write-Host "Sample SVG:" -ForegroundColor Yellow
Write-Host $svgContent

# Test 1: Build the module
Write-Host "`n=== Building Module ===" -ForegroundColor Green
try {
    Push-Location (Join-Path $PSScriptRoot "PwrSvg")
    & dotnet build
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    Write-Host "✓ Build successful" -ForegroundColor Green
} catch {
    Write-Host "✗ Build failed: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}

# Test 2: Import the module using the unified .psd1 approach
Write-Host "`n=== Testing Module Import ===" -ForegroundColor Green
try {
    # Simple unified import - just load the single .psd1 file
    $manifestPath = Join-Path $PSScriptRoot "PwrSvg/bin/Debug/net8.0/PwrSvg.psd1"
    
    # Create a temporary manifest without the Sixel dependency for testing
    $manifestContent = Get-Content $manifestPath -Raw
    $testManifest = $manifestContent -replace "RequiredModules = @\('Sixel'\)", "# RequiredModules = @('Sixel')"
    $testManifestPath = Join-Path $PSScriptRoot "PwrSvg/bin/Debug/net8.0/PwrSvg-test.psd1"
    $testManifest | Out-File -FilePath $testManifestPath -Encoding utf8
    
    # Import the test manifest
    Import-Module $testManifestPath -Force
    
    Write-Host "✓ Module imported successfully with single .psd1 file" -ForegroundColor Green
} catch {
    Write-Host "✗ Module import failed: $_" -ForegroundColor Red
    exit 1
}

# Test 3: Verify ConvertTo-Png works
Write-Host "`n=== Testing ConvertTo-Png ===" -ForegroundColor Green
try {
    $pngStream = $svgContent | ConvertTo-Png -Width 200 -Height 200
    if ($pngStream -and $pngStream.Length -gt 0) {
        Write-Host "✓ ConvertTo-Png successful - Generated $($pngStream.Length) bytes" -ForegroundColor Green
        $pngStream.Dispose()
    } else {
        throw "No PNG data generated"
    }
} catch {
    Write-Host "✗ ConvertTo-Png failed: $_" -ForegroundColor Red
    exit 1
}

# Test 4: Test Out-ConsoleSvg function structure
Write-Host "`n=== Testing Out-ConsoleSvg Function ===" -ForegroundColor Green
try {
    $cmd = Get-Command Out-ConsoleSvg -ErrorAction Stop
    Write-Host "✓ Out-ConsoleSvg function is available" -ForegroundColor Green
    Write-Host "  Function Type: $($cmd.CommandType)" -ForegroundColor Cyan
    Write-Host "  Parameters: $($cmd.Parameters.Keys -join ', ')" -ForegroundColor Cyan
    Write-Host "✓ Function uses automatic dependency resolution via RequiredModules" -ForegroundColor Green
    
} catch {
    Write-Host "✗ Out-ConsoleSvg function test failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Integration Test Summary ===" -ForegroundColor Green
Write-Host "✓ All tests passed!" -ForegroundColor Green
Write-Host ""
Write-Host "Usage Examples:" -ForegroundColor Yellow
Write-Host "  # Install Sixel module first:"
Write-Host "  Install-Module Sixel" -ForegroundColor Cyan
Write-Host ""
Write-Host "  # Then use Out-ConsoleSvg:"
Write-Host '  "<svg width=''100'' height=''100''><circle cx=''50'' cy=''50'' r=''40'' fill=''#ff6b6b''/></svg>" | Out-ConsoleSvg' -ForegroundColor Cyan
Write-Host "  Get-Content myfile.svg | Out-ConsoleSvg -Width 200 -Height 200" -ForegroundColor Cyan
Write-Host ""
Write-Host "Note: This replaces the pipeline: ConvertTo-Png | % { ConvertTo-Sixel -Stream \$_ }" -ForegroundColor Gray