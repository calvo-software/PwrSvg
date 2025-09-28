# PwrSvg
PowerShell module for rendering SVG files to raw image buffers using SkiaSharp, optimized for terminal and pipeline integration.

## Installation

Import the module from the directory containing the module files:

```powershell
Import-Module ./PwrSvg.psd1
```

## Usage

### ConvertTo-Png

Convert SVG files to PNG format as raw binary data.

```powershell
# Basic conversion
$bytes = ConvertTo-Png -InputPath "icon.svg"
[System.IO.File]::WriteAllBytes("output.png", $bytes)

# Specify dimensions
$bytes = ConvertTo-Png -InputPath "icon.svg" -Width 256 -Height 256

# With background color
$bytes = ConvertTo-Png -InputPath "icon.svg" -Width 512 -Height 512 -BackgroundColor "white"

# Pipeline usage
Get-ChildItem "*.svg" | ForEach-Object {
    $png = ConvertTo-Png -InputPath $_.FullName -Width 64 -Height 64
    [System.IO.File]::WriteAllBytes("$($_.BaseName).png", $png)
}
```

### Parameters

- **InputPath**: Path to the SVG file to convert (mandatory)
- **Width**: Width of the output PNG image in pixels (optional, defaults to SVG width)
- **Height**: Height of the output PNG image in pixels (optional, defaults to SVG height)
- **BackgroundColor**: Background color for the output image (optional, default: transparent)
  - Supports named colors: white, black, red, green, blue, etc.
  - Supports hex colors: #FF0000, #00FF00, etc.
- **Quality**: PNG quality (1-100, default: 90)

## Requirements

- PowerShell 5.1 or later
- .NET Framework 4.7.2 or .NET Core/5+

## Examples

```powershell
# Convert SVG to PNG with specific size and white background
$pngData = ConvertTo-Png -InputPath "logo.svg" -Width 300 -Height 300 -BackgroundColor "white"

# Save to file
[System.IO.File]::WriteAllBytes("logo.png", $pngData)

# Process multiple files
Get-ChildItem "icons/*.svg" | ForEach-Object {
    $pngBytes = ConvertTo-Png -InputPath $_.FullName -Width 48 -Height 48
    $outputPath = Join-Path "output" "$($_.BaseName).png"
    [System.IO.File]::WriteAllBytes($outputPath, $pngBytes)
}
```