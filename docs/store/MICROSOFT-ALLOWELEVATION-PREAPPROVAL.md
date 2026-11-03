# Pre-Submission Inquiry: `rescap:allowElevation` and `rescap:unvirtualizedResources` for PrivGuard

**Document Date:** November 2026
**Product Name:** PrivGuard
**Target Store ID:** `9NQ56N45XH5N`
**Package Identity Name:** `cdevstudios.PrivGuard`
**Publisher ID:** `CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD`
**Publisher Display Name:** `cdev studios`
**Package Family Name (PFN):** `cdevstudios.PrivGuard_tv7b4yqg10sw2`
**Contact Email:** `reportapp@microsoft.com` (Microsoft Store Restricted Capabilities Team)
**Developer Account Type:** Individual Developer Account

> [!IMPORTANT]
> **Pre-Submission Draft:** This document constitutes a formal pre-submission consultation draft to be reviewed and sent by the developer to `reportapp@microsoft.com`. It has **not** been submitted automatically, and Microsoft has **not** yet granted approval for these restricted capabilities.

---

## 1. Email Draft to Microsoft Store Review Team

**To:** `reportapp@microsoft.com`
**Subject:** Pre-Submission Capability Approval Inquiry: `allowElevation` and `unvirtualizedResources` â€” PrivGuard (`9NQ56N45XH5N`)

