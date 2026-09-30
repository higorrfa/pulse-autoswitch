# Dependencies and package contents

| Requirement | Purpose | Source | Administrator |
| --- | --- | --- | --- |
| 64-bit Windows | Audio, tray, WinForms and native USB APIs | Installed operating system | Not for normal use |
| .NET Framework 4.8 or later | Application runtime and launcher compiler | [Microsoft downloads](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48) | May be needed for runtime installation |
| PowerShell 5.1 or later | Installation and build scripts | Windows PowerShell | Not for package scripts |
| WinUSB on control interface `MI_03` | Wireless connection, battery and volume reads | Windows driver associated with [Zadig](https://zadig.akeo.ie/) | Required for association |
| PULSE 3D receiver `054C:0D5E` | Supported hardware | User's headset and receiver | No |

The default source build uses the framework compiler. MSBuild also requires .NET Framework 4.8 development components. Reference hardware was tested on Windows 11; other receiver batches and Windows releases need validation.

## Included

- `PulseAutoSwitch.exe`: application.
- `PulseUsbProbe.exe`: read-only diagnostic.
- `Start PULSE AutoSwitch.exe`: portable launcher next to the application.
- `scripts/CheckRequirements.ps1`: prerequisite and receiver-driver checks.
- `scripts/Install.ps1`: check prerequisites, generate Desktop launcher and launch.
- `scripts/CreateDesktopLauncher.ps1` and `scripts/PulseLauncher.cs`: generate the personalized Desktop executable.
- `assets/`, example settings, README, installation/protocol/validation documents, license and third-party references.

The application relies on Windows-provided APIs and framework libraries. Zadig, its GPL/LGPL components, the .NET installer and machine-specific driver packages are not redistributed. Official links above provide those external prerequisites. See [Third-party references](../THIRD_PARTY_NOTICES.md).

Local settings, logs, personalized launchers and generated driver packages must not be committed to Git.
