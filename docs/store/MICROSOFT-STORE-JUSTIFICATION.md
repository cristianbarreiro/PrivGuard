# PrivGvard — Microsoft Store Restricted Capabilities Justification

**Document Version:** 1.0.0  
**Date:** September 2026  
**Application Name:** PrivGvard  
**Package Identity (Name):** `cdevstudios.PrivGvard`  
**Publisher:** `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`  
**Publisher Display Name:** `cdev studios`  
**Package Family Name (PFN):** `cdevstudios.PrivGvard_tv7b4yqg10sw2`  
**Microsoft Store ID:** `9N079FF8XLVN`  
**Target Platform:** Windows 10 (Build 19041+) and Windows 11 (x64, ARM64)  
**Submission Contact:** `reportapp@microsoft.com` / Partner Center Restricted Capabilities Review  

---

## 1. Executive Summary

PrivGvard is a native Windows privacy utility that applies and reports Windows privacy controls for cameras and microphones. The available controls and their results depend on the selected mode, hardware, driver, permissions, and system configuration.

The package declares three restricted MSIX capabilities for the Windows integration described below. The exact necessity and behavior of each capability must be assessed against the packaged application and its supported scenarios:

1. `rescap:runFullTrust`
2. `rescap:allowElevation`
3. `rescap:unvirtualizedResources`

The main user interface runs as a standard user (`asInvoker`). Operations that require administrative access request it through a separate authenticated helper launched with Windows `runas` behavior. The helper may be reused across operations within a scope and is closed when that scope ends; neither operation duration nor helper shutdown is guaranteed to be immediate.

---

## 2. Capability Details & Technical Justifications

### 2.1 `rescap:allowElevation`

#### Purpose
Allows the application to launch an on-demand, elevated helper process (`PrivGvard.exe --privileged-worker`) via the standard Windows UAC dialog (`Verb="runas"`).

#### Why It Is Necessary
PrivGvard operates two layers of privacy protection:

1. **Standard Layer (No Elevation):**
   - Mutes Core Audio recording endpoints via Windows Core Audio COM APIs (`IMMDeviceEnumerator`, `IAudioEndpointVolume`).
   - Toggles current user privacy preferences under `HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore`.

2. **Secure Layer (Requires Administrator Privileges):**
   - **PnP device management:** For supported devices, PrivGvard calls Windows Configuration Manager (`CfgMgr32.dll` APIs including `CM_Locate_DevNode_W`, `CM_Disable_DevNode`, and `CM_Enable_DevNode`) to request a device-node state change and checks the reported state. This check does not establish that every sensor, driver, or data path is blocked in every configuration.
   - **Machine-level Windows privacy policy:** PrivGvard can write the camera and microphone AppPrivacy values under `HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy` (`LetAppsAccessCamera` and `LetAppsAccessMicrophone` set to `2`) and reads them back where possible. The effective result depends on Windows, device, and policy configuration; this registry operation is not a guarantee that every sensor or application access path is blocked.

These device-management and machine-level registry operations are issued through the elevated helper. Windows permissions, device support, policy configuration, or other system conditions can cause an operation or its verification to fail; the application reports such failures rather than treating the requested state as confirmed.

#### Least-Privilege Architecture (Why Not a Background Service?)
Rather than installing a permanent background Windows Service running as `NT AUTHORITY\SYSTEM` (which would create an ongoing security attack surface), PrivGvard uses **transient on-demand elevation**:

- The main interactive application (`PrivGvard.exe`) runs as a standard user (`asInvoker`).
- When the user clicks "Protect" or "Unprotect" in the Secure Layer, PrivGvard launches its own executable with the argument `--privileged-worker` using `ShellExecuteEx` with `Verb="runas"`.
- The helper is launched with Windows `runas` behavior. The elevation or consent experience follows the account and system policy; cancellation or failure is reported as an unsuccessful operation.
- The standard process and the elevated worker communicate over an in-memory **Named Pipe** secured with:
  - An explicit `PipeSecurity` Access Control List (ACL) granting access only to the current user SID and Administrators.
  - A cryptographically random 256-bit authentication nonce passed via CLI and validated before accepting any command.
  - Bilateral kernel process ID (PID) verification using `GetNamedPipeServerProcessId` / `GetNamedPipeClientProcessId`.
  - A strictly closed whitelist of 7 permitted operations (`apply-policy`, `restore-policy`, `apply-device`, `restore-device`, `verify-policy-ownership`, `verify-device-ownership`, `ping`).
- The helper can be reused while its operation scope remains active. When the scope closes, PrivGvard attempts to close the helper and checks whether it stopped; the shutdown path can wait and may attempt termination if needed. The UI process remains `asInvoker`, while the helper has an elevated token only while it is running.

---

### 2.2 `rescap:runFullTrust`

#### Purpose
Allows the application to run outside the AppContainer sandbox as a standard Win32 desktop bridge process.

