# PrivGuard 2.0 — Microsoft Store Certification Reviewer Notes

**Application Name:** PrivGuard
**Package Identity:** `cdevstudios.PrivGuard`
**Publisher ID:** `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`
**Version:** `2.0.0.0`
**Target OS:** Windows 10 (10.0.19041.0+) and Windows 11 (x64, ARM64)
**Architecture:** x64
**Contact:** [https://github.com/cristianbarreiro/PrivGuard/issues](https://github.com/cristianbarreiro/PrivGuard/issues)

---

## 1. Dear Microsoft Certification Team

Thank you for reviewing **PrivGuard 2.0**.

PrivGuard is a local-first, local-first Windows privacy utility designed to give users transparent, reliable control over their camera and microphone privacy. It features a modern Avalonia 11 Fluent Dark interface, runs with standard user rights (`asInvoker`), and uses native Windows platform APIs to apply and verify privacy protections.

This document provides clear instructions to easily test the application's features, explains why restricted capabilities are declared, and outlines our least-privilege security model.

---

## 2. Fast Step-by-Step Testing Guide for Reviewers

No specialized test accounts or external hardware are required. The application can be tested on any physical Windows 10/11 machine or standard Hyper-V / Azure / VirtualBox virtual machine with an emulated or real camera/microphone.

### Test 1: Standard Protection (No Admin Rights / No Elevation)
1. Launch **PrivGuard** from the Start Menu.
2. Note that the main window launches instantly with standard user privileges (`asInvoker`).
3. Under the **Camera** card, click **Enable Standard Protection**:
   - The UI updates to **Locked / Protected**.
   - PrivGuard updates the Windows Camera consent policy under `HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam`.
   - Open **Windows Settings** > **Privacy & security** > **Camera**; notice that app access to the camera is set to Off.
4. Under the **Microphone** card, click **Enable Standard Protection**:
   - The UI updates to **Locked / Protected**.
   - PrivGuard mutes the active audio capture endpoint using the official Windows Core Audio COM API (`IAudioEndpointVolume`).
   - Notice the Windows taskbar microphone icon indicates muted state.
5. Click **Disable Standard Protection** on both devices:
   - All settings and mute states are immediately restored to their original values.

### Test 2: Secure Protection & UAC Elevation Handling
1. First, enable **Standard Protection** for Camera (Secure Protection is an enhancement layer on top of Standard).
2. Click **Enable Secure Protection** (or toggle **Advanced Protection** in Settings):
   - Windows displays the standard User Account Control (UAC) elevation prompt asking for permission to run the helper process (`PrivGuard.exe --privileged-worker`).
3. **Cancellation Test (Safe Fail):**
   - Click **No** on the UAC prompt.
   - PrivGuard detects the cancellation cleanly, displays "Operation cancelled: Administrator permissions were denied," and leaves the camera safely in Standard protection without crashing or corrupting state.
4. **Approval Test:**
   - Click **Enable Secure Protection** again and click **Yes** on the UAC prompt.
   - The worker executes the privileged request via `CfgMgr32.dll` (`CM_Disable_DevNode`) and writes the machine-wide Group Policy (`HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy\LetAppsAccessCamera` set to `2`).
   - The UI verifies the native hardware state and displays **Secure Active (Shield icon)**.
   - The elevated worker immediately closes or idles for at most 30s. The main UI process **never** retains elevated privileges.
5. Click **Disable Protection**:
   - PrivGuard restores the PnP device node and policy cleanly.

### Test 3: System Tray & Window Lifecycle
1. Click the window titlebar close button (`X`) or press `Alt+F4`:
   - Notice the application does **not** terminate. It cleanly minimizes to the Windows Notification Area (System Tray) to keep privacy protections active in the background.
2. Double-click the PrivGuard tray icon:
   - The main window restores smoothly.
3. Right-click the tray icon and select **Exit**:
   - PrivGuard coordinates a safe shutdown, restoring any owned hardware changes before exiting.

### Test 4: Windows Autostart (Startup Task)
1. Click the gear icon to open **Settings**.
2. Toggle **Start with system** (Iniciar con el sistema) to On.
3. Open **Windows Task Manager** > **Startup apps**:
   - Notice `PrivGuard` is listed with command line parameter `--minimized`.
4. Sign out and sign back in to Windows:
   - PrivGuard launches quietly minimized to the System Tray without showing an intrusive window.

---

## 3. Restricted Capabilities Justification

PrivGuard declares three restricted capabilities in `Package.appxmanifest` under `xmlns:rescap`:

| Capability | Justification |
|---|---|
| `runFullTrust` | **Required Win32 Desktop APIs:** PrivGuard is a packaged desktop application (.NET 10 / Win32) that communicates with native system subsystems: Core Audio COM interfaces (`IMMDeviceEnumerator`, `IAudioEndpointVolume`), Windows Configuration Manager (`CfgMgr32.dll`), and WMI (`Win32_PnPEntity`). It does not run untrusted scripts or download external code. |
| `allowElevation` | **On-Demand Transient Helper (`--privileged-worker`):** To avoid running the entire application as administrator or installing a permanent background Windows Service, PrivGuard uses standard user rights (`asInvoker`) for 99% of its lifecycle. When the user explicitly requests Secure Protection, it uses `allowElevation` to launch its own binary with verb `runas` to toggle PnP hardware states and write HKLM Group Policies. The helper authenticates over a named pipe with a 256-bit nonce and kernel PID verification, and terminates upon completion. |
| `unvirtualizedResources` | **System-Wide Privacy Enforcement:** The purpose of PrivGuard is to protect the user across all Windows applications (browsers, video conferencing, native apps). Registry writes to `HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy` must be written to the real system registry so that Windows enforces camera/microphone access denials for all applications, rather than being virtualized inside an isolated container. |

---

## 4. Security & Privacy Guarantees

- **No Media Capture:** PrivGuard has zero camera video capture or audio recording code. It contains no media capture pipeline.
- **Zero Telemetry / Offline:** PrivGuard makes zero outbound network requests. No personal information, hardware identifiers, or telemetry are collected or uploaded.
- **Durable Recovery (No Stranded Hardware):** The application implements a Write-Ahead Log (WAL) session journal in `%LOCALAPPDATA%\PrivGuard\Recovery`. If the computer crashes or loses power while protected, PrivGuard automatically recovers and restores original hardware states on the next launch.

We appreciate your review. If any clarification is needed, please contact us through our public repository at [https://github.com/cristianbarreiro/PrivGuard/issues](https://github.com/cristianbarreiro/PrivGuard/issues).
