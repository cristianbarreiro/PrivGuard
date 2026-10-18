# PrivGvard — Privacy Policy

**Effective Date:** September 15, 2026<br>
**Last Updated:** October 18, 2026<br>
**Developer:** cdev Studio<br>
**Application:** PrivGvard (Camera & Microphone Privacy Guard)<br>
**Website / Repository:** [https://github.com/cristianbarreiro/PrivGuard](https://github.com/cristianbarreiro/PrivGuard)

---

## 1. Introduction

PrivGvard is a desktop application that lets users request operating-system privacy controls for cameras and microphones. On Windows, depending on the selected protection mode and device support, it can use Windows privacy settings, device management, and microphone capture-endpoint mute controls. It checks effective system state where the relevant Windows APIs make that state available. Results depend on the mode, hardware, driver, permissions, and configuration; PrivGvard cannot guarantee that every device can be protected or that every operation will succeed.

This policy distinguishes information the application stores locally from information it sends remotely. The application code reviewed for this policy writes configuration, recovery, and diagnostic files to local application-data directories. A review of the active application source found no product telemetry or analytics feature and no application upload path for settings, logs, or crash reports. This source review does not establish that every third-party dependency, Windows component, or other software on the computer makes no network requests.

## 2. Remote Collection and Media Access

- **No in-app account or advertising feature:** The application has no account-registration, advertising, or marketing-tracking flow in the reviewed code.
- **No audio or video recording feature:** The Windows implementation enumerates camera and microphone devices, reads their system status, changes supported device or privacy-policy state, and can mute microphone capture endpoints. The reviewed application code does not capture, record, preview, or transmit camera images or microphone audio.
- **No automatic diagnostic upload found:** Logs and crash reports are written locally. The reviewed active application source contains no feature that uploads them or sends product analytics. This is a statement about the reviewed application code, not a guarantee about all network activity by dependencies, Windows, or other software.
- **Device information is accessed and may be stored locally:** Device instance IDs, friendly names, and audio endpoint IDs are used to identify the exact resources whose prior state may need to be restored. These can be device-specific identifiers; see Section 3 for the local files that may contain them.

If a user chooses to attach logs or crash reports to a support request or GitHub issue, that is a separate user-initiated disclosure to the selected service and is subject to that service's terms and privacy policy. Review and redact diagnostic files before sharing them.

## 3. Windows Permissions and Local Data

Windows is the application's primary platform for camera and microphone protection. Linux and macOS currently provide discovery or interface scaffolding; the application does not offer supported persistent camera/microphone protection on those platforms.

The `state.json`, journal, log, and crash-report paths below are based on the `LocalApplicationData` directory returned by .NET. Packaged installations may resolve that directory to an app-specific location. The active-session marker uses the explicit current-user profile path shown in Section 3.3.

### 3.1 Device Enumeration and Protection Controls

On Windows, PrivGvard queries Plug and Play device information to identify cameras and microphone capture endpoints and read their reported status. Depending on the selected protection mode and device compatibility, it can change Windows camera/microphone privacy settings, manage supported Plug and Play device nodes, and mute capture audio endpoints. Some system-level operations request administrator authorization through Windows User Account Control (UAC) when needed. The application does not access the captured audio or video as part of these controls. Muting an audio endpoint is not a physical disconnection, and not all devices or drivers support every protection method.

### 3.2 Preferences and Desired Protection State

The application stores a JSON preferences file named `state.json` beneath that directory. In a standard desktop profile, this is typically:

```text
%LOCALAPPDATA%\PrivGvard\state.json
```

The file stores the selected UI language, desired standard and advanced protection states for the camera and microphone, the advanced-protection setting, and an autostart preference/status value. It stores the user's desired protection state separately from the system's current effective state; a saved preference alone does not prove that protection is active. When upgrading from an older version, the migration code can copy an existing `state.json`, recovery files, and crash reports from `%LOCALAPPDATA%\PrivLock` when the corresponding PrivGvard files do not already exist. This is a local migration.

### 3.3 Recovery Journal and Startup Reconciliation

The recovery journal is stored beneath the same application-data directory:

```text
%LOCALAPPDATA%\PrivGvard\Recovery\privacy-session-v1.json
%LOCALAPPDATA%\PrivGvard\Recovery\privacy-session-v1.json.bak
```

On Windows, the separate active-session marker is stored at:

```text
%USERPROFILE%\AppData\Local\PrivGvard\Recovery\active-session-v1.marker
```

The marker contains an active session identifier. The journal and its backup record the information needed to recover reversible system changes. Depending on the resources involved, this can include session and operation identifiers, timestamps, process and recovery status, error details, device instance IDs and friendly names, audio endpoint IDs and mute state, and the original and application-applied Windows device or privacy-policy values. These files are stored locally; the reviewed application code contains no automatic upload feature for them.

When the application exits normally, it attempts to restore system changes that it can confirm it owns. The desired protection state is stored separately and is not cleared by that restoration. On a later startup, PrivGvard attempts to recover interrupted operations and then reconcile Windows' effective state with the saved desired state. Reconciliation may reapply the requested protection. Recovery or reconciliation can encounter conflicts, errors, unsupported devices, or states that cannot be verified, and may not complete. The journal does not by itself guarantee successful recovery or uninterrupted protection during a restart. Check the reported device status after signing in.

### 3.4 Diagnostic Logs and Crash Reports

Diagnostic logs are written locally beneath the `LocalApplicationData` directory:

```text
%LOCALAPPDATA%\PrivGvard\Logs\PrivGvard-*.log
```

The application can also write JSON diagnostic reports when its error-handling code reports an exception or operation error and file output is available, to:

```text
%LOCALAPPDATA%\PrivGvard\CrashReports\crash-*.json
```

Logs can include timestamps, severity, process and thread identifiers, operating-system and runtime information, operation outcomes, native error codes, and exception details. Crash reports can include the exception message and stack trace, inner-exception details, operating-system and runtime information, process identifiers, and a summary of recovery-session status. Exception messages or traces may contain machine-specific details such as local paths; do not assume diagnostic files are free of identifying information. These files are not automatically uploaded by the application code reviewed for this policy. Inspect and redact them before sharing.

## 4. Children’s Privacy

The reviewed application code has no account-registration or child-specific data-collection feature. The local preferences, recovery, and diagnostic files described in Section 3 may still be created when the application is used, regardless of the user's age.

## 5. Open Source Transparency

PrivGvard's source code is available for inspection at:
[https://github.com/cristianbarreiro/PrivGuard](https://github.com/cristianbarreiro/PrivGuard)

## 6. Contact Us

For questions or feedback, use:

- **GitHub Issues:** [https://github.com/cristianbarreiro/PrivGuard/issues](https://github.com/cristianbarreiro/PrivGuard/issues)
- **Publisher:** cdev Studio
