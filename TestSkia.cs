using System;
using SkiaSharp;
using Svg.Skia;

class TestSkia
{
    static void Main()
    {
        try
        {
            Console.WriteLine("Testing SkiaSharp initialization...");
            var info = new SKImageInfo(100, 100);
            Console.WriteLine($"SKImageInfo created: {info.Width}x{info.Height}");
            
            using var bitmap = new SKBitmap(100, 100);
            Console.WriteLine("SKBitmap created successfully");
            
            var svg = new SKSvg();
            Console.WriteLine("SKSvg created successfully");
            
            Console.WriteLine("All tests passed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
            }
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}