# Third-party references

Project code is distributed under the MIT license in `LICENSE`. Application binaries use Windows and .NET Framework APIs installed on the user's computer. The preview does not redistribute Zadig, libwdi, HIDAPI or generated driver packages.

| Reference | Purpose |
| --- | --- |
| [promanski/pulse3d-ksystemstats](https://github.com/promanski/pulse3d-ksystemstats) | PULSE 3D B0 report and battery field |
| [audioswitch IPolicyConfig.h](https://github.com/tartakynov/audioswitch/blob/master/IPolicyConfig.h) | COM interface layout for default audio output selection |
| [WinUsb_ControlTransfer](https://learn.microsoft.com/en-us/windows/win32/api/winusb/nf-winusb-winusb_controltransfer) | USB control transport |
| [WinUsb_QueryPipe](https://learn.microsoft.com/en-us/windows/win32/api/winusb/nf-winusb-winusb_querypipe) | Input endpoint discovery |
| [WinUsb_ReadPipe](https://learn.microsoft.com/en-us/windows/win32/api/winusb/nf-winusb-winusb_readpipe) | USB input packets |
| [SendInput](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput) | Windows media keys |
| [Windows Script Host shortcuts](https://learn.microsoft.com/en-us/troubleshoot/windows-client/admin-development/create-desktop-shortcut-with-wsh) | Startup folder shortcut |
| [Zadig](https://zadig.akeo.ie/) / [libwdi](https://github.com/pbatard/libwdi) | External driver association tool |
| [Inno Setup](https://jrsoftware.org/isinfo.php) | Compiler and installer engine; original upstream engine is included in the compiled setup |

Zadig is GPLv3 or later and libwdi is LGPLv3 or later; they are downloaded separately from their official sources. The application's MIT license does not change external component licenses. Before incorporating external source or distributing an additional dependency, include its required notices and license.