#### Why It Is Necessary
1. **P/Invoke to Native System Libraries:** PrivGvard requires direct P/Invoke calls to `CfgMgr32.dll` (`CM_Locate_DevNode_W`, `CM_Disable_DevNode`, `CM_Enable_DevNode`, `CM_Get_DevNode_Status`) and `kernel32.dll`.
2. **COM Interop for Windows Core Audio:** PrivGvard enumerates active audio recording endpoints and toggles master mute states using native COM interfaces (`IMMDeviceEnumerator`, `IMMDevice`, `IAudioEndpointVolume`).
3. **WMI Hardware Detection:** PrivGvard queries `Win32_PnPEntity` via `System.Management` to discover plugged-in camera and microphone hardware and correlate them with PnP device IDs.
4. **Inter-Process Communication:** The application creates and manages local named pipes (`\\.\pipe\PrivLock_...`) to coordinate the standard UI process and the elevated worker.

These operations are implemented in the full-trust desktop process because they rely on Win32 system APIs, COM, WMI, and local inter-process communication.

---

### 2.3 `rescap:unvirtualizedResources`

#### Purpose
The package declares `rescap:unvirtualizedResources`. The application stores preferences and its recovery journal beneath the directory returned by `LocalApplicationData`; a separate active-session marker uses the current user's profile path, and the privileged worker stores ownership records under `HKLM`. The declaration alone does not establish the exact virtualization behavior of these paths or that this capability is required for each operation. Those points require validation with an installed MSIX package.

#### Data and Recovery Behavior
1. **Recovery journal:** The application writes a journal and backup under its `LocalApplicationData` directory. On startup, it attempts to recover interrupted, journal-owned operations and then reconcile the operating system with the saved desired state. Recovery and reconciliation can fail because of storage errors, conflicts, unavailable devices, permissions, or other system conditions.
2. **Normal exit and package removal:** During normal application shutdown, PrivGvard attempts to restore system changes it can confirm it owns. Startup recovery only runs when the application starts and can access its recovery data. The journal does not guarantee restoration during or after MSIX uninstall, nor does it guarantee uninterrupted protection during a restart. The resulting state must be checked after signing in.
3. **Preferences and ownership records:** The desired settings file is written under the application data directory. The elevated worker separately accesses `HKLM\SOFTWARE\PrivLock\PrivilegedOwnership\v2` for ownership records. Whether the declared capability is needed for these paths, and how the packaged application resolves and retains them across servicing or removal, must be confirmed with an installed-package test.
---

## 3. Threat Mitigation & Security Controls Summary

| Security Concern | PrivGvard Mitigation |
|---|---|
| Privilege Escalation | Main UI runs asInvoker. Operations requiring administrative access use the helper launched with Windows `runas` behavior; cancellation and permission failures are reported. The worker accepts only whitelisted commands. |
| Malicious IPC Injection | Named pipe uses random GUID name, restricted ACL, 256-bit nonce authentication, and kernel PID verification. |
| Recovery of managed changes | Normal shutdown attempts to restore changes PrivGvard can confirm it owns. Startup attempts recovery and reconciliation; either can fail or encounter conflicts. The journal does not guarantee recovery during or after MSIX uninstall or uninterrupted protection during restart. |
| Background Resource Drain | The reviewed application uses no persistent Windows service. The elevated helper may be reused within an operation scope and is closed when the scope ends; immediate termination is not guaranteed. |
| Privacy & Telemetry | The reviewed active application source contains no product telemetry or automatic diagnostics-upload feature, and no application-level network client was identified. This source review does not establish that Windows or every dependency makes no network requests. |

---

## 4. Microsoft Store Certification Reviewer Notes

The following steps describe behaviors to inspect in a packaged Windows installation; they are not results of a completed MSIX certification test:

1. Launch PrivGvard from the Start Menu and confirm that the main UI runs as a standard user.
2. Record the displayed state for the selected device and protection mode. Compare it with the relevant Windows-reported state; a saved desired state alone does not confirm that protection is active.
3. Enable protection for one camera or microphone and note which controls apply for that mode. Standard controls may update Windows privacy preferences or mute supported microphone capture endpoints. Secure operations that require administrative access launch the helper using Windows `runas` behavior; the prompt and outcome depend on system policy and permissions.
4. If elevation is accepted, check the operation result and verify the reported state for that specific device or policy. Do not infer that every sensor or driver is blocked from a single successful device-state query.
5. If elevation is cancelled, permission is denied, a device is unsupported, or verification fails, confirm that the operation is reported as unsuccessful or uncertain and that the displayed effective state is not presented as confirmed protection.
6. Request restoration and verify the resulting device, endpoint, or policy state. After restart or package update, sign in and check both the application status and the relevant Windows-reported state. Startup recovery is an attempt and does not guarantee restoration during or after MSIX uninstall.
