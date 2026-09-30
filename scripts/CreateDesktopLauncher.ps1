[CmdletBinding(SupportsShouldProcess=$true)]
param([string]$ApplicationDirectory, [switch]$Force)
$ErrorActionPreference='Stop'
$projectDir=Split-Path -Parent $PSScriptRoot
if(-not $ApplicationDirectory) {
    $ApplicationDirectory=Join-Path $projectDir 'bin'
    if(-not (Test-Path -LiteralPath (Join-Path $ApplicationDirectory 'PulseAutoSwitch.exe'))) { $ApplicationDirectory=$projectDir }
}
$appDirectory=(Resolve-Path -LiteralPath $ApplicationDirectory).Path
$appExecutable=Join-Path $appDirectory 'PulseAutoSwitch.exe'
if(-not (Test-Path -LiteralPath $appExecutable)) { throw 'Build the project before creating the launcher.' }
$source=Join-Path $projectDir 'src\PulseLauncher.cs'
if(-not (Test-Path -LiteralPath $source)) { $source=Join-Path $PSScriptRoot 'PulseLauncher.cs' }
if(-not (Test-Path -LiteralPath $source)) { throw 'Launcher source code was not found.' }
$compiler=Join-Path $env:windir 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if(-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework compiler was not found.' }
$desktopDir=[Environment]::GetFolderPath('Desktop')
$destination=Join-Path $desktopDir 'Start PULSE AutoSwitch.exe'
if((Test-Path -LiteralPath $destination) -and -not $Force) { throw 'The launcher already exists. Use -Force to replace it.' }
if($PSCmdlet.ShouldProcess($destination,'Create a launcher with an embedded application path')) {
    $buildDir=Join-Path $projectDir ('work\desktop-launcher-'+[Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $buildDir -Force | Out-Null
    $targetFile=Join-Path $buildDir 'target.txt'
    [IO.File]::WriteAllText($targetFile,$appExecutable)
    $launcher=Join-Path $buildDir 'Start PULSE AutoSwitch.exe'
    $icon=Join-Path $projectDir 'assets\pulse.ico'
    $arguments=@('/nologo','/target:winexe','/platform:x64','/main:PulseLauncher','/reference:System.Windows.Forms.dll',("/out:$launcher"),("/resource:{0},PulseAutoSwitch.Target" -f $targetFile))
    if(Test-Path -LiteralPath $icon) { $arguments+=("/win32icon:$icon") }
    & $compiler $arguments $source
    if($LASTEXITCODE -ne 0) { throw 'Desktop launcher compilation failed.' }
    Copy-Item -LiteralPath $launcher -Destination $destination -Force
    Write-Output "Launcher created: $destination"
}
