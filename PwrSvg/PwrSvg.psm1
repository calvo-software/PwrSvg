# The manifest must stay static for the PowerShell Gallery tooling, so the
# assembly for the running PowerShell is chosen here.
$framework = if ($PSEdition -eq 'Core') { 'net8' } else { 'net48' }
Import-Module (Join-Path (Join-Path $PSScriptRoot $framework) 'PwrSvg.dll')

. (Join-Path $PSScriptRoot 'Out-ConsoleSvg.ps1')

Export-ModuleMember -Cmdlet 'ConvertTo-Png' -Function 'Out-ConsoleSvg'
