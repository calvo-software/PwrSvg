# PwrSvg PowerShell Module
# This module combines C# cmdlets with PowerShell functions

# Load the binary module (DLL is in the same directory for published modules)
Import-Module (Join-Path $PSScriptRoot 'PwrSvg.dll') -Force

# Load PowerShell functions
. (Join-Path $PSScriptRoot 'Out-ConsoleSvg.ps1')