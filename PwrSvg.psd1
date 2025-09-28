#
# Module manifest for module 'PwrSvg'
#

@{
    # Script module or binary module file associated with this manifest.
    RootModule = 'PwrSvg.psm1'

    # Version number of this module.
    ModuleVersion = '1.0.0'

    # Supported PSEditions
    CompatiblePSEditions = @('Desktop', 'Core')

    # ID used to uniquely identify this module
    GUID = '12345678-1234-1234-1234-123456789abc'

    # Author of this module
    Author = 'calvo-software'

    # Company or vendor of this module
    CompanyName = 'calvo-software'

    # Copyright statement for this module
    Copyright = '(c) 2025 calvo-software. All rights reserved.'

    # Description of the functionality provided by this module
    Description = 'PowerShell module for rendering SVG files to raw image buffers using SkiaSharp, optimized for terminal and pipeline integration.'

    # Minimum version of the PowerShell engine required by this module
    PowerShellVersion = '5.1'

    # Minimum version of the .NET Framework required by this module
    DotNetFrameworkVersion = '4.7.2'

    # Minimum version of the common language runtime (CLR) required by this module
    CLRVersion = '4.0'

    # Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
    FunctionsToExport = @()

    # Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
    CmdletsToExport = @('Convert-SvgToImageBuffer')

    # Variables to export from this module
    VariablesToExport = @()

    # Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
    AliasesToExport = @()

    # List of all files packaged with this module
    FileList = @(
        'PwrSvg.psd1',
        'PwrSvg.psm1',
        'PwrSvg.dll'
    )

    # Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
    PrivateData = @{
        PSData = @{
            # Tags applied to this module. These help with module discovery in online galleries.
            Tags = @('SVG', 'Image', 'SkiaSharp', 'Graphics', 'Rendering', 'Buffer')

            # A URL to the license for this module.
            LicenseUri = 'https://github.com/calvo-software/PwrSvg/blob/main/LICENSE'

            # A URL to the main website for this project.
            ProjectUri = 'https://github.com/calvo-software/PwrSvg'

            # ReleaseNotes of this module
            ReleaseNotes = 'Initial release of PwrSvg module for SVG to image buffer conversion using SkiaSharp.'
        }
    }
}