[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
if(-not [Environment]::Is64BitOperatingSystem) { throw 'Windows x64 is required.' }
if($PSVersionTable.PSVersion -lt [Version]'5.1') { throw 'PowerShell 5.1 or newer is required.' }
$framework=Get-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full'
if($framework.Release -lt 528040) { throw 'Install .NET Framework 4.8 or newer.' }
$compiler=Join-Path $env:windir 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if(-not (Test-Path -LiteralPath $compiler)) { throw 'The .NET Framework compiler is unavailable.' }
$devices=@(Get-PnpDevice -PresentOnly | Where-Object { $_.InstanceId -like 'USB\VID_054C&PID_0D5E&MI_03*' })
if($devices.Count -eq 0) { throw 'Connect the PULSE 3D receiver. Its control interface 054C:0D5E:03 was not found.' }
foreach($device in $devices) {
    $properties=Get-ItemProperty -LiteralPath ('HKLM:\SYSTEM\CurrentControlSet\Enum\'+$device.InstanceId)
    if($properties.Service -ne 'WinUSB') { throw 'Install WinUSB on interface 03 only using Zadig. See docs/INSTALLATION.md.' }
    if($device.Status -ne 'OK') { throw ('The receiver reports an error: '+$device.Status) }
}
Write-Output 'Requirements passed: Windows x64, .NET Framework, PowerShell and receiver WinUSB interface.'
