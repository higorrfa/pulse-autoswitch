# Changelog

## 0.3.0 — setup preview

- Single Windows setup executable with runtime and receiver-driver checks.
- Download missing .NET Framework or Zadig from official sources, with publisher verification and a pinned Zadig checksum.
- Guided WinUSB association for control interface 03, followed by verification before installation.
- Per-user application installation, Desktop and Start menu shortcuts, optional startup and registered uninstaller.
- Preserve local settings during upgrades and removal.

## 0.2.0 — development preview

- Minimal dashboard with connection status, battery and volume cards, output selectors and an automatic-routing toggle.
- Advanced section for experimental shortcuts and diagnostics.
- English UI, scripts and code strings; bilingual English/Portuguese README.
- Code comments removed; protocol references retained in documentation.
- Requirement checks, installation script and complete package documentation.

## 0.1.3 — development preview

- Desktop launcher with an embedded application path.
- Hardware volume display and temporary focus-free overlay.

## 0.1.2 — development preview

- Startup folder shortcut with application working directory.
- Repeated `--tray` launches preserve the current minimized instance.

## 0.1.1 — development preview

- Documented balance-limit and MONITOR limitations.
- Decoder support for the declared consumer report; reception not observed on hardware.
- Repeated B0 snapshots do not create false media commands.

## 0.1.0 — development preview

- Wireless connection reads, automatic output selection and mute isolation.
- Tray icons, battery percentage, battery flyout and single-instance launcher.
- Experimental CHAT/GAME media shortcuts.
- Source structure, build/test/package scripts, workflow and MIT license.
