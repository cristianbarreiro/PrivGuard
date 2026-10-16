# Windows Autostart Lifecycle

This document describes the implemented Windows startup behavior and the acceptance checks that still require an isolated Windows VM.

## Distribution-specific mechanism

PrivGvard uses exactly one startup mechanism for the distribution currently running:

| Distribution | Registration | Effective state |
| --- | --- | --- |
| Unpackaged Win32 installer or portable build | `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\PrivGvard`, with a quoted absolute executable path and `--minimized` | The Run value must match the current verified `PrivGvard.exe`, the file must exist, and Windows must not have disabled the entry in `StartupApproved\Run`. |
| MSIX | The declared `PrivGvardStartupTask` using `Windows.ApplicationModel.StartupTask` | The state reported by Windows (`Enabled`, `Disabled`, `DisabledByUser`, `DisabledByPolicy`, and policy-enabled states). The manifest passes `--minimized` to the packaged full-trust process. |

The MSIX task is enabled only through `RequestEnableAsync`, which allows Windows to present its own consent UI. The app never removes the current `PrivGvard` `StartupApproved` user-disable override. If the user or policy has disabled startup, the setting remains off and explains where the state is controlled. Disabling uses the distribution's matching API and verifies the effective state afterward.

Win32 registration is rejected when the current path is not fully qualified, does not name a verified `PrivGvard.exe`, is missing, or is a known unstable development output (`bin\Debug\<TFM>`, `bin\Release\<TFM>`, or `publish_out`). A published executable under a plain `bin\Release` directory is not classified as an SDK build output without the following target-framework segment. A registration to a missing or different executable is not reported as enabled. Registry and StartupTask API exceptions, invalid values, policy blocks, user disablement, and migration conflicts remain distinct statuses.

## Migration and single-instance ordering

After acquiring the Windows single-instance and uninstall-gate mutexes, Win32 startup migrates a recognized legacy `PrivLock` Run entry to the current verified executable. It validates any existing `PrivGvard` replacement and rejects conflicting `StartupApproved` user states before changing either registration. The old values are removed only after the replacement and its effective approval state are verified; failures attempt a rollback. In MSIX context, startup first reads and validates the package `StartupTask`, both app-owned Run commands, and their approval states. A prior disabled Win32 preference is carried to an enabled, non-policy-controlled task before stale Run entries are removed. If the task cannot be read, policy conflicts with that preference, or an entry is malformed, migration leaves the Win32 values in place and reports the conflict. A migration error is shown separately from privacy recovery/reconciliation errors; it does not start elevation or mutate privacy state.

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

The app hides at minimized startup and hides on window close only after the active Avalonia backend returns a native tray implementation and tray initialization succeeds. If tray support is unavailable, startup is shown normally and X/Alt+F4 goes through the same coordinated restore flow as the tray Exit command. The window remains open and surfaces the restore error if recovery is not safe, allowing the user to retry or intervene.

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
| Win32 installer | Seed only a recognized legacy `PrivLock` Run entry and StartupApproved value | Starting the app migrates the entry to the verified current executable, preserves a user-disabled state, removes the old owned values only after verification, and leaves unrelated Run values unchanged. |
| Win32 installer | Seed an invalid current `PrivGvard` command or conflicting `PrivLock`/`PrivGvard` StartupApproved states | Migration reports failure and leaves both Run values and approval records unchanged. |
| Win32 installer | Upgrade installed version in place | The next startup registration points to the current installed executable, not an older directory. |
| MSIX | Fresh package; enable startup | Windows consent appears only after the user enables the setting; accepted consent results in `Enabled` after readback and the manifest launches with `--minimized`. |
| MSIX | Decline consent, or disable in Windows Startup Apps | UI reports the Windows/user-disabled state and explains how to change it; PrivGvard does not silently re-enable the task. |
| MSIX | Apply a policy that disables startup | UI reports `DisabledByPolicy`; enable does not claim success or create a Run-key fallback. |
| MSIX | Transition from Win32 with Startup Apps disabled | A readable, non-policy-enabled StartupTask is disabled before old Run and approval entries are removed; policy/state conflicts leave the old registrations intact and report migration failure. |
| MSIX | Disable from PrivGvard | StartupTask is disabled and verified; app-owned obsolete Win32 Run and approval values are removed; no parallel registration remains. |
| MSIX | Update package version with startup enabled | StartupTask continues to target the installed package version; no old `PrivGvard` or `PrivLock` Run value contains an obsolete package path. |
| Both | Run `--minimized` with a working tray, then with tray support unavailable | Working tray allows hiding and restoring. Unsupported tray keeps the main window visible; X/Alt+F4 runs coordinated restoration and cannot close the window while retryable recovery remains. |
| Both | Launch two processes concurrently | Exactly one acquires the Windows single-instance mutex; only that process reaches recovery/reconciliation. |
| Both | Exercise startup recovery failure and UAC denial independently | Each failure is reported as recovery/elevation, not as autostart state; the autostart checkbox continues to reflect its actual mechanism. |

**Status:** the manual matrix above has not been executed in a VM. No claim is made that either installer or MSIX startup after a real sign-out/sign-in or reboot has been validated.
