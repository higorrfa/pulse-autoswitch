# Validation

Reference hardware: one Sony PULSE 3D receiver `054C:0D5E`, Windows 11 x64 and an LG fallback monitor.

## Confirmed manually

- Headset off / on returns audio to the monitor / headset while the receiver stays connected.
- Microphone mute does not change output.
- Receiver removal / reconnection selects the fallback / headset as appropriate.
- Hardware volume changes show the temporary overlay; confirmed by the user.
- A Desktop executable with an embedded path opens the application; confirmed by the user.
- CHAT reduces balance; GAME increases it. Presses at the corresponding limit do not provide a usable separate signal.
- OFF/MONITOR with mute off provided no mapped event.
- The receiver reported 100% after charging and short usage, and later 70%. Reading availability is confirmed; accuracy is not calibrated.

These observations do not establish support for every headset batch, Windows release or Sony model.

## Automated checks

Run `scripts/Test.ps1`. Tests cover connection samples, invalid/truncated reports, unknown state, mute isolation, media direction, duplicate suppression, reconnect behavior and simulated consumer press/release edges. They do not access hardware, change drivers, switch audio or send media keys.

A simulated consumer event does not prove that the receiver will send it. UI layout and package installation are verified separately.

## Remaining hardware validation

| Area | Remaining check |
| --- | --- |
| Startup | Actual Windows sign-in with headset on, off and receiver removed |
| Sleep | Suspend/resume with receiver connected |
| Battery | Discharge, charging indication, accuracy and update steps |
| Buttons | Find an independent signal for balance-limit presses and MONITOR |
| Driver | Restore HID manually |
| Compatibility | Other receiver batches and Windows versions |

For manual smoke tests: configure outputs, test power off/on with audio playing, toggle mute and volume, remove/reconnect USB, check tray/battery/volume, open the Desktop launcher twice, and record results. Keep hardware tests separate from decoder tests.
