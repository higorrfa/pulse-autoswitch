[CmdletBinding()]
param([string]$ApplicationDirectory,[switch]$Force,[switch]$NoLaunch)
$ErrorActionPreference='Stop'
$projectDir=Split-Path -Parent $PSScriptRoot
if(-not $ApplicationDirectory) {
    $ApplicationDirectory=Join-Path $projectDir 'bin'
    if(-not (Test-Path -LiteralPath (Join-Path $ApplicationDirectory 'PulseAutoSwitch.exe'))) { $ApplicationDirectory=$projectDir }
}
$appDirectory=(Resolve-Path -LiteralPath $ApplicationDirectory).Path
$executable=Join-Path $appDirectory 'PulseAutoSwitch.exe'
if(-not (Test-Path -LiteralPath $executable)) { throw 'Build the source or extract the complete preview package first.' }
& (Join-Path $PSScriptRoot 'CheckRequirements.ps1')
if(-not (Test-Path -LiteralPath (Join-Path $appDirectory 'config.xml'))) {
    Copy-Item -LiteralPath (Join-Path $projectDir 'config.example.xml') -Destination (Join-Path $appDirectory 'config.xml')
}
& (Join-Path $PSScriptRoot 'CreateDesktopLauncher.ps1') -ApplicationDirectory $appDirectory -Force:$Force
if(-not $NoLaunch) { (New-Object -ComObject Shell.Application).ShellExecute($executable,'',$appDirectory,'open',1) }
Write-Output 'Installation complete. Select your headset and fallback outputs, then enable automatic routing.'
