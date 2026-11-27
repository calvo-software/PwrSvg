@{
    # Module manifest for PwrSvg
    ModuleVersion = '1.0.6' # x-release-please-version
    GUID = 'c23a160d-a648-41bd-963f-d42c67483018'
    Author = 'Oscar Calvo'
    CompanyName = 'shoji-labs'
    Copyright = '(c) 2025 shoji-labs. All rights reserved.'
    Description = 'PowerShell module for rendering SVG files to raw image buffers using SkiaSharp, optimized for terminal and pipeline integration.'
    PowerShellVersion = '5.1'
    DotNetFrameworkVersion = '4.8'
    CLRVersion = '4.0'

    # Required modules
    RequiredModules = @(
        @{ ModuleName = 'Sixel'; ModuleVersion = '0.6.1' }
    )

    # PwrSvg.psm1 loads net8/PwrSvg.dll in PowerShell 7 and net48/PwrSvg.dll in Windows PowerShell
    RootModule = 'PwrSvg.psm1'

    # Cmdlets to export from this module
    CmdletsToExport = @('ConvertTo-Png')

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
            LicenseUri = 'https://github.com/shoji-labs/PwrSvg/blob/main/LICENSE'

            # A URL to the main website for this project
            ProjectUri = 'https://github.com/shoji-labs/PwrSvg'

            # ReleaseNotes
            ReleaseNotes = 'Added Out-ConsoleSvg function for direct SVG to console output using Sixel graphics. Existing ConvertTo-Png cmdlet for SVG to PNG conversion.'
        }
    }

    # Help file
    HelpInfoURI = 'https://github.com/shoji-labs/PwrSvg'
}
