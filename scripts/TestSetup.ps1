[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$projectDir=Split-Path -Parent $PSScriptRoot
$testDirectory=Join-Path $projectDir ('work\setup-check-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testDirectory -Force | Out-Null
$output=Join-Path $testDirectory 'result.ini'
function Get-PnpDevice {
    [CmdletBinding()]
    param([switch]$PresentOnly)
    if($scenario -eq 'Receiver') { return }
    if($scenario -eq 'WrongInterface') { return [pscustomobject]@{InstanceId='USB\VID_054C&PID_0D5E&MI_00\fixture';Status='OK'} }
    $status='OK'
    if($scenario -eq 'DeviceError') { $status='Error' }
    return [pscustomobject]@{InstanceId='USB\VID_054C&PID_0D5E&MI_03\fixture';Status=$status}
}
function Get-ItemProperty {
    [CmdletBinding()]
    param([string]$LiteralPath)
    if($LiteralPath -like '*NDP\v4\Full') {
        $release=528040
        if($scenario -eq 'Runtime') { $release=0 }
        return [pscustomobject]@{Release=$release}
    }
    $service='WinUSB'
    if($scenario -eq 'Driver') { $service='HidUsb' }
    return [pscustomobject]@{Service=$service}
}
$cases=@{Ready='Ready';Receiver='Receiver';WrongInterface='Receiver';Runtime='Runtime';Driver='Driver';DeviceError='Driver'}
foreach($scenario in $cases.Keys) {
    & (Join-Path $PSScriptRoot 'SetupPrerequisites.ps1') -Mode Check -OutputPath $output
    $actual=(Get-Content -LiteralPath $output | Where-Object { $_ -like 'State=*' }) -replace '^State=',''
    if($actual -ne $cases[$scenario]) { throw ('Setup gate failed for '+$scenario+': '+$actual) }
}
Write-Output '6 setup requirement gates passed using simulated device states. No downloads or driver writes were performed.'
