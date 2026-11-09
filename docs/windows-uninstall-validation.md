# Windows uninstall handoff and validation

## Implemented execution paths

### Windows Settings, Control Panel, or Programs and Features

Inno Setup creates the uninstall executable for the installed build and exposes its actual path as `{uninstallexe}`. Its generated name follows `unins???.exe` (the number is selected by Inno; it is not assumed to be `unins000.exe`). `installer/setup.iss` registers `UninstallString` and `QuietUninstallString` to the installed `PrivGvard.exe` wrapper, passing that actual `{uninstallexe}` path; the stable Inno `AppId` is unchanged. Windows therefore starts the unelevated application as the safety launcher, not the Inno uninstaller. See [Inno Setup uninstaller command-line documentation](https://jrsoftware.org/ishelp/topic_uninstcmdline.htm).

The wrapper validates its own installed path and the real Inno executable, acquires the instance and uninstall gates, waits for any prior privileged operation, recovers only journal-owned privacy changes, disables and verifies only PrivGvard's autostart entry, and closes its transient privileged worker. If the UI process is already running, the wrapper signals a same-session uninstall request. The UI runs the normal `ShutdownCoordinator` restoration and exits only when restoration is safe; otherwise it stays open and reports the recovery problem. The wrapper continues only after it acquires the released instance and uninstall gates.

The wrapper then starts the validated Inno executable with `runas`, using its installed directory as the working directory. Since ShellExecute is required for `runas`, the fixed handoff token and optional quiet switches are passed in `ProcessStartInfo.Arguments`. Inno's `InitializeUninstall` rechecks active-session markers and machine ownership claims, acquires the uninstall gate, signals the wrapper's admission event, and waits for the main-instance mutex to be released. Only then does it allow its UI/removal flow to proceed; a failed wait cancels before deletion. This keeps the wrapper's exit and Inno's admission ordered without a circular wait. Inno, not the wrapper, displays the uninstall UI and removes installed files.

If privacy restoration, worker shutdown, autostart cleanup, or handoff fails, the wrapper reports the failure and does not start or continue the uninstall unsafely. The existing finite admission bound remains explicit; its expiry fails the handoff rather than claiming success.

### Uninstall action inside PrivGvard

The current UI has no application-owned uninstall action. There is no second launcher path to duplicate. If one is added later, it must invoke the same validated Windows uninstall request flow rather than starting Inno or duplicating cleanup independently.

## Root cause and follow-up

The earlier handoff fixes remain in place: the wrapper passes the admission token through `ProcessStartInfo.Arguments`, Inno signals admission, and an already-running UI can receive a safe shutdown request. The remaining failure was before that flow: `Program.Main` rejected every elevated user-facing process before logging or parsing the uninstall command. The installed machine-wide uninstall command points to the wrapper, and the user's report that Programs and Features asks for UAC before exiting matches this early-rejection path: after consent, the elevated wrapper returned before initializing logs or launching Inno.

`Program.Main` now permits an elevated token only when the first argument requests `--safe-uninstall`. The existing strict plan validation still runs before services or uninstall work, and normal UI/CLI invocations remain rejected when elevated. The regression test locks down that narrow exception. This is a code-level diagnosis based on the installed command and the silent pre-logging guard; confirm the actual Programs and Features/UAC launch path after installing a rebuilt package in a disposable Windows VM.

## Automated checks and artifact validation

Run the targeted Windows tests and complete active suite on Windows:

```powershell
dotnet test tests\PrivLock.Platform.Windows.Tests\PrivLock.Platform.Windows.Tests.csproj
dotnet test PrivGvard.sln
```

The Windows tests cover ShellExecute argument transport, paths containing spaces, dynamically numbered Inno executable validation, working directory validation, process-start/UAC failures, admission success/timeout, the Inno second-phase process behavior, handle cleanup, and the running-instance shutdown request. Existing recovery, ownership, autostart, mutex, and lifecycle tests remain in the active suite.

Compile and publish the real Inno installer through the repository release script (without building the unrelated MSIX package):

```powershell
.\build-installer.ps1 -SkipMsix
```

This confirms the installer script compiles against the published Windows binary. It does not prove the generated installer's registry values on a live Windows installation; verify the installed `UninstallString` and `QuietUninstallString` point to the installed wrapper and real `{uninstallexe}` path in the manual VM run below.

## Manual Windows VM acceptance — pending

The following checks require an isolated disposable Windows VM; unit tests and compiling the setup do not establish them. Record Windows build, architecture, installer version, registry values, process observations, and redacted logs.

1. Fresh install with paths containing spaces. In Apps/Settings and Programs and Features, inspect both uninstall values and confirm a single entry points to the installed PrivGvard wrapper with the current Inno `{uninstallexe}`.
2. With PrivGvard closed, select **Uninstall** from Windows. Confirm the real Inno UI appears, complete removal, and verify the entry, installed files, and owned shortcuts disappear.
3. With PrivGvard open and privacy state active, select **Uninstall** from Windows. Confirm the UI receives the request, restores the exact owned state, disables only PrivGvard's autostart entry, exits, and allows exactly one Inno uninstaller to continue.
4. Cause a recoverable privacy restore failure. Confirm PrivGvard remains available with an actionable error, the wrapper does not launch Inno, the WAL/ownership evidence remains, and retry can proceed after the issue is resolved.
5. Cause an autostart-cleanup failure. Confirm the wrapper surfaces the failure and does not remove the executable while an owned startup entry may still reference it.
6. Cancel UAC, deny elevation, and exercise missing/unreadable recovery metadata. Confirm errors are visible and no state is reported as safely removed.
7. Upgrade and reinstall over an existing installation. Confirm one Add/Remove Programs entry remains, the AppId is unchanged, and both uninstall values identify the current installed wrapper and Inno executable.
8. Trigger two uninstall requests concurrently. Confirm only one Inno uninstaller obtains admission and the other receives a finite, explicit failure instead of hanging.

Do not run forced-kill, logout, or hardware recovery scenarios on the development workstation. No live installer/uninstaller or destructive VM acceptance is claimed until those steps are run.
