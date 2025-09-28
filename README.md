# PwrSvg
PowerShell module for rendering SVG files to raw image buffers using SkiaSharp, optimized for terminal and pipeline integration.

## Features

- **ConvertTo-Png** — Render SVG to PNG in-memory or to disk
- Pipeline-friendly design for composability
- High-performance SkiaSharp rendering engine
- Terminal and headless server optimized

## Installation

### From Source
```powershell
# Clone and build
git clone https://github.com/calvo-software/PwrSvg.git
cd PwrSvg/PwrSvg
dotnet publish -c Release -o bin/Publish

# Import module
Import-Module ./bin/Publish/PwrSvg.dll
```

## Usage

### Basic SVG to PNG Conversion
```powershell
# Convert SVG to PNG file
ConvertTo-Png -Path "input.svg" -OutFile "output.png"

# Convert with custom dimensions
ConvertTo-Png -Path "input.svg" -OutFile "output.png" -Width 800 -Height 600

# Convert with background color
ConvertTo-Png -Path "input.svg" -OutFile "output.png" -BackgroundColor "White"
```

### Pipeline Integration
```powershell
# Get PNG as byte array for further processing
$pngBytes = ConvertTo-Png -Path "input.svg"

# Pipeline with file processing
Get-ChildItem "*.svg" | ForEach-Object {
    ConvertTo-Png -Path $_.FullName -OutFile ($_.BaseName + ".png")
}
```

### Parameters

- **Path**: Input SVG file path (mandatory)
- **OutFile**: Output PNG file path (optional - returns byte array if not specified)
- **Width**: Output width in pixels (optional - uses SVG dimensions if not specified)
- **Height**: Output height in pixels (optional - uses SVG dimensions if not specified)
- **BackgroundColor**: Background color (optional - "Transparent", "White", "Black", or hex color like "#FFFFFF")
- **Quality**: PNG compression quality 0-100 (optional - default 95)

## Requirements

- PowerShell 5.1 or later
- .NET 8.0 runtime
- Linux: fontconfig system package

## Architecture

This module uses:
- **SixLabors.ImageSharp**: High-performance, cross-platform 2D graphics engine
- **Enhanced SVG Parsing**: XML-based SVG content analysis and rendering
- **PowerShell Cmdlets**: Native PowerShell integration

Designed for headless server environments and terminal workflows with excellent Linux compatibility.
