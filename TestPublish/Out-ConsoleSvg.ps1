function Out-ConsoleSvg {
    <#
    .SYNOPSIS
    Outputs SVG content directly to the console using Sixel graphics.
    
    .DESCRIPTION
    The Out-ConsoleSvg cmdlet converts SVG content to PNG format and then outputs it to the console as Sixel graphics.
    This is a convenience function that combines ConvertTo-Png and ConvertTo-Sixel functionality in a single pipeline step.
    
    .PARAMETER SvgContent
    The SVG content string to convert and display. Can be piped from another command.
    
    .PARAMETER Width
    Width of the output image. If not specified, uses SVG's natural width.
    
    .PARAMETER Height
    Height of the output image. If not specified, uses SVG's natural height.
    
    .PARAMETER BackgroundColor
    Background color for the PNG (e.g., 'White', 'Transparent', '#FFFFFF'). Default is 'Transparent'.
    
    .PARAMETER Quality
    Quality/compression level for PNG (0-100). Default is 95.
    
    .EXAMPLE
    "<svg width='100' height='100'><circle cx='50' cy='50' r='40' fill='#ff6b6b' stroke='#333' stroke-width='3'/></svg>" | Out-ConsoleSvg
    
    Displays the SVG circle directly in the console as Sixel graphics.
    
    .EXAMPLE
    Get-Content circle.svg | Out-ConsoleSvg -Width 200 -Height 200
    
    Reads an SVG file and displays it in the console with custom dimensions.
    
    .NOTES
    This function requires the Sixel module to be installed for console output.
    If the Sixel module is not available, an error will be thrown.
    #>
    [CmdletBinding()]
    param(
        [Parameter(
            Mandatory = $true,
            ValueFromPipeline = $true,
            HelpMessage = "SVG content string to convert and display"
        )]
        [string]$SvgContent,
        
        [Parameter(
            Mandatory = $false,
            HelpMessage = "Width of the output image"
        )]
        [ValidateRange(1, [int]::MaxValue)]
        [int]$Width = 0,
        
        [Parameter(
            Mandatory = $false,
            HelpMessage = "Height of the output image"
        )]
        [ValidateRange(1, [int]::MaxValue)]
        [int]$Height = 0,
        
        [Parameter(
            Mandatory = $false,
            HelpMessage = "Background color for the PNG (e.g., 'White', 'Transparent', '#FFFFFF')"
        )]
        [string]$BackgroundColor = "Transparent",
        
        [Parameter(
            Mandatory = $false,
            HelpMessage = "Quality/compression level for PNG (0-100)"
        )]
        [ValidateRange(0, 100)]
        [int]$Quality = 95
    )
    
    process {
        try {
            # Check if Sixel module is available
            if (-not (Get-Module -ListAvailable -Name Sixel)) {
                throw "The Sixel module is required for Out-ConsoleSvg but is not installed. Please install it using: Install-Module Sixel"
            }
            
            # Import Sixel module if not already imported
            if (-not (Get-Module -Name Sixel)) {
                Import-Module Sixel -ErrorAction Stop
            }
            
            # Convert SVG to PNG using ConvertTo-Png
            $convertParams = @{
                SvgContent = $SvgContent
                BackgroundColor = $BackgroundColor
                Quality = $Quality
            }
            
            if ($Width -gt 0) {
                $convertParams.Width = $Width
            }
            
            if ($Height -gt 0) {
                $convertParams.Height = $Height
            }
            
            $pngStream = ConvertTo-Png @convertParams
            
            # Convert PNG stream to Sixel and output to console
            ConvertTo-Sixel -Stream $pngStream
            
        } catch {
            Write-Error "Failed to output SVG to console: $_"
        }
    }
}