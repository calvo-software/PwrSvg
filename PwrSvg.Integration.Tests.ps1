<#
.SYNOPSIS
    Pester integration tests for the published PwrSvg module layout.
.DESCRIPTION
    Imports the module from the layout produced by build/New-ModuleLayout.ps1,
    the same layout that is published to the PowerShell Gallery. The manifest
    selects the assembly for the running PowerShell, so the tests are identical
    under Windows PowerShell and PowerShell 7.
.PARAMETER ModulePath
    The PwrSvg module folder: the directory that holds PwrSvg.psd1.
#>
param(
    [Parameter(Mandatory)]
    [string]$ModulePath
)

BeforeAll {
    $script:ModuleBase = (Resolve-Path $ModulePath).Path
    Import-Module (Join-Path $script:ModuleBase 'PwrSvg.psd1') -Force -ErrorAction Stop

    $script:TestSvgPath = Join-Path $PSScriptRoot 'test.svg'
    $script:PngSignature = [byte[]](0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)

    function Get-LeadingByte {
        param([System.IO.Stream]$Stream, [int]$Count)
        $Stream.Position = 0
        $buffer = New-Object byte[] $Count
        $null = $Stream.Read($buffer, 0, $Count)
        $buffer
    }
}

AfterAll {
    Remove-Module PwrSvg -ErrorAction Ignore
}

Describe 'PwrSvg module' {
    It 'loads from the layout under test' {
        (Get-Module PwrSvg).ModuleBase | Should -Be $script:ModuleBase
    }

    It 'exports the ConvertTo-Png cmdlet' {
        (Get-Command ConvertTo-Png).CommandType | Should -Be 'Cmdlet'
    }

    It 'defines the Out-ConsoleSvg function' {
        (Get-Command Out-ConsoleSvg).CommandType | Should -Be 'Function'
    }

    It 'has a static manifest that the PowerShell Gallery tooling can read' {
        $manifest = Import-PowerShellDataFile (Join-Path $script:ModuleBase 'PwrSvg.psd1')

        $manifest.RootModule | Should -Be 'PwrSvg.psm1'
    }
}

Describe 'ConvertTo-Png' {
    It 'renders an SVG file to a PNG stream' {
        $stream = ConvertTo-Png -Path $script:TestSvgPath

        $stream | Should -BeOfType System.IO.MemoryStream
        Get-LeadingByte -Stream $stream -Count 8 | Should -Be $script:PngSignature
    }

    It 'renders SVG markup from the pipeline' {
        $stream = Get-Content $script:TestSvgPath -Raw | ConvertTo-Png

        Get-LeadingByte -Stream $stream -Count 8 | Should -Be $script:PngSignature
    }

    It 'writes a PNG file with -OutFile' {
        $outFile = Join-Path $TestDrive 'out.png'

        $result = ConvertTo-Png -Path $script:TestSvgPath -OutFile $outFile

        $result | Should -BeOfType System.IO.FileInfo
        $result.FullName | Should -Be $outFile
        [System.IO.File]::ReadAllBytes($outFile)[0..7] | Should -Be $script:PngSignature
    }

    It 'renders at the requested size' {
        $smallLength = (ConvertTo-Png -Path $script:TestSvgPath -Width 16 -Height 16).Length
        $largeLength = (ConvertTo-Png -Path $script:TestSvgPath -Width 256 -Height 256).Length

        $largeLength | Should -BeGreaterThan $smallLength
    }
}

Describe 'Out-ConsoleSvg' {
    It 'emits Sixel output for SVG markup from the pipeline' {
        $sixel = Get-Content $script:TestSvgPath -Raw | Out-ConsoleSvg -ErrorAction Stop

        $sixel | Should -Not -BeNullOrEmpty
    }
}
