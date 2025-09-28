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
    [Cmdlet(VerbsData.ConvertTo, "Png", DefaultParameterSetName = "FromPath")]
    [OutputType(typeof(MemoryStream), typeof(FileInfo))]
    public class ConvertToPngCommand : PSCmdlet
    {
        /// <summary>
        /// Path to the SVG file to convert
        /// </summary>
        [Parameter(
            ParameterSetName = "FromPath",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public FileInfo Path { get; set; }

        /// <summary>
        /// SVG content string to convert
        /// </summary>
        [Parameter(
            ParameterSetName = "FromContent",
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string SvgContent { get; set; }

        /// <summary>
        /// Output file path for PNG. If not specified, returns readonly MemoryStream
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = false,
            HelpMessage = "Output file path for PNG. If not specified, returns readonly MemoryStream")]
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
                    if (ParameterSetName == "FromPath")
                    {
                        // Handle file path input
                        if (Path == null)
                        {
                            WriteError(new ErrorRecord(
                                new ArgumentException("Path cannot be null"),
                                "InvalidPath",
                                ErrorCategory.InvalidArgument,
                                Path));
                            return;
                        }

                        if (!Path.Exists)
                        {
                            WriteError(new ErrorRecord(
                                new FileNotFoundException($"SVG file not found: {Path.FullName}"),
                                "SvgFileNotFound",
                                ErrorCategory.ObjectNotFound,
                                Path.FullName));
                            return;
                        }

                        WriteVerbose($"Processing SVG file: {Path.FullName}");
                        WriteVerbose($"Loading SVG document: {Path.FullName}");
                        svgDocument = svg.Load(Path.FullName);
                        WriteVerbose("SVG document loaded successfully.");
                    }
                    else // ParameterSetName == "FromContent"
                    {
                        // Handle SVG content string input
                        if (string.IsNullOrEmpty(SvgContent))
                        {
                            WriteError(new ErrorRecord(
                                new ArgumentException("SvgContent cannot be null or empty"),
                                "InvalidSvgContent",
                                ErrorCategory.InvalidArgument,
                                SvgContent));
                            return;
                        }

                        WriteVerbose("Processing SVG content string");
                        WriteVerbose($"Loading SVG from content: {SvgContent.Substring(0, Math.Min(100, SvgContent.Length))}...");
                        
                        // Load SVG from string using a MemoryStream
                        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(SvgContent)))
                        {
                            svgDocument = svg.Load(stream);
                        }
                        WriteVerbose("SVG document loaded successfully from content string.");
                    }
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException(
                            $"Failed to load SVG. " +
                            $"Error details: {ex.ToString()}"),
                        "SvgLoadException",
                        ErrorCategory.InvalidData,
                        ParameterSetName == "FromPath" ? Path.FullName : SvgContent));
                    return;
                }
                
                if (svgDocument == null)
                {
                    var inputSource = ParameterSetName == "FromPath" ? $"SVG file: {Path.FullName}" : "SVG content string";
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to load {inputSource} - SVG document is null"),
                        "SvgLoadFailed",
                        ErrorCategory.InvalidData,
                        ParameterSetName == "FromPath" ? Path.FullName : SvgContent));
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
                            // Return readonly MemoryStream for pipeline
                            var memoryStream = new MemoryStream(pngBytes, false);
                            WriteObject(memoryStream);
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
                    ParameterSetName == "FromPath" ? Path?.FullName : SvgContent));
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