```text
Dear Microsoft Store Application Review Team,

We are preparing to submit PrivGuard (Store ID: 9NQ56N45XH5N, Package: cdevstudios.PrivGuard) to the Microsoft Store from an Individual Developer Account (Publisher: CN=85AB4167-A0AE-4FDF-B840-B95CD225F7DD).

In accordance with Microsoft Store Policies and the App Capability Declarations documentation (https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/app-capability-declarations), we are contacting you prior to submission to request formal clarification and guidance regarding two restricted capabilities declared in our MSIX package:
1. rescap:allowElevation
2. rescap:unvirtualizedResources

Below is our technical architecture, least-privilege security model, and justification for each capability.

================================================================================
1. PRODUCT OVERVIEW & LEAST-PRIVILEGE ARCHITECTURE
================================================================================

PrivGuard is an open-source, local-first Windows desktop privacy utility (built with .NET 10 / Win32) designed to allow users to inspect, control, and verify the privacy state of their webcams and microphones.

Key Security Characteristics:
- 100% Offline & Zero Telemetry: PrivGuard declares NO network capabilities (no internetClient). It never collects, transmits, or uploads any data.
- Zero Media Capture: PrivGuard never captures, reads, previews, or streams camera video frames or microphone audio. It only manages hardware node enablement and system privacy flags.
- Default Standard Token (asInvoker): The main interactive desktop UI process runs strictly as a standard, unprivileged user (requestedExecutionLevel="asInvoker").
- Zero Persistent Daemons / Services: PrivGuard does NOT install a permanent Windows Service running as NT AUTHORITY\SYSTEM, thereby preventing a persistent elevated attack surface.

PrivGuard provides two distinct protection layers:
A. Standard Protection (No Elevation Needed):
   - Mutes audio capture endpoints using the official Windows Core Audio COM API (IAudioEndpointVolume).
   - Toggles per-user camera consent under HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore.
   - Operates 100% within the standard user's privilege scope without any UAC prompt.

B. Secure Protection (Hardware Node Isolation):
   - Disables Plug-and-Play device nodes at the driver level using Windows Configuration Manager APIs (CfgMgr32.dll: CM_Locate_DevNode_W, CM_Disable_DevNode, CM_Enable_DevNode).
   - Sets machine-wide Windows privacy policies under HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy (LetAppsAccessCamera = 2, LetAppsAccessMicrophone = 2).
   - Because CfgMgr32 device disabling and HKLM policy writing require administrative rights on Windows, this layer requires elevation.

================================================================================
2. JUSTIFICATION & AUDIT OF `rescap:allowElevation`
================================================================================

Rather than requiring the entire application to run as Administrator or installing a permanent elevated background service, PrivGuard uses an on-demand, operation-scoped transient worker pattern:

1. When the user explicitly clicks "Enable Secure Protection" or "Disable Secure Protection", the standard application self-invokes its own binary with the argument `--privileged-worker` using ShellExecuteEx with Verb="runas", which triggers the standard Windows UAC consent dialog.
2. If the user clicks "No" on the UAC prompt, PrivGuard safely catches Win32 error 1223 (ERROR_CANCELLED) and leaves the system in its previous uncorrupted state without crashing.
3. If the user grants consent, the elevated worker connects back to the standard-user process over a Named Pipe secured by:
   - Kernel-enforced PipeSecurity Access Control List (ACL) granting access ONLY to the current user's local SID, Administrators, and SYSTEM.
   - A cryptographically random 256-bit authentication nonce generated via RandomNumberGenerator and validated on connection.
   - Bilateral kernel Process ID (PID) verification using GetNamedPipeServerProcessId and GetNamedPipeClientProcessId to prevent token hijacking or rogue pipe clients.
   - A closed, hardcoded whitelist of 7 permitted commands (apply-policy, restore-policy, apply-device, restore-device, verify-policy-ownership, verify-device-ownership, ping).
4. The worker process performs the requested device/policy change, flushes the journal, and exits. The user interface process never retains administrative privileges.

Why allowElevation is Necessary:
MSIX packaging sandboxes child process creation. Without rescap:allowElevation, a packaged application calling Process.Start with Verb="runas" is blocked by the Windows runtime with an access violation, preventing the on-demand worker from launching.

================================================================================
3. AUDIT & CLARIFICATION OF `rescap:unvirtualizedResources`
================================================================================

Our audit identified that our initial justification for unvirtualizedResources incorrectly referenced HKLM and PnP. Those operations are executed by the elevated worker and do not require unvirtualizedResources.

The real technical consideration for unvirtualizedResources relates to Standard Protection:
- Standard Protection toggles HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam and microphone.
- By default, MSIX Registry Write Virtualization intercepts HKCU writes and redirects them to a private virtual hive (%LOCALAPPDATA%\Packages\<PFN>\SystemAppData\Helium\User.dat).
- As a result, the real Windows operating system, Windows Settings, and third-party desktop apps (browsers, conference tools) do not see the consent change unless the ConsentStore key tree is exempted from registry write virtualization using desktop6:RegistryWriteVirtualization.
- In accordance with Microsoft's Flexible Virtualization documentation (https://learn.microsoft.com/en-us/windows/msix/desktop/flexible-virtualization), using desktop6:RegistryWriteVirtualization requires the declaration of rescap:unvirtualizedResources.

================================================================================
4. SPECIFIC QUESTIONS FOR THE STORE REVIEW TEAM
================================================================================

Given that our account is registered as an Individual Developer Account:

Question 1 (allowElevation Eligibility):
Does Microsoft Store permit Individual Developer Accounts to publish desktop utilities that declare rescap:allowElevation using this on-demand transient worker pattern with strict ACLs and nonces? Or is allowElevation strictly restricted to Company/Enterprise accounts?

Question 2 (unvirtualizedResources Eligibility):
Is rescap:unvirtualizedResources eligible for Store approval for an Individual Account when strictly scoped to excluding the CapabilityAccessManager\ConsentStore registry key via desktop6:RegistryWriteVirtualization?

Question 3 (Recommended Architectural Path):
If either capability cannot be approved for an Individual Account on the Microsoft Store:
- Would Microsoft prefer that we offer a Store-edition of PrivGuard that restricts functionality to un-elevated Core Audio microphone muting and guides users to our open-source standalone package for hardware-level PnP node control?
- Or can we proceed with submission after your review of our source code and security architecture?

We have prepared comprehensive Reviewer Certification Notes and step-by-step testing instructions. Our complete source code is publicly inspectable at:
https://github.com/cristianbarreiro/PrivGuard

Thank you for your time and guidance.

Sincerely,
cdev studios / Cristian Barreiro
PrivGuard Engineering Team
```

---

## 2. Status & Pre-Submission Action Plan

| Vector | Current Status | Required Action |
|---|:---:|---|
| **allowElevation Pre-Approval** | **PENDING DEVELOPER DISPATCH** | The developer should copy the text above, insert their contact information, and send it to `reportapp@microsoft.com`. |
| **Microsoft Response Tracking** | **AWAITING RESPONSE** | Document the official reply in this file upon receipt. |
| **Store Readiness Impact** | **CONDITIONAL HOLD** | Do NOT click "Submit for certification" in Partner Center until Microsoft provides written guidance on capability eligibility for Individual Accounts. |
