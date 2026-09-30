[CmdletBinding()]
param([string]$Compiler)
$ErrorActionPreference='Stop'
$projectDir=Split-Path -Parent $PSScriptRoot
if(-not $Compiler) {
    $command=Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if($command) { $Compiler=$command.Source }
    else { $Compiler=Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe' }
}
if(-not (Test-Path -LiteralPath $Compiler)) { throw 'Install Inno Setup 6.7.3 or newer from https://jrsoftware.org/isdl.php, then pass its ISCC.exe path using -Compiler.' }
& (Join-Path $PSScriptRoot 'TestSetup.ps1')
& (Join-Path $PSScriptRoot 'Package.ps1')
$stagingDir=Join-Path $projectDir ('work\setup-'+[Guid]::NewGuid().ToString('N'))
Expand-Archive -LiteralPath (Join-Path $projectDir 'dist\PulseAutoSwitch-preview-win-x64.zip') -DestinationPath $stagingDir
& $Compiler ('/DPackageDir='+ (Join-Path $stagingDir 'PulseAutoSwitch')) (Join-Path $projectDir 'installer\PulseAutoSwitch.iss')
if($LASTEXITCODE -ne 0) { throw 'Setup compilation failed.' }
Write-Output 'Setup created in dist. External prerequisites are downloaded only when needed.'
