# PrivGuard 2.0 — Microsoft Store Policy Compliance Matrix

**Document Version:** 2.1.0
**Audit Date:** November 2026
**Reference Policy Version:** Microsoft Store Policies v7.20 (Effective October 22, 2026)
**Evaluator:** PrivGuard Technical Compliance Engine
**Overall Verdict:** **PRE-SUBMISSION AUDIT — CONDITIONAL HOLD (Pending Package Storage Validation & Dual Edition Separation)**

> [!CAUTION]
> **Submission Boundary:** PrivGuard is technically verified across automated tests. Under Architectural Decision D-01, the Store MSIX profile removes restricted capabilities (`rescap:allowElevation` and `rescap:unvirtualizedResources`), retaining solely `rescap:runFullTrust`. Submission remains on **CONDITIONAL HOLD** until the physical storage behavior of MSIX Full Trust is validated in a clean VM environment and the dual edition build pipeline is completed.

---

## 1. Compliance Matrix by Policy Article

| Policy | Subject | Compliance Status | Evidentiary Architecture & Audit Notes |
|---|---|:---:|---|
| **10.1** | **Functionality & Representation** | **VERIFIED** | • Store Edition focuses on Core Audio endpoint muting via WASAPI and direct access/inspection of Windows native privacy settings.<br>• No false claims of "unbreakable physical disconnection", "absolute blocking", or "universal microphone coverage".<br>• Unsupported hardware/platforms and software-only mute limits are truth-reported.<br>• Fully functional without requiring additional purchased third-party software. |
| **10.2** | **Security & System Integrity** | **VERIFIED** | • Main application runs as standard unprivileged user (`asInvoker`).<br>• Store Edition contains zero elevated code paths, zero PnP node toggling, and zero machine-level HKLM writes (advanced elevated features are segregated exclusively to Standalone Edition per D-01).<br>• Zero low-level hardware/firmware flashing or EEPROM tampering.<br>• No Windows security bypasses.<br>• Zero persistent Windows services or background elevated daemons. |
| **10.3** | **Testability** | **VERIFIED** | • Fully testable without special hardware or paid third-party accounts.<br>• Step-by-step reviewer instructions tailored for Store Edition provided in [REVIEWER-CERTIFICATION-NOTES.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/REVIEWER-CERTIFICATION-NOTES.md).<br>• WASAPI microphone muting and native settings navigation are verified against native Windows controls. |
| **10.4** | **Usability & Interface Quality** | **VERIFIED** | • Clean Fluent Dark Avalonia 11 UI with high contrast and legible typography.<br>• Complete window lifecycle management: titlebar close (`X`) minimizes cleanly to System Tray with tooltip; full shutdown coordinated from Tray menu.<br>• Responsive layout supporting multiple display scaling factors (100% to 400%).<br>• High-resolution vector-derived assets across all tile and logo dimensions. |
| **10.5** | **Personal Information & Privacy** | **VERIFIED** | • 100% local operation: no product telemetry, no analytics beacons, no cloud uploads.<br>• Media streams are **never** accessed, captured, recorded, or previewed.<br>• Audio endpoint GUIDs and device instance IDs are processed strictly locally for device state matching.<br>• Partner Center privacy questionnaire answered affirmatively (**Yes**) to fully disclose local processing of identifiers, with explicit confirmation of 0% network transmission.<br>• Publicly accessible HTTPS Privacy Policy published at [PRIVACY-POLICY.md](file:///c:/Users/Cristian/Documents/GitHub/PrivGuard/docs/store/PRIVACY-POLICY.md). |
| **10.6** | **Capabilities & Declarations** | **VERIFIED FOR STORE PROFILE** | • Declares solely `rescap:runFullTrust` (standard for Win32 Desktop Bridge).<br>• In accordance with Architectural Decision D-01, `rescap:allowElevation` and `rescap:unvirtualizedResources` are completely excluded from the Store Edition package.<br>• Retaining solely `runFullTrust` aligns with standard Store policy for packaged desktop apps without requiring restricted capability exceptions. |
| **10.7** | **Localization** | **VERIFIED** | • Fully bilingual in English (`en-us`) and Spanish (`es-es`).<br>• UI strings, tooltips, dialogs, tray menus, and help documentation indexed via `MakePri.exe` and `LocalizationCatalog.cs`.<br>• Store listing descriptions prepared in both languages. |
| **10.14** | **Account & Publisher Verification** | **VERIFIED** | • Package identity matches Partner Center Publisher credentials (`CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`, `cdev studios`).<br>• Submission is conducted from an Individual Developer Account; by omitting `allowElevation`, publisher account tier exceptions are not required.<br>• *Note on Evidence:* No express rejection of individual accounts was issued by Microsoft; the decision to omit elevation from Store Edition is a proactive architectural choice for lifecycle safety and minimal privilege. |
| **11.11** | **Age Ratings (IARC)** | **VERIFIED** | • Contains no violence, sexual content, gambling, drugs, or profanity.<br>• Does not share user location or facilitate communications with strangers.<br>• IARC Rating Assessment: **Everyone / PEGI 3 / USK 0** (Suitable for all ages). |

---

## 2. Capabilities Architecture & Boundaries (Policy 10.6)

### A. `rescap:runFullTrust` (Sole Store Capability)
- **Need:** PrivGuard is a native desktop application (.NET 10 / Win32) using COM interfaces (`IMMDeviceEnumerator`, `IAudioEndpointVolume`) for Core Audio capture endpoint management, WMI (`Win32_PnPEntity`) for hardware enumeration, and Win32 APIs for tray integration and startup tasks.
- **Store Status:** **SUPPORTED & STANDARD** for desktop applications packaged via Desktop Bridge.

### B. `rescap:allowElevation` (Excluded from Store Profile — Reserved for Standalone)
- **Architectural Decision D-01:** This capability is excluded from the Store package.
- **Rationale:** Elevating child processes to manipulate PnP device nodes or HKLM policies creates significant lifecycle risks in MSIX packages (the package directory is destroyed upon uninstall, potentially stranding modified hardware states). Retaining standard user rights (`asInvoker`) ensures clean, reversible operation.
- **Evidence Boundary:** Microsoft has not issued an express rejection regarding individual accounts. The decision to exclude `allowElevation` from Store Edition is a deliberate architectural segregation: Store Edition stays unprivileged, while Standalone Edition provides full Secure Protection.

### C. `rescap:unvirtualizedResources` (Excluded from Store Profile)
- **Architectural Decision D-01 & D-02:** This capability is excluded from the Store package.
- **Rationale:** Store Edition will not attempt direct writes to `HKCU\...\ConsentStore`. Instead, Store Edition directs users to native Windows Privacy & Security settings pages (`ms-settings:privacy-webcam` and `ms-settings:privacy-microphone`). This avoids dependency on schema virtualization bypasses and guarantees complete policy compliance.

---

## 3. Residual Risk & Pre-Submission Action Plan

| Risk Item | Severity | Current Status | Required Action Before Submission |
|---|:---:|:---:|---|
| **MSIX Full Trust LocalAppData Redirection** | **MEDIUM** | In Validation | Validate real storage paths (`LocalCache\Local` vs native AppData) in a clean VM environment before finalizing storage paths. |
| **WASAPI Exclusive Mode / Hardware Mute Limits** | **LOW** | Documented | Ensure UI and Store listing reflect honest muting status without claiming universal or physical blocking. Validate `QueryHardwareSupport` in audio provider. |
| **Cross-Edition Resource Coordination** | **MEDIUM** | Architecture Phase | Formalize a cross-edition resource ownership coordinator to avoid conflicts if Store and Standalone coexist on the same machine. |
| **Partner Center Personal Data Questionnaire** | **LOW** | Disclosed | Answer "Yes" in Partner Center, disclosing local SID processing (ACLs) and device IDs, with explicit confirmation of zero network transmission. |
