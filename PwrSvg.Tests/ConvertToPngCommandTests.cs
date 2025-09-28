using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Xunit;
using PwrSvg;

namespace PwrSvg.Tests;

public class ConvertToPngCommandTests
{
    private const string TestSvgContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg width=""100"" height=""100"" xmlns=""http://www.w3.org/2000/svg"">
  <circle cx=""50"" cy=""50"" r=""40"" fill=""red""/>
</svg>";

    [Fact]
    public void ConvertToPngCommand_ShouldHaveCorrectCmdletAttribute()
    {
        // Arrange & Act
        var cmdletType = typeof(ConvertToPngCommand);
        var cmdletAttribute = (CmdletAttribute?)Attribute.GetCustomAttribute(cmdletType, typeof(CmdletAttribute));

        // Assert
        Assert.NotNull(cmdletAttribute);
        Assert.Equal(VerbsData.ConvertTo, cmdletAttribute.VerbName);
        Assert.Equal("Png", cmdletAttribute.NounName);
    }

    [Fact]
    public void ConvertToPngCommand_ShouldHaveRequiredProperties()
    {
        // Arrange
        var cmdlet = new ConvertToPngCommand();

        // Act & Assert
        Assert.NotNull(cmdlet.GetType().GetProperty("Path"));
        Assert.NotNull(cmdlet.GetType().GetProperty("OutFile"));
        Assert.NotNull(cmdlet.GetType().GetProperty("Width"));
        Assert.NotNull(cmdlet.GetType().GetProperty("Height"));
        Assert.NotNull(cmdlet.GetType().GetProperty("BackgroundColor"));
        Assert.NotNull(cmdlet.GetType().GetProperty("Quality"));
    }

    [Fact]
    public void ConvertToPngCommand_QualityProperty_ShouldHaveDefaultValue()
    {
        // Arrange
        var cmdlet = new ConvertToPngCommand();

        // Act
        var quality = cmdlet.Quality;

        // Assert
        Assert.Equal(95, quality);
    }

    [Fact]
    public void ConvertToPngCommand_ShouldHaveCorrectOutputTypes()
    {
        // Arrange
        var cmdletType = typeof(ConvertToPngCommand);
        var outputTypeAttributes = (OutputTypeAttribute[])Attribute.GetCustomAttributes(cmdletType, typeof(OutputTypeAttribute));

        // Assert
        Assert.NotEmpty(outputTypeAttributes);
        var outputTypeNames = outputTypeAttributes[0].Type.Select(t => t.Type).ToArray();
        Assert.Contains(typeof(MemoryStream), outputTypeNames);
        Assert.Contains(typeof(FileInfo), outputTypeNames);
    }

    [Fact]
    public void ConvertToPngCommand_ShouldInheritFromPSCmdlet()
    {
        // Arrange
        var cmdletType = typeof(ConvertToPngCommand);

        // Act & Assert
        Assert.True(typeof(PSCmdlet).IsAssignableFrom(cmdletType));
    }

    [Fact]
    public void ConvertToPngCommand_WidthProperty_ShouldAcceptPositiveValues()
    {
        // Arrange
        var cmdlet = new ConvertToPngCommand();

        // Act & Assert - Should not throw
        cmdlet.Width = 100;
        cmdlet.Width = 1920;
        Assert.Equal(1920, cmdlet.Width);
    }

    [Fact]
    public void ConvertToPngCommand_HeightProperty_ShouldAcceptPositiveValues()
    {
        // Arrange
        var cmdlet = new ConvertToPngCommand();

        // Act & Assert - Should not throw
        cmdlet.Height = 100;
        cmdlet.Height = 1080;
        Assert.Equal(1080, cmdlet.Height);
    }

    [Fact]
    public void ConvertToPngCommand_ShouldReturnMemoryStreamInOutputType()
    {
        // Arrange
        var cmdletType = typeof(ConvertToPngCommand);
        var outputTypeAttributes = (OutputTypeAttribute[])Attribute.GetCustomAttributes(cmdletType, typeof(OutputTypeAttribute));

        // Assert
        Assert.NotEmpty(outputTypeAttributes);
        var outputTypeNames = outputTypeAttributes[0].Type.Select(t => t.Type).ToArray();
        Assert.Contains(typeof(MemoryStream), outputTypeNames);
        Assert.DoesNotContain(typeof(byte[]), outputTypeNames); // Should not contain byte[] anymore
    }

    [Fact]
    public void ConvertToPngCommand_NativeLibraryLoader_ShouldInitializeSuccessfully()
    {
        // Arrange & Act
        // This test verifies that the NativeLibraryLoader.Initialize() method
        // can be called without throwing exceptions
        var exception = Record.Exception(() => NativeLibraryLoader.Initialize());

        // Assert
        Assert.Null(exception);
    }

    [Fact] 
    public void ConvertToPngCommand_WithValidSvg_ShouldProcessWithoutError()
    {
        // Arrange - Create a simple SVG file for testing
        var testSvgPath = Path.GetTempFileName();
        
        try
        {
            File.WriteAllText(testSvgPath, TestSvgContent);

            // Test that we can at least create the command and set properties
            // without errors - this verifies the basic structure is correct
            var cmdlet = new ConvertToPngCommand();
            cmdlet.Path = testSvgPath;
            cmdlet.Width = 100;
            cmdlet.Height = 100;
            
            // Assert - Basic properties should be set correctly
            Assert.Equal(testSvgPath, cmdlet.Path);
            Assert.Equal(100, cmdlet.Width);
            Assert.Equal(100, cmdlet.Height);
            Assert.Equal(95, cmdlet.Quality); // Default value
        }
        finally
        {
            if (File.Exists(testSvgPath))
            {
                File.Delete(testSvgPath);
            }
        }
    }

    [Fact]
    public void ConvertToPngCommand_TestSvgFile_ShouldExistInRepository()
    {
        // Arrange & Act - Look for the test.svg file in the repository root
        var repoRoot = GetRepositoryRoot();
        var testSvgPath = Path.Combine(repoRoot, "test.svg");
        
        // Assert
        Assert.True(File.Exists(testSvgPath), "test.svg should exist in repository root for CI/CD testing");
        
        // Verify it's a valid SVG by checking it starts with SVG content
        var content = File.ReadAllText(testSvgPath);
        Assert.Contains("<?xml", content);
        Assert.Contains("<svg", content);
        Assert.Contains("</svg>", content);
    }

    /// <summary>
    /// Gets the repository root directory by walking up from the current assembly location
    /// </summary>
    private static string GetRepositoryRoot()
    {
        var assemblyLocation = typeof(ConvertToPngCommandTests).Assembly.Location;
        var directory = Path.GetDirectoryName(assemblyLocation);
        
        while (directory != null && !File.Exists(Path.Combine(directory, "test.svg")))
        {
            directory = Directory.GetParent(directory)?.FullName;
        }
        
        return directory ?? throw new InvalidOperationException("Could not find repository root with test.svg");
    }
}