@{
    ModuleVersion = '1.0.0'
    GUID = 'b8c9c4c7-8f2b-4a45-9c7e-1d2f3a4b5c6d'
    Author = 'calvo-software'
    CompanyName = 'calvo-software'
    Copyright = '(c) 2025 calvo-software. All rights reserved.'
    Description = 'PowerShell module for rendering SVG files to raw image buffers using SkiaSharp'
    
    PowerShellVersion = '5.1'
    DotNetFrameworkVersion = '4.7.2'
    
    ModuleToProcess = 'PwrSvg.dll'
    
    CmdletsToExport = @('ConvertTo-Png')
    FunctionsToExport = @()
    VariablesToExport = @()
    AliasesToExport = @()
    
    PrivateData = @{
        PSData = @{
            Tags = @('SVG', 'PNG', 'Image', 'Conversion', 'SkiaSharp', 'Graphics')
            LicenseUri = 'https://github.com/calvo-software/PwrSvg/blob/main/LICENSE'
            ProjectUri = 'https://github.com/calvo-software/PwrSvg'
            ReleaseNotes = 'Initial release of PwrSvg module with ConvertTo-Png cmdlet for SVG to PNG conversion'
        }
    }
    
    HelpInfoURI = 'https://github.com/calvo-software/PwrSvg'
}