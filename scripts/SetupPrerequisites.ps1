[CmdletBinding()]
param(
    [ValidateSet('Check','Runtime','Driver')][string]$Mode='Check',
    [Parameter(Mandatory=$true)][string]$OutputPath
)
$ErrorActionPreference='Stop'
$result='Error'
$message=''
try {
    if(-not [Environment]::Is64BitOperatingSystem) { throw 'Windows x64 is required.' }
    if($PSVersionTable.PSVersion -lt [Version]'5.1') { throw 'PowerShell 5.1 or newer is required.' }
    [Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
    $cacheDir=Split-Path -Parent ([IO.Path]::GetFullPath($OutputPath))
    function Get-VerifiedDownload([string]$Url,[string]$Name,[string]$Publisher,[string]$Hash) {
        $file=Join-Path $cacheDir $Name
        Invoke-WebRequest -Uri $Url -OutFile $file -UseBasicParsing
        if($Hash -and (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $Hash) { throw 'Download checksum verification failed.' }
        $signature=Get-AuthenticodeSignature -LiteralPath $file
        if($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch $Publisher) { throw 'Download publisher verification failed.' }
        return $file
    }
    if($Mode -eq 'Runtime') {
        $runtime=Get-VerifiedDownload 'https://go.microsoft.com/fwlink/?LinkId=2085155' 'dotnet48.exe' 'O=Microsoft Corporation' ''
        $process=Start-Process -FilePath $runtime -Verb RunAs -PassThru -Wait
        if($process.ExitCode -eq 3010 -or $process.ExitCode -eq 1641) { $result='Restart'; $message='Restart Windows, then run PULSE AutoSwitch Setup again.' }
        elseif($process.ExitCode -ne 0) { throw ('The Microsoft installer exited with code '+$process.ExitCode+'.') }
    }
    if($Mode -eq 'Driver') {
        $receivers=@(Get-PnpDevice -PresentOnly -ErrorAction Stop | Where-Object { $_.InstanceId -like 'USB\VID_054C&PID_0D5E&MI_03*' })
        if($receivers.Count -eq 0) { throw 'Connect the PULSE 3D receiver before opening driver setup.' }
        $zadig=Get-VerifiedDownload 'https://github.com/pbatard/libwdi/releases/download/v1.5.1/zadig-2.9.exe' 'zadig-2.9.exe' 'O=Akeo Consulting' '4ECAA95DF3DA3621486A043AEF8B3050B8BAFE7C901402871E816229EF82039B'
        @'
[general]
advanced_mode=true
exit_on_success=true
log_level=1
[device]
list_all=true
include_hubs=false
trim_whitespaces=true
[driver]
default_driver=0
extract_only=false
'@ | Set-Content -LiteralPath (Join-Path $cacheDir 'zadig.ini') -Encoding ASCII
        Start-Process -FilePath $zadig -WorkingDirectory $cacheDir -Verb RunAs -Wait | Out-Null
    }
    if($result -ne 'Restart') {
        $framework=Get-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -ErrorAction SilentlyContinue
        if(-not $framework -or $framework.Release -lt 528040) { $result='Runtime'; $message='.NET Framework 4.8 or newer is required.' }
        else {
            $receivers=@(Get-PnpDevice -PresentOnly -ErrorAction Stop | Where-Object { $_.InstanceId -like 'USB\VID_054C&PID_0D5E&MI_03*' })
            if($receivers.Count -eq 0) { $result='Receiver'; $message='Connect the PULSE 3D USB receiver, then click Check again.' }
            else {
                $ready=$true
                foreach($receiver in $receivers) {
                    $device=Get-ItemProperty -LiteralPath ('HKLM:\SYSTEM\CurrentControlSet\Enum\'+$receiver.InstanceId)
                    if($device.Service -ne 'WinUSB' -or $receiver.Status -ne 'OK') { $ready=$false }
                }
                if($ready) { $result='Ready'; $message='Runtime and PULSE 3D control driver are ready.' }
                else { $result='Driver'; $message='Open driver setup. Select USB ID 054C / 0D5E / 03, choose WinUSB, click Replace Driver, then close Zadig. Keep the audio interfaces on their Windows audio driver.' }
            }
        }
    }
}
catch { $result='Error'; $message=$_.Exception.Message }
$message=$message -replace '[\r\n]+',' '
[IO.File]::WriteAllText($OutputPath,"[Prerequisites]`r`nState=$result`r`nMessage=$message`r`n",[Text.Encoding]::Unicode)
if($result -eq 'Error') { exit 1 }
