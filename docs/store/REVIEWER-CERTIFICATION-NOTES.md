# PrivGuard 2.0 — Microsoft Store Certification Reviewer Notes

**Application Name:** PrivGuard
**Package Identity:** `cdevstudios.PrivGuard`
**Publisher ID:** `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`
**Publisher Display Name:** `cdev studios`
**Version:** `2.0.0.0`
**Target OS:** Windows 10 (10.0.19041.0+) and Windows 11 (x64)
**Architecture:** x64
**Contact:** [https://github.com/cristianbarreiro/PrivGuard/issues](https://github.com/cristianbarreiro/PrivGuard/issues)

---

## 1. Dear Microsoft Certification Team

Thank you for reviewing **PrivGuard 2.0 (Store Edition)**.

PrivGuard is a local-first Windows privacy utility designed to give users transparent, reliable control over their microphone muting and quick access to native Windows camera and microphone privacy settings. It features a modern Avalonia 11 Fluent Dark interface, runs with standard user rights (`asInvoker`), and uses official native Windows platform APIs.

This document provides clear instructions to easily test the application's features, explains the sole capability declared in the manifest (`rescap:runFullTrust`), and outlines our least-privilege security model.

---

## 2. Fast Step-by-Step Testing Guide for Reviewers

No specialized test accounts or external hardware are required. The application can be tested on any physical Windows 10/11 machine or standard Hyper-V / Azure / VirtualBox virtual machine with an emulated or real microphone.

### Test 1: Microphone Privacy Mute (Core Audio WASAPI) & System Settings Access
1. Launch **PrivGuard** from the Start Menu.
2. Note that the main window launches instantly with standard user privileges (`asInvoker`).
3. Under the **Microphone** card, click **Mute Microphone** (or toggle privacy mute):
   - The UI updates to **Muted**.
   - PrivGuard mutes active audio capture endpoints using the official Windows Core Audio COM API (`IAudioEndpointVolume`).
   - Notice the Windows taskbar microphone icon indicates muted state (or check in Windows Sound Settings `mmsys.cpl`).
4. Click **Unmute Microphone**:
   - The audio capture endpoint returns to its previous unmuted state.
5. Under the **Camera** card, click **Open Windows Camera Privacy Settings**:
   - PrivGuard opens the native Windows Settings page (`ms-settings:privacy-webcam`) so the user can review and configure system-level app access.

### Test 2: System Tray & Window Lifecycle
1. Click the window titlebar close button (`X`) or press `Alt+F4`:
   - Notice the application does **not** terminate unexpectedly. It cleanly minimizes to the Windows Notification Area (System Tray) with a notification tooltip.
2. Double-click the PrivGuard tray icon:
   - The main window restores smoothly.
3. Right-click the tray icon and select **Exit**:
   - PrivGuard coordinates a safe shutdown, cleanly restoring any runtime mute changes if configured, and terminates.

### Test 3: Windows Autostart (Startup Task)
1. Click the gear icon to open **Settings**.
2. Toggle **Start with system** (Iniciar con el sistema) to On.
3. Open **Windows Task Manager** > **Startup apps**:
   - Notice `PrivGuard` is listed as enabled with the command line parameter `--minimized`.
4. Sign out and sign back in to Windows:
   - PrivGuard launches quietly minimized to the System Tray without showing an intrusive window.

---

## 3. Capability Declaration: `rescap:runFullTrust`

PrivGuard Store Edition declares solely **`rescap:runFullTrust`** in `Package.appxmanifest`:

| Capability | Technical Justification |
|---|---|
| `runFullTrust` | **Required Win32 Desktop APIs:** PrivGuard is a packaged desktop application (.NET 10 / Win32) that communicates with native system subsystems: Core Audio COM interfaces (`IMMDeviceEnumerator`, `IAudioEndpointVolume`), WMI (`Win32_PnPEntity`) for audio/camera detection, and Win32 desktop windowing/tray APIs. It runs entirely as standard user (`asInvoker`). It does not run untrusted scripts or download external code. |

*Note on Restricted Capabilities:* In accordance with Store best practices, PrivGuard Store Edition declares **neither `allowElevation` nor `unvirtualizedResources`**. Advanced administrative hardware node management is excluded from the Store package and reserved for our open-source standalone distribution.

---

## 4. Security & Privacy Guarantees

- **No Media Capture:** PrivGuard has zero camera video capture or audio recording code. It contains no media capture pipeline.
- **Zero Telemetry / Offline:** PrivGuard makes zero outbound network requests. No personal information, hardware identifiers, or telemetry are collected or uploaded. Declares no internet capabilities.
- **Standard User Execution:** The entire application runs as a standard user (`asInvoker`). It never requests administrative rights or triggers UAC prompts in Store Edition.
- **Transparent Audio Guarantees:** WASAPI muting silences shared mode audio streams in the Windows Audio Engine. We transparently disclose that applications opening exclusive mode streams or devices lacking hardware mute controls are governed by Windows Core Audio SDK architecture.

We appreciate your review. If any clarification is needed, please contact us through our public repository at [https://github.com/cristianbarreiro/PrivGuard/issues](https://github.com/cristianbarreiro/PrivGuard/issues).
