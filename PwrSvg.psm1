# PowerShell module for PwrSvg
# This module provides functionality to convert SVG files to raw image buffers using SkiaSharp

# Import the binary module
$ModulePath = Split-Path -Parent $MyInvocation.MyCommand.Path
$BinaryModulePath = Join-Path $ModulePath "PwrSvg.dll"

if (Test-Path $BinaryModulePath) {
    Import-Module $BinaryModulePath -Force
    Write-Verbose "PwrSvg binary module loaded from: $BinaryModulePath"
} else {
    throw "PwrSvg binary module not found at: $BinaryModulePath. Please ensure the module is properly built."
}

# Export the cmdlets
Export-ModuleMember -Cmdlet Convert-SvgToImageBuffer