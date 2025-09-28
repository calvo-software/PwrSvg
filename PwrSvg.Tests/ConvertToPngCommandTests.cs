using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Xunit;
using PwrSvg;

namespace PwrSvg.Tests;

public class ConvertToPngCommandTests
{
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
    public void ConvertToPngCommand_PathProperty_ShouldAcceptFromPipeline()
    {
        // Arrange
        var cmdletType = typeof(ConvertToPngCommand);
        var pathProperty = cmdletType.GetProperty("Path");

        // Act
        var pathParameterAttributes = pathProperty?.GetCustomAttributes(typeof(ParameterAttribute), false).Cast<ParameterAttribute>().ToArray();

        // Assert
        Assert.NotNull(pathParameterAttributes);
        Assert.NotEmpty(pathParameterAttributes);
        Assert.True(pathParameterAttributes[0].ValueFromPipeline);
        Assert.Equal("Path to the SVG file to convert, or SVG content string", pathParameterAttributes[0].HelpMessage);
    }
}