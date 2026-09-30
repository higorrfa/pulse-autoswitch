# Setup assistant

## Install

1. Download `PulseAutoSwitch-Setup-0.3.0-win-x64.exe` from [Releases](https://github.com/higorrfa/pulse-autoswitch/releases).
2. Connect the PULSE 3D receiver, quit any running PULSE AutoSwitch instance through its tray menu, and open the executable.
3. The assistant checks Windows x64, Windows PowerShell, .NET Framework 4.8 and the receiver control interface.
4. If the runtime is missing, click **Install .NET Framework**. Review and complete Microsoft's installer, approve its administrator prompt and restart Windows if requested. Run PULSE Setup again after restarting.
5. If the control driver is missing, click **Open driver setup**. Approve Zadig's administrator prompt. In Zadig, select **USB ID `054C / 0D5E / 03`**, choose **WinUSB**, click **Replace Driver**, then close Zadig. The assistant checks the resulting driver before continuing. Do not select the audio interfaces or entire composite receiver.
6. Choose the install directory and whether to create a Desktop shortcut or start with Windows.
7. Open the application, select headset and fallback outputs, and enable routing.

The application is installed per user, by default under `%LOCALAPPDATA%\Programs\PulseAutoSwitch`. Normal application use does not require elevation. Missing prerequisites are downloaded from their official sources, so this is a single setup executable with optional online downloads, not an offline bundle. Driver association and Microsoft's runtime installation still require user interaction. Setup does not bypass Windows approval, accept Microsoft's terms automatically or select a USB device on the user's behalf.

The Setup-created Desktop shortcut points directly to the installed application. The portable ZIP continues to provide the executable launcher. New settings have routing disabled until outputs are selected. Existing `config.xml` is preserved on upgrades. Setup is currently unsigned.

## Downloads and verification

| Component | Download | Verification |
| --- | --- | --- |
| .NET Framework 4.8 web installer | Microsoft's official `https://go.microsoft.com/fwlink/?LinkId=2085155` | Valid Authenticode signature from Microsoft Corporation |
| Zadig 2.9 | [libwdi v1.5.1 release](https://github.com/pbatard/libwdi/releases/tag/v1.5.1) | Valid Akeo Consulting signature and SHA-256 `4ECAA95DF3DA3621486A043AEF8B3050B8BAFE7C901402871E816229EF82039B` |

Failed download or publisher verification stops that step. Cancelled administrator prompts, receiver absence and non-WinUSB control interfaces do not pass the requirement gate. Computer policy may prevent script execution or driver association; the assistant reports failure instead of changing that policy. PowerShell's process-level execution-policy option is used only for the embedded setup script.

## Removal

Quit the application and uninstall **PULSE AutoSwitch** from Windows Installed apps. Setup removes its installed files and shortcuts. Local `config.xml` is retained for a future reinstall. Logs created after installation may remain. The WinUSB association is retained; restore the original HID driver separately using [Installation and removal](INSTALLATION.md).

## Build

Install [Inno Setup 6.7.3 or newer](https://jrsoftware.org/isdl.php) and run:

```powershell
./scripts/BuildInstaller.ps1 -Compiler 'C:\path\to\ISCC.exe'
```

The build compiles the app, runs decoder tests, prepares the portable ZIP and compiles `installer/PulseAutoSwitch.iss`. The result is written to `dist/`. No runtime or Zadig binary is included in the setup.

## Validation

On the reference Windows 11 PC, requirement detection reported Ready. An isolated installation verified the application payload, reinstallation preserved an existing settings file, and uninstall removed the application while retaining settings. This test used an already-configured receiver and did not reinstall its driver or runtime. First-time driver/runtime flows and the interactive wizard appearance still need validation on a fresh PC. Existing routing limitations are unchanged.
