# PrivGuard 2.0 â€” Microsoft Store Policy Compliance Matrix

**Document Version:** 2.0.0
**Audit Date:** November 2026
**Reference Policy Version:** Microsoft Store Policies v7.20 (Effective October 22, 2026)
**Evaluator:** PrivGuard Technical Compliance Engine
**Overall Verdict:** **PRE-SUBMISSION AUDIT â€” CONDITIONAL HOLD (Pending Microsoft Capability Pre-Approval)**

> [!CAUTION]
> **Submission Boundary:** PrivGuard is technically verified across 419 automated tests. However, the package declares two restricted capabilities (`rescap:allowElevation` and `rescap:unvirtualizedResources`). Under Microsoft Store Policy 10.6, restricted capabilities cannot be assumed approved. Pre-approval must be obtained from `reportapp@microsoft.com` before clicking "Submit for certification" in Partner Center.

---

## 1. Compliance Matrix by Policy Article

| Policy | Subject | Compliance Status | Evidentiary Architecture & Audit Notes |
|---|---|:---:|---|
| **10.1** | **Functionality & Representation** | **VERIFIED** | â€¢ The application description accurately represents real capabilities: controls software PnP nodes, Core Audio mute endpoints, and Windows AppPrivacy Group Policies.<br>â€¢ No false claims of "unbreakable physical disconnection" or "absolute privacy guarantee".<br>â€¢ Unsupported hardware/platforms are truth-reported as `Unsupported`.<br>â€¢ Fully functional without requiring additional purchased third-party software. |
| **10.2** | **Security & System Integrity** | **VERIFIED (Architecture) / PENDING APPROVAL** | â€¢ Zero low-level hardware/firmware flashing or EEPROM tampering.<br>â€¢ No Windows security bypasses (no UAC bypasses, no driver signature enforcement bypasses, no Defender tampering).<br>â€¢ Transient self-invocation (`--privileged-worker`) with bilateral kernel PID verification (`WindowsPipePeerVerifier`), 256-bit cryptographically secure nonces, and exclusive named pipe ACLs.<br>â€¢ Host binary runs as unprivileged standard user (`asInvoker`).<br>â€¢ Zero persistent Windows services or background elevated daemons.<br>â€¢ *Pending:* Formal Microsoft confirmation that transient worker elevation is permitted for Individual accounts. |
| **10.3** | **Testability** | **VERIFIED** | â€¢ Fully testable without special hardware or paid third-party accounts.<br>â€¢ Step-by-step reviewer instructions provided in [REVIEWER-CERTIFICATION-NOTES.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/REVIEWER-CERTIFICATION-NOTES.md).<br>â€¢ Standard and Secure modes can be independently toggled and verified against native Windows Settings. |
| **10.4** | **Usability & Interface Quality** | **VERIFIED** | â€¢ Clean Fluent Dark Avalonia 11 UI with high contrast and legible typography.<br>â€¢ Complete window lifecycle management: titlebar close (`X`) minimizes cleanly to System Tray with tooltip; full shutdown coordinated from Tray menu.<br>â€¢ Responsive layout supporting multiple display scaling factors (100% to 400%).<br>â€¢ High-resolution vector-derived assets across all tile and logo dimensions. |
| **10.5** | **Personal Information & Privacy** | **VERIFIED** | â€¢ 100% local operation: no product telemetry, no analytics beacons, no cloud uploads.<br>â€¢ Media streams are **never** accessed, captured, recorded, or previewed.<br>â€¢ Local SID accessed strictly for Named Pipe ACL enforcement; Device Instance IDs used strictly for PnP matching.<br>â€¢ Partner Center privacy questionnaire answered affirmatively (**Yes**) to fully disclose local SID and hardware processing, with explicit confirmation of 0% network transmission.<br>â€¢ Publicly accessible HTTPS Privacy Policy published at [PRIVACY-POLICY.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/PRIVACY-POLICY.md). |
| **10.6** | **Capabilities & Declarations** | **PENDING MICROSOFT APPROVAL** | â€¢ Declares `rescap:runFullTrust` (standard for Win32 Desktop Bridge).<br>â€¢ Declares `rescap:allowElevation` and `rescap:unvirtualizedResources`.<br>â€¢ Both capabilities require prior written authorization from Microsoft (`reportapp@microsoft.com`). Consultation draft staged in [MICROSOFT-ALLOWELEVATION-PREAPPROVAL.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/MICROSOFT-ALLOWELEVATION-PREAPPROVAL.md). |
| **10.7** | **Localization** | **VERIFIED** | â€¢ Fully bilingual in English (`en-us`) and Spanish (`es-es`).<br>â€¢ UI strings, tooltips, dialogs, tray menus, and help documentation indexed via `MakePri.exe` and `LocalizationCatalog.cs`.<br>â€¢ Store listing descriptions prepared in both languages. |
| **10.14** | **Account & Publisher Verification** | **PENDING CLARIFICATION** | â€¢ Package identity matches Partner Center Publisher credentials (`CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`, `cdev studios`).<br>â€¢ Submission is made from an Individual Developer Account; confirmation required regarding whether `allowElevation` can be granted to Individual accounts. |
| **11.11** | **Age Ratings (IARC)** | **VERIFIED** | â€¢ Contains no violence, sexual content, gambling, drugs, or profanity.<br>â€¢ Does not share user location or facilitate communications with strangers.<br>â€¢ IARC Rating Assessment: **Everyone / PEGI 3 / USK 0** (Suitable for all ages). |

