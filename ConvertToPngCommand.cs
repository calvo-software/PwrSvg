using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using SkiaSharp;
using Svg.Skia;

namespace PwrSvg
{
    [Cmdlet(VerbsData.ConvertTo, "Png")]
    [OutputType(typeof(byte[]))]
    public class ConvertToPngCommand : PSCmdlet
    {
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Path to the SVG file to convert")]
        [ValidateNotNullOrEmpty]
        [Alias("Path", "FilePath")]
        public string InputPath { get; set; }

        [Parameter(
            HelpMessage = "Width of the output PNG image in pixels")]
        [ValidateRange(1, 10000)]
        public int Width { get; set; } = 0;

        [Parameter(
            HelpMessage = "Height of the output PNG image in pixels")]
        [ValidateRange(1, 10000)]
        public int Height { get; set; } = 0;

        [Parameter(
            HelpMessage = "Background color for the output image (default: transparent)")]
        public string BackgroundColor { get; set; } = "transparent";

        [Parameter(
            HelpMessage = "Quality of the PNG output (1-100, default: 90)")]
        [ValidateRange(1, 100)]
        public int Quality { get; set; } = 90;

        protected override void ProcessRecord()
        {
            try
            {
                // Resolve the path
                string resolvedPath = GetResolvedProviderPathFromPSPath(InputPath, out ProviderInfo provider).FirstOrDefault();
                
                if (string.IsNullOrEmpty(resolvedPath) || !File.Exists(resolvedPath))
                {
                    WriteError(new ErrorRecord(
                        new FileNotFoundException($"SVG file not found: {InputPath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        InputPath));
                    return;
                }

                // Load and render the SVG
                byte[] pngBytes = ConvertSvgToPng(resolvedPath);
                
                if (pngBytes != null && pngBytes.Length > 0)
                {
                    WriteObject(pngBytes);
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException("Failed to convert SVG to PNG"),
                        "ConversionFailed",
                        ErrorCategory.InvalidOperation,
                        InputPath));
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "ConversionError",
                    ErrorCategory.InvalidOperation,
                    InputPath));
            }
        }

        private byte[] ConvertSvgToPng(string svgFilePath)
        {
            try
            {
                // Initialize SkiaSharp explicitly
                var testInfo = SKImageInfo.Empty;
                
                // Load the SVG
                var svg = new SKSvg();
                var picture = svg.Load(svgFilePath);

                if (picture == null)
                {
                    WriteWarning($"Failed to load SVG from: {svgFilePath}");
                    return null;
                }

                // Get dimensions
                var bounds = picture.CullRect;
                int renderWidth = Width > 0 ? Width : (int)Math.Max(bounds.Width, 100);
                int renderHeight = Height > 0 ? Height : (int)Math.Max(bounds.Height, 100);

                if (renderWidth <= 0 || renderHeight <= 0)
                {
                    renderWidth = 512;  // Default fallback
                    renderHeight = 512;
                }

                // Parse background color
                SKColor backgroundColor = ParseBackgroundColor(BackgroundColor);

                // Create bitmap and canvas
                using var bitmap = new SKBitmap(renderWidth, renderHeight);
                using var canvas = new SKCanvas(bitmap);

                // Clear with background color
                canvas.Clear(backgroundColor);

                // Scale to fit the specified dimensions
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    float scaleX = renderWidth / bounds.Width;
                    float scaleY = renderHeight / bounds.Height;
                    
                    // Use uniform scaling to maintain aspect ratio
                    float scale = Math.Min(scaleX, scaleY);
                    
                    canvas.Scale(scale);

                    // Center the image
                    float offsetX = (renderWidth / scale - bounds.Width) / 2;
                    float offsetY = (renderHeight / scale - bounds.Height) / 2;
                    canvas.Translate(offsetX, offsetY);
                }

                // Draw the SVG
                canvas.DrawPicture(picture);

                // Encode to PNG
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, Quality);
                return data.ToArray();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Error processing SVG: {ex.Message}", ex),
                    "SvgProcessingError",
                    ErrorCategory.InvalidOperation,
                    svgFilePath));
                return null;
            }
        }

        private SKColor ParseBackgroundColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString) || colorString.Equals("transparent", StringComparison.OrdinalIgnoreCase))
            {
                return SKColors.Transparent;
            }

            try
            {
                // Try to parse as hex color
                if (colorString.StartsWith("#"))
                {
                    return SKColor.Parse(colorString);
                }

                // Try to parse as named color
                var colorProperty = typeof(SKColors).GetProperty(colorString, 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
                if (colorProperty != null)
                {
                    return (SKColor)colorProperty.GetValue(null);
                }

                // Fallback: try parsing directly
                return SKColor.Parse(colorString);
            }
            catch
            {
                WriteWarning($"Could not parse color '{colorString}', using transparent background");
                return SKColors.Transparent;
            }
        }
    }
}