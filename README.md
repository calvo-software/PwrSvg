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
# Get PNG as MemoryStream for further processing
$pngStream = ConvertTo-Png -Path "input.svg"

# Pipeline with file processing
Get-ChildItem "*.svg" | ForEach-Object {
    ConvertTo-Png -Path $_.FullName -OutFile ($_.BaseName + ".png")
}
```

### Parameters

- **Path**: Input SVG file path (mandatory)
- **OutFile**: Output PNG file path (optional - returns readonly MemoryStream if not specified)
- **Width**: Output width in pixels (optional - uses SVG dimensions if not specified)
- **Height**: Output height in pixels (optional - uses SVG dimensions if not specified)
- **BackgroundColor**: Background color (optional - "Transparent", "White", "Black", or hex color like "#FFFFFF")
- **Quality**: PNG compression quality 0-100 (optional - default 95)

## Installation

**⚠️ Important**: This module requires proper deployment of native libraries to function correctly.

### Method 1: Use Published Build (Recommended)

```powershell
# Build and publish the module with all native dependencies
cd PwrSvg
dotnet publish -c Release -f net8.0 -o bin/Publish

# Import the published module (includes all native libraries)
Import-Module ./bin/Publish/PwrSvg.dll
```

### Method 2: Windows PowerShell (.NET Framework)

```powershell
# For Windows PowerShell 5.1
dotnet publish -c Release -f net48 -o bin/Publish-Net48
Import-Module ./bin/Publish-Net48/PwrSvg.dll
```

### Common Issues

If you see errors like:
- `The type initializer for 'SkiaSharp.SKImageInfo' threw an exception`
- `Failed to initialize SkiaSharp SVG engine`

**Solution**: Always use `dotnet publish` instead of `dotnet build`. The publish command ensures all native libraries (libSkiaSharp.dll/.so/.dylib) are properly deployed for your platform.

## Requirements

- PowerShell 5.1 or later
- .NET 8.0 runtime (or .NET Framework 4.8 for Windows PowerShell)

## Architecture

This module uses:
- **SkiaSharp**: High-performance 2D graphics engine with automatic SVG dimension detection
- **Svg.Skia**: Mature SVG parsing and rendering with native bound extraction  
- **PowerShell Cmdlets**: Native PowerShell integration

Designed for headless server environments and terminal workflows. SkiaSharp provides superior SVG processing with automatic dimension detection from the SVG content structure.
