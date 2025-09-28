@{
    # Module manifest for PwrSvg
    ModuleVersion = '1.0.0'
    GUID = 'c23a160d-a648-41bd-963f-d42c67483018'
    Author = 'calvo-software'
    CompanyName = 'calvo-software'
    Copyright = '(c) 2025 calvo-software. All rights reserved.'
    Description = 'PowerShell module for rendering SVG files to raw image buffers using SkiaSharp, optimized for terminal and pipeline integration.'
    PowerShellVersion = '5.1'
    DotNetFrameworkVersion = '4.8'
    CLRVersion = '4.0'

    # Assemblies that must be loaded prior to importing this module
    RequiredAssemblies = @('PwrSvg.dll')

    # Cmdlets to export from this module
    CmdletsToExport = @('*')

    # Functions to export from this module
    FunctionsToExport = @()

    # Variables to export from this module
    VariablesToExport = @()

    # Aliases to export from this module
    AliasesToExport = @()

    # Private data to pass to the module
    PrivateData = @{
        PSData = @{
            # Tags applied to this module
            Tags = @('SVG', 'PNG', 'Image', 'Conversion', 'SkiaSharp', 'Graphics', 'Terminal', 'Pipeline')

            # A URL to the license for this module
            LicenseUri = 'https://github.com/calvo-software/PwrSvg/blob/main/LICENSE'

            # A URL to the main website for this project
            ProjectUri = 'https://github.com/calvo-software/PwrSvg'

            # ReleaseNotes
            ReleaseNotes = 'Initial release with ConvertTo-Png cmdlet for SVG to PNG conversion.'
        }
    }

    # Help file
    HelpInfoURI = 'https://github.com/calvo-software/PwrSvg'
}