# Windows Autostart Lifecycle

This document describes the implemented Windows startup behavior and the acceptance checks that still require an isolated Windows VM.

## Distribution-specific mechanism

PrivGvard uses exactly one startup mechanism for the distribution currently running:

| Distribution | Registration | Effective state |
| --- | --- | --- |
| Unpackaged Win32 installer or portable build | `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\PrivGvard`, with a quoted absolute executable path and `--minimized` | The Run value must match the current verified `PrivGvard.exe`, the file must exist, and Windows must not have disabled the entry in `StartupApproved\Run`. |
| MSIX | The declared `PrivGvardStartupTask` using `Windows.ApplicationModel.StartupTask` | The state reported by Windows (`Enabled`, `Disabled`, `DisabledByUser`, `DisabledByPolicy`, and policy-enabled states). The manifest passes `--minimized` to the packaged full-trust process. |

The MSIX task is enabled only through `RequestEnableAsync`, which allows Windows to present its own consent UI. The app never removes the current `PrivGvard` `StartupApproved` user-disable override. If the user or policy has disabled startup, the setting remains off and explains where the state is controlled. Disabling uses the distribution's matching API and verifies the effective state afterward.

Win32 registration is rejected when the current path is not fully qualified, does not name a verified `PrivGvard.exe`, is missing, or is a known unstable development output (`bin\Debug`, `bin\Release`, or `publish_out`). A registration to a missing or different executable is not reported as enabled. Registry and StartupTask API exceptions, invalid values, policy blocks, user disablement, and migration conflicts remain distinct statuses.

## Migration and single-instance ordering

After acquiring the Windows single-instance and uninstall-gate mutexes, startup migrates a recognized legacy `PrivLock` Run entry to the current verified Win32 executable, carrying forward its Windows `StartupApproved` user-disable state. It removes the old values only after the replacement is written and checked. Malformed legacy values or registry failures are reported and left for explicit remediation rather than treated as enabled. In MSIX context, startup removes obsolete app-owned Win32 `PrivGvard`/`PrivLock` Run registrations because the package StartupTask is authoritative. A migration error is logged and shown separately from privacy recovery/reconciliation errors; it does not start elevation or mutate privacy state.

The order remains:

```text
Launch (--minimized from the selected registration)
  -> acquire single-instance/uninstall-gate mutexes
  -> migrate only owned obsolete autostart entries
  -> wait for a prior privileged operation to quiesce
  -> recover the durable privacy journal
  -> reconcile desired privacy state
  -> initialize the UI and tray
```

A duplicate process exits before autostart migration, recovery, or privacy mutation. `asInvoker`, the authenticated transient privileged worker, and on-demand UAC are unchanged.

The app hides at minimized startup and hides on window close only after tray initialization succeeds. If tray initialization fails, startup is shown normally, the main-window close action closes the app rather than hiding it, and the desktop lifetime uses main-window-close behavior. This keeps the app reachable and gives the user a normal way to exit.

## Automated evidence and limits

Windows provider tests exercise real reads and writes against unique, isolated per-test HKCU registry keys. Injected registry and StartupTask seams cover failures and the Windows-reported MSIX states. Lifecycle tests that simulate a restart do not log out, reboot Windows, or prove that the OS launches the app after a real login; they are not restart acceptance evidence.

## Manual acceptance matrix — pending isolated Windows VM

Run each distribution in a disposable Windows VM with a standard user account. Record Windows version/build, package or installer version, effective UI state, relevant registration state, and screenshots/log excerpts with personal paths redacted. Do not treat a simulated lifecycle test as a substitute for the actual sign-out/sign-in steps below.

| Distribution | Scenario | Acceptance |
| --- | --- | --- |
| Win32 installer | Fresh install; enable startup | UI reports enabled only after verified Run registration; exactly one `PrivGvard` Run value contains a quoted installed path plus `--minimized`; no `PrivLock` value remains. |
| Win32 installer | Sign out and sign in with tray available | One process starts minimized, tray can restore the window, and no duplicate process remains. This is the required real logon test. |
| Win32 installer | Disable from PrivGvard, then sign out/sign in | Run registration is absent; no PrivGvard startup launch occurs. |
| Win32 installer | Disable from Windows Startup Apps, then open settings | PrivGvard reports disabled-by-user, leaves the Windows override intact, and does not report effective startup as enabled. |
| Win32 installer | Attempt enable from a development output, after deleting/moving the registered executable, or with a non-PrivGvard executable | Enable fails with a visible reason; stale paths are not reported enabled. |
| Win32 installer | Seed only the legacy `PrivLock` app-owned Run and StartupApproved values | Starting the app removes those owned legacy values; no second PrivGvard/PrivLock launch remains. Unrelated Run values are unchanged. |
| Win32 installer | Upgrade installed version in place | The next startup registration points to the current installed executable, not an older directory. |
| MSIX | Fresh package; enable startup | Windows consent appears only after the user enables the setting; accepted consent results in `Enabled` after readback and the manifest launches with `--minimized`. |
| MSIX | Decline consent, or disable in Windows Startup Apps | UI reports the Windows/user-disabled state and explains how to change it; PrivGvard does not silently re-enable the task. |
| MSIX | Apply a policy that disables startup | UI reports `DisabledByPolicy`; enable does not claim success or create a Run-key fallback. |
| MSIX | Disable from PrivGvard | StartupTask is disabled and verified; app-owned obsolete Win32 Run and approval values are removed; no parallel registration remains. |
| MSIX | Update package version with startup enabled | StartupTask continues to target the installed package version; no old `PrivGvard` or `PrivLock` Run value contains an obsolete package path. |
| Both | Run `--minimized` with a working tray, then with tray initialization unavailable | Working tray allows hiding and restoring. Tray failure keeps the main window visible and its close action exits instead of hiding it. |
| Both | Launch two processes concurrently | Exactly one acquires the Windows single-instance mutex; only that process reaches recovery/reconciliation. |
| Both | Exercise startup recovery failure and UAC denial independently | Each failure is reported as recovery/elevation, not as autostart state; the autostart checkbox continues to reflect its actual mechanism. |

**Status:** the manual matrix above has not been executed in a VM. No claim is made that either installer or MSIX startup after a real sign-out/sign-in or reboot has been validated.
