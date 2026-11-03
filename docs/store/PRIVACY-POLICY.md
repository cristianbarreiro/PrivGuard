# PrivGuard â€” Privacy Policy

**Effective Date:** November 2026<br/>
**Last Updated:** November 2026<br/>
**Developer:** cdev studios<br/>
**Lead Maintainer:** Cristian Barreiro<br/>
**Application:** PrivGuard (Camera & Microphone Privacy Guard for Windows)<br/>
**Public Policy URL:** `https://github.com/cristianbarreiro/PrivGuard/blob/main/docs/store/PRIVACY-POLICY.md`<br/>
**Website / Repository:** [https://github.com/cristianbarreiro/PrivGuard](https://github.com/cristianbarreiro/PrivGuard)

---

## 1. Introduction & Core Philosophy

PrivGuard is a Windows desktop application designed to give users transparent, reliable control over their camera and microphone privacy. Depending on user-selected preferences, PrivGuard interacts with Windows privacy settings, Core Audio capture endpoint mute states, and Configuration Manager (PnP) device nodes.

PrivGuard is built with an absolute **Local-First, Zero-Telemetry Privacy Architecture**:
- **Zero Remote Telemetry:** PrivGuard contains no analytics beacons, marketing trackers, telemetry collectors, or crash-reporting webhooks.
- **Zero Cloud Storage:** No user preferences, device IDs, or session journals are ever transmitted to any remote server or third-party service.
- **Zero Media Access:** PrivGuard never accesses, previews, records, processes, or transmits camera video streams, photos, microphone audio, or voice data.

This Privacy Policy explains what local system identifiers are processed, where local configuration files are stored, how your data is safeguarded, and how you retain complete control over your system.

---

## 2. Information Accessed & Processed Locally

All data accessed by PrivGuard is processed strictly in-memory or stored locally on your device. **No data is ever transmitted across a network.**

### 2.1 Local Windows Security Identifier (SID)
- **What is accessed:** The application calls `WindowsIdentity.GetCurrent().User` to retrieve the local Windows Security Identifier (SID) of the current user account (e.g., `S-1-5-21-...`).
- **Exclusive Purpose:** To construct and enforce a kernel-level Access Control List (`PipeSecurity`) for the local in-memory Named Pipe used to communicate between the unprivileged user interface (`asInvoker`) and the transient, on-demand elevated worker (`--privileged-worker`).
- **Data Safeguard:** The SID is used solely to restrict access so that only your current user account, local Administrators, and SYSTEM can connect to the pipe. The SID is **never** written to persistent storage, **never** logged to disk, and **never** transmitted across a network.

### 2.2 Hardware & Device Identifiers
- **What is accessed:** Plug and Play (PnP) Device Instance IDs (e.g., `USB\VID_...`), device friendly names, and Windows Core Audio endpoint GUIDs (e.g., `{c166523c-...}`).
- **Exclusive Purpose:** To enumerate connected webcams and microphones, display their friendly names in the application interface, and identify which device node to disable or mute when protection is enabled.
- **Persistence:** Device Instance IDs and original volume/mute states are stored in the local session journal (`privacy-session-v1.json`) to guarantee accurate rollback when protection is turned off or after a reboot.

### 2.3 System State & Privacy Preferences
- **What is accessed:** Reads and writes Windows user-level privacy preferences under `HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore` and machine-level Group Policies under `HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy`.
- **Exclusive Purpose:** To enforce camera and microphone blocking across Windows applications and verify observable effective states.

---

## 3. Local Storage Locations & Data Retention

All files created by PrivGuard reside strictly in local directories beneath your Windows user profile:

### 3.1 Primary Application Data Directory (`%LOCALAPPDATA%\PrivGuard`)
1. **User Preferences (`%LOCALAPPDATA%\PrivGuard\state.json`):**
   - Stores interface language (`en` / `es`), desired protection mode (Standard / Secure), theme preference, and autostart preference.
2. **Durable Privacy Session Journal (`%LOCALAPPDATA%\PrivGuard\Recovery\privacy-session-v1.json`):**
   - An atomic Write-Ahead Log (WAL) recording the original state of hardware and policies before any change. Used exclusively to verify that previous system states can be restored cleanly during application shutdown or crash recovery.
3. **Local Diagnostic Logs (`%LOCALAPPDATA%\PrivGuard\Logs\PrivGuard-*.log`):**
   - Human-readable rolling diagnostic text files recording operational milestones, status transitions, and native API exit codes. Automatically pruned on a rotating basis.
4. **Local Crash Reports (`%LOCALAPPDATA%\PrivGuard\CrashReports\crash-*.json`):**
   - Generated only when an unhandled exception occurs. Contains exception stack traces, timestamp, and runtime diagnostic metadata.
   - **Privacy Protection:** In accordance with our security requirements, personal machine names and user names are strictly stripped and excluded from JSON crash reports.

### 3.2 Compatibility Sentinel Marker (`%USERPROFILE%\AppData\Local\PrivGvard\Recovery\active-session-v1.marker`)
- **Purpose:** A fixed 128-byte sentinel file containing a session UUID.
- **Why It Exists:** Retained under the legacy path for cross-version compatibility so that system uninstallers, recovery watchers, or maintenance scripts across different user profiles can detect whether a protection session is currently in flight before removing application binaries.
- **Lifecycle:** Automatically removed upon clean unprotection or application exit.

---

## 4. Microsoft Partner Center Privacy Disclosure

In the Microsoft Partner Center submission questionnaire, Microsoft asks:
> *"Does this product access, collect, or transmit personal information?"*

**Our Official Disclosure:**
- In strict adherence to Microsoft Partner Center definitions, we answer **Yes**, because PrivGuard queries the local Windows Security Identifier (SID) for inter-process ACL configuration and processes local Plug and Play hardware device identifiers.
- **Important Distinction:**
  - **Local Access & Processing:** Yes (local SID for pipe security; local device IDs for hardware toggling).
  - **Local Persistent Storage:** Yes (device IDs and preferences stored in `%LOCALAPPDATA%\PrivGuard`).
  - **Network Transmission:** **NO (0%)**. PrivGuard does not declare network capabilities and transmits zero bytes over the internet.
  - **Third-Party Disclosure:** **NO (0%)**. No third parties have access to your device data.

---

## 5. Security & Protection Controls

- **NTFS Access Controls:** All local storage files are written with file permissions inherited from the user's local application data folder, restricting read and write access strictly to the logged-in user and system administrators.
- **Least-Privilege Token Model:** The main application always runs with standard user privileges (`asInvoker`).
- **Cryptographic Nonce & Kernel PID Verification:** When elevated Secure Protection is used, communication with the transient worker is authenticated using a 256-bit cryptographically secure random nonce (`RandomNumberGenerator`) and verified bilateral kernel process IDs.

---

## 6. How to Delete All Stored Information

You have full control over your data. To permanently remove all stored preferences and files:
1. Open PrivGuard and click **Disable Protection** (this restores all hardware and policies to original system states).
2. Exit PrivGuard from the System Tray menu (**Exit**).
3. Uninstall the application via **Windows Settings > Apps > Installed apps > PrivGuard > Uninstall**.
4. Delete the local folders:
   - `%LOCALAPPDATA%\PrivGuard`
   - `%USERPROFILE%\AppData\Local\PrivGvard` (if present from previous versions)

---

## 7. Contact Information

For any questions, security inquiries, or privacy feedback:
- **Developer / Publisher:** cdev studios
- **Lead Maintainer:** Cristian Barreiro
- **Public GitHub Repository & Issues:** [https://github.com/cristianbarreiro/PrivGuard/issues](https://github.com/cristianbarreiro/PrivGuard/issues)
- **Public Policy Document:** [https://github.com/cristianbarreiro/PrivGuard/blob/main/docs/store/PRIVACY-POLICY.md](https://github.com/cristianbarreiro/PrivGuard/blob/main/docs/store/PRIVACY-POLICY.md)
