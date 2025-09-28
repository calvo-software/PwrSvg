# PwrSvg
PowerShell module for rendering SVG files to raw image buffers using SkiaSharp, optimized for terminal and pipeline integration.

## Features

- Convert SVG files to raw image buffers (byte arrays)
- Support for PNG, JPEG, and WebP output formats
- Customizable image dimensions and background colors
- Built on SkiaSharp for high-quality rendering
- Cross-platform support (Windows, macOS, Linux)
- Pipeline-friendly design for PowerShell automation

## Installation

### From Source

1. Clone this repository
2. Run the build script:
   ```powershell
   ./build.ps1
   ```
3. Import the module:
   ```powershell
   Import-Module ./PwrSvg.psd1
   ```

## Usage

### Basic Usage

Convert an SVG file to a PNG image buffer:
```powershell
$imageBytes = Convert-SvgToImageBuffer -InputPath "input.svg"
[System.IO.File]::WriteAllBytes("output.png", $imageBytes)
```

### Custom Dimensions

Specify custom width and height:
```powershell
$imageBytes = Convert-SvgToImageBuffer -InputPath "input.svg" -Width 800 -Height 600
```

### Different Output Formats

Convert to JPEG:
```powershell
$imageBytes = Convert-SvgToImageBuffer -InputPath "input.svg" -Format "Jpeg" -Quality 90
```

Convert to WebP:
```powershell
$imageBytes = Convert-SvgToImageBuffer -InputPath "input.svg" -Format "Webp"
```

### Custom Background Color

Set a custom background color:
```powershell
$imageBytes = Convert-SvgToImageBuffer -InputPath "input.svg" -BackgroundColor "White"
$imageBytes = Convert-SvgToImageBuffer -InputPath "input.svg" -BackgroundColor "#FF0000"
```

### Pipeline Usage

Process multiple SVG files:
```powershell
Get-ChildItem "*.svg" | ForEach-Object {
    $bytes = Convert-SvgToImageBuffer -InputPath $_.FullName
    [System.IO.File]::WriteAllBytes("$($_.BaseName).png", $bytes)
}
```

## Parameters

### Convert-SvgToImageBuffer

- **InputPath** (required): Path to the SVG file to convert
- **Width** (optional): Width of the output image (default: SVG width or 800)
- **Height** (optional): Height of the output image (default: SVG height or 600)
- **BackgroundColor** (optional): Background color for the output image (default: Transparent)
- **Format** (optional): Output image format - Png, Jpeg, or Webp (default: Png)
- **Quality** (optional): Quality for Jpeg format, 1-100 (default: 100)

## Requirements

- PowerShell 5.1 or later
- .NET 6.0 or later (for building)

## License

MIT License - see [LICENSE](LICENSE) file for details.
