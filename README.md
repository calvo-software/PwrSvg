# PwrSvg
PowerShell module for rendering SVG files to raw image buffers using SkiaSharp, optimized for terminal and pipeline integration.

## Demo

See PwrSvg in action with terminal graphics rendering:

```powershell
# Direct SVG string to terminal
"<svg width='100' height='100'><circle cx='50' cy='50' r='40' fill='#ff6b6b' stroke='#333' stroke-width='3'/></svg>" | ConvertTo-Png |% { ConvertTo-Sixel -stream $_ }

# Or use existing SVG files
ConvertTo-Png -Path "test.svg" |% { ConvertTo-Sixel -stream $_ }
```

![Terminal Demo](https://private-user-images.githubusercontent.com/2091582/494918632-16c01d78-2dfd-41df-b059-788026cba6e9.png?jwt=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3NTkwOTcyMDAsIm5iZiI6MTc1OTA5NjkwMCwicGF0aCI6Ii8yMDkxNTgyLzQ5NDkxODYzMi0xNmMwMWQ3OC0yZGZkLTQxZGYtYjA1OS03ODgwMjZjYmE2ZTkucG5nP1gtQW16LUFsZ29yaXRobT1BV1M0LUhNQUMtU0hBMjU2JlgtQW16LUNyZWRlbnRpYWw9QUtJQVZDT0RZTFNBNTNQUUs0WkElMkYyMDI1MDkyOCUyRnVzLWVhc3QtMSUyRnMzJTJGYXdzNF9yZXF1ZXN0JlgtQW16LURhdGU9MjAyNTA5MjhUMjIwMTQwWiZYLUFtei1FeHBpcmVzPTMwMCZYLUFtei1TaWduYXR1cmU9YzUzYjBkZjZlNjI4YzY2NjE3YzMwNDM2OWQ0MTlkNjlkYjQyMjRhMWJmNmZjZDMxYzNlM2M1NmFjNWJmMmIyMSZYLUFtei1TaWduZWRIZWFkZXJzPWhvc3QifQ.25mN9vIzSErSiTB6XrwYgktmt3FjvmzQjftaUF9m994)

*The screenshot above shows Windows Terminal Preview on WSL Ubuntu displaying a rendered circle directly in the terminal using PwrSvg's pipeline integration with Sixel graphics.*

### Why PwrSvg?

- **ConvertTo-Png Cmdlet**: Render SVG to PNG in-memory or to disk
- **Pipeline-First Design**: Seamlessly integrates with PowerShell pipelines and terminal graphics protocols
- **Zero File I/O**: Process SVG directly from strings or files to memory streams  
- **Terminal Graphics**: Perfect companion for `ConvertTo-Sixel` and other terminal image tools
- **High-Performance**: SkiaSharp rendering engine with automatic SVG dimension detection
- **Cross-Platform**: Works on Windows, Linux, and macOS with PowerShell Core

## Installation

```powershell
Install-Module -Name PwrSvg -Force
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

## Requirements

- PowerShell 5.1 or later
- .NET 8.0 runtime (or .NET Framework 4.8 for Windows PowerShell)

## Architecture

This module uses:
- **SkiaSharp**: High-performance 2D graphics engine with automatic SVG dimension detection
- **Svg.Skia**: Mature SVG parsing and rendering with native bound extraction  
- **PowerShell Cmdlets**: Native PowerShell integration

Designed for headless server environments and terminal workflows. SkiaSharp provides superior SVG processing with automatic dimension detection from the SVG content structure.

## Building from Source

If you prefer to build from source instead of using the published module:

### Method 1: .NET 8.0 (Recommended)
```powershell
# Clone and build
git clone https://github.com/calvo-software/PwrSvg.git
cd PwrSvg/PwrSvg
dotnet publish -c Release -f net8.0 -o bin/Publish

# Import the published module
Import-Module ./bin/Publish/PwrSvg.dll
```

### Method 2: Windows PowerShell (.NET Framework)
```powershell
# For Windows PowerShell 5.1
dotnet publish -c Release -f net48 -o bin/Publish-Net48
Import-Module ./bin/Publish-Net48/PwrSvg.dll
```
