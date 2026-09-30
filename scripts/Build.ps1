[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$projectDir=Split-Path -Parent $PSScriptRoot
$outputDir=Join-Path $projectDir 'bin'
$compiler=Join-Path $env:windir 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if(-not (Test-Path -LiteralPath $compiler)) { throw 'The 64-bit .NET Framework C# compiler was not found. See README.md.' }
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$sources=@(Get-ChildItem -LiteralPath (Join-Path $projectDir 'src') -Filter '*.cs' -Recurse | ForEach-Object { $_.FullName })
$icon=Join-Path $projectDir 'assets\pulse.ico'
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /main:PulseApp /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Core.dll ("/win32icon:$icon") ("/out:$outputDir\PulseAutoSwitch.exe") $sources
if($LASTEXITCODE -ne 0) { throw 'Application compilation failed.' }
& $compiler /nologo /target:exe /platform:x64 /main:UsbProbe ("/out:$outputDir\PulseUsbProbe.exe") (Join-Path $projectDir 'src\PulseUsb.cs')
if($LASTEXITCODE -ne 0) { throw 'USB diagnostic compilation failed.' }
& $compiler /nologo /target:winexe /platform:x64 /main:PulseLauncher /reference:System.Windows.Forms.dll ("/win32icon:$icon") ("/out:$outputDir\Start PULSE AutoSwitch.exe") (Join-Path $projectDir 'src\PulseLauncher.cs')
if($LASTEXITCODE -ne 0) { throw 'Launcher compilation failed.' }
$config=Join-Path $outputDir 'config.xml'
if(-not (Test-Path -LiteralPath $config)) { Copy-Item -LiteralPath (Join-Path $projectDir 'config.example.xml') -Destination $config }
Write-Output "Built in: $outputDir"
