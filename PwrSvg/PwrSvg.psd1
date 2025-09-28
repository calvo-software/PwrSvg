@{
    # Module manifest for PwrSvg
    ModuleVersion = '1.0.1'
    GUID = 'c23a160d-a648-41bd-963f-d42c67483018'
    Author = 'Oscar Calvo'
    CompanyName = 'calvo-software'
    Copyright = '(c) 2025 calvo-software. All rights reserved.'
    Description = 'PowerShell module for rendering SVG files to raw image buffers using SkiaSharp, optimized for terminal and pipeline integration.'
    PowerShellVersion = '5.1'
    DotNetFrameworkVersion = '4.8'
    CLRVersion = '4.0'

    # Required modules
    RequiredModules = @('Sixel')

    # Scripts to process on module import
    ScriptsToProcess = @('Out-ConsoleSvg.ps1')

    # For published builds, DLL is in the same directory as the manifest
    RootModule = 'PwrSvg.dll'
    
    # Also load as required assembly for dependency resolution  
    RequiredAssemblies = @('PwrSvg.dll')

    # Cmdlets to export from this module
    CmdletsToExport = @('*')

    # Functions to export from this module
    FunctionsToExport = @('Out-ConsoleSvg')

    # Variables to export from this module
    VariablesToExport = @()

    # Aliases to export from this module
    AliasesToExport = @()

    # Private data to pass to the module
    PrivateData = @{
        PSData = @{
            # Tags applied to this module
            Tags = @('SVG', 'PNG', 'Image', 'Conversion', 'SkiaSharp', 'Graphics', 'Terminal', 'Pipeline', 'Console', 'Sixel')

            # A URL to the license for this module
            LicenseUri = 'https://github.com/calvo-software/PwrSvg/blob/main/LICENSE'

            # A URL to the main website for this project
            ProjectUri = 'https://github.com/calvo-software/PwrSvg'

            # ReleaseNotes
            ReleaseNotes = 'Added Out-ConsoleSvg function for direct SVG to console output using Sixel graphics. Existing ConvertTo-Png cmdlet for SVG to PNG conversion.'
        }
    }

    # Help file
    HelpInfoURI = 'https://github.com/calvo-software/PwrSvg'
}
