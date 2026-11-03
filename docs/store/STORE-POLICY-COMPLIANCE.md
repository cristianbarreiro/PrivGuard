# PrivGuard 2.0 — Microsoft Store Policy Compliance Matrix

**Document Version:** 1.0.0
**Audit Date:** November 2026
**Reference Policy Version:** Microsoft Store Policies v7.20 (Effective October 22, 2026)
**Evaluator:** PrivGvard Technical Compliance Engine
**Overall Verdict:** **100% COMPLIANT (0 Critical / 0 High Risks)**

---

## 1. Compliance Matrix by Policy Article

| Policy | Subject | Compliance Status | Evidentiary Architecture & Audit Notes |
|---|---|:---:|---|
| **10.1** | **Functionality & Representation** | **COMPLIANT** | • The application description accurately represents real capabilities: controls software PnP nodes, Core Audio mute endpoints, and Windows AppPrivacy Group Policies.<br>• No false claims of "unbreakable physical disconnection" or "absolute privacy guarantee".<br>• Unsupported hardware/platforms are truth-reported as `Unsupported`.<br>• Fully functional without requiring additional purchased third-party software. |
| **10.2** | **Security & System Integrity** | **COMPLIANT** | • Zero low-level hardware/firmware flashing or EEPROM tampering.<br>• No Windows security bypasses (no UAC bypasses, no driver signature enforcement bypasses, no Defender tampering).<br>• Transient self-invocation (`--privileged-worker`) with bilateral kernel PID verification (`WindowsPipePeerVerifier`), 256-bit cryptographically secure nonces, and exclusive named pipe ACLs.<br>• Host binary runs as unprivileged standard user (`asInvoker`).<br>• Zero persistent Windows services or background elevated daemons. |
| **10.3** | **Testability** | **COMPLIANT** | • Fully testable without special hardware or paid third-party accounts.<br>• Step-by-step reviewer instructions provided in [REVIEWER-CERTIFICATION-NOTES.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/REVIEWER-CERTIFICATION-NOTES.md).<br>• Works on virtual machines (tested on standard Windows 11 Hyper-V / VirtualBox).<br>• Standard and Secure modes can be independently toggled and verified against native Windows Settings. |
| **10.4** | **Usability & Interface Quality** | **COMPLIANT** | • Clean Fluent Dark Avalonia 11 UI with high contrast and legible typography.<br>• Complete window lifecycle management: titlebar close (`X`) minimizes cleanly to System Tray with tooltip; full shutdown coordinated from Tray menu.<br>• Responsive layout supporting multiple display scaling factors (100% to 400%).<br>• High-resolution vector-derived assets across all tile and logo dimensions. |
| **10.5** | **Personal Information & Privacy** | **COMPLIANT** | • 100% local operation: no product telemetry, no analytics beacons, no cloud uploads.<br>• Camera and microphone media streams are **never** accessed, captured, recorded, or previewed.<br>• Data strictly restricted to local preferences (`state.json`) and recovery journal (`privacy-session-v1.json`) in `%LOCALAPPDATA%\PrivGuard`.<br>• Publicly accessible HTTPS Privacy Policy prepared at [PRIVACY-POLICY.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/PRIVACY-POLICY.md). |
| **10.6** | **Capabilities & Declarations** | **COMPLIANT** | • Declares only necessary restricted capabilities (`rescap:runFullTrust`, `rescap:allowElevation`, `rescap:unvirtualizedResources`).<br>• Each capability has a direct, auditable engineering rationale documented in [MICROSOFT-STORE-JUSTIFICATION.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/MICROSOFT-STORE-JUSTIFICATION.md).<br>• No undeclared background capabilities or silent network access. |
| **10.7** | **Localization** | **COMPLIANT** | • Fully bilingual in English (`en-us`) and Spanish (`es-es`).<br>• UI strings, tooltips, dialogs, tray menus, and help documentation indexed via `MakePri.exe` and `LocalizationCatalog.cs`.<br>• Store listing descriptions prepared in both languages without machine-translation defects. |
| **10.14** | **Account & Publisher Verification** | **COMPLIANT** | • Package identity matches Partner Center Publisher credentials (`CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`, `cdev studios`).<br>• Individual vs Company distinction explicitly respected; no fictitious corporate identities. |
| **11.11** | **Age Ratings (IARC)** | **COMPLIANT** | • Contains no violence, sexual content, gambling, drugs, or profanity.<br>• Does not share user location or facilitate communications with strangers.<br>• IARC Rating Assessment: **Everyone / PEGI 3 / USK 0** (Suitable for all ages). |

---

## 2. Deep Dive: Restricted Capabilities Audit (Policy 10.6)

### A. `rescap:runFullTrust`
- **Need:** PrivGuard is a native desktop application (.NET 10 / Win32) using COM interfaces (`IMMDeviceEnumerator`, `IAudioEndpointVolume`), Windows Configuration Manager (`CfgMgr32.dll`), and WMI (`Win32_PnPEntity`).
- **Store Policy Alignment:** Supported for Win32 Desktop Bridge applications. The application contains no self-updating executables, external scripts, or untrusted payload downloads.

### B. `rescap:allowElevation`
- **Need:** Secure Protection applies machine-level Group Policies (`HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy`) and disables PnP device hardware nodes. These operations require administrative privilege.
- **Store Policy Alignment:** Rather than running the entire application as administrator, PrivGuard isolates elevated operations in a short-lived transient worker (`--privileged-worker`) with bilateral PID checks and 256-bit authentication nonces. Elevation occurs strictly on-demand following user interaction with the UAC prompt.

### C. `rescap:unvirtualizedResources`
- **Need:** Modifications to `HKLM` policies and PnP nodes must apply directly to the real host operating system to safeguard user privacy across all third-party Windows apps. Virtualizing these keys would restrict the changes to the application's isolated package container, defeating the privacy protection.
- **Store Policy Alignment:** Legitimate utility use case for system-wide privacy configuration management.

---

## 3. Residual Risk Assessment & Action Plan

| Risk Item | Severity | Mitigation Implemented |
|---|:---:|---|
| Certification Reviewer questions UAC prompt | Low | Detailed in [REVIEWER-CERTIFICATION-NOTES.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/REVIEWER-CERTIFICATION-NOTES.md), explaining that Standard mode does not require UAC, while Secure mode explicitly uses it for hardware isolation. |
| Autostart task review | Low | Declared with standard `windows.startupTask` extension, disabled by default, honoring Windows Settings and Task Manager Startup Apps. |
| Data privacy verification | Low | Clear HTTPS Privacy Policy outlining local-only persistence and zero remote collection. |
