using System;
using System.IO;
using System.Management.Automation;
using SkiaSharp;
using Svg.Skia;

namespace PwrSvg
{
    [Cmdlet(VerbsData.Convert, "SvgToImageBuffer")]
    [OutputType(typeof(byte[]))]
    public class ConvertSvgToImageBufferCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Path to the SVG file to convert")]
        [ValidateNotNullOrEmpty]
        [Alias("Path", "FilePath")]
        public string InputPath { get; set; } = string.Empty;

        [Parameter(
            Mandatory = false,
            HelpMessage = "Width of the output image (default: SVG width or 800)")]
        [ValidateRange(1, 10000)]
        public int Width { get; set; } = 0;

        [Parameter(
            Mandatory = false,
            HelpMessage = "Height of the output image (default: SVG height or 600)")]
        [ValidateRange(1, 10000)]
        public int Height { get; set; } = 0;

        [Parameter(
            Mandatory = false,
            HelpMessage = "Background color for the output image (default: Transparent)")]
        public string BackgroundColor { get; set; } = "Transparent";

        [Parameter(
            Mandatory = false,
            HelpMessage = "Output image format (default: Png)")]
        [ValidateSet("Png", "Jpeg", "Webp")]
        public string Format { get; set; } = "Png";

        [Parameter(
            Mandatory = false,
            HelpMessage = "Quality for Jpeg format (1-100, default: 100)")]
        [ValidateRange(1, 100)]
        public int Quality { get; set; } = 100;

        protected override void ProcessRecord()
        {
            try
            {
                // Validate input file exists
                if (!File.Exists(InputPath))
                {
                    var error = new ErrorRecord(
                        new FileNotFoundException($"SVG file not found: {InputPath}"),
                        "SvgFileNotFound",
                        ErrorCategory.ObjectNotFound,
                        InputPath);
                    ThrowTerminatingError(error);
                    return;
                }

                // Load the SVG file
                using var svg = new SKSvg();
                var picture = svg.Load(InputPath);

                if (picture == null)
                {
                    var error = new ErrorRecord(
                        new InvalidOperationException($"Failed to load SVG file: {InputPath}"),
                        "SvgLoadFailed",
                        ErrorCategory.InvalidData,
                        InputPath);
                    ThrowTerminatingError(error);
                    return;
                }

                // Determine dimensions
                var cullRect = picture.CullRect;
                int imageWidth = Width > 0 ? Width : (cullRect.Width > 0 ? (int)cullRect.Width : 800);
                int imageHeight = Height > 0 ? Height : (cullRect.Height > 0 ? (int)cullRect.Height : 600);

                // Create image info
                var info = new SKImageInfo(imageWidth, imageHeight);

                // Create surface and canvas
                using var surface = SKSurface.Create(info);
                if (surface == null)
                {
                    var error = new ErrorRecord(
                        new InvalidOperationException("Failed to create drawing surface"),
                        "SurfaceCreationFailed",
                        ErrorCategory.ResourceUnavailable,
                        null);
                    ThrowTerminatingError(error);
                    return;
                }

                var canvas = surface.Canvas;

                // Parse background color
                SKColor bgColor = ParseBackgroundColor(BackgroundColor);
                canvas.Clear(bgColor);

                // Scale the drawing if needed
                if (Width > 0 || Height > 0)
                {
                    float scaleX = cullRect.Width > 0 ? imageWidth / cullRect.Width : 1;
                    float scaleY = cullRect.Height > 0 ? imageHeight / cullRect.Height : 1;
                    canvas.Scale(scaleX, scaleY);
                }

                // Draw the SVG
                canvas.DrawPicture(picture);

                // Create image and encode
                using var image = surface.Snapshot();
                var encodedFormat = GetEncodedImageFormat(Format);
                using var data = image.Encode(encodedFormat, Quality);

                if (data == null)
                {
                    var error = new ErrorRecord(
                        new InvalidOperationException("Failed to encode image"),
                        "ImageEncodingFailed",
                        ErrorCategory.InvalidResult,
                        null);
                    ThrowTerminatingError(error);
                    return;
                }

                // Return raw byte array
                byte[] rawBytes = data.ToArray();
                WriteObject(rawBytes);

                WriteVerbose($"Successfully converted SVG to {rawBytes.Length} bytes ({imageWidth}x{imageHeight} {Format})");
            }
            catch (Exception ex)
            {
                var error = new ErrorRecord(
                    ex,
                    "SvgConversionError",
                    ErrorCategory.InvalidOperation,
                    InputPath);
                ThrowTerminatingError(error);
            }
        }

        private SKColor ParseBackgroundColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString) || colorString.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
            {
                return SKColors.Transparent;
            }

            // Handle common named colors manually for better compatibility
            switch (colorString.ToLowerInvariant())
            {
                case "white": return SKColors.White;
                case "black": return SKColors.Black;
                case "red": return SKColors.Red;
                case "green": return SKColors.Green;
                case "blue": return SKColors.Blue;
                case "yellow": return SKColors.Yellow;
                case "cyan": return SKColors.Cyan;
                case "magenta": return SKColors.Magenta;
                case "gray": return SKColors.Gray;
                case "transparent": return SKColors.Transparent;
            }

            // Try to parse as hex color
            if (colorString.StartsWith("#") && SKColor.TryParse(colorString, out SKColor hexColor))
            {
                return hexColor;
            }

            // Try SKColor.TryParse for other formats
            if (SKColor.TryParse(colorString, out SKColor parsedColor))
            {
                return parsedColor;
            }

            WriteWarning($"Could not parse color '{colorString}', using Transparent");
            return SKColors.Transparent;
        }

        private SKEncodedImageFormat GetEncodedImageFormat(string format)
        {
            return format.ToLowerInvariant() switch
            {
                "png" => SKEncodedImageFormat.Png,
                "jpeg" => SKEncodedImageFormat.Jpeg,
                "webp" => SKEncodedImageFormat.Webp,
                _ => SKEncodedImageFormat.Png
            };
        }
    }
}