[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$projectDir=Split-Path -Parent $PSScriptRoot
$outputDir=Join-Path $projectDir 'bin\tests'
$compiler=Join-Path $env:windir 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if(-not (Test-Path -LiteralPath $compiler)) { throw 'The 64-bit .NET Framework C# compiler was not found.' }
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$sources=@('tests\PulseTests.cs','src\PulseSettings.cs','src\PulseButtons.cs','src\PulseMedia.cs') | ForEach-Object { Join-Path $projectDir $_ }
$testExecutable=Join-Path $outputDir 'PulseTests.exe'
& $compiler /nologo /target:exe /platform:x64 /main:PulseTests ("/out:$testExecutable") $sources
if($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
& $testExecutable
if($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
