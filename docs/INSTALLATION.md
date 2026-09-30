# Installation and removal

## Prepare Windows and the package

Use 64-bit Windows and [.NET Framework 4.8 or later](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48). Install the Microsoft runtime if missing. PowerShell 5.1 or later is required for scripts. Desktop launcher generation uses the framework C# compiler.

Extract the complete package into a permanent, writable folder. Keep `scripts/` and `assets/`; do not run from inside the ZIP. From a source checkout, run `scripts/Build.ps1` first; binaries go to `bin/`.

## Associate WinUSB with the control interface

The original HID driver association did not allow the status reads used here on the reference PC. WinUSB enables those reads while the audio interface keeps its Windows audio driver.

1. Connect the PULSE 3D receiver.
2. Download Zadig from its [official website](https://zadig.akeo.ie/). Verify the **Akeo Consulting** digital signature.
3. Run Zadig and approve the administrator prompt.
4. Enable **Options → List All Devices**.
5. Select **USB ID `054C / 0D5E / 03`**. It appeared as **Hid Interface** on the reference PC; names vary.
6. Select **WinUSB** as the destination and click **Replace Driver**.
7. Close Zadig after installation completes.

**Select interface `03` only.** Leave the audio interface `MI_00` on USBAudio. Do not replace the entire composite receiver or its audio interface. The name “Wireless Stereo Headset” alone does not identify the required interface. The application reads the protocol without sending SET_REPORT commands.

Driver association requires administrator approval. Normal application use does not. Zadig and machine-specific driver packages are obtained separately from the official source.

## Install and configure

Open PowerShell in the extracted package or repository root:

```powershell
./scripts/CheckRequirements.ps1
./scripts/Install.ps1 -Force
```

The installer checks requirements, creates the Desktop executable and launches the application. It does not automatically install USB drivers. `-NoLaunch` can be used to finish setup without opening the application.

If execution policy blocks the scripts, use a session allowed by your computer's policy. On a personal unmanaged PC, after reviewing the scripts, a process-only policy can be set with `Set-ExecutionPolicy -Scope Process -ExecutionPolicy RemoteSigned`. If Windows marks the downloaded ZIP as blocked, review its source and use **Properties → Unblock** before extracting. Managed policy may require your administrator.

Select **Headset output**, choose **Fallback output**, and enable automatic routing. Test headset off / on without unplugging the receiver, mute, and receiver removal/reconnection. Settings are saved only in local `config.xml`. Automation and media shortcuts start disabled in fresh installations.

Use **Start with Windows** to control the Startup folder shortcut. Minimize or close the window to keep it in the tray; use **Quit** to stop it.

## Troubleshooting

| Symptom | Action |
| --- | --- |
| Desktop launcher cannot locate the application | Keep the application folder in place; rerun `scripts/CreateDesktopLauncher.ps1 -ApplicationDirectory . -Force` from the extracted package |
| Headset state unavailable | Check receiver connection, `054C:0D5E`, and WinUSB on `MI_03`; run the requirements script |
| No startup at sign-in | Enable Start with Windows; open `shell:startup`, check target/working directory, and check Windows Startup apps |
| Missing tray icons | Check the Windows hidden icon menu and verify the process is running |
| A player ignores output changes | Choose its Default/System audio output |
| No battery or volume reading | Power on the headset; transient values outside 0–100 are ignored |
| CHAT/GAME stops at a limit | This is a documented receiver limitation |

Quit the application before using the read-only USB probe:

```powershell
./PulseUsbProbe.exe
./PulseUsbProbe.exe --watch
```

For a source checkout, use `./bin/PulseUsbProbe.exe`. A normal read lasts approximately one second; watch mode lasts 90 seconds. Restart the application afterwards. Review device names and identifiers in `pulse.log` before publishing a log.

## Remove and restore the original HID driver

1. Disable **Start with Windows** and select **Quit**.
2. Remove the Desktop executable and extracted application folder.
3. In Device Manager, identify `USB\VID_054C&PID_0D5E&MI_03`.
4. Remove the WinUSB association/package for that interface or use the driver-update options to select the original HID driver.
5. Reconnect the receiver and confirm that the audio interface still uses USBAudio.

Driver restoration requires administrator rights and remains unvalidated by this project. Restoring HID may block the B0 reads used here. No automatic driver-removal script is included. Older launchers may have left `HKCU\Software\PulseAutoSwitch\ApplicationDirectory`; newly generated Desktop launchers do not use that value.
