# PrivGuard — Microsoft Store Capabilities Justification (Store Edition)

**Document Version:** 2.0.0
**Date:** November 2026
**Application Name:** PrivGuard
**Package Identity (Name):** `cdevstudios.PrivGuard`
**Publisher:** `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`
**Publisher Display Name:** `cdev studios`
**Package Family Name (PFN):** `cdevstudios.PrivGuard_tv7b4yqg10sw2`
**Microsoft Store ID:** `9NQ56N45XH5N`
**Target Platform:** Windows 10 (Build 19041+) and Windows 11 (x64)
**Submission Context:** Partner Center Capabilities Review

---

## 1. Executive Summary

PrivGuard is a native Windows desktop privacy utility that enables users to inspect and manage their microphone and camera privacy controls.

Under **Architectural Decision D-01**, PrivGuard employs a dual-edition model:
1. **PrivGuard Store Edition (MSIX):** Declares solely the standard Win32 Desktop Bridge capability `rescap:runFullTrust`. It provides unprivileged Core Audio endpoint muting via WASAPI and direct access/inspection of Windows native privacy settings (`ms-settings:`). It does not declare `allowElevation` or `unvirtualizedResources`, avoiding child process elevation and registry virtualization overrides within the MSIX package.
2. **PrivGuard Standalone Edition (Inno Setup / Portable):** Retains full system-level Secure Protection (PnP DevNode management and HKLM Group Policies) using an authenticated on-demand worker.

This document provides the technical justification for the sole capability declared in the Microsoft Store package: **`rescap:runFullTrust`**.

---

## 2. Capability Details & Technical Justification: `rescap:runFullTrust`

### 2.1 Purpose
Allows the packaged desktop application to run outside the AppContainer sandbox as a standard Win32 desktop process executing under standard user privileges (`requestedExecutionLevel="asInvoker"`).

### 2.2 Why `rescap:runFullTrust` Is Strictly Necessary
PrivGuard Store Edition relies directly on native Windows desktop subsystems and COM interfaces that are inaccessible from an AppContainer UWP sandbox:

1. **COM Interop for Windows Core Audio (WASAPI):**
   - PrivGuard enumerates active audio recording endpoints using `IMMDeviceEnumerator` (`eCapture`, `DEVICE_STATE_ACTIVE`).
   - It controls and observes master mute states across audio capture endpoints using `IAudioEndpointVolume` (`SetMute`, `GetMute`).
   - It inspects hardware volume capabilities via `IAudioEndpointVolume::QueryHardwareSupport`.
2. **WMI Hardware Detection & Device Enumeration:**
   - PrivGuard queries `Win32_PnPEntity` via `System.Management` to discover plugged-in camera and microphone hardware, correlate friendly names with device identifiers, and display real-time availability to the user.
3. **Desktop Windowing & Lifecycle Integration:**
   - Avalonia 11 Fluent Dark desktop UI with custom title bar controls.
   - Native Windows Notification Area (System Tray) integration, enabling the application to remain minimized to the tray when the window is closed (`X`) or hidden.
   - Integration with the Windows Startup Task extension (`windows.startupTask`).

These operations require full trust execution because they depend on native Win32 APIs, COM interfaces, and Windows Management Instrumentation.

---

## 3. Delimitation of Scopes: Omission of Restricted Capabilities

In accordance with Store policy and architectural best practices:

- **No `allowElevation`:** The Store Edition package contains zero elevated helper execution. It runs entirely as a standard user (`asInvoker`). Operations requiring administrator privileges (PnP device node disabling and `HKLM` Group Policy modification) are excluded from the Store build and reserved exclusively for the Standalone Edition.
  *(Note on Evidence: Microsoft has not issued an express rejection regarding publisher account tiers; the exclusion of elevation is a deliberate architectural design to ensure clean package lifecycle and avoid orphaned hardware states upon MSIX uninstallation).*
- **No `unvirtualizedResources`:** Store Edition does not attempt direct writes to `HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore`. Instead, it provides one-click navigation to Windows native privacy settings (`ms-settings:privacy-webcam` and `ms-settings:privacy-microphone`). This eliminates any requirement for registry virtualization bypasses or flexible virtualization schemas.

---

## 4. Threat Mitigation & Security Controls Summary

| Security Concern | PrivGuard Store Edition Mitigation |
|---|---|
| **Privilege Escalation** | Operates strictly as a standard user (`asInvoker`). Zero elevation code paths, zero UAC prompts, zero background Windows services. |
| **System Integrity & Reversibility** | Modifies only runtime Core Audio endpoint mute states via standard COM APIs. Does not modify machine-wide registry keys (`HKLM`) or PnP hardware device tree. |
| **Data Isolation & Clean Lifecycle** | Operates within the standard user space. Runtime session state and logs are maintained locally. Uninstallation cleanly removes the application. |
| **Hardware State Guarantees** | Transparent UI reporting: indicates when endpoints are muted via WASAPI. Explicitly documents the distinction between shared mode muting and driver/hardware limitations. |
| **Privacy & Telemetry** | 100% local operation: zero analytics, zero telemetry, zero media capture, zero network transmission. Declares no internet capabilities. |
