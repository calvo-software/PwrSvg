using System;
using System.Management.Automation;
using Xunit;

namespace PwrSvg.Tests
{
    public class OutConsoleSvgTests
    {
        [Fact]
        public void OutConsoleSvg_ManifestShouldContainFunction()
        {
            // Arrange
            var manifestPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(ConvertToPngCommand).Assembly.Location),
                "PwrSvg.psd1");
            
            // Act
            var manifestContent = System.IO.File.ReadAllText(manifestPath);
            
            // Assert
            Assert.Contains("Out-ConsoleSvg", manifestContent);
            Assert.Contains("FunctionsToExport = @('Out-ConsoleSvg')", manifestContent);
        }
        
        [Fact]
        public void OutConsoleSvg_ManifestShouldHaveSixelModuleDependency()
        {
            // Arrange
            var manifestPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(ConvertToPngCommand).Assembly.Location),
                "PwrSvg.psd1");
            
            // Act
            var manifestContent = System.IO.File.ReadAllText(manifestPath);
            
            // Assert
            Assert.Contains("RequiredModules = @('Sixel')", manifestContent);
        }
        
        [Fact]
        public void OutConsoleSvg_ManifestShouldHaveScriptsToProcess()
        {
            // Arrange
            var manifestPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(ConvertToPngCommand).Assembly.Location),
                "PwrSvg.psd1");
            
            // Act
            var manifestContent = System.IO.File.ReadAllText(manifestPath);
            
            // Assert
            Assert.Contains("ScriptsToProcess = @('Out-ConsoleSvg.ps1')", manifestContent);
        }
        
        [Fact]
        public void OutConsoleSvg_ScriptFileShouldExist()
        {
            // Arrange
            var scriptPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(ConvertToPngCommand).Assembly.Location),
                "Out-ConsoleSvg.ps1");
            
            // Act & Assert
            Assert.True(System.IO.File.Exists(scriptPath), $"Out-ConsoleSvg.ps1 should exist at {scriptPath}");
        }
        
        [Fact]
        public void OutConsoleSvg_ScriptShouldContainFunction()
        {
            // Arrange
            var scriptPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(ConvertToPngCommand).Assembly.Location),
                "Out-ConsoleSvg.ps1");
            
            // Act
            var scriptContent = System.IO.File.ReadAllText(scriptPath);
            
            // Assert
            Assert.Contains("function Out-ConsoleSvg", scriptContent);
            Assert.Contains("ValueFromPipeline", scriptContent);
            Assert.Contains("ConvertTo-Png", scriptContent);
            Assert.Contains("ConvertTo-Sixel", scriptContent);
        }
        
        [Fact]
        public void OutConsoleSvg_ScriptShouldHaveProperParameters()
        {
            // Arrange
            var scriptPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(ConvertToPngCommand).Assembly.Location),
                "Out-ConsoleSvg.ps1");
            
            // Act
            var scriptContent = System.IO.File.ReadAllText(scriptPath);
            
            // Assert
            Assert.Contains("[string]$SvgContent", scriptContent);
            Assert.Contains("[int]$Width", scriptContent);
            Assert.Contains("[int]$Height", scriptContent);
            Assert.Contains("[string]$BackgroundColor", scriptContent);
            Assert.Contains("[int]$Quality", scriptContent);
        }
        
        [Fact]
        public void OutConsoleSvg_ManifestShouldHaveUpdatedReleaseNotes()
        {
            // Arrange
            var manifestPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(ConvertToPngCommand).Assembly.Location),
                "PwrSvg.psd1");
            
            // Act
            var manifestContent = System.IO.File.ReadAllText(manifestPath);
            
            // Assert
            Assert.Contains("Out-ConsoleSvg", manifestContent);
            Assert.Contains("Sixel graphics", manifestContent);
        }
    }
}