---

## 2. Deep Dive: Restricted Capabilities Audit & Blockers (Policy 10.6)

### A. `rescap:runFullTrust`
- **Need:** PrivGuard is a native desktop application (.NET 10 / Win32) using COM interfaces (`IMMDeviceEnumerator`, `IAudioEndpointVolume`), Windows Configuration Manager (`CfgMgr32.dll`), and WMI (`Win32_PnPEntity`).
- **Store Status:** **SUPPORTED & APPROVED** for desktop applications packaged via Desktop Bridge.

### B. `rescap:allowElevation` (Critical Reviewer Blocker)
- **Need:** Secure Protection disables PnP device hardware nodes and applies machine-level Group Policies (`HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy`). These operations require administrative privilege.
- **Store Policy Alignment:** Rather than running the entire application elevated or installing a background service running as `SYSTEM`, PrivGuard invokes an ephemeral worker (`--privileged-worker`) with standard UAC consent, bilateral kernel PID verification, and a 256-bit nonce.
- **Store Status:** **PENDING MICROSOFT WRITTEN PRE-APPROVAL.** Microsoft policy explicitly requires prior contact via `reportapp@microsoft.com`. Because the publisher account is an Individual account, Microsoft may require a Company account or mandate that Store builds exclude elevated features.

### C. `rescap:unvirtualizedResources` (Flexible Virtualization Blocker)
- **Audit Finding:** Standard Protection writes to `HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam`. In an MSIX package, Windows intercepts un-elevated HKCU writes and redirects them to `%LOCALAPPDATA%\Packages\<PFN>\SystemAppData\Helium\User.dat`.
- **Manifest Inconsistency Identified:** The current manifest declared `rescap:unvirtualizedResources` without declaring `<desktop6:RegistryWriteVirtualization>`, meaning that ConsentStore writes remained virtualized in the installed MSIX.
- **Store Status:** **POTENTIAL CERTIFICATION BLOCKER.**
  - **Plan A:** Add `<desktop6:RegistryWriteVirtualization>` scoped strictly to `CapabilityAccessManager\ConsentStore` and request approval from `reportapp@microsoft.com`.
  - **Plan B (Store-Optimized Fallback):** Remove `unvirtualizedResources` from the Store manifest. Retain un-elevated Core Audio microphone muting in Standard mode, and designate camera privacy as requiring Secure Mode (elevated HKLM Group Policy + PnP), which completely avoids HKCU virtualization.

---

## 3. Residual Risk & Pre-Submission Action Plan

| Risk Item | Severity | Current Status | Required Action Before Submission |
|---|:---:|:---:|---|
| **allowElevation rejection for Individual Account** | **HIGH** | Pending Pre-Approval | Send [MICROSOFT-ALLOWELEVATION-PREAPPROVAL.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/MICROSOFT-ALLOWELEVATION-PREAPPROVAL.md) to `reportapp@microsoft.com` and wait for official response. |
| **unvirtualizedResources scrutiny** | **MEDIUM** | Architectural Options Defined | Request confirmation on Plan A vs Plan B from Microsoft review team. |
| **OneDrive sync of session journal** | **LOW** | Mitigated | Instruct user to leave OneDrive backup unchecked in Partner Center until WAL restoration is validated across cloud syncing. |
| **Partner Center personal data questionnaire** | **LOW** | Disclosed | Answer "Yes" in Partner Center, noting local SID processing and zero network transmission. |
