using System;
using System.Management.Automation;
using Xunit;

namespace PwrSvg.Tests
{
    public class OutConsoleSvgTests
    {
        private string GetSourceDirectoryPath()
        {
            // The working directory during tests is the solution root
            return System.IO.Directory.GetCurrentDirectory();
        }
        [Fact]
        public void OutConsoleSvg_ManifestShouldContainFunction()
        {
            // Arrange - look for manifest in source directory  
            var sourceDir = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(typeof(ConvertToPngCommand).Assembly.Location)));
            var manifestPath = System.IO.Path.Combine(sourceDir, "PwrSvg", "PwrSvg.psd1");
            
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
            var sourceDir = GetSourceDirectoryPath();
            var manifestPath = System.IO.Path.Combine(sourceDir, "PwrSvg", "PwrSvg.psd1");
            
            // Act
            var manifestContent = System.IO.File.ReadAllText(manifestPath);
            
            // Assert
            Assert.Contains("RequiredModules = @('Sixel')", manifestContent);
        }
        
        [Fact]
        public void OutConsoleSvg_ManifestShouldHaveScriptsToProcess()
        {
            // Arrange
            var sourceDir = GetSourceDirectoryPath();
            var manifestPath = System.IO.Path.Combine(sourceDir, "PwrSvg", "PwrSvg.psd1");
            
            // Act
            var manifestContent = System.IO.File.ReadAllText(manifestPath);
            
            // Assert
            Assert.Contains("ScriptsToProcess = @('Out-ConsoleSvg.ps1')", manifestContent);
        }
        
        [Fact]
        public void OutConsoleSvg_ScriptFileShouldExist()
        {
            // Arrange
            var sourceDir = GetSourceDirectoryPath();
            var scriptPath = System.IO.Path.Combine(sourceDir, "PwrSvg", "Out-ConsoleSvg.ps1");
            
            // Act & Assert
            Assert.True(System.IO.File.Exists(scriptPath), $"Out-ConsoleSvg.ps1 should exist at {scriptPath}");
        }
        
        [Fact]
        public void OutConsoleSvg_ScriptShouldContainFunction()
        {
            // Arrange
            var sourceDir = GetSourceDirectoryPath();
            var scriptPath = System.IO.Path.Combine(sourceDir, "PwrSvg", "Out-ConsoleSvg.ps1");
            
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
            var sourceDir = GetSourceDirectoryPath();
            var scriptPath = System.IO.Path.Combine(sourceDir, "PwrSvg", "Out-ConsoleSvg.ps1");
            
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
            var sourceDir = GetSourceDirectoryPath();
            var manifestPath = System.IO.Path.Combine(sourceDir, "PwrSvg", "PwrSvg.psd1");
            
            // Act
            var manifestContent = System.IO.File.ReadAllText(manifestPath);
            
            // Assert
            Assert.Contains("Out-ConsoleSvg", manifestContent);
            Assert.Contains("Sixel graphics", manifestContent);
        }
    }
}