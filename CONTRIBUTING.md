# Contributing

Build with `scripts/Build.ps1` and run `scripts/Test.ps1`. Keep each change focused and document behavior changes. Source code, UI strings, scripts and detailed documentation use English; README sections are maintained in English and Portuguese.

For protocol changes, include anonymized reports and the physical actions that produced them. Clearly distinguish simulated tests from observed hardware behavior. Update README and `docs/VALIDATION.md` when a limitation changes.

Discover devices by USB identity. Do not embed device instance paths, serial numbers, audio IDs or machine-specific GUIDs. Do not assume that PULSE Elite or PlayStation Link share the PULSE 3D protocol.

Keep runtime data, driver packages, certificates, credentials and generated executables out of Git. Media changes must preserve initialization, reconnect, battery, mute and volume guards. Use the documentation for protocol explanations and references rather than code comments.

Contributions use the repository's MIT license.
