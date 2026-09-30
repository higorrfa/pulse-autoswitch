[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$projectDir=Split-Path -Parent $PSScriptRoot
& (Join-Path $PSScriptRoot 'Build.ps1')
& (Join-Path $PSScriptRoot 'Test.ps1')
$distDir=Join-Path $projectDir 'dist'
New-Item -ItemType Directory -Path $distDir -Force | Out-Null
$archive=Join-Path $distDir 'PulseAutoSwitch-preview-win-x64.zip'
$stagingDir=Join-Path $projectDir ('work\package-'+[Guid]::NewGuid().ToString('N'))
$packageDir=Join-Path $stagingDir 'PulseAutoSwitch'
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null
foreach($file in @('bin\PulseAutoSwitch.exe','bin\PulseUsbProbe.exe','bin\Start PULSE AutoSwitch.exe','README.md','LICENSE','THIRD_PARTY_NOTICES.md','CONTRIBUTING.md','CHANGELOG.md','config.example.xml')) {
    Copy-Item -LiteralPath (Join-Path $projectDir $file) -Destination $packageDir
}
foreach($folder in @('docs','assets','scripts')) { New-Item -ItemType Directory -Path (Join-Path $packageDir $folder) | Out-Null }
Get-ChildItem -LiteralPath (Join-Path $projectDir 'docs') -Filter '*.md' | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $packageDir 'docs') }
foreach($asset in @('pulse.ico','pulse.png')) { Copy-Item -LiteralPath (Join-Path $projectDir ('assets\'+$asset)) -Destination (Join-Path $packageDir 'assets') }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CreateDesktopLauncher.ps1') -Destination (Join-Path $packageDir 'scripts')
foreach($script in @('CheckRequirements.ps1','Install.ps1')) { Copy-Item -LiteralPath (Join-Path $PSScriptRoot $script) -Destination (Join-Path $packageDir 'scripts') }
Copy-Item -LiteralPath (Join-Path $projectDir 'src\PulseLauncher.cs') -Destination (Join-Path $packageDir 'scripts')
Compress-Archive -LiteralPath $packageDir -DestinationPath $archive -Force
Write-Output "Preview package created: $archive"
Write-Output 'The package excludes local config.xml, logs, driver installers and remote publication.'
