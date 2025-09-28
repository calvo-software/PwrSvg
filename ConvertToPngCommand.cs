using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using SkiaSharp;
using Svg.Skia;

namespace PwrSvg
{
    [Cmdlet(VerbsData.ConvertTo, "Png")]
    [OutputType(typeof(byte[]))]
    public class ConvertToPngCommand : PSCmdlet
    {
        private static bool _isInitialized = false;
        private static readonly object _initLock = new object();
        
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

        protected override void BeginProcessing()
        {
            InitializeSkiaSharp();
        }

        private void InitializeSkiaSharp()
        {
            if (_isInitialized) return;
            
            lock (_initLock)
            {
                if (_isInitialized) return;
                
                try
                {
                    // Force native library path resolution
                    string moduleDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string runtimeDir = Path.Combine(moduleDir, "runtimes");
                    
                    if (Directory.Exists(runtimeDir))
                    {
                        string arch = Environment.Is64BitProcess ? "x64" : "x86";
                        string osId = GetOSIdentifier();
                        string nativeDir = Path.Combine(runtimeDir, $"{osId}-{arch}", "native");
                        
                        if (Directory.Exists(nativeDir))
                        {
                            // Add native library directory to PATH for library loading
                            string currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                            if (!currentPath.Contains(nativeDir))
                            {
                                Environment.SetEnvironmentVariable("PATH", $"{nativeDir}{Path.PathSeparator}{currentPath}");
                            }
                        }
                    }
                    
                    // Force SkiaSharp initialization by creating a simple object
                    var testBitmap = new SKBitmap(1, 1);
                    testBitmap.Dispose();
                    
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    WriteWarning($"Failed to initialize SkiaSharp: {ex.Message}");
                }
            }
        }
        
        private string GetOSIdentifier()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return "win";
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                if (File.Exists("/etc/alpine-release"))
                    return "linux-musl";
                return "linux";
            }
            if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                return "osx";
            
            return "linux"; // Default fallback
        }

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
                // Read SVG content from file
                string svgContent = File.ReadAllText(svgFilePath);
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent));
                
                // Load the SVG from stream
                var svg = new SKSvg();
                var picture = svg.Load(stream);

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

                // Try to parse as named color using reflection
                var colorProperty = typeof(SKColors).GetProperty(colorString, 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
                if (colorProperty != null)
                {
                    return (SKColor)colorProperty.GetValue(null);
                }

                // Try common color names manually
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
                    case "gray": case "grey": return SKColors.Gray;
                    case "orange": return SKColors.Orange;
                    case "purple": return SKColors.Purple;
                    case "pink": return SKColors.Pink;
                    case "brown": return SKColors.Brown;
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