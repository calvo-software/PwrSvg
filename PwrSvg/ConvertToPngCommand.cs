using System;
using System.IO;
using System.Management.Automation;
using SkiaSharp;
using Svg.Skia;

namespace PwrSvg
{
    /// <summary>
    /// ConvertTo-Png cmdlet for converting SVG files to PNG format
    /// </summary>
    [Cmdlet(VerbsData.ConvertTo, "Png")]
    [OutputType(typeof(byte[]), typeof(FileInfo))]
    public class ConvertToPngCommand : PSCmdlet
    {
        /// <summary>
        /// Path to the SVG file to convert
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Path to the SVG file to convert")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Output file path for PNG. If not specified, returns byte array
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = false,
            HelpMessage = "Output file path for PNG. If not specified, returns byte array")]
        public string OutFile { get; set; }

        /// <summary>
        /// Width of the output image. If not specified, uses SVG's natural width
        /// </summary>
        [Parameter(
            Mandatory = false,
            HelpMessage = "Width of the output image")]
        [ValidateRange(1, int.MaxValue)]
        public int Width { get; set; } = 0;

        /// <summary>
        /// Height of the output image. If not specified, uses SVG's natural height
        /// </summary>
        [Parameter(
            Mandatory = false,
            HelpMessage = "Height of the output image")]
        [ValidateRange(1, int.MaxValue)]
        public int Height { get; set; } = 0;

        /// <summary>
        /// Background color for the PNG. Default is transparent
        /// </summary>
        [Parameter(
            Mandatory = false,
            HelpMessage = "Background color for the PNG (e.g., 'White', 'Transparent', '#FFFFFF')")]
        public string BackgroundColor { get; set; } = "Transparent";

        /// <summary>
        /// Quality/compression level for PNG (0-100). Default is 95
        /// </summary>
        [Parameter(
            Mandatory = false,
            HelpMessage = "Quality/compression level for PNG (0-100)")]
        [ValidateRange(0, 100)]
        public int Quality { get; set; } = 95;

        protected override void ProcessRecord()
        {
            try
            {
                // Validate input file
                if (string.IsNullOrEmpty(Path))
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("Path cannot be null or empty"),
                        "InvalidPath",
                        ErrorCategory.InvalidArgument,
                        Path));
                    return;
                }

                var resolvedPath = GetResolvedProviderPathFromPSPath(Path, out var provider);
                if (resolvedPath.Count == 0)
                {
                    WriteError(new ErrorRecord(
                        new FileNotFoundException($"Cannot find path '{Path}'"),
                        "PathNotFound",
                        ErrorCategory.ObjectNotFound,
                        Path));
                    return;
                }

                var svgFilePath = resolvedPath[0];
                if (!File.Exists(svgFilePath))
                {
                    WriteError(new ErrorRecord(
                        new FileNotFoundException($"SVG file not found: {svgFilePath}"),
                        "SvgFileNotFound",
                        ErrorCategory.ObjectNotFound,
                        svgFilePath));
                    return;
                }

                WriteVerbose($"Processing SVG file: {svgFilePath}");

                // Load SVG using Svg.Skia with enhanced error handling
                SKSvg svg;
                SKPicture svgDocument;
                
                try
                {
                    WriteVerbose("Initializing SkiaSharp SVG engine...");
                    svg = new SKSvg();
                    WriteVerbose("SkiaSharp SVG engine initialized successfully.");
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException(
                            $"Failed to initialize SkiaSharp SVG engine. This usually indicates missing native libraries. " +
                            $"Please ensure you are using the published module (dotnet publish) which includes all native dependencies. " +
                            $"Error details: {ex.ToString()}"),
                        "SkiaSharpInitializationFailed",
                        ErrorCategory.InvalidOperation,
                        null));
                    return;
                }

                try
                {
                    WriteVerbose($"Loading SVG document: {svgFilePath}");
                    svgDocument = svg.Load(svgFilePath);
                    WriteVerbose("SVG document loaded successfully.");
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException(
                            $"Failed to load SVG file: {svgFilePath}. " +
                            $"Error details: {ex.ToString()}"),
                        "SvgLoadException",
                        ErrorCategory.InvalidData,
                        svgFilePath));
                    return;
                }
                
                if (svgDocument == null)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to load SVG file: {svgFilePath} - SVG document is null"),
                        "SvgLoadFailed",
                        ErrorCategory.InvalidData,
                        svgFilePath));
                    return;
                }

                // Determine output dimensions
                var bounds = svgDocument.CullRect;
                var outputWidth = Width > 0 ? Width : (int)Math.Ceiling(bounds.Width);
                var outputHeight = Height > 0 ? Height : (int)Math.Ceiling(bounds.Height);

                if (outputWidth <= 0 || outputHeight <= 0)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Invalid output dimensions: {outputWidth}x{outputHeight}"),
                        "InvalidDimensions",
                        ErrorCategory.InvalidData,
                        null));
                    return;
                }

                WriteVerbose($"Output dimensions: {outputWidth}x{outputHeight}");

                // Parse background color
                var backgroundColor = ParseBackgroundColor(BackgroundColor);

                // Create bitmap and render with enhanced error handling
                SKSurface surface;
                try
                {
                    WriteVerbose($"Creating SkiaSharp surface: {outputWidth}x{outputHeight}");
                    surface = SKSurface.Create(new SKImageInfo(outputWidth, outputHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
                    if (surface == null)
                    {
                        WriteError(new ErrorRecord(
                            new InvalidOperationException(
                                "Failed to create SkiaSharp surface. This may indicate missing native libraries or insufficient memory. " +
                                "Please ensure you are using the published module (dotnet publish) which includes all native dependencies."),
                            "SurfaceCreationFailed",
                            ErrorCategory.ResourceUnavailable,
                            null));
                        return;
                    }
                    WriteVerbose("SkiaSharp surface created successfully.");
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException(
                            $"Failed to create SkiaSharp surface. This usually indicates missing native libraries. " +
                            $"Please ensure you are using the published module (dotnet publish) which includes all native dependencies. " +
                            $"Error details: {ex.ToString()}"),
                        "SurfaceCreationException",
                        ErrorCategory.InvalidOperation,
                        null));
                    return;
                }

                using (surface)
                {
                    var canvas = surface.Canvas;
                    
                    // Clear with background color
                    canvas.Clear(backgroundColor);

                    // Scale to fit if dimensions were specified
                    if (Width > 0 || Height > 0)
                    {
                        var scaleX = outputWidth / bounds.Width;
                        var scaleY = outputHeight / bounds.Height;
                        var scale = Math.Min(scaleX, scaleY);
                        
                        canvas.Scale(scale, scale);
                        
                        // Center the image
                        var offsetX = (outputWidth - bounds.Width * scale) / 2 / scale;
                        var offsetY = (outputHeight - bounds.Height * scale) / 2 / scale;
                        canvas.Translate(offsetX, offsetY);
                    }

                    // Render SVG
                    canvas.DrawPicture(svgDocument);
                    canvas.Flush();

                    // Create image and encode to PNG
                    using (var image = surface.Snapshot())
                    using (var data = image.Encode(SKEncodedImageFormat.Png, Quality))
                    {
                        var pngBytes = data.ToArray();

                        WriteVerbose($"Generated PNG: {pngBytes.Length} bytes");

                        // Output result
                        if (!string.IsNullOrEmpty(OutFile))
                        {
                            // Write to file
                            var outPath = GetUnresolvedProviderPathFromPSPath(OutFile);
                            var directoryPath = System.IO.Path.GetDirectoryName(outPath);
                            if (!string.IsNullOrEmpty(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                            File.WriteAllBytes(outPath, pngBytes);
                            WriteVerbose($"PNG written to: {outPath}");
                            WriteObject(new FileInfo(outPath));
                        }
                        else
                        {
                            // Return byte array for pipeline
                            WriteObject(pngBytes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "ConvertToPngError",
                    ErrorCategory.NotSpecified,
                    Path));
            }
        }

        private SKColor ParseBackgroundColor(string colorString)
        {
            if (string.IsNullOrEmpty(colorString) || colorString.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
            {
                return SKColors.Transparent;
            }

            if (colorString.Equals("White", StringComparison.OrdinalIgnoreCase))
            {
                return SKColors.White;
            }

            if (colorString.Equals("Black", StringComparison.OrdinalIgnoreCase))
            {
                return SKColors.Black;
            }

            // Try to parse hex color
            if (colorString.StartsWith("#"))
            {
                if (SKColor.TryParse(colorString, out var color))
                {
                    return color;
                }
            }

            // Try to parse named color
            if (SKColor.TryParse(colorString, out var namedColor))
            {
                return namedColor;
            }

            WriteWarning($"Could not parse color '{colorString}', using transparent");
            return SKColors.Transparent;
        }
    }